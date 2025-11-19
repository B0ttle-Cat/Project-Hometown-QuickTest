using System.Collections.Generic;

using static StrategyGamePlayData;
public partial class StrategyUpdate
{
    public class StrategyUpdate_MaterialSupply : StrategyUpdateSubClass<StrategyUpdate_MaterialSupply.ResourcesSupply>
	{
		public StrategyUpdate_MaterialSupply(StrategyUpdate updater) : base(updater)
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

			private const StatsType MaxType = StatsType.거점_재료_최대;
			private const StatsType SupplyType = StatsType.거점_재료_회복;
			private const StatsType CurrType = StatsType.거점_재료_현재;

			private const float resetResupplyTime = 10f;
			float currentResupplyTime; // 다음 보충까지 남은 시간.
			float supplement; // 다음에 보충될 양

			public ResourcesSupply(StrategyUpdateSubClass<ResourcesSupply> thisSubClass, SectorObject sector) : base(thisSubClass)
			{
				this.sector = sector;
				currentResupplyTime = resetResupplyTime;
				supplement = 0f;
			}
			protected override void OnDispose()
			{
				sector = null;
			}
			protected override void OnUpdate(in float deltaTime)
			{
				if (sector == null || !sector.isActiveAndEnabled) return;
				if (sector.CaptureData.captureFactionID < 0) return;

				int max = sector.SectorStatsGroup.GetValue(MaxType);
				int supply = sector.SectorStatsGroup.GetValue(SupplyType);
				int curr = sector.CurrStatsList.GetValue(CurrType);

				if (ResourcesUpdate(ref curr, in max, in supply, ref supplement, ref currentResupplyTime, resetResupplyTime, in deltaTime))
				{
					sector.SetMaterial(curr);

					string key = $"{sector.SectorName}_{UpdateLogicSort.거점_자원갱신종료이벤트}";
					TempData.SetTrigger(key, UpdateLogicSort.거점_자원갱신종료이벤트);

					int factionID = sector.CaptureData.captureFactionID;
					key = $"{factionID}_{UpdateLogicSort.세력_자원갱신종료이벤트}";
					TempData.SetTrigger(key, UpdateLogicSort.세력_자원갱신종료이벤트);
					if (TempData.TryGetValue<FactionTempSupplyValue>(FactionTempSupplyValueKey(factionID), out var tempValue))
					{
						tempValue.material += curr;
						tempValue.manpowerMax += max;
					}
				}
			}
		}

	}
}