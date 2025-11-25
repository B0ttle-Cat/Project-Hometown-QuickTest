using System.Collections.Generic;

using static StrategyUpdate.StrategyUpdate_FSMUpdater;
public partial class StrategyUpdate
{
	public class StrategyUpdate_FSMUpdater : StrategyUpdateSubClass<FSMUpdater>
	{
		List<FSMUpdater> operationUpdate;
		List<FSMUpdater> unitUpdate;

		public StrategyUpdate_FSMUpdater(StrategyUpdate updater) : base(updater)
		{
			operationUpdate = new List<FSMUpdater>();
			unitUpdate = new List<FSMUpdater>();
		}
		protected override void Dispose()
		{
			StrategyManager.Collector.RemoveOtherChangeListener<IFSMUpdater>(OnChangeItem);
			operationUpdate = null;
			unitUpdate = null;
		}
		protected override void Start()
		{
			StrategyManager.Collector.AddOtherChangeListener<IFSMUpdater>(OnChangeItem, ForeachAll);
			void ForeachAll(IFSMUpdater item)
			{
				OnChangeItem(item, true);
			}
		}
		private void OnChangeItem(IFSMUpdater item, bool added)
		{
			if (item == null) return;

			if (item is OperationFiniteStateMachine operationFsm)
			{
				if (added)
				{
					operationUpdate.Add(new FSMUpdater(operationFsm, this));
				}
				else
				{
					int findIndex = operationUpdate.FindIndex(f=>f.fsm == item);
					if (findIndex < 0) return;
					operationUpdate.RemoveAt(findIndex);
				}
			}
			else if (item is UnitFiniteStateMachine unitFsm)
			{
				if (added)
				{
					unitUpdate.Add(new FSMUpdater(unitFsm, this));
				}
				else
				{
					int findIndex = unitUpdate.FindIndex(f=>f.fsm == item);
					if (findIndex < 0) return;
					unitUpdate.RemoveAt(findIndex);
				}
			}
			else 
			{
				if (added)
				{
					UpdateList.Add(new FSMUpdater(item, this));
				}
				else
				{
					int findIndex = UpdateList.FindIndex(f=>f.fsm == item);
					if (findIndex < 0) return;
					UpdateList.RemoveAt(findIndex);
				}
			}
		}
		protected override void Update(in float deltaTime)
		{
			int length = UpdateList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				var item = UpdateList[i];
				if (item == null) continue;
				item.Update(deltaTime);
			}
			length = operationUpdate.Count;
			for (int i = 0 ; i < length ; i++)
			{
				var item = operationUpdate[i];
				if (item == null) continue;
				item.Update(deltaTime);
			}
			length = unitUpdate.Count;
			for (int i = 0 ; i < length ; i++)
			{
				var item = unitUpdate[i];
				if (item == null) continue;
				item.Update(deltaTime);
			}
		}
		public class FSMUpdater : UpdateLogic
		{
			public IFSMUpdater fsm;
			public FSMUpdater(IFSMUpdater fsm, StrategyUpdate_FSMUpdater thisSubClass) : base(thisSubClass)
			{
				this.fsm = fsm;
			}

			protected override void OnDispose()
			{
			}

			protected override void OnUpdate(in float deltaTime)
			{
				if (fsm == null) return;
				fsm.StateUpdate(in deltaTime);
			}
		}
	}

}