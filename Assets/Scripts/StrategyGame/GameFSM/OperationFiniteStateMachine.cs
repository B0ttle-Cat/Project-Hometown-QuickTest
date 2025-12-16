using UnityEngine;

public enum OperationFSMType
{
	Idle = 0,
	Combat,
}

[RequireComponent(typeof(OperationObject))]
public class OperationFiniteStateMachine : FiniteStateMachine<OperationFSMType>
{
	private OperationObject operation;
	private INearbySearcherAPI actionSearcherAPI;
	private ITargetableCombatant nearActionTarget;
	public override IState<OperationFSMType>[] GetStateList()
	{
		this.operation = GetComponent<OperationObject>();
		actionSearcherAPI = operation.ActionSearcherAPI;

		return new IState<OperationFSMType>[]
		{
			new IdleState(operation, this, OperationFSMType.Idle),
			new CombatState(operation, this, OperationFSMType.Combat),
		};
	}

	protected override void OnStateUpdate(in float deltaTime)
	{
		if (operation == null || actionSearcherAPI.IsNullRef())
		{
			nearActionTarget = null;
			return;
		}

		nearActionTarget = actionSearcherAPI.NearbyCount() <= 0 
			? null 
			: actionSearcherAPI.GetNearbyItemType<ITargetableCombatant>(
				target => FactionAPI.IsEnemyBetween(operation.FactionID, target.FactionID));

		base.OnStateUpdate(deltaTime);
	}
	private abstract class OperationState : BaseState
	{
		protected readonly OperationObject operation;
		protected readonly OperationFiniteStateMachine operationFsm;

		protected OperationState(OperationObject operation, OperationFiniteStateMachine fsm, OperationFSMType type) : base(fsm, type)
		{
			this.operation = operation;
			operationFsm = fsm;
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
		protected virtual bool SomthingEnterActionRange()
		{
			if (operationFsm == null) return false;
			if (operationFsm.nearActionTarget.IsNullRef()) return false;
			return true;
		}
	}
	private class IdleState : OperationState
	{
		public IdleState(OperationObject operation, OperationFiniteStateMachine fsm, OperationFSMType type) : base(operation, fsm, type)
		{

		}
		protected override OperationFSMType OnStateUpdate(in float deltaTime)
		{
			if (SomthingEnterActionRange())
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
			SendOpNearTarget();
		}
		protected override void OnStateExit()
		{
			SendClearOpTarget();
		}
		protected override OperationFSMType OnStateUpdate(in float deltaTime)
		{
			if (SomthingEnterActionRange())
			{
				return OperationFSMType.Combat;
			}
			return OperationFSMType.Idle;
		}
		protected override void OnAliveUpdate(in float deltaTime)
		{
			SendOpNearTarget();
		}

		private void SendOpNearTarget()
		{
			var unitList = operation.UnitOrganizationList;
			foreach (var unit in unitList)
			{
				if (unit == null) continue;
				if (unit is not ICombatHandler combat) continue;

				combat.IsOperationCombatState = true;
				combat.OperationCurrentTarget = operationFsm.nearActionTarget;
			}
		}
		private void SendClearOpTarget()
		{
			var unitList = operation.UnitOrganizationList;
			foreach (var unit in unitList)
			{
				if (unit == null) continue;
				if (unit is not ICombatHandler combat) continue;

				combat.IsOperationCombatState = false;
				combat.OperationCurrentTarget = null;
			}
		}
	}
}
