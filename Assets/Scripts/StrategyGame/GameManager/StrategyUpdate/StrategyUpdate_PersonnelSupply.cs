using System.Collections.Generic;

using UnityEngine;

using static StrategyGamePlayData;
public partial class StrategyUpdate
{
    public class StrategyUpdate_PersonnelSupply : StrategyUpdateSubClass<StrategyUpdate_PersonnelSupply.ResourcesSupply>
	{
		public StrategyUpdate_PersonnelSupply(StrategyUpdate updater) : base(updater)
		{
		}

		protected override void Start()
		{
			updateList = new List<ResourcesSupply>();
			var list = StrategyManager.Collector.ControlBaseList;
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
			private ControlBase cb;

			private const StatsType MaxType = StatsType.거점_인력_최대보유량;
			private const StatsType SupplyType = StatsType.거점_인력_분당회복량;
			private const StatsType CurrType = StatsType.거점_인력_현재보유량;
			private const float resupplyTime = 10f;

			float replenish; // 다음 보충까지 남은 시간.
			float surplus; // 여분의 보충량

			public ResourcesSupply(StrategyUpdateSubClass<ResourcesSupply> thisSubClass, ControlBase cb) : base(thisSubClass)
			{
				this.cb = cb;
				replenish = resupplyTime;
				surplus = 0f;
			}
			protected override void OnDispose()
			{
				cb = null;
			}
			protected override void OnUpdate(in float deltaTime)
			{
				if (cb == null || !cb.isActiveAndEnabled) return;

				StatsList MainStatsList = cb.MainStatsList;
				StatsGroup FacilitiesBuffGroup = cb.FacilitiesBuffGroup;
				StatsGroup supportBuffGroup = cb.SupportBuffGroup;

				var maxMain = MainStatsList.GetValue(MaxType);
				var maxFacilities = FacilitiesBuffGroup.GetValue(MaxType);
				var maxSupport = supportBuffGroup.GetValue(MaxType);
				var maxTotal = maxMain+ maxFacilities + maxSupport;

				var supplyMain = MainStatsList.GetValue(SupplyType);
				var supplyFacilities = FacilitiesBuffGroup.GetValue(SupplyType);
				var supplySupport = supportBuffGroup.GetValue(SupplyType);
				var supplyTotal = supplyMain + supplyFacilities + supplySupport;

				var currMain = MainStatsList.GetValue(CurrType);
				var currTotal = currMain;

				int 최대보유량 = maxTotal.Value;
				int 분당회복량 = supplyTotal.Value;
				int 현재보유량 = currTotal.Value;

				bool isUpdate = false;

				CumulativeUpdate(in 현재보유량, in 최대보유량, in 분당회복량, ref surplus, in deltaTime);
				if (UpdateResupplyTime(ref replenish, deltaTime, resupplyTime))
					SupplyUpdate(ref 현재보유량, in 최대보유량, ref surplus, ref isUpdate);

				bool UpdateResupplyTime(ref float currentResupplyTime, in float deltaTime, float resupplyTime)
				{
					currentResupplyTime -= deltaTime;
					if (currentResupplyTime <= 0)
					{
						currentResupplyTime = resupplyTime;
						return true;
					}
					return false;
				}
				void CumulativeUpdate(in int current, in int max, in int supplyPerMinute, ref float cumulative, in float deltaTime)
				{
					if (current >= max)
					{
						cumulative = 0;
						return;
					}
					float supplyPerDelta  = ((float)supplyPerMinute / 60f) * deltaTime;
					cumulative += supplyPerDelta;
				}
				void SupplyUpdate(ref int current, in int max, ref float cumulative, ref bool isUpdate)
				{
					if (current >= max)
					{
						cumulative = 0;
						return;
					}
					int intCumulative = Mathf.FloorToInt(cumulative);
					float rate = cumulative - intCumulative;

					current = Mathf.Clamp(current + intCumulative, 0, max);
					isUpdate = true;
				}

				if (isUpdate)
				{
					MainStatsList.SetValue(CurrType, 현재보유량);

					string key = $"{cb.ControlBaseName}_{UpdateLogicSort.거점_자원갱신이벤트}";
					TempData.SetValue(key, true, UpdateLogicSort.거점_자원갱신이벤트);
				}
			}
		}

	}
}

