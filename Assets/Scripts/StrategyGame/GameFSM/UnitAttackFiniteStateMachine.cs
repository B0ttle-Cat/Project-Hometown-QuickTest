using System;

using UnityEngine;

using static StrategyGamePlayData;

public interface IUnitAttackState : IFSMInterface<UnitAttackFSMType>
{
	IUnitAttackState ThisAttackState { get; }
	public event Action OnAttackTiming;
	public event Action OnReloadingTiming;
	public void SetChangeNewTargetFlag(bool changeFlag);
}

public enum UnitAttackFSMType
{
	Idle = 0,       // 모든 대기 상태 중에서 => 공격목표가 삭제될 경우 => Idle => Aiming
	Aiming,         // 모든 대기 상태 중에서 => 공격목표가 변경될 경우 => Aiming => Attacking
	Attacking,      // Attacking => Reattacking or Reloading 
	Reloading,      // Reloading => Reattacking	ot Aiming
	Reattacking,    // Reattacking => Attacking
}

[RequireComponent(typeof(UnitObject))]
public partial class UnitAttackFiniteStateMachine : FiniteStateMachine<UnitAttackFSMType>
{
	IUnitCombatController CombatController { get; set; }
	IFSMController<UnitMainFSMType> MainFSMController { get; set; }
	IStateValueControl StateControl { get; set; }

	public override IState<UnitAttackFSMType>[] GetStateList()
	{
		UnitObject unitObject = GetComponent<UnitObject>();
		CombatController = unitObject.ThisCombatController;
		MainFSMController = unitObject.FSMController;
		StateControl = unitObject;

		var states = new IState<UnitAttackFSMType>[]
		{
			new IdleStatte(this, UnitAttackFSMType.Idle),
			new AimingState(this, UnitAttackFSMType.Aiming),
			new AttackingState(this, UnitAttackFSMType.Attacking, OnAttack),
			new Reloading(this, UnitAttackFSMType.Reloading, OnReloading),
			new ReattackingState(this, UnitAttackFSMType.Reattacking),
		};
		return states;
	}

	private void OnAttack()
	{
		OnAttackTiming?.Invoke();
	}
	private void OnReloading()
	{
		OnReloadingTiming?.Invoke();
	}
	public abstract class AttackState : BaseState
	{
		protected readonly UnitAttackFiniteStateMachine attackState;
		protected readonly IUnitCombatController CombatController;
		protected readonly IFSMController<UnitMainFSMType> MainFSMController;
		protected readonly IStateValueControl StateControl;
		public AttackState(UnitAttackFiniteStateMachine attackState, UnitAttackFSMType type) : base(attackState, type)
		{
			this.attackState = attackState;
			CombatController = attackState.CombatController;
			MainFSMController = attackState.MainFSMController;
			StateControl = attackState.StateControl;
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
		protected override UnitAttackFSMType OnStateUpdate(in float deltaTime)
		{
			return ThisType;
		}
	}
	public class IdleStatte : AttackState
	{
		public IdleStatte(UnitAttackFiniteStateMachine attackState, UnitAttackFSMType type) : base(attackState, type)
		{
		}
		protected override void OnStateEnter()
		{
			attackState.didAiming = false;
		}
		protected override void OnStateExit()
		{
		}
		protected override UnitAttackFSMType OnStateUpdate(in float deltaTime)
		{
			if (MainFSMController.CurrentStateType == UnitMainFSMType.Fighting && attackState.changeNewTargetFlag)
			{
				float ammoRemaining = StateControl.GetStateValue(StatsType.유닛_사용탄수);
				return ammoRemaining > 0 ? UnitAttackFSMType.Aiming : UnitAttackFSMType.Reloading;
			}
			return ThisType;
		}
	}
	public class AimingState : AttackState
	{
		float aimingTime;
		public AimingState(UnitAttackFiniteStateMachine attackState, UnitAttackFSMType type) : base(attackState, type)
		{
		}
		protected override void OnStateEnter()
		{
			attackState.didAiming = true;
			attackState.changeNewTargetFlag = false;
			aimingTime = StateControl.GetStateValuePercent(StatsType.유닛_조준지연시간_c);
		}
		protected override void OnStateExit()
		{
		}
		protected override UnitAttackFSMType OnStateUpdate(in float deltaTime)
		{
			if (MainFSMController.CurrentStateType != UnitMainFSMType.Fighting)
			{
				return UnitAttackFSMType.Idle;
			}
			aimingTime -= deltaTime;
			if (aimingTime <= 0f)
			{
				return UnitAttackFSMType.Attacking;
			}
			;
			return ThisType;
		}
	}
	public class AttackingState : AttackState
	{
		private readonly Action onAttack;
		int comboRemainingCount;
		float comboDelayTime;

		int ammoMaxCount;
		int ammoUsedCount;
		public AttackingState(UnitAttackFiniteStateMachine attackState, UnitAttackFSMType type, Action onAttack) : base(attackState, type)
		{
			this.onAttack = onAttack;
		}
		protected override void OnStateEnter()
		{
			comboRemainingCount = StateControl.GetStateValue(StatsType.유닛_연속공격횟수);
			comboDelayTime = StateControl.GetStateValue(StatsType.유닛_연속공격지연시간_c);

			ammoMaxCount = StateControl.GetStateValue(StatsType.유닛_탄용량);
			ammoUsedCount = StateControl.GetStateValue(StatsType.유닛_사용탄수);
		}
		protected override void OnStateExit()
		{
		}
		protected override UnitAttackFSMType OnStateUpdate(in float deltaTime)
		{
			if (MainFSMController.CurrentStateType != UnitMainFSMType.Fighting)
			{
				return UnitAttackFSMType.Idle;
			}

			if (ammoUsedCount < ammoMaxCount && comboRemainingCount > 0)
			{
				comboDelayTime -= deltaTime;
				if (comboDelayTime <= 0)
				{
					OnAttack();
				}
			}

			if (ammoUsedCount <= 0)
			{
				return UnitAttackFSMType.Reloading;
			}
			else if (comboRemainingCount <= 0)
			{
				return UnitAttackFSMType.Reattacking;
			}
			return ThisType;
		}
		private void OnAttack()
		{
			try
			{
				onAttack?.Invoke();
				--comboRemainingCount;
				++ammoUsedCount;
				comboDelayTime += StateControl.GetStateValue(StatsType.유닛_연속공격지연시간_c);
				ammoMaxCount = StateControl.GetStateValue(StatsType.유닛_탄용량);
			}
			catch
			{
				comboRemainingCount = 0;
				comboDelayTime = 0;
				ammoMaxCount = 0;
				ammoUsedCount = 0;
			}
		}
	}
	public class ReattackingState : AttackState
	{
		private float reattackingTime;
		public ReattackingState(UnitAttackFiniteStateMachine attackState, UnitAttackFSMType type) : base(attackState, type)
		{
		}
		protected override void OnStateEnter()
		{
			reattackingTime = StateControl.GetStateValuePercent(StatsType.유닛_재공격지연시간_c);
		}
		protected override void OnStateExit()
		{
		}
		protected override UnitAttackFSMType OnStateUpdate(in float deltaTime)
		{
			if (MainFSMController.CurrentStateType != UnitMainFSMType.Fighting)
			{
				return UnitAttackFSMType.Idle;
			}
			if (attackState.changeNewTargetFlag)
			{
				return UnitAttackFSMType.Aiming;
			}
			reattackingTime -= deltaTime;
			if (reattackingTime <= 0f)
			{
				return UnitAttackFSMType.Attacking;
			}
			return ThisType;
		}
	}
	public class Reloading : AttackState
	{
		private readonly Action onReloading;
		private float reloadingTime;
		public Reloading(UnitAttackFiniteStateMachine attackState, UnitAttackFSMType type, Action onReloading) : base(attackState, type)
		{
			this.onReloading = onReloading;
		}
		protected override void OnStateEnter()
		{
			reloadingTime = StateControl.GetStateValuePercent(StatsType.유닛_재장전시간_c);
		}
		protected override void OnStateExit()
		{
		}
		protected override UnitAttackFSMType OnStateUpdate(in float deltaTime)
		{
			reloadingTime -= deltaTime;
			if (reloadingTime <= 0)
			{
				OnReloading();
				if(attackState.didAiming)
				{
					return UnitAttackFSMType.Reattacking;
				}
				else
				{
					return UnitAttackFSMType.Aiming;
				}
			}
			return ThisType;
		}
		private void OnReloading()
		{
			onReloading?.Invoke();
		}
	}
}
public partial class UnitAttackFiniteStateMachine : IUnitAttackState
{
	public IUnitAttackState ThisAttackState => this;
	public event Action OnAttackTiming;
	public event Action OnReloadingTiming;

	private bool changeNewTargetFlag;
	private bool didAiming;
	public void SetChangeNewTargetFlag(bool changeFlag)
	{
		changeNewTargetFlag = changeFlag;
	}
	public override bool IsCanStateUpdate()
    {
		if (MainFSMController == null) return false;
		return MainFSMController.CurrentStateType == UnitMainFSMType.Fighting;
	}
}


