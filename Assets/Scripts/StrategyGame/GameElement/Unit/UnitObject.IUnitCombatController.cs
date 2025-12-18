using System;

using UnityEngine;

using Collider = UnityEngine.Collider;

public partial class UnitObject : ICombatHandler, ITargetableCombatant
{
	public ICombatHandler ThisCombatHandler => this;
	public ITargetableCombatant TargetableObject => this;

	private Collider hitCollider;
	private ITargetableCombatant combatAttackTarget;
	private ITargetableCombatant combatActionTarget;
	public event Action<ITargetableCombatant> OnChangeCurrentCombatTarget;
	partial void InitCombat()
	{
		hitCollider = GetComponentInChildren<Collider>();
		combatAttackTarget = null;
		combatActionTarget = null;
	}
	partial void DeinitCombat()
	{
		hitCollider = null;
		combatAttackTarget = null;
		combatActionTarget = null;

		OnChangeCurrentCombatTarget = null;
	}
	bool ICombatHandler.IsCombatState => FSMController.CurrentStateType is UnitMainFSMType.Chasing or UnitMainFSMType.Fighting;
	Collider IHitableCombatant.HitCollider => hitCollider;
	Vector3 ITargetableCombatant.HitTargetPosition => hitCollider == null ? ThisMovement.CurrentPosition + Vector3.up : hitCollider.bounds.center;
	Vector3 ICombatHandler.Position => ThisMovement.CurrentPosition;
	Vector3 ICombatHandler.AttackStartPosition => ThisMovement.CurrentPosition + Vector3.up;
	ITargetableCombatant ICombatHandler.ActionTarget => combatActionTarget; //set => combatActionTarget = value; }
	ITargetableCombatant ICombatHandler.AttackTarget => combatAttackTarget; //set => combatAttackTarget = value; }

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
		if (ThisCombatHandler.HasAttackTarget)
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
		if (ThisCombatHandler.HasAttackTarget)
		{
			return AttackLimitSearcherAPI.HasNearby(ThisCombatHandler.AttackTarget);
		}
		return false;
	}
	void ICombatHandler.UpdateNewNearbyTarget()
	{
		ITargetableCombatant newCombatActionTarget = null;
		ITargetableCombatant newCombatAttackTarget = null;
		UpdateTarget();
		void UpdateTarget()
		{
			newCombatActionTarget = (HasOperation ? Operation.ActionSearcherAPI : ActionSearcherAPI).GetNearbyItemType<ITargetableCombatant>();

			if (AttackStartSearcherAPI.HasNearbySomthing())
			{
				newCombatAttackTarget = AttackStartSearcherAPI.GetNearbyItemType<ITargetableCombatant>();
			}
			else if (AttackLimitSearcherAPI.HasNearbySomthing())
			{
				newCombatAttackTarget = AttackLimitSearcherAPI.GetNearbyItemType<ITargetableCombatant>();
			}
		}


		var prevTarget = ThisCombatHandler.CurrentTarget;

		ChangeCombatActionTarget(newCombatActionTarget);
		ChangeCombatAttackTarget(newCombatAttackTarget);
		void ChangeCombatActionTarget(in ITargetableCombatant newTarget)
		{
			if (newTarget.IsNullRef())
			{
				if (combatActionTarget.IsNotNullRef())
				{
					combatActionTarget = null;
				}
			}
			else
			{
				if (combatActionTarget.IsNullRef())
				{
					combatActionTarget = newTarget;
				}
				else if (combatActionTarget.ThisElement.ID != newTarget.ThisElement.ID)
				{
					combatActionTarget = newTarget;
				}
			}
		}
		void ChangeCombatAttackTarget(in ITargetableCombatant newTarget)
		{
			if (newTarget.IsNullRef())
			{
				if (combatAttackTarget.IsNotNullRef())
				{
					combatAttackTarget = null;
				}
			}
			else
			{
				if (combatAttackTarget.IsNullRef())
				{
					combatAttackTarget = newTarget;
				}
				else if (combatAttackTarget.ThisElement.ID != newTarget.ThisElement.ID)
				{
					combatAttackTarget = newTarget;
				}
			}
		}

		var currentTarget = ThisCombatHandler.CurrentTarget;

		if(prevTarget != currentTarget)
		{
			OnChangeCurrentCombatTarget?.Invoke(currentTarget);
		}
	}
}