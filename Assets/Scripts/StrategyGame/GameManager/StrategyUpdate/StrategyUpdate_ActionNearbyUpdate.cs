namespace StrategyManagerModule
{
	public partial class StrategyUpdate
	{
		public class StrategyUpdate_ActionNearbyUpdate : StrategyUpdateSubClass<StrategyUpdate_ActionNearbyUpdate.NearbyUpdate>
		{
			public StrategyUpdate_ActionNearbyUpdate(StrategyUpdate updater) : base(updater)
			{
			}
			protected override void Dispose()
			{
				StrategyManager.Collector.RemoveChangeListener<ActionRangeSearching>(OnChangeElement);
			}
			protected override void Start()
			{
				StrategyManager.Collector.AddChangeListener<ActionRangeSearching>(OnChangeElement, true);
			}
			private void OnChangeElement(ActionRangeSearching element, bool added)
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
			}
			public class NearbyUpdate : UpdateLogic
			{
				public readonly NearbySearching searching;
				public Faction faction;
				public INearbySearcherAPI SearcherAPI => searching;

				public NearbyUpdate(NearbySearching searching, StrategyUpdate_ActionNearbyUpdate thisSubClass) : base(thisSubClass)
				{
					this.searching = searching;

					faction = FactionAPI.ID2Faction(searching.FactionID);
				}

				protected override void OnDispose()
				{
					faction = null;
				}

				protected override void OnUpdate(in float deltaTime)
				{
					if (faction == null)
					{
						faction = FactionAPI.ID2Faction(searching.FactionID);
					}

					if (faction == null || SearcherAPI.IsNullRef()) return;
					if (!SearcherAPI.IsEnable) return;
					SearcherAPI.OnNearbySearching(faction.DetectedList.NearbyList);
				}
			}
		}
	}
}