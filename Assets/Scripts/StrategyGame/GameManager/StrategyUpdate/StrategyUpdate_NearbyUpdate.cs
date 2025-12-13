using System.Collections.Generic;

using static StrategyManagerModule.StrategyUpdate.StrategyUpdate_NearbyUpdate;

namespace StrategyManagerModule
{
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
				StrategyManager.Collector.RemoveChangeListener<INearbySearcherValueGetter>(OnChangeElement);
				StrategyManager.Collector.RemoveChangeListener<INearbyElement>(OnChangeElement);
				serchTargets.Clear();
				serchTargets = null;
			}
			protected override void Start()
			{
				StrategyManager.Collector.AddChangeListener<INearbySearcherValueGetter>(OnChangeElement, true);
				StrategyManager.Collector.AddChangeListener<INearbyElement>(OnChangeElement, true);
			}
			private void OnChangeElement(object element, bool added)
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
				else if (element is INearbyElement serchTarget)
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
					if (searcher == null) return;
					searcher.UpdateNearby(allElements);
				}
			}
		}
	} 
}