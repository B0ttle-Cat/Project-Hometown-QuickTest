using static StrategyManagerModule.StrategyUpdate.StrategyUpdate_OperationNearbyUpdate;

namespace StrategyManagerModule
{
	public partial class StrategyUpdate
	{
		public class StrategyUpdate_OperationNearbyUpdate : StrategyUpdateSubClass<OperationNearbyUpdate>
		{
			public StrategyUpdate_OperationNearbyUpdate(StrategyUpdate updater) : base(updater)
			{
			}
			protected override void Dispose()
			{
				StrategyManager.Collector.RemoveChangeListener<OperationObject>(OnChangeElement);
			}
			protected override void Start()
			{
				StrategyManager.Collector.AddChangeListener<OperationObject>(OnChangeElement, true);
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
					item.ExitViewUpdate();
				}
				for (int i = 0 ; i < length ; i++)
				{
					var item = this[i];
					if (item == null) continue;
					item.EnterViewUpdate();
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
				public readonly Faction faction;
				private readonly BaseList<INearbyElement> allNearbyElements;
				public INearbySearcher ViewSearcher => operation == null ? null : operation.ViewSearcher;
				public INearbySearcher ActionSearcher => operation == null ? null : operation.ActionSearcher;

				public OperationNearbyUpdate(OperationObject operation, StrategyUpdate_OperationNearbyUpdate thisSubClass) : base(thisSubClass)
				{
					this.operation = operation;
					faction = FactionAPI.ID2Object(operation.FactionID);
					allNearbyElements = StrategyManager.Collector.GetList<INearbyElement>();
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
					if (ViewSearcher.IsNullRef()) return;
					ViewSearcher.SearcherAPI.UpdateNearby(allNearbyElements);
				}
				public void ExitViewUpdate()
				{
					if (faction == null || ViewSearcher.IsNullRef()) return;
					faction.RemoveDetects(ViewSearcher.SearcherAPI.ExitRageThisFrame());
				}
				public void EnterViewUpdate()
				{
					if (faction == null || ViewSearcher.IsNullRef()) return;
					faction.AddDetects(ViewSearcher.SearcherAPI.EnterRageThisFrame());
				}
				public void ActionUpdate()
				{
					if (ActionSearcher.IsNullRef()) return;

					if(faction == null)
					{
						if (ViewSearcher.IsNullRef()) return;
						ActionSearcher.SearcherAPI.UpdateNearby(ViewSearcher.SearcherAPI.GetNearbyItems());
					}
					else
					{
						ActionSearcher.SearcherAPI.UpdateNearby(faction.DetectedList.NearbyType);
					}
				}
			}
		}
	} 
}