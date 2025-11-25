using System.Collections.Generic;

using static StrategyUpdate.StrategyUpdate_NearbyUpdate;
public partial class StrategyUpdate
{
	public class StrategyUpdate_NearbyUpdate : StrategyUpdateSubClass<NearbyUpdate>
	{
		private HashSet<INearbyElement> serchTargets;
		public StrategyUpdate_NearbyUpdate(StrategyUpdate updater) : base(updater)
		{
			serchTargets = new HashSet<INearbyElement>();
		}
		protected override void Dispose()
		{
			StrategyManager.Collector.RemoveChangeAnyListener(OnChangeElement);
			serchTargets.Clear();
			serchTargets = null;
		}
		protected override void Start()
		{
			StrategyManager.Collector.AddChangeAnyListener(OnChangeElement, ForeachAll);
			void ForeachAll(IStrategyElement element)
			{
				OnChangeElement(element, true);
			}
		}
		private void OnChangeElement(IStrategyElement element, bool added)
		{
			if (element == null) return;
			else if (element is INearbySearcherValueGetter valueGetter)
			{
				INearbySearcher searcher = valueGetter.Searcher;
				if (added)
				{
					UpdateList.Add(new NearbyUpdate(searcher, this));
				}
				else
				{
					int findIndex = UpdateList.FindIndex(i=>i.searcher == searcher);
					if (findIndex < 0) return;
					UpdateList.RemoveAt(findIndex);
				}
			}
			else if(element is INearbyElement serchTarget)
			{
				if (added) serchTargets.Add(serchTarget);
				else serchTargets.Remove(serchTarget);
			}
		}
		protected override void Update(in float deltaTime)
		{
			int length = UpdateList == null ? 0 : UpdateList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				var item = UpdateList[i];
				if (item == null) continue;
				item.Update(deltaTime);
			}
		}

		public class NearbyUpdate : UpdateLogic
		{
			public readonly INearbySearcher searcher;
			private readonly HashSet<INearbyElement> allElements;
			public NearbyUpdate(INearbySearcher searcher, StrategyUpdate_NearbyUpdate thisSubClass) : base(thisSubClass)
			{
				this.searcher = searcher;
				allElements = thisSubClass.serchTargets;
			}

			protected override void OnDispose()
			{
			}

			protected override void OnUpdate(in float deltaTime)
			{
				if(searcher == null) return;
				searcher.UpdateNearby(allElements);
			}
		}
	}
}