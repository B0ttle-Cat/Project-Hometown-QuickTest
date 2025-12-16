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
				StrategyManager.Collector.RemoveChangeListener<INearbySearcherAPI>(OnChangeElement);
				StrategyManager.Collector.RemoveChangeListener<INearbyElement>(OnChangeElement);
				serchTargets.Clear();
				serchTargets = null;
			}
			protected override void Start()
			{
				StrategyManager.Collector.AddChangeListener<INearbySearcherAPI>(OnChangeElement, true);
				StrategyManager.Collector.AddChangeListener<INearbyElement>(OnChangeElement, true);
			}
			private void OnChangeElement(object element, bool added)
			{
				if (element == null) return;
				else if (element is INearbySearcherAPI searcher)
				{
					if (added)
					{
						this.Add(new NearbyUpdate(searcher, this));
					}
					else
					{
						int findIndex = this.FindIndex(i=>i.searcher == searcher);
						if (findIndex < 0) return;
						this.RemoveAt(findIndex);
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
				int length = this == null ? 0 : this.Count;
				for (int i = 0 ; i < length ; i++)
				{
					var item = this[i];
					if (item == null) continue;
					item.Update(deltaTime);
				}
			}

			public class NearbyUpdate : UpdateLogic
			{
				public readonly INearbySearcherAPI searcher;
				private readonly HashSet<INearbyElement> allElements;
				public NearbyUpdate(INearbySearcherAPI searcher, StrategyUpdate_NearbyUpdate thisSubClass) : base(thisSubClass)
				{
					this.searcher = searcher;
					allElements = thisSubClass.serchTargets;
				}

				protected override void OnDispose()
				{
				}

				protected override void OnUpdate(in float deltaTime)
				{
					if (searcher.IsNullRef()) return;

					searcher.UpdateNearby(allElements);
				}
			}
		}
	} 
}