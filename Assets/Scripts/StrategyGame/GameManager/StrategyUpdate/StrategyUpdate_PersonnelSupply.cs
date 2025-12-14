//using System.Collections.Generic;

//using static StrategyGamePlayData;
//public partial class StrategyUpdate
//{
//    public class StrategyUpdate_ManpowerSupply : StrategyUpdateSubClass<StrategyUpdate_ManpowerSupply.ResourcesSupply>
//	{
//		public StrategyUpdate_ManpowerSupply(StrategyUpdate updater) : base(updater)
//		{
//		}

//		protected override void Start()
//		{
//			this = new PoolList<ResourcesSupply>();
//			var activeList = StrategyManager.Collector.SectorList;
//			int length = activeList.Count;
//			for (int i = 0 ; i < length ; i++)
//			{
//				var cb = activeList[i];
//				this.Add(new ResourcesSupply(this, cb));
//			}
//		}
//		protected override void Update(in float deltaTime)
//		{
//			int length = this.Count;
//			for (int i = 0 ; i < length ; i++)
//			{
//				var item = this[i];
//				if (item == null) continue;

//				item.Update(in deltaTime);
//			}
//		}

//		public class ResourcesSupply : UpdateLogic
//		{
//			private SectorObject sector;
//			private const StatsType MaxType = StatsType.거점_인력_최대;
//			private const StatsType SupplyType = StatsType.거점_인력_회복;
//			private const StatsType CurrType = StatsType.거점_인력_현재;
			
//			private const float resetResupplyTime = 1f;
//			float currentResupplyTime; // 다음 보충까지 남은 시간.
//			float supplement; // 다음에 보충될 양

//			public ResourcesSupply(StrategyUpdateSubClass<ResourcesSupply> thisSubClass, SectorObject sector) : base(thisSubClass)
//			{
//				this.sector = sector;
//				currentResupplyTime = resetResupplyTime;
//				supplement = 0f;
//			}
//			protected override void OnDispose()
//			{
//				sector = null;
//			}
//			protected override void OnUpdate(in float deltaTime)
//			{
//				if (sector == null || !sector.isActiveAndEnabled) return;
//				if (sector.CaptureData.captureFactionID < 0) return;

//				int max = sector.SectorStatsGroup.GetValue(MaxType);
//				int supply = sector.SectorStatsGroup.GetValue(SupplyType);
//				int curr = sector.CurrStatsList.GetValue(CurrType);

//				if (ResourcesUpdate(ref curr, in max, in supply, ref supplement, ref currentResupplyTime, resetResupplyTime, in deltaTime))
//				{
//					sector.SetManpower(curr);

//					string key = $"{sector.SectorName}_{UpdateLogicSort.거점_자원갱신종료}";
//					TempData.SetTrigger(key, UpdateLogicSort.거점_자원갱신종료);

//					int elementID = sector.CaptureData.captureFactionID;
//					key = $"{elementID}_{UpdateLogicSort.세력_자원갱신종료}";
//					TempData.SetTrigger(key, UpdateLogicSort.세력_자원갱신종료);
//					if (TempData.TryGetValue<TempSupplyValue>(FactionTempSupplyValueKey(elementID), out var tempValue))
//					{
//						tempValue.manpower += curr;
//						tempValue.manpowerMax += max;
//					}
//				}
//			}
//		}

//	}
//}

