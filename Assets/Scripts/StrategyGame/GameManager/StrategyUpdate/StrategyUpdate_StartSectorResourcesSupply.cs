using System.Collections.Generic;

using static StrategyGamePlayData;
public partial class StrategyUpdate
{
	public class StrategyUpdate_StartSectorResourcesSupply : StrategyUpdateSubClass<StrategyUpdate_StartSectorResourcesSupply.ResourcesSupply>
	{
		public StrategyUpdate_StartSectorResourcesSupply(StrategyUpdate updater) : base(updater)
		{
		}
		protected override void Dispose()
		{
			StrategyManager.Collector.RemoveChangeListener<SectorObject>(OnChangeSector);
		}
		protected override void Start()
		{
			updateList = new List<ResourcesSupply>();
			var list = StrategyManager.Collector.SectorList;
			int length = list.Count;
			for (int i = 0 ; i < length ; i++)
			{
				var sector = list[i];
				if (sector == null) continue;
				updateList.Add(new ResourcesSupply(sector, this));
			}
			StrategyManager.Collector.AddChangeListener<SectorObject>(OnChangeSector);
		}
		private void OnChangeSector(IStrategyElement element, bool isAdd)
		{
			if (element == null || element is not SectorObject sector || sector == null) return;

			if (isAdd)
			{
				UpdateList.Add(new ResourcesSupply(sector, this));
			}
			else
			{
				int findIndex = UpdateList.FindIndex(i=>i.Sector.Equals(sector));
				if (findIndex < 0) return;
				UpdateList.RemoveAt(findIndex);
			}
		}
		protected override void Update(in float deltaTime)
		{
			int length = updateList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				var item = updateList[i];
				if (item == null) continue;

				item.Update(in deltaTime);
			}
		}


		public class ResourcesSupply : UpdateLogic
		{
			private SectorObject sector;
			public SectorObject Sector => sector;

			SupplyPlanner electricPlanner;
			SupplyPlanner materialPlanner;
			SupplyPlanner manpowerPlanner;

			public struct SupplyPlanner
			{
				private readonly StatsType maxType;
				private readonly StatsType supplyType;
				private readonly StatsType currType;
				private readonly float resetResupplyTime;
				private float currentResupplyTime;
				private float supplement;

				public SupplyPlanner(StatsType maxType, StatsType supplyType, StatsType currType, float resupplyTime)
				{
					this.maxType = maxType;
					this.supplyType = supplyType;
					this.currType = currType;
					currentResupplyTime = resetResupplyTime = resupplyTime;
					supplement = 0;
				}

				public readonly StatsType MaxType => maxType;
				public readonly StatsType SupplyType => supplyType;
				public readonly StatsType CurrType => currType;
				public readonly float ResetResupplyTime => resetResupplyTime;
				public float Supplement { readonly get => supplement; set => supplement = value; }
				public float CurrentResupplyTime { readonly get => currentResupplyTime; set => currentResupplyTime = value; }

			}

			public ResourcesSupply(SectorObject sector, StrategyUpdateSubClass<ResourcesSupply> thisSubClass) : base(thisSubClass)
			{
				this.sector = sector;
				electricPlanner = new SupplyPlanner(StatsType.거점_전력_최대, StatsType.거점_전력_회복, StatsType.거점_전력_현재, 1);
				materialPlanner = new SupplyPlanner(StatsType.거점_재료_최대, StatsType.거점_재료_회복, StatsType.거점_재료_현재, 10);
				manpowerPlanner = new SupplyPlanner(StatsType.거점_인력_최대, StatsType.거점_인력_회복, StatsType.거점_인력_현재, 30);
			}
			protected override void OnDispose()
			{
				sector = null;
			}
			protected override void OnUpdate(in float deltaTime)
			{
				if (sector == null || !sector.isActiveAndEnabled) return;
				if (sector.CaptureData.captureFactionID < 0) return;

				(int curr, int max, int supply, bool isUpdate) electric = Update_EtchOther(in sector, ref electricPlanner, in deltaTime, 1f);
				(int curr, int max, int supply, bool isUpdate) material = Update_EtchOther(in sector, ref materialPlanner, in deltaTime, 1f);
				(int curr, int max, int supply, bool isUpdate) manpower = Update_EtchOther(in sector, ref manpowerPlanner, in deltaTime, 1f);
				if (electric.isUpdate || material.isUpdate || manpower.isUpdate) UpdateTempData(sector, TempData);

				void UpdateTempData(SectorObject sector, StrategyUpdate.StrategyUpdateTempData tempData)
				{
					string key = $"{sector.SectorName}_ResourcesSupply";
					tempData.SetTrigger(key, UpdateLogicSort.거점_자원갱신종료);
					TempSupplyValue sectorTempSupplyValue = new (sector)
					{
						electric = electric.curr,
						material = material.curr,
						manpower = manpower.curr,
						electricMax = electric.max,
						materialMax = material.max,
						manpowerMax = manpower.max,
						electricSupply = electric.supply,
						materialSupply = material.supply,
						manpowerSupply = manpower.supply,
						electricIsUpdate = electric.isUpdate,
						materialIsUpdate = material.isUpdate,
						manpowerIsUpdate = manpower.isUpdate,
					};
					tempData.SetValue(SectorTempSupplyValueKey(sector), sectorTempSupplyValue, UpdateLogicSort.거점_자원갱신종료);

					int factionID = sector.CaptureData.captureFactionID;
					key = $"{factionID}_ResourcesSupply";
					tempData.SetTrigger(key, UpdateLogicSort.세력_자원갱신종료);
					if (tempData.TryGetValue<TempSupplyValue>(FactionTempSupplyValueKey(factionID), out var tempFactionValue))
					{
						tempData.SetValue(FactionTempSupplyValueKey(factionID), tempFactionValue + sectorTempSupplyValue);
					}
				}

				#region Update EtchOther
				static (int curr, int max, int supply, bool isUpdate) Update_EtchOther(in SectorObject sector, ref SupplyPlanner planner, in float deltaTime, float supplyFactor = 1f)
				{
					int max = sector.SectorStatsGroup.GetValue(planner.MaxType);
					int supply = sector.SectorStatsGroup.GetValue(planner.SupplyType);
					int curr = sector.CurrStatsList.GetValue(planner.CurrType);
					float resetResupplyTime = planner.ResetResupplyTime;
					float supplement = planner.Supplement;
					float currentResupplyTime = planner.CurrentResupplyTime;

					bool result = ResourcesUpdate(ref curr, in max, in supply, ref supplement, ref currentResupplyTime, in resetResupplyTime, deltaTime * supplyFactor);
					planner.Supplement = supplement;
					return (curr, max, supply, result);
				}
				#endregion
				#region ResourcesUpdate
				static bool ResourcesUpdate(ref int current, in int max, in int supplyPerTanSec, ref float supplement, ref float currentResupplyTime, in float resetResupplyTime, in float deltaTime)
				{
					if (max <= 0 || max < current) return false;
					CumulativeUpdate(in current, in max, in supplyPerTanSec, ref supplement, in deltaTime);
					if (CheckResupplyTime(ref currentResupplyTime, in resetResupplyTime, in deltaTime))
					{
						return SupplyUpdate(ref current, in max, ref supplement);
					}
					return false;
				}
				static void CumulativeUpdate(in int current, in int max, in int supplyPerTanSec, ref float supplement, in float deltaTime)
				{
					if (current >= max)
					{
						supplement = 0;
						return;
					}
					float supplyPerDelta  = supplyPerTanSec * 0.1f * deltaTime;
					supplement += supplyPerDelta;
				}
				static bool CheckResupplyTime(ref float currentResupplyTime, in float resetResupplyTime, in float deltaTime)
				{
					currentResupplyTime -= deltaTime;
					if (currentResupplyTime <= 0)
					{
						currentResupplyTime = resetResupplyTime;
						return true;
					}
					return false;
				}
				static bool SupplyUpdate(ref int current, in int max, ref float cumulative)
				{
					if (current >= max)
					{
						cumulative = 0;
						return false;
					}
					if (cumulative < 1) return false;

					int intCumulative = (int)cumulative;
					cumulative -= intCumulative;
					current += intCumulative;
					//current = Mathf.Clamp(current + intCumulative, 0, max);

					return true;
				}
				#endregion
			}
		}
	}
}

