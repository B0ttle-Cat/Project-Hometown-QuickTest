using System.Collections.Generic;

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

			private const StatsType MaxType = StatsType.거점_인력_최대;
			private const StatsType SupplyType = StatsType.거점_인력_회복;
			private const StatsType CurrType = StatsType.거점_인력_현재;
			private const float resupplyTime = 10f;

			float replenish; // 다음 보충까지 남은 시간.
			float surplus; // 여분의 보충량

			public ResourcesSupply(StrategyUpdateSubClass<ResourcesSupply> thisSubClass, SectorObject sector) : base(thisSubClass)
			{
				this.sector = sector;
				replenish = resupplyTime;
				surplus = 0f;
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

				bool isUpdate = false;

				CumulativeUpdate(in curr, in max, in supply, ref surplus, in deltaTime);
				if (UpdateResupplyTime(ref replenish, deltaTime, resupplyTime))
					SupplyUpdate(ref curr, in max, ref surplus, ref isUpdate);

				if (isUpdate)
				{
					sector.SetPersonnel(curr);

					string key = $"{sector.SectorName}_{UpdateLogicSort.거점_자원갱신이벤트}";
					TempData.SetTrigger(key, UpdateLogicSort.거점_자원갱신이벤트);
					//Debug.Log($"Pressed PersonnelSupply| Sector:{sector.SectorName,-10} | Faction:{sector.CaptureData.captureFactionID,-10} | Point:{현재보유량,4}/{최대보유량 - 4}");
				}
			}
		}

	}
}

