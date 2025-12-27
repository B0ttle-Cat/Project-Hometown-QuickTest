//using System.Collections.Generic;

//using static StrategyGamePlayData;
//public partial class StrategyUpdate
//{
//    public class StrategyUpdate_MaterialSupply : StrategyUpdateSubClass<StrategyUpdate_MaterialSupply.ResourcesSupply>
//	{
//		public StrategyUpdate_MaterialSupply(StrategyUpdate updater) : base(updater)
//		{
//		}

//		protected override void Start()
//		{
//			this = new PoolList<ResourcesSupply>();
//			var activeList = StrategyManager.Collector.CapturedList;
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
//			private SectorObject target;

//			private const StatsType MaxType = StatsType.자원_재료_최대;
//			private const StatsType SupplyType = StatsType.자원_재료_회복;
//			private const StatsType CurrType = StatsType.자원_재료_현재;

//			private const float resetResupplyTime = 10f;
//			float currentResupplyTime; // 다음 보충까지 남은 시간.
//			float supplement; // 다음에 보충될 양

//			public ResourcesSupply(StrategyUpdateSubClass<ResourcesSupply> thisSubClass, SectorObject target) : base(thisSubClass)
//			{
//				this.target = target;
//				currentResupplyTime = resetResupplyTime;
//				supplement = 0f;
//			}
//			protected override void OnDispose()
//			{
//				target = null;
//			}
//			protected override void OnUpdate(in float deltaTime)
//			{
//				if (target == null || !target.isActiveAndEnabled) return;
//				if (target.CaptureData.captureFactionID < 0) return;

//				int max = target.SectorStatsGroup.GetValue(MaxType);
//				int supply = target.SectorStatsGroup.GetValue(SupplyType);
//				int curr = target.CurrStatsList.GetValue(CurrType);

//				if (ResourcesUpdate(ref curr, in max, in supply, ref supplement, ref currentResupplyTime, resetResupplyTime, in deltaTime))
//				{
//					target.SetMaterial(curr);

//					string key = $"{target.SectorName}_{UpdateLogicSort.거점_자원갱신종료}";
//					TempData.SetTrigger(key, UpdateLogicSort.거점_자원갱신종료);

//					int elementID = target.CaptureData.captureFactionID;
//					key = $"{elementID}_{UpdateLogicSort.세력_자원갱신종료}";
//					TempData.SetTrigger(key, UpdateLogicSort.세력_자원갱신종료);
//					if (TempData.TryGetValue<TempSupplyValue>(FactionTempSupplyValueKey(elementID), out var tempValue))
//					{
//						tempValue.reservationMaterial += curr;
//						tempValue.personnelMax += max;
//					}
//				}
//			}
//		}

//	}
//}