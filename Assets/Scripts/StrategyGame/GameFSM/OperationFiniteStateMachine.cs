using UnityEngine;

public enum OperationFSMType
{
	Idle = 0,
	Combat,
}

[RequireComponent(typeof(OperationObject))]
public class OperationFiniteStateMachine : FiniteStateMachine<OperationFSMType>
{
	public override IState<OperationFSMType>[] GetStateList()
	{
		OperationObject operation = GetComponent<OperationObject>();

		return new IState<OperationFSMType>[]
		{
			new IdleState(operation, this, OperationFSMType.Idle),
			new CombatState(operation, this, OperationFSMType.Combat),
		};
	}
	private abstract class OperationState : BaseState
	{
		protected readonly OperationObject operation;
		protected readonly INearbySearcher nearbySearcher;
		protected readonly OperationFiniteStateMachine operationFsm;

		protected OperationState(OperationObject operation, OperationFiniteStateMachine fsm, OperationFSMType type) : base(fsm, type)
		{
			this.operation = operation;
			operationFsm = fsm;
			if (operation is INearbySearcherValueGetter searcherValueGetter)
			{
				nearbySearcher = searcherValueGetter.Searcher;
			}
		}
		#region
		protected override void OnDispose()
		{

		}
		protected override void OnStateAwake()
		{

		}
		protected override void OnStateEnter()
		{

		}
		protected override void OnStateExit()
		{

		}
		protected override void OnStateStart()
		{

		}
		#endregion
		protected virtual bool SomeUnitStateIsCombat()
		{
			var unitList = operation.GetAllUnitObj;
			int length = unitList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				var unit = unitList[i];
				if (unit == null || unit is not IUnitCombatController combat) continue;

				if (combat.IsCombatState)
				{
					return true;
				}
			}
			return false;
		}
	}
	private class IdleState : OperationState
	{
		public IdleState(OperationObject operation, OperationFiniteStateMachine fsm, OperationFSMType type) : base(operation, fsm, type)
		{

		}
		protected override OperationFSMType OnStateUpdate(in float deltaTime)
		{
			if (SomeUnitStateIsCombat())
			{
				return OperationFSMType.Combat;
			}
			return OperationFSMType.Idle;
		}

		protected override bool SomeUnitStateIsCombat()
		{
			bool toCombat = base.SomeUnitStateIsCombat();
			if (toCombat)
			{
				OnChangeCombat();
			}
			return toCombat;
		}

		private void OnChangeCombat()
		{
			var unitList = operation.GetAllUnitObj;
			int length = unitList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				var unit = unitList[i];
				if (unit == null || unit is not IUnitCombatController combat) continue;

				if (!combat.IsRootCombatState)
				{
					combat.IsRootCombatState = true;
				}
			}
		}
	}
	private class CombatState : OperationState
	{
		public CombatState(OperationObject operation, OperationFiniteStateMachine fsm, OperationFSMType type) : base(operation, fsm, type)
		{
		}
		protected override void OnStateEnter()
		{
			SetRootTarget();
		}
		protected override void OnStateExit()
		{
			ClearRootTarget();
		}
		protected override OperationFSMType OnStateUpdate(in float deltaTime)
		{
			if (SomeUnitStateIsCombat())
			{
				return OperationFSMType.Combat;
			}
			return OperationFSMType.Idle;
		}
		protected override void OnAliveUpdate(in float deltaTime)
		{
			SetRootTarget();
		}

		private void SetRootTarget()
		{
			ITargetableCombatant target = FindNearTarget();
			if (target == null) return;

			var unitList = operation.GetAllUnitObj;
			int length = unitList == null ? 0 : unitList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				var unit = unitList[i];
				if (unit == null) continue;
				if (unit is not IUnitCombatController combat) continue;

				combat.IsRootCombatState = true;
				combat.RootCurrentTarget = target;
			}
			ITargetableCombatant FindNearTarget()
			{
				if (nearbySearcher == null) return null;
				var nearUnits = nearbySearcher.GetNearbyItemsType<UnitObject>();
				if (nearUnits == null) return null;

				int operationFactionID = operation.FactionID;
				foreach (var unit in nearUnits)
				{
					if (unit == null) continue;
					if (unit is not ITargetableCombatant target) continue;
					if (target.FactionID != operationFactionID)
					{
						return target;
					}
				}
				return null;
			}
		}
		private void ClearRootTarget()
		{
			var unitList = operation.GetAllUnitObj;
			int length = unitList == null ? 0 : unitList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				var unit = unitList[i];
				if (unit == null) continue;
				if (unit is not IUnitCombatController combat) continue;

				combat.IsRootCombatState = false;
				combat.RootCurrentTarget = null;
			}
		}
	}
}
