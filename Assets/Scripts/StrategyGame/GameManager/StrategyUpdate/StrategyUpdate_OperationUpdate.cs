public partial class StrategyUpdate
{
    public class StrategyUpdate_OperationUpdate : StrategyUpdateSubClass<StrategyUpdate_OperationUpdate.OperationUpdate>
	{
		public StrategyUpdate_OperationUpdate(StrategyUpdate updater) : base(updater)
		{
		}

		protected override void Start()
		{
			UpdateList = new();
			var iList = StrategyManager.Collector.OperationList;
			foreach (var item in iList)
			{
				if (item == null) continue;
				UpdateList.Add(new(item, this));
			}
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
				UpdateList.Add(new OperationUpdate(op, this));
			}
			else
			{
				int findIndex = UpdateList.FindIndex(l => l.operationObject == op);
				if (findIndex >= 0) return;
				UpdateList.RemoveAt(findIndex);
			}
		}

		protected override void Update(in float deltaTime)
		{
			int length = UpdateList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				var update = updateList[i];
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