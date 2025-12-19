namespace StrategyManagerModule
{
	public partial class StrategyUpdate
	{
		public class StrategyUpdate_VisionNearbyUpdate : StrategyUpdateSubClass<StrategyUpdate_VisionNearbyUpdate.NearbyUpdate>
		{
			public StrategyUpdate_VisionNearbyUpdate(StrategyUpdate updater) : base(updater)
			{
			}
			protected override void Dispose()
			{
				StrategyManager.Collector.RemoveChangeListener<VisionRangeSearching>(OnChangeElement);
			}
			protected override void Start()
			{
				StrategyManager.Collector.AddChangeListener<VisionRangeSearching>(OnChangeElement, true);
			}
			private void OnChangeElement(VisionRangeSearching element, bool added)
			{
				if (element.IsNullRef()) return;

				if (added)
				{
					this.Add(new NearbyUpdate(element, this));
				}
				else
				{
					int findIndex = this.FindIndex(i=>i.searching == element);
					if (findIndex < 0) return;
					this.RemoveAt(findIndex);
				}
			}
			protected override void Update(in float deltaTime)
			{
				int length = this.Count;
				for (int i = 0 ; i < length ; i++)
				{
					var item = this[i];
					if (item == null) continue;
					item.Update(in deltaTime);
				}
				for (int i = 0 ; i < length ; i++)
				{
					var item = this[i];
					if (item == null) continue;
					item.ExitViewUpdate();
				}
				for (int i = 0 ; i < length ; i++)
				{
					var item = this[i];
					if (item == null) continue;
					item.EnterViewUpdate();
				}
			}
			public class NearbyUpdate : UpdateLogic
			{
				public readonly NearbySearching searching;
				public Faction faction;
				private readonly BaseList<INearbyElement> allNearbyElements;
				public INearbySearcherAPI SearcherAPI => searching;

				public NearbyUpdate(NearbySearching searching, StrategyUpdate_VisionNearbyUpdate thisSubClass) : base(thisSubClass)
				{
					this.searching = searching;
					allNearbyElements = StrategyManager.Collector.GetList<INearbyElement>();

					faction = FactionAPI.ID2Faction(searching.FactionID);
				}

				protected override void OnDispose()
				{
					faction = null;
				}

				protected override void OnUpdate(in float deltaTime)
				{
					if(faction == null)
					{
						faction = FactionAPI.ID2Faction(searching.FactionID);
					}

					if (faction == null || SearcherAPI.IsNullRef()) return;
					if (!SearcherAPI.IsEnable) return;
					SearcherAPI.OnNearbySearching(allNearbyElements);
				}
				public void ExitViewUpdate()
				{
					if (faction == null || SearcherAPI.IsNullRef()) return;
					if (!SearcherAPI.IsEnable) return;
					faction.RemoveDetects(SearcherAPI.ExitRageThisFrame());
				}
				public void EnterViewUpdate()
				{
					if (faction == null || SearcherAPI.IsNullRef()) return;
					if (!SearcherAPI.IsEnable) return;
					faction.AddDetects(SearcherAPI.EnterRageThisFrame());
				}
			}
		}
	}
}