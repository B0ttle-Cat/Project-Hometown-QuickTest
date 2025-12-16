using System.Collections.Generic;

using static StrategyManagerModule.StrategyUpdate.StrategyUpdate_OperationNearbyUpdate;

namespace StrategyManagerModule
{
	public partial class StrategyUpdate
	{
		public class StrategyUpdate_OperationNearbyUpdate : StrategyUpdateSubClass<OperationNearbyUpdate>
		{
			private HashSet<INearbyElement> serchTargets;
			public StrategyUpdate_OperationNearbyUpdate(StrategyUpdate updater) : base(updater)
			{
				serchTargets = new HashSet<INearbyElement>();
			}
			protected override void Dispose()
			{
				StrategyManager.Collector.RemoveChangeListener<OperationObject>(OnChangeElement);
				StrategyManager.Collector.RemoveChangeListener<INearbyElement>(OnChangeElement);
				serchTargets.Clear();
				serchTargets = null;
			}
			protected override void Start()
			{
				StrategyManager.Collector.AddChangeListener<OperationObject>(OnChangeElement, true);
				StrategyManager.Collector.AddChangeListener<INearbyElement>(OnChangeElement, true);
			}
			private void OnChangeElement(object element, bool added)
			{
				if (element == null) return;
				else if (element is OperationObject operation)
				{
					if (added)
					{
						this.Add(new OperationNearbyUpdate(operation, this));
					}
					else
					{
						int findIndex = this.FindIndex(i=>i.operation == operation);
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
					item.ViewUpdate();
				}
				for (int i = 0 ; i < length ; i++)
				{
					var item = this[i];
					if (item == null) continue;
					item.ActionUpdate();
				}
			}
			public class OperationNearbyUpdate : UpdateLogic
			{
				public readonly OperationObject operation;
				public INearbySearcher viewSearcher => operation == null ? null : operation.ViewSearcher;
				public INearbySearcher actionSearcher => operation == null ? null : operation.ActionSearcher;

				private readonly HashSet<INearbyElement> allElements;
				public OperationNearbyUpdate(OperationObject operation, StrategyUpdate_OperationNearbyUpdate thisSubClass) : base(thisSubClass)
				{
					this.operation = operation;
					allElements = thisSubClass.serchTargets;
				}

				protected override void OnDispose()
				{
				}

				protected override void OnUpdate(in float deltaTime) 
				{
					// 필요한 매개변수 업데이트 하기
				}

				public void ViewUpdate()
				{
					if (viewSearcher.IsNullRef()) return;
					viewSearcher.SearcherAPI.UpdateNearby(allElements);
				}
				public void ActionUpdate()
				{
					if (actionSearcher.IsNullRef()) return;
					actionSearcher.SearcherAPI.UpdateNearby(viewSearcher.SearcherAPI.GetNearbyItems());
				}
			}
		}
	} 
}