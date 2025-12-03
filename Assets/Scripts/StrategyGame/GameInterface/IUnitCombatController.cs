using System;

using UnityEngine;

public interface IUnitCombatController
{
	IUnitCombatController ThisCombatController { get; }
	IStrategyElement ThisElement { get; }
	int FactionID { get; }

	Vector3 Position { get; }
	Vector2 AttackStartRange { get; }
	Vector2 AttackLimitRange { get; }
	float ActionRange { get; }
	float VisionRange { get; }
	bool IsCombatState { get; }
	ITargetableCombatant CurrentTarget { get; set; }
	bool HasCurrentTarget => CurrentTarget != null;
	bool TargetInStartAttackRange { get; }
	bool TargetInLimitAttackRange { get; }
	bool TargetInActionRange { get; }

	bool IsRootCombatState { get; set; }
	ITargetableCombatant RootCurrentTarget { get; set; }
	bool HasRootCurrentTarget => RootCurrentTarget != null;

	event Action<ITargetableCombatant> OnChangeCurrentCombatTarget;
	void UpdateParameters();
	bool IsKeepingTargetAllowed();
	bool SearchingNewTarget(out ITargetableCombatant newTarget);
	void ChangeCombatTarget(in ITargetableCombatant newTarget);
}
