using UnityEngine;

public interface IUnitAttackFSMController : IFSMController<UnitAttackFSMType>
{
	IUnitAttackFSMController AttackController { get; }
	IUnitAttackState AttackState { get; }
}



[UnityEngine.RequireComponent(typeof(UnitAttackFiniteStateMachine))]
public partial class UnitObject : IUnitAttackFSMController
{
	private IUnitAttackState unitAttackState;
	public IUnitAttackFSMController AttackController => this;
	public IUnitAttackState AttackState => unitAttackState;
	IFSMController<UnitAttackFSMType> IFSMController<UnitAttackFSMType>.FSMController => this;
	IFSMInterface<UnitAttackFSMType> IFSMController<UnitAttackFSMType>.FSMInterface { get => unitAttackState; set { } }

	partial void InitAttack()
	{
		if (unitAttackState == null)
		{
			unitAttackState = GetComponent<UnitAttackFiniteStateMachine>();
		}

		AttackController.InitState(OnStateEnterCallback, OnStateExitCallback, UnitAttackFSMType.Idle, AttackController.GetStateList());

		AttackState.OnAttackReady -= AttackState_OnAttackReady;
		AttackState.OnAttackTiming -= AttackState_OnAttackTiming;
		AttackState.OnReloadingTiming -= AttackState_OnReloadingTiming;
		ThisCombatHandler.OnChangeCurrentCombatTarget -= ThisCombatController_OnChangeCurrentCombatTarget;

		AttackState.OnAttackReady += AttackState_OnAttackReady;
		AttackState.OnAttackTiming += AttackState_OnAttackTiming;
		AttackState.OnReloadingTiming += AttackState_OnReloadingTiming;
		ThisCombatHandler.OnChangeCurrentCombatTarget += ThisCombatController_OnChangeCurrentCombatTarget;
	}
	partial void DeinitAttack()
	{
		if (unitAttackState != null)
		{
			AttackState.OnAttackReady -= AttackState_OnAttackReady;
			AttackState.OnAttackTiming -= AttackState_OnAttackTiming;
			AttackState.OnReloadingTiming -= AttackState_OnReloadingTiming;
			ThisCombatHandler.OnChangeCurrentCombatTarget -= ThisCombatController_OnChangeCurrentCombatTarget;

			AttackController.DeinitState();
			unitAttackState = null;
		}
	}

	private void AttackState_OnReloadingTiming()
	{
		StatsValue.SetRuntimeDataValue(StrategyGamePlayData.StatsType.유닛_사용탄수, 0);
	}

	private void AttackState_OnAttackReady(int continuousAttackCount, int simultaneousAttackCount, float continuousAttackDelay)
	{
		// n 초 동안 총알이 발사된다고 가정한다.
		float attackLifeTime = 1;
		// 그렇다면, n 초 동안 이만큼 공격할수 있을것...
		int burstCount = 1 + Mathf.FloorToInt(attackLifeTime / continuousAttackDelay);
		// 하지만, 시간과 관계없이 최대 공격 횟수는 continuousAttackCount 로 정해져 있음.
		if (burstCount > continuousAttackCount) burstCount = continuousAttackCount;
		// 따라서, n 초 동안 사용할 적정 Pool 개수는 아래와 같다고 보임.
		int requiredPoolSize = burstCount * simultaneousAttackCount;


		StrategyElementFactory.ReadyPoolCount(StatsData.ProjectileKey, requiredPoolSize);
	}
	private async void AttackState_OnAttackTiming(int count)
	{
		if (this is not ICombatHandler unitCombat || unitCombat == null) return;
		ITargetableCombatant target  = unitCombat.CurrentTarget;
		if (target == null) return;

		var projectiles = await StrategyElementFactory.Instantiate(this, target, StatsData.ProjectileKey, count);
	}

	private void ThisCombatController_OnChangeCurrentCombatTarget(ITargetableCombatant obj)
	{
		AttackState.SetChangeNewTargetFlag(obj != null);
	}


	private void OnStateExitCallback(UnitAttackFSMType type)
	{
		switch (type)
		{
			case UnitAttackFSMType.Idle:
			break;
			case UnitAttackFSMType.Aiming:
			break;
			case UnitAttackFSMType.Attacking:
			break;
			case UnitAttackFSMType.Reloading:
			break;
			case UnitAttackFSMType.Reattacking:
			break;
		}
	}

	private void OnStateEnterCallback(UnitAttackFSMType type)
	{
	}
}