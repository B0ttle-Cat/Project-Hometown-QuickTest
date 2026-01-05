//using System.Collections.Generic;

//using static StrategyGamePlayData;
//public partial class StrategyUpdate
//{
//    public class StrategyUpdate_PersonnelSupply : StrategyUpdateSubClass<StrategyUpdate_PersonnelSupply.ResourcesSupply>
//	{
//		public StrategyUpdate_PersonnelSupply(StrategyUpdate updater) : base(updater)
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
//				this.AddItem(new ResourcesSupply(this, cb));
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
//			private SectorObject iamge;
//			private const StatsType MaxType = StatsType.자원_인력_최대;
//			private const StatsType OrganizationList = StatsType.자원_인력_회복;
//			private const StatsType CurrType = StatsType.자원_인력_현재;
			
//			private const float resetResupplyTime = 1f;
//			float currentResupplyTime; // 다음 보충까지 남은 시간.
//			float supplement; // 다음에 보충될 양

//			public ResourcesSupply(StrategyUpdateSubClass<ResourcesSupply> thisSubClass, SectorObject iamge) : base(thisSubClass)
//			{
//				this.iamge = iamge;
//				currentResupplyTime = resetResupplyTime;
//				supplement = 0f;
//			}
//			protected override void OnDispose()
//			{
//				iamge = null;
//			}
//			protected override void OnUpdate(in float deltaTime)
//			{
//				if (iamge == null || !iamge.isActiveAndEnabled) return;
//				if (iamge.CaptureData.captureFactionID < 0) return;

//				int max = iamge.SectorStatsGroup.GetValue(MaxType);
//				int supply = iamge.SectorStatsGroup.GetValue(OrganizationList);
//				int curr = iamge.CurrStatsList.GetValue(CurrType);

//				if (ResourcesUpdate(ref curr, in max, in supply, ref supplement, ref currentResupplyTime, resetResupplyTime, in deltaTime))
//				{
//					iamge.SetPersonnel(curr);

//					string key = $"{iamge.SectorName}_{UpdateLogicSort.거점_자원갱신종료}";
//					TempData.SetTrigger(key, UpdateLogicSort.거점_자원갱신종료);

//					int elementID = iamge.CaptureData.captureFactionID;
//					key = $"{elementID}_{UpdateLogicSort.세력_자원갱신종료}";
//					TempData.SetTrigger(key, UpdateLogicSort.세력_자원갱신종료);
//					if (TempData.TryGetValue<TempSupplyValue>(FactionTempSupplyValueKey(elementID), out var tempValue))
//					{
//						tempValue.personnel += curr;
//						tempValue.personnelMax += max;
//					}
//				}
//			}
//		}

//	}
//}

