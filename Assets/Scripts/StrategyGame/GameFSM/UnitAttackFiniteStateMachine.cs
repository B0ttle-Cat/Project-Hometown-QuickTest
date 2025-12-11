using System;

using UnityEngine;

using static StrategyGamePlayData;

public interface IUnitAttackState : IFSMInterface<UnitAttackFSMType>
{
	IUnitAttackState ThisAttackState { get; }
	public event Action<int,int, float> OnAttackReady;
	public event Action<int> OnAttackTiming;
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
			new AttackingState(this, UnitAttackFSMType.Attacking, OnReady, OnAttack),
			new Reloading(this, UnitAttackFSMType.Reloading, OnReloading),
			new ReattackingState(this, UnitAttackFSMType.Reattacking),
		};
		return states;
	}
	private void OnReady(int continuousAttackCount,int simultaneousAttackCount, float continuousAttackDelay)
	{
		OnAttackReady?.Invoke(continuousAttackCount, simultaneousAttackCount, continuousAttackDelay);
	}
	private void OnAttack(int count)
	{
		OnAttackTiming?.Invoke(count);
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
				int ammoMaxCount = StateControl.GetStateValue(StatsType.유닛_탄용량);
				int ammoUsedCount = StateControl.GetStateValue(StatsType.유닛_사용탄수);
				return ammoUsedCount >= ammoMaxCount ? UnitAttackFSMType.Reloading : UnitAttackFSMType.Aiming;
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
		private readonly Action<int,int,float> onReady;
		private readonly Action<int> onAttack;
		int continuousAttackCount;
		int simultaneousAttackCount;
		float continuousAttackDelay;

		int ammoMaxCount;
		int ammoUsedCount;
		public AttackingState(UnitAttackFiniteStateMachine attackState, UnitAttackFSMType type, Action<int, int, float> onReady, Action<int> onAttack) : base(attackState, type)
		{
			this.onReady = onReady;
			this.onAttack = onAttack;
		}
		protected override void OnStateEnter()
		{
			continuousAttackCount = StateControl.GetStateValue(StatsType.유닛_연속공격횟수);
			simultaneousAttackCount = StateControl.GetStateValue(StatsType.유닛_동시공격개수);
			continuousAttackDelay = StateControl.GetStateValuePercent(StatsType.유닛_연속공격지연시간_c);
			ammoMaxCount = StateControl.GetStateValue(StatsType.유닛_탄용량);
			ammoUsedCount = StateControl.GetStateValue(StatsType.유닛_사용탄수);

			if (continuousAttackCount < 1) continuousAttackCount = 1;
			if (simultaneousAttackCount < 1) simultaneousAttackCount = 1;
			if (continuousAttackDelay < 0.01) continuousAttackDelay = 0.01f;

			onReady?.Invoke(continuousAttackCount, simultaneousAttackCount, continuousAttackDelay);
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

			if (ammoUsedCount < ammoMaxCount && continuousAttackCount > 0)
			{
				continuousAttackDelay -= deltaTime;
				if (continuousAttackDelay <= 0)
				{
					OnAttack();
				}
			}

			if (ammoUsedCount >= ammoMaxCount)
			{
				return UnitAttackFSMType.Reloading;
			}
			else if (continuousAttackCount <= 0)
			{
				return UnitAttackFSMType.Reattacking;
			}
			return ThisType;
		}
		private void OnAttack()
		{
			try
			{
				onAttack?.Invoke(simultaneousAttackCount);
				--continuousAttackCount;
				++ammoUsedCount;
				continuousAttackDelay += StateControl.GetStateValuePercent(StatsType.유닛_연속공격지연시간_c);
				ammoMaxCount = StateControl.GetStateValue(StatsType.유닛_탄용량);
			}
			catch
			{
				continuousAttackCount = 0;
				continuousAttackDelay = 0;
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
				if (attackState.didAiming)
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
	public event Action<int,int,float > OnAttackReady;
	public event Action<int> OnAttackTiming;
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
		else if(MainFSMController.CurrentStateType == UnitMainFSMType.Fighting)
		{
			return true;
		}
		else if (ThisAttackState.CurrentStateType != UnitAttackFSMType.Idle)
		{
			return true;
		}
		return false;
	}
}


