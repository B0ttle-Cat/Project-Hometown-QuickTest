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

	public abstract class OperationState : BaseState
	{
		protected OperationObject operation;
		protected OperationState(OperationObject operation, FiniteStateMachine<OperationFSMType> fsm, OperationFSMType type) : base(fsm, type)
		{
			this.operation = operation;
		}
		~OperationState()
		{
			operation = null;
		}
		protected bool NextStateIsCombat()
		{
			return false;
		}
	}
	public class IdleState : OperationState
	{
		public IdleState(OperationObject operation, OperationFiniteStateMachine fsm, OperationFSMType type) : base(operation, fsm, type)
		{
		}
		protected override void OnDispose()
		{
			operation = null;
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
		protected override OperationFSMType OnStateUpdate(in float deltaTime)
		{
			if (NextStateIsCombat())
			{
				return OperationFSMType.Combat;
			}
			return OperationFSMType.Idle;
		}
	}
	public class CombatState : OperationState
	{
		public CombatState(OperationObject operation, FiniteStateMachine<OperationFSMType> fsm, OperationFSMType type) : base(operation, fsm, type)
		{
		}
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
		protected override OperationFSMType OnStateUpdate(in float deltaTime)
		{
			if (NextStateIsCombat())
			{
				return ThisType;
			}
			return OperationFSMType.Idle;
		}
	}
}
