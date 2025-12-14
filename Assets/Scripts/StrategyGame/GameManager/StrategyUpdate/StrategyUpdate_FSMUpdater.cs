using System.Collections.Generic;

using static StrategyManagerModule.StrategyUpdate.StrategyUpdate_FSMUpdater;

namespace StrategyManagerModule
{
	public partial class StrategyUpdate
	{
		public class StrategyUpdate_FSMUpdater : StrategyUpdateSubClass<FSMUpdater>
		{
			List<FSMUpdater> operationMainFsmList;
			List<FSMUpdater> unitMainFsmList;
			List<FSMUpdater> unitAttackFsnList;

			public StrategyUpdate_FSMUpdater(StrategyUpdate updater) : base(updater)
			{
				operationMainFsmList = new List<FSMUpdater>();
				unitMainFsmList = new List<FSMUpdater>();

				unitAttackFsnList = new List<FSMUpdater>();
			}
			protected override void Dispose()
			{
				StrategyManager.Collector.RemoveChangeListener<IFSMUpdater>(OnChangeItem);
				operationMainFsmList = null;
				unitMainFsmList = null;
				unitAttackFsnList = null;
			}
			protected override void Start()
			{
				StrategyManager.Collector.AddChangeListener<IFSMUpdater>(OnChangeItem, true);
			}
			private void OnChangeItem(IFSMUpdater item, bool added)
			{
				if (item == null) return;

				if (OnChangeItem_FSMClass<UnitFiniteStateMachine>(unitMainFsmList)) return;
				else if (OnChangeItem_FSMClass<OperationFiniteStateMachine>(operationMainFsmList)) return;
				else if (OnChangeItem_FSMClass<UnitAttackFiniteStateMachine>(unitAttackFsnList)) return;
				else OnChangeItem_OtherMainFSM();

				bool OnChangeItem_FSMClass<T>(List<FSMUpdater> list) where T : class, IFSMUpdater
				{
					if (item is not T fsmClass) return false;
					if (added)
					{
						list.Add(new FSMUpdater(fsmClass, this));
					}
					else
					{
						int findIndex = list.FindIndex(f=>f.fsm == item);
						if (findIndex >= 0) list.RemoveAt(findIndex);
					}
					return true;
				}
				void OnChangeItem_OtherMainFSM()
				{
					if (added)
					{
						this.Add(new FSMUpdater(item, this));
					}
					else
					{
						int findIndex = this.FindIndex(f=>f.fsm == item);
						if (findIndex < 0) return;
						this.RemoveAt(findIndex);
					}
				}
			}
			protected override void Update(in float deltaTime)
			{
				UpdateFSMList(unitMainFsmList, in deltaTime);
				UpdateFSMList(operationMainFsmList, in deltaTime);

				UpdateFSMList(unitAttackFsnList, in deltaTime);

				UpdateFSMList(this, in deltaTime);

				static void UpdateFSMList(IList<FSMUpdater> list, in float deltaTime)
				{
					int length = list.Count;
					for (int i = 0 ; i < length ; i++)
					{
						var item = list[i];
						if (item == null) continue;
						item.Update(in deltaTime);
					}
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
					if (fsm == null || !fsm.IsCanStateUpdate()) return;
					fsm.StateUpdate(in deltaTime);
				}
			}
		}

	} 
}