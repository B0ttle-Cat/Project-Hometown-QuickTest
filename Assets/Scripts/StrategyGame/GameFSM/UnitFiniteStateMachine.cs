using UnityEngine;

public enum UnitMainFSMType
{
	Idle = 0,
	Fighting,
	Chasing,
}

[RequireComponent(typeof(UnitObject))]
public class UnitFiniteStateMachine : FiniteStateMachine<UnitMainFSMType>
{
	public override IState<UnitMainFSMType>[] GetStateList()
	{
		UnitObject unitObject = GetComponent<UnitObject>();
		return new IState<UnitMainFSMType>[]
		{
			new IdleState(unitObject, this, UnitMainFSMType.Idle),
			new FightingState(unitObject, this, UnitMainFSMType.Fighting),
			new ChasingState(unitObject, this, UnitMainFSMType.Chasing),
		};
	}
	private abstract class UnitState : BaseState
	{
		protected readonly UnitObject unitObject;
		protected readonly ICombatHandler combatController;
		protected UnitState(UnitObject unitObject, UnitFiniteStateMachine fsm, UnitMainFSMType type) : base(fsm, type)
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
		protected virtual bool SomthingInAttackRange()
		{
			return combatController.SomthingInAttackRange();
		}
		protected virtual bool SomthingInActionRange()
		{
			if (combatController.SomthingInActionRange())
			{
				return true;
			}
			else if(unitObject.HasOperation && unitObject.Operation.FsmFlag.HasFlag(OperationObject.FSMFlag.Combat))
			{
				return true;
			}
			return false;
		}
	}
	private class IdleState : UnitState
	{
		public IdleState(UnitObject unitObject, UnitFiniteStateMachine fsm, UnitMainFSMType type) : base(unitObject, fsm, type) {}

		protected override UnitMainFSMType OnStateUpdate(in float deltaTime)
		{
			if (SomthingInActionRange())
			{
				if (SomthingInAttackRange())
				{
					return UnitMainFSMType.Fighting;
				}
				else return UnitMainFSMType.Chasing;
			}
			else return UnitMainFSMType.Idle;
		}
	}
	private class FightingState : UnitState
	{
		public FightingState(UnitObject unitObject, UnitFiniteStateMachine fsm, UnitMainFSMType type) : base(unitObject, fsm, type) {}
	
		protected override UnitMainFSMType OnStateUpdate(in float deltaTime)
		{
			if (SomthingInActionRange())
			{
				if (SomthingInAttackRange())
				{
					return UnitMainFSMType.Fighting;
				}
				else return UnitMainFSMType.Chasing;
			}
			else return UnitMainFSMType.Idle;
		}
		protected override bool SomthingInAttackRange()
		{
			return combatController.SomthingInAttackRange();
		}
	}
	private class ChasingState : UnitState
	{
		public ChasingState(UnitObject unitObject, UnitFiniteStateMachine fsm, UnitMainFSMType type) : base(unitObject, fsm, type) {}
	
		protected override UnitMainFSMType OnStateUpdate(in float deltaTime)
		{
			if (SomthingInActionRange())
			{
				if (SomthingInAttackRange())
				{
					return UnitMainFSMType.Fighting;
				}
				else return UnitMainFSMType.Chasing;
			}
			else return UnitMainFSMType.Idle;
		}
	}
}