//using System;


//namespace StrategyManagerModule
//{
//	public partial class StrategyUpdate
//	{
//		[Obsolete("NearbyUpdate 에서 해결함", true)]
//		public class StrategyUpdate_FactionDetectListUpdate : StrategyUpdateSubClass<StrategyUpdate_FactionDetectListUpdate.DetectListUpdate>
//		{
//			public StrategyUpdate_FactionDetectListUpdate(StrategyUpdate updater) : base(updater)
//			{
//			}
//			protected override void Dispose()
//			{
//				StrategyManager.Collector.RemoveChangeListener<Faction>(OnChangeValue);
//			}
//			protected override void Start()
//			{
//				StrategyManager.Collector.AddChangeListener<Faction>(OnChangeValue, true);
//			}

//			private void OnChangeValue(IStrategyElement element, bool added)
//			{
//				if (element == null) return;
//				if (element is Faction faction)
//				{
//					if (added)
//					{
//						this.AddItem(new DetectListUpdate(faction, this));
//					}
//					else
//					{
//						int findIndex = this.FindIndex(i=>i.thisFactionID == faction.FactionID);
//						if (findIndex < 0) return;
//						this.RemoveAt(findIndex);
//					}
//				}
//			}
//			protected override void Update(in float deltaTime)
//			{
//				int length = this == null ? 0 : this.Count;
//				for (int i = 0 ; i < length ; i++)
//				{
//					var item = this[i];
//					if (item == null) continue;
//					item.Update(in deltaTime);
//				}
//			}
//			public class DetectListUpdate : UpdateLogic
//			{
//				public readonly Faction faction;
//				public readonly int thisFactionID;

//				public DetectListUpdate(Faction faction, StrategyUpdate_FactionDetectListUpdate thisSubClass) : base(thisSubClass)
//				{
//					this.faction = faction;
//					thisFactionID = faction.FactionID;
//				}

//				protected override void OnDispose()
//				{
//				}

//				protected override void OnUpdate(in float deltaTime)
//				{
//					faction.ClearElementSet();

//					var allElement =  StrategyManager.Collector.GetAllElementLists();
//					foreach (var items in allElement)
//					{
//						int length = items.Count;
//						for (int i = 0 ; i < length ; i++)
//						{
//							var item = items[i];
//							if (item is not INearbySearcher searcher) break; // 내가 원하는 타입이 아니기 때문이 이번 List를 버린다.
//							if (searcher.IsNullRef() || searcher.SearcherAPI.IsNullRef() || searcher.FactionID != thisFactionID) continue;

//							var nearbyItems = searcher.SearcherAPI.GetNearbyItemsType<INearbyElement>(i => i.FactionID != thisFactionID);
//							foreach (var iamge in nearbyItems)
//							{
//								faction.AddElementSet(iamge as IStrategyElement);
//							}
//						}
//					}
//				}
//			}
//		}
//	}
//}