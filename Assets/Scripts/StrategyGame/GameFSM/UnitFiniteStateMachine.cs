using UnityEngine;

public enum UnitFSMType
{
	Idle = 0,
	Fighting,
	Chasing,
}

[RequireComponent(typeof(UnitObject))]
public class UnitFiniteStateMachine : FiniteStateMachine<UnitFSMType>
{
	public override IState<UnitFSMType>[] GetStateList()
	{
		UnitObject unitObject = GetComponent<UnitObject>();
		return new IState<UnitFSMType>[]
		{
			new IdleState(unitObject, this, UnitFSMType.Idle),
			new FightingState(unitObject, this, UnitFSMType.Fighting),
			new ChasingState(unitObject, this, UnitFSMType.Chasing),
		};
	}
	private abstract class UnitState : BaseState
	{
		protected readonly UnitObject unitObject;
		protected readonly IUnitCombatController combatController;
		protected UnitState(UnitObject unitObject, UnitFiniteStateMachine fsm, UnitFSMType type) : base(fsm, type)
		{
			this.unitObject = unitObject;
			combatController = unitObject;
		}
		#region	UnitState
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
		protected bool IsCombat()
		{
			var op = unitObject.operationObject;
			if (op == null)
			{
				return false;
			}
			else
			{
				return op.FsmFlag.HasFlag(OperationObject.FSMFlag.Combat);
			}
		}
		protected virtual bool NextStateIsFighting()
		{
			return combatController.TargetInAttackRange;
		}
		protected virtual bool NextStateIsChasing()
		{
			return combatController.TargetInActionRange;
		}
	}
	private class IdleState : UnitState
	{
		public IdleState(UnitObject unitObject, UnitFiniteStateMachine fsm, UnitFSMType type) : base(unitObject, fsm, type) {}
	
		protected override UnitFSMType OnStateUpdate(in float deltaTime)
		{
			if (IsCombat() && NextStateIsChasing())
			{
				return UnitFSMType.Chasing;
			}
			return UnitFSMType.Idle;
		}
	}
	private class FightingState : UnitState
	{
		public FightingState(UnitObject unitObject, UnitFiniteStateMachine fsm, UnitFSMType type) : base(unitObject, fsm, type){}
	
		protected override UnitFSMType OnStateUpdate(in float deltaTime)
		{
			if (NextStateIsFighting())
			{
				return UnitFSMType.Fighting;
			}
			return UnitFSMType.Chasing;
		}
	}
	private class ChasingState : UnitState
	{
		public ChasingState(UnitObject unitObject, UnitFiniteStateMachine fsm, UnitFSMType type) : base(unitObject, fsm, type){}
	
		protected override UnitFSMType OnStateUpdate(in float deltaTime)
		{
			if (!IsCombat()) return UnitFSMType.Idle;

			if (NextStateIsFighting())
			{
				return UnitFSMType.Fighting;
			}
			if (NextStateIsChasing())
			{
				return UnitFSMType.Chasing;
			}
			return UnitFSMType.Idle;
		}
	}
}