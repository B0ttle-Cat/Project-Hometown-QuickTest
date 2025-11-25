using UnityEngine;
public interface ITargetableCombatant : INearbyElement
{

}

public partial class UnitObject : IUnitCombatController, ITargetableCombatant
{

	public IUnitCombatController ThisCombatController => this;
	bool IUnitCombatController.IsCombatState { get; set; } = false;
	Vector3 IUnitCombatController.CombatMoveTarget { get; set; } = Vector3.zero;

	private float combatAttackRange;
	private float combatActionRange;
	private float combatVisionRange;
	private ITargetableCombatant combatCurrentTarget;
	private bool isTargetInAttackRange;
	private bool isTargetInActionRange;

	partial void InitCombat()
	{
		ThisCombatController.IsCombatState = false;
		ThisCombatController.CombatMoveTarget = Vector3.zero;

		combatCurrentTarget = null;
		combatAttackRange = 0f;
		combatActionRange = 0f;
		combatVisionRange = 0f;
		isTargetInAttackRange = false;
		isTargetInActionRange = false;
	}
	partial void DeinitCombat()
	{
		combatCurrentTarget = null;
	}
	float INearbyElement.Radius => moveRadius;
	Vector3 INearbyElement.Position => movePosition;

	Vector3 IUnitCombatController.Position => movePosition;
	float IUnitCombatController.AttackRange => combatAttackRange;
	float IUnitCombatController.ActionRange => combatActionRange;
	float IUnitCombatController.VisionRange => combatVisionRange;
	ITargetableCombatant IUnitCombatController.CurrentTarget => ThisCombatController.IsCombatState ? combatCurrentTarget : null;
	bool IUnitCombatController.TargetInAttackRange => ThisCombatController.HasCurrentTarget ? isTargetInAttackRange : false;
	bool IUnitCombatController.TargetInActionRange => ThisCombatController.HasCurrentTarget ? isTargetInActionRange : false;

	void IUnitCombatController.UpdateParameters()
	{
		combatAttackRange = StatsData.GetValue(StrategyGamePlayData.StatsType.유닛_공격범위) * 0.01f;
		combatActionRange = StatsData.GetValue(StrategyGamePlayData.StatsType.유닛_행동범위) * 0.01f;
		combatVisionRange = StatsData.GetValue(StrategyGamePlayData.StatsType.유닛_시야범위) * 0.01f;

		if (combatCurrentTarget != null)
		{
			float distance = Vector3.Distance(combatCurrentTarget.Position,ThisCombatController.Position);

			isTargetInAttackRange = distance <= combatAttackRange;
			isTargetInActionRange = distance <= combatAttackRange;
		}
	}

	bool IUnitCombatController.IsKeepingTargetAllowed()
	{
		var currentTarget = combatCurrentTarget;
		if (currentTarget == null) return false;

		Vector3 distance = currentTarget.Position - ThisCombatController.Position;
		float sqrDistance = distance.sqrMagnitude;
		float sqrAttackRange = ThisCombatController.AttackRange;
		sqrAttackRange *= sqrAttackRange;

		if (sqrDistance < sqrAttackRange)
		{
			// 공격 범위 안에 있음
			// => 계속 공격
			return true;
		}
		return false;
	}
	bool IUnitCombatController.SearchingNewTarget(out ITargetableCombatant newTarget)
	{
		newTarget = null;
		var detectingList = Faction.DetectingList;
		if (detectingList == null || detectingList.Count == 0) return false;

		Vector3 thisPosition = ThisCombatController.Position;
		float sqrSearchingRange = Mathf.Max(ThisCombatController.AttackRange, ThisCombatController.ActionRange);
		sqrSearchingRange *= sqrSearchingRange;
		float minDistance = float.MaxValue;

		foreach (var item in detectingList)
		{
			if (item is not ITargetableCombatant targetable) continue;
			Vector3 distance = targetable.Position - thisPosition;
			float sqrDistance = distance.sqrMagnitude;
			if (sqrDistance < sqrSearchingRange && sqrDistance < minDistance)
			{
				minDistance = sqrDistance;
				newTarget = targetable;
			}
		}
		return newTarget != null;
	}
	void IUnitCombatController.SetCombatTarget(in ITargetableCombatant newTarget)
	{
		if (!ThisCombatController.IsCombatState) return;
		if (newTarget == null)
		{
			ThisCombatController.ClearCombatTarget();
			return;
		}
		if (combatCurrentTarget.ThisElement.ID == newTarget.ThisElement.ID) return;
		combatCurrentTarget = newTarget;

		if (combatCurrentTarget != null)
		{
			float distance = Vector3.Distance(combatCurrentTarget.Position,ThisCombatController.Position);

			isTargetInAttackRange = distance <= combatAttackRange;
			isTargetInActionRange = distance <= combatAttackRange;
		}
	}
	void IUnitCombatController.ClearCombatTarget()
	{
		combatCurrentTarget = null;
	}
}
