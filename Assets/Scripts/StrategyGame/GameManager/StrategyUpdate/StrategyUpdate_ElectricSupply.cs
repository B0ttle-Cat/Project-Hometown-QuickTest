using System.Collections.Generic;

using static StrategyGamePlayData;
public partial class StrategyUpdate
{
	public class StrategyUpdate_ResourcesSupply : StrategyUpdateSubClass<StrategyUpdate_ResourcesSupply.ResourcesSupply>
	{
		public StrategyUpdate_ResourcesSupply(StrategyUpdate updater) : base(updater)
		{
		}
		protected override void Start()
		{
			updateList = new List<ResourcesSupply>();
			var list = StrategyManager.Collector.SectorList;
			int length = list.Count;
			for (int i = 0 ; i < length ; i++)
			{
				var cb = list[i];
				updateList.Add(new ResourcesSupply(this, cb));
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

			public ResourcesSupply(StrategyUpdateSubClass<ResourcesSupply> thisSubClass, SectorObject sector) : base(thisSubClass)
			{
				this.sector = sector;
				manpowerPlanner = new SupplyPlanner(StatsType.거점_인력_최대, StatsType.거점_인력_회복, StatsType.거점_인력_현재, 30);
				materialPlanner = new SupplyPlanner(StatsType.거점_재료_최대, StatsType.거점_재료_회복, StatsType.거점_재료_현재, 10);
				electricPlanner = new SupplyPlanner(StatsType.거점_전력_최대, StatsType.거점_전력_회복, StatsType.거점_전력_현재, 1);
			}
			protected override void OnDispose()
			{
				sector = null;
			}
			protected override void OnUpdate(in float deltaTime)
			{
				if (sector == null || !sector.isActiveAndEnabled) return;
				if (sector.CaptureData.captureFactionID < 0) return;

				bool isUpdate = false;
				(int curr, int max) electric = Update_Electric(ref electricPlanner, in deltaTime, 1f);
				(int curr, int max) material = Update_Material(ref materialPlanner, in deltaTime, 1f);
				(int curr, int max) manpower = Update_Manpower(ref electricPlanner, in deltaTime, 1f);
				if (isUpdate)
				{
					string key = $"{sector.SectorName}_{UpdateLogicSort.거점_자원갱신종료이벤트}";
					TempData.SetTrigger(key, UpdateLogicSort.거점_자원갱신종료이벤트);

					int factionID = sector.CaptureData.captureFactionID;
					key = $"{factionID}_{UpdateLogicSort.세력_자원갱신종료이벤트}";
					TempData.SetTrigger(key, UpdateLogicSort.세력_자원갱신종료이벤트);
					if (TempData.TryGetValue<FactionTempSupplyValue>(FactionTempSupplyValueKey(factionID), out var tempValue))
					{
						tempValue.electric += electric.curr;
						tempValue.electricMax += electric.max;

						tempValue.material += material.curr;
						tempValue.materialMax += material.max;

						tempValue.manpower += manpower.curr;
						tempValue.manpowerMax += manpower.max;
					}
				}

				(int curr, int max) Update_Electric(ref SupplyPlanner planner, in float deltaTime, float supplyFactor = 1f)
				{
					int max = sector.SectorStatsGroup.GetValue(planner.MaxType);
					int supply = sector.SectorStatsGroup.GetValue(planner.SupplyType);
					int curr = sector.CurrStatsList.GetValue(planner.CurrType);
					float resetResupplyTime = planner.ResetResupplyTime;
					float supplement = planner.Supplement;
					float currentResupplyTime = planner.CurrentResupplyTime;

					if (ResourcesUpdate(ref curr, in max, in supply, ref supplement, ref currentResupplyTime, in resetResupplyTime, deltaTime * supplyFactor))
					{
						sector.SetElectric(curr);
						isUpdate = true;
					}
					planner.Supplement = supplement;

					return (curr, max);
				}
				(int curr, int max) Update_Material(ref SupplyPlanner planner, in float deltaTime, float supplyFactor = 1f)
				{
					int max = sector.SectorStatsGroup.GetValue(planner.MaxType);
					int supply = sector.SectorStatsGroup.GetValue(planner.SupplyType);
					int curr = sector.CurrStatsList.GetValue(planner.CurrType);
					float resetResupplyTime = planner.ResetResupplyTime;
					float supplement = planner.Supplement;
					float currentResupplyTime = planner.CurrentResupplyTime;

					if (ResourcesUpdate(ref curr, in max, in supply, ref supplement, ref currentResupplyTime, in resetResupplyTime, deltaTime * supplyFactor))
					{
						sector.SetMaterial(curr);
						isUpdate = true;
					}
					planner.Supplement = supplement;

					return (curr, max);
				}
				(int curr, int max) Update_Manpower(ref SupplyPlanner planner, in float deltaTime, float supplyFactor = 1f)
				{
					int max = sector.SectorStatsGroup.GetValue(planner.MaxType);
					int supply = sector.SectorStatsGroup.GetValue(planner.SupplyType);
					int curr = sector.CurrStatsList.GetValue(planner.CurrType);
					float resetResupplyTime = planner.ResetResupplyTime;
					float supplement = planner.Supplement;
					float currentResupplyTime = planner.CurrentResupplyTime;

					if (ResourcesUpdate(ref curr, in max, in supply, ref supplement, ref currentResupplyTime, in resetResupplyTime, deltaTime * supplyFactor))
					{
						sector.SetManpower(curr);
						isUpdate = true;
					}
					planner.Supplement = supplement;

					return (curr, max);
				}
			}
		}
	}
}

