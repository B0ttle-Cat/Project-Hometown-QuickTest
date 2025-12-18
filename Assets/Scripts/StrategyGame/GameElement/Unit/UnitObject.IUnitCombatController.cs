using System;

using UnityEngine;

public partial class UnitObject : ICombatHandler, ITargetableCombatant
{
	public ICombatHandler ThisCombatHandler => this;
	bool ICombatHandler.IsCombatState => FSMController.CurrentStateType is UnitMainFSMType.Chasing or UnitMainFSMType.Fighting;
	bool ICombatHandler.IsOperationCombatState { get => HasOperation && isOperationCombatState; set => isOperationCombatState = value; }

	private Collider hitCollider;

	private Vector2 combatAttackStartRange;
	private Vector2 combatAttackLimitRange;

	private Vector2 sqrCombatAttackStartRange;
	private Vector2 sqrCombatAttackLimitRange;
	private float sqrCombatActionRange;
	private float sqrCombatVisionRange;

	private bool isOperationCombatState;
	private ITargetableCombatant currentCombatTarget;
	private ITargetableCombatant rootCurrentCombatTarget;
	private bool isTargetInStartAttackRange;
	private bool isTargetInLimitAttackRange;
	private bool isTargetInActionRange;

	public event Action<ITargetableCombatant> OnChangeCurrentCombatTarget;

	partial void InitCombat()
	{
		hitCollider = GetComponentInChildren<Collider>();

		combatAttackStartRange = Vector2.zero;
		combatAttackLimitRange = Vector2.zero;
		sqrCombatAttackStartRange = Vector2.zero;
		sqrCombatAttackLimitRange = Vector2.zero;
		sqrCombatActionRange = 0f;
		sqrCombatVisionRange = 0f;
		currentCombatTarget = null;
		isTargetInStartAttackRange = false;
		isTargetInLimitAttackRange = false;
		isTargetInActionRange = false;


	}
	partial void DeinitCombat()
	{
		currentCombatTarget = null;
		OnChangeCurrentCombatTarget = null;
	}
	Collider IHitableCombatant.HitCollider => hitCollider;
	Vector3 ITargetableCombatant.HitTargetPosition => hitCollider == null ? ThisMovement.CurrentPosition + Vector3.up : hitCollider.bounds.center;
	public ITargetableCombatant TargetableObject => this;

	Vector3 ICombatHandler.Position => ThisMovement.CurrentPosition;
	Vector3 ICombatHandler.AttackStartPosition => ThisMovement.CurrentPosition + Vector3.up;
	Vector2 ICombatHandler.AttackStartRange => combatAttackStartRange;
	Vector2 ICombatHandler.AttackLimitRange => combatAttackLimitRange;

	ITargetableCombatant ICombatHandler.CurrentTarget { get => currentCombatTarget.IsNotNullRef() ? currentCombatTarget : rootCurrentCombatTarget; set => rootCurrentCombatTarget = value; }
	ITargetableCombatant ICombatHandler.OperationCurrentTarget { get => rootCurrentCombatTarget; set => rootCurrentCombatTarget = value; }
	bool ICombatHandler.TargetInStartAttackRange => ThisCombatHandler.HasCurrentTarget && isTargetInStartAttackRange;
	bool ICombatHandler.TargetInLimitAttackRange => ThisCombatHandler.HasCurrentTarget && isTargetInLimitAttackRange;
	//bool ICombatHandler.TargetInActionRange => (ThisCombatHandler.HasOperationCurrentTarget && isOperationCombatState) || (ThisCombatHandler.HasCurrentTarget && isTargetInActionRange);
	void ICombatHandler.UpdateParameters()
	{
		// TODO NearbySearching 기반으로 수정해야 함

	}
	bool ICombatHandler.SomthingInActionRange()
	{
		if (HasOperation)
		{
			return Operation.ActionSearcherAPI.HasNearbySomthing();
		}
		return ActionSearcherAPI.HasNearbySomthing();
	}
	bool ICombatHandler.SomthingInAttackRange()
	{
		if (ThisCombatHandler.HasCurrentTarget)
		{
			return AttackLimitSearcherAPI.HasNearbySomthing();
		}
		else
		{
			return AttackStartSearcherAPI.HasNearbySomthing();
		}
	}
	bool ICombatHandler.HasKeepAttackTarget()
	{

		if (ThisCombatHandler.HasCurrentTarget)
		{
			return AttackLimitSearcherAPI.HasNearby(ThisCombatHandler.CurrentTarget);
		}
		return false;
	}
	bool ICombatHandler.SearchingNewTarget(out ITargetableCombatant newTarget)
	{
		if(AttackStartSearcherAPI.HasNearbySomthing())
		{
			newTarget = AttackStartSearcherAPI.GetNearbyItemType<ITargetableCombatant>();
		}
		else if( AttackLimitSearcherAPI.HasNearbySomthing())
		{
			newTarget = AttackLimitSearcherAPI.GetNearbyItemType<ITargetableCombatant>();
		}
		else
		{
			newTarget = null;
		}
		return newTarget.IsNotNullRef();
	}
	void ICombatHandler.ChangeCombatTarget(in ITargetableCombatant newTarget)
	{
		if (newTarget.IsNullRef())
		{
			ClearCombatTarget();
		}
		else if (ThisCombatHandler.CurrentTarget.IsNullRef() || ThisCombatHandler.CurrentTarget.ThisElement.ID != newTarget.ThisElement.ID)
		{
			SetCombatTarget(newTarget);
		}
	}
	void ClearCombatTarget()
	{
		if (!ThisCombatHandler.HasCurrentTarget) return;
		ThisCombatHandler.CurrentTarget = null;

		OnChangeCurrentCombatTarget?.Invoke(null);
	}
	void SetCombatTarget(in ITargetableCombatant newTarget)
	{
		ThisCombatHandler.CurrentTarget = newTarget;
		if (!ThisCombatHandler.HasCurrentTarget) return;

		OnChangeCurrentCombatTarget?.Invoke(newTarget);
	}
}