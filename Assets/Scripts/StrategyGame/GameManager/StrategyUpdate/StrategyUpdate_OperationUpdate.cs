namespace StrategyManagerModule
{
	public partial class StrategyUpdate
	{
		public class StrategyUpdate_OperationUpdate : StrategyUpdateSubClass<StrategyUpdate_OperationUpdate.OperationUpdate>
		{
			public StrategyUpdate_OperationUpdate(StrategyUpdate updater) : base(updater)
			{
			}

			protected override void Start()
			{
				StrategyManager.Collector.AddChangeListener<OperationObject>(ChangeList);
			}
			protected override void Dispose()
			{
				StrategyManager.Collector.RemoveChangeListener<OperationObject>(ChangeList);
			}
			private void ChangeList(IStrategyElement element, bool isAdd)
			{
				if (element is not OperationObject op) return;

				if (isAdd)
				{
					this.Add(new OperationUpdate(op, this));
				}
				else
				{
					int findIndex = this.FindIndex(l => l.operationObject == op);
					if (findIndex >= 0) return;
					this.RemoveAt(findIndex);
				}
			}

			protected override void Update(in float deltaTime)
			{
				int length = this.Count;
				for (int i = 0 ; i < length ; i++)
				{
					var update = this[i];
					if (update == null) continue;
					update.Update(deltaTime);
				}
			}
			public class OperationUpdate : UpdateLogic
			{
				public OperationObject operationObject;
				public OperationUpdate(OperationObject operationObject, StrategyUpdateSubClass<OperationUpdate> thisSubClass) : base(thisSubClass)
				{
					this.operationObject = operationObject;
				}

				protected override void OnDispose()
				{
					operationObject = null;
				}

				protected override void OnUpdate(in float deltaTime)
				{
					if (operationObject == null) return;
					operationObject.ComputeOperationValue();
				}
			}
		}
	} 
}