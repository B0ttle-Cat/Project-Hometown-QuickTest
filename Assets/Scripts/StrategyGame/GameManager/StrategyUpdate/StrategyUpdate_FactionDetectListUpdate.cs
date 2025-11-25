using static StrategyUpdate.StrategyUpdate_FactionDetectListUpdate;
public partial class StrategyUpdate
{
	public class StrategyUpdate_FactionDetectListUpdate : StrategyUpdateSubClass<DetectListUpdate>
	{
		public StrategyUpdate_FactionDetectListUpdate(StrategyUpdate updater) : base(updater)
		{
		}
		protected override void Dispose()
		{
			StrategyManager.Collector.RemoveChangeListener<Faction>(OnChangeValue);
		}
		protected override void Start()
		{
			StrategyManager.Collector.AddChangeListener<Faction>(OnChangeValue, ForeachAll);
			void ForeachAll(IStrategyElement element)
			{
				if (element == null) return;
				OnChangeValue(element, true);
			}
		}

		private void OnChangeValue(IStrategyElement element, bool added)
		{
			if (element == null) return;
			if (element is Faction faction)
			{
				if (added)
				{
					UpdateList.Add(new DetectListUpdate(faction, this));
				}
				else
				{
					int findIndex = UpdateList.FindIndex(i=>i.thisFactionID == faction.FactionID);
					if (findIndex < 0) return;
					UpdateList.RemoveAt(findIndex);
				}
			}
		}
		protected override void Update(in float deltaTime)
		{
			int length = UpdateList == null ? 0 : UpdateList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				var item = UpdateList[i];
				if (item == null) continue;
				item.Update(in deltaTime);
			}
		}
		public class DetectListUpdate : UpdateLogic
		{
			public readonly Faction faction;
			public readonly int thisFactionID;

			public DetectListUpdate(Faction faction, StrategyUpdate_FactionDetectListUpdate thisSubClass) : base(thisSubClass)
			{
				this.faction = faction;
				thisFactionID = faction.FactionID;
			}

			protected override void OnDispose()
			{
			}

			protected override void OnUpdate(in float deltaTime)
			{
				faction.ClearDetect();

				StrategyManager.Collector.ForEachAll(item =>
				{
					if (item is not INearbySearcherValueGetter searcherValueGetter) return;
					if (searcherValueGetter.FactionID == thisFactionID) return;
					var searcher = searcherValueGetter.Searcher;
					if (searcher == null) return;

					var nearbyItems = searcher.GetNearbyItemsType<INearbyElement>(i => i.FactionID != thisFactionID);
					foreach (var target in nearbyItems)
					{
						faction.AddDetect(target as IStrategyElement);
					}
				});
			}
		}
	}
}