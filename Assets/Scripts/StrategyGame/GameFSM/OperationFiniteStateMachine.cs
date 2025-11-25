using UnityEngine;

public enum OperationFSMType
{
	Idle = 0,
	Combat,
}

[RequireComponent(typeof(OperationObject))]
public class OperationFiniteStateMachine : FiniteStateMachine<OperationFSMType>
{
	private OperationObject combatTarget;

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
		protected virtual bool NextStateIsCombat()
		{
			if (nearbySearcher == null) return false;
			var nearUnits = nearbySearcher.GetNearbyItemsType<UnitObject>();
			if (nearUnits == null) return false;

			int operationFactionID = operation.FactionID;
			foreach (var unit in nearUnits)
			{
				if (unit == null) continue;
				if (unit.FactionID != operationFactionID)
				{
					operationFsm.combatTarget = unit.operationObject;
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
			if (NextStateIsCombat())
			{
				return OperationFSMType.Combat;
			}
			return OperationFSMType.Idle;
		}
	}
	private class CombatState : OperationState
	{
		public CombatState(OperationObject operation, OperationFiniteStateMachine fsm, OperationFSMType type) : base(operation, fsm, type)
		{
		}
		protected override void OnStateEnter()
		{
			var unitList = operation.GetAllUnitObj;
			Vector3 position = operationFsm.combatTarget.ThisMovement.CurrentPosition;
			int length = unitList == null ? 0 : unitList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				var unit = unitList[i];
				if (unit == null) continue;
				if (unit is not IUnitCombatController combat) continue;

				combat.IsCombatState = true;
				combat.CombatMoveTarget = position;
			}
		}
		protected override void OnStateExit()
		{
			var unitList = operation.GetAllUnitObj;
			int length = unitList == null ? 0 : unitList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				var unit = unitList[i];
				if (unit == null) continue;
				if (unit is not IUnitCombatController combat) continue;

				combat.IsCombatState = false;
			}
		}
		protected override OperationFSMType OnStateUpdate(in float deltaTime)
		{
			if (NextStateIsCombat())
			{
				return OperationFSMType.Combat;
			}
			return OperationFSMType.Idle;
		}
		protected override void OnAliveUpdate(in float deltaTime)
		{
			if(operationFsm.combatTarget == null) return;

			var unitList = operation.GetAllUnitObj;
			Vector3 position = operationFsm.combatTarget.ThisMovement.CurrentPosition;
			int length = unitList == null ? 0 : unitList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				var unit = unitList[i];
				if (unit == null) continue;
				if (unit is not IUnitCombatController combat) continue;

				combat.IsCombatState = true;
				combat.CombatMoveTarget = position;
			}
		}
	}
}
