using UnityEngine;

public interface IUnitCombatController
{
	IUnitCombatController ThisCombatController { get; }
	IStrategyElement ThisElement { get; }
	int FactionID { get; }

	bool IsCombatState { get; set; }
	Vector3 CombatMoveTarget { get; set; }

	Vector3 Position { get; }
	float AttackRange { get; }
	float ActionRange { get; }
	float VisionRange { get; }
	ITargetableCombatant CurrentTarget { get; }
	bool HasCurrentTarget => CurrentTarget != null;
	bool TargetInAttackRange { get; }
	bool TargetInActionRange { get; }

	void UpdateParameters();
	bool IsKeepingTargetAllowed();
	bool SearchingNewTarget(out ITargetableCombatant newTarget);
	void SetCombatTarget(in ITargetableCombatant newTarget);
	void ClearCombatTarget();
}
