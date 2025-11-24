using static StrategyUpdate.StrategyUpdate_FSMUpdater;
public partial class StrategyUpdate
{
	public class StrategyUpdate_FSMUpdater : StrategyUpdateSubClass<FSMUpdater>
	{
		public StrategyUpdate_FSMUpdater(StrategyUpdate updater) : base(updater)
		{
		}
		protected override void Dispose()
		{
			StrategyManager.Collector.RemoveOtherChangeListener<IFSMUpdater>(OnChangeItem);
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
		protected override void Update(in float deltaTime)
		{
			int length = UpdateList.Count;
            for (int i = 0 ; i < length ; i++)
            {
				var item = UpdateList[i];
				if(item == null) continue;
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
				fsm.StateUpdate(in  deltaTime);
			}
		}
	}

}