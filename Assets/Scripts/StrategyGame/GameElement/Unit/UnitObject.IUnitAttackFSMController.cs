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

		AttackState.OnAttackTiming -= AttackState_OnAttackTiming;
		AttackState.OnAttackTiming += AttackState_OnAttackTiming;

		AttackState.OnReloadingTiming -= AttackState_OnReloadingTiming;
        AttackState.OnReloadingTiming += AttackState_OnReloadingTiming;

		ThisCombatController.OnChangeCurrentCombatTarget -= ThisCombatController_OnChangeCurrentCombatTarget;
		ThisCombatController.OnChangeCurrentCombatTarget += ThisCombatController_OnChangeCurrentCombatTarget;
	}

    private void AttackState_OnReloadingTiming()
    {
		SetValueInMainState(StrategyGamePlayData.StatsType.유닛_사용탄수, 0);
    }

    private void AttackState_OnAttackTiming()
    {
		// TODO:
		// 공격 발생시 처리
		// 1. 총알을 생성한다.
		// 2. 총알에 목표를 지정한다.
		// 3. 총알이 목표에 도달하면 데미지를 입힌다.
	}

	private void ThisCombatController_OnChangeCurrentCombatTarget(ITargetableCombatant obj)
    {
		AttackState.SetChangeNewTargetFlag(obj != null);
	}

    partial void DeinitAttack()
	{
		if(unitAttackState != null)
		{
			AttackState.OnAttackTiming -= AttackState_OnAttackTiming;
			AttackState.OnReloadingTiming -= AttackState_OnReloadingTiming;
			ThisCombatController.OnChangeCurrentCombatTarget -= ThisCombatController_OnChangeCurrentCombatTarget;

			AttackController.DeinitState();
			unitAttackState = null;
		}
	}
	private void OnStateExitCallback(UnitAttackFSMType type)
	{
	}

	private void OnStateEnterCallback(UnitAttackFSMType type)
	{
	}
}
