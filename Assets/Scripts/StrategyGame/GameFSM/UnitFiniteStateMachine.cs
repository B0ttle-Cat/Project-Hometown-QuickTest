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
		protected virtual bool NextStateIsFighting()
		{
			return combatController.TargetInStartAttackRange;
		}
		protected virtual bool NextStateIsChasing()
		{
			if (combatController.TargetInActionRange)
			{
				return true;
			}
			else if(unitObject.HasOperation && unitObject.operationObject.FsmFlag.HasFlag(OperationObject.FSMFlag.Combat))
			{
				return true;
			}
			return false;
		}
	}
	private class IdleState : UnitState
	{
		public IdleState(UnitObject unitObject, UnitFiniteStateMachine fsm, UnitFSMType type) : base(unitObject, fsm, type) {}

		protected override UnitFSMType OnStateUpdate(in float deltaTime)
		{
			if (NextStateIsChasing())
			{
				if (NextStateIsFighting())
				{
					return UnitFSMType.Fighting;
				}
				else return UnitFSMType.Chasing;
			}
			else return UnitFSMType.Idle;
		}
	}
	private class FightingState : UnitState
	{
		public FightingState(UnitObject unitObject, UnitFiniteStateMachine fsm, UnitFSMType type) : base(unitObject, fsm, type) {}
	
		protected override UnitFSMType OnStateUpdate(in float deltaTime)
		{
			if (NextStateIsFighting())
			{
				return UnitFSMType.Fighting;
			}
			else if (NextStateIsChasing())
			{
				return UnitFSMType.Chasing;
			}
			else return UnitFSMType.Idle;
		}
		protected override bool NextStateIsFighting()
		{
			return combatController.TargetInLimitAttackRange;
		}
	}
	private class ChasingState : UnitState
	{
		public ChasingState(UnitObject unitObject, UnitFiniteStateMachine fsm, UnitFSMType type) : base(unitObject, fsm, type) {}
	
		protected override UnitFSMType OnStateUpdate(in float deltaTime)
		{
			if (NextStateIsChasing())
			{
				if (NextStateIsFighting())
				{
					return UnitFSMType.Fighting;
				}
				else return UnitFSMType.Chasing;
			}
			else return UnitFSMType.Idle;
		}
	}
}