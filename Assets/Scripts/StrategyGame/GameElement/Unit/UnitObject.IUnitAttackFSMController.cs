using UnityEngine;

using static StrategyGamePlayData;

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


	public ProjectileKey minaProjectileKey;

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

		minaProjectileKey = ProfileData.projectileKey;
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
		StatsValue.SetValueInMainStats(StrategyGamePlayData.StatsType.유닛_사용탄수, 0);
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


		StrategyElementFactory.ReadyPoolCount(minaProjectileKey, requiredPoolSize);
	}
	private async void AttackState_OnAttackTiming(int count)
	{
		// TODO:
		// 공격 발생시 처리
		// 1. 총알을 생성한다.
		// 2. 총알에 목표를 지정한다.
		// 3. 총알이 목표에 도달하면 데미지를 입힌다.
		// 4. 이떄 데미지 게산을 위해 콜백으로 데미지 계산 함수를 넘긴다.
		// 5. 데미지 함수의 매개변수로는 공격자, 피격자, 스킬 정보 등이 있다.

		if (this is not ICombatHandler unitCombat || unitCombat == null) return;
		ITargetableCombatant target  = unitCombat.CurrentTarget;
		if (target == null) return;


		var projectiles = await StrategyElementFactory.Instantiate(minaProjectileKey, count);
		for (int i = 0 ; i < count ; i++)
		{
			OnShot(projectiles[i], this, target);
		}



		static void OnShot(ProjectileObject projectile, ICombatHandler order, ITargetableCombatant target)
		{
			if (projectile == null) return;
			projectile.SetTarget(order, target);
		}
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