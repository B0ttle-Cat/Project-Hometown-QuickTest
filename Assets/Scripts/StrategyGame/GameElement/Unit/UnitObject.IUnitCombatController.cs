using System;
using System.Linq;

using UnityEditor;

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
	Vector3 ITargetableCombatant.Position => ThisMovement.CurrentPosition;
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
	bool ICombatHandler.TargetInActionRange => (ThisCombatHandler.HasOperationCurrentTarget && isOperationCombatState) || (ThisCombatHandler.HasCurrentTarget && isTargetInActionRange);
	void ICombatHandler.UpdateParameters()
	{

		float combatAttackRangeLimitMin = ThisCombatStats.AttackRangeLimitMin;
		float combatAttackRangeStartMin = ThisCombatStats.AttackRangeStartMin;
		float combatAttackRangeStartMax = ThisCombatStats.AttackRangeStartMax;
		float combatAttackRangeLimitMax = ThisCombatStats.AttackRangeLimitMax;
		combatAttackStartRange = new Vector2(combatAttackRangeStartMin, combatAttackRangeStartMax);
		combatAttackLimitRange = new Vector2(combatAttackRangeLimitMin, combatAttackRangeLimitMax);
		var combatActionRange = ThisCombatStats.ActionRange;
		var combatVisionRange = ThisCombatStats.VisionRange;

		float sqrCombatAttackStartMinRange = combatAttackRangeStartMin * combatAttackRangeStartMin;
		float sqrCombatAttackStartMaxRange = combatAttackRangeStartMax * combatAttackRangeStartMax;
		float sqrCombatAttackLimitMinRange = combatAttackRangeLimitMin * combatAttackRangeLimitMin;
		float sqrCombatAttackLimitMaxRange = combatAttackRangeLimitMax * combatAttackRangeLimitMax;
		sqrCombatAttackStartRange = new Vector2(sqrCombatAttackStartMinRange, sqrCombatAttackStartMaxRange);
		sqrCombatAttackLimitRange = new Vector2(sqrCombatAttackLimitMinRange, sqrCombatAttackLimitMaxRange);
		sqrCombatActionRange = combatActionRange * combatActionRange;
		sqrCombatVisionRange = combatVisionRange * combatVisionRange;

		if (ThisCombatHandler.HasCurrentTarget)
		{
			float sqrDistance = (ThisCombatHandler.CurrentTarget.Position - ThisCombatHandler.Position).sqrMagnitude;

			isTargetInStartAttackRange = sqrCombatAttackStartMinRange <= sqrDistance && sqrDistance <= sqrCombatAttackStartMaxRange;
			isTargetInLimitAttackRange = sqrCombatAttackLimitMinRange <= sqrDistance && sqrDistance <= sqrCombatAttackLimitMaxRange;
			isTargetInActionRange = sqrDistance <= sqrCombatActionRange;
		}
		else
		{
			ThisCombatHandler.CurrentTarget = null;
		}
	}
	bool ICombatHandler.IsInAttackRange()
	{

		if (!ThisCombatHandler.HasCurrentTarget) return false;

		Vector3 distance = ThisCombatHandler.CurrentTarget.Position - ThisCombatHandler.Position;
		float sqrDistance = distance.sqrMagnitude;
		float sqrCombatAttackLimitMinRange = combatAttackLimitRange.x;
		float sqrCombatAttackLimitMaxRange = combatAttackLimitRange.y;
		if (sqrCombatAttackLimitMinRange <= sqrDistance && sqrDistance <= sqrCombatAttackLimitMaxRange)
		{
			// 공격 범위 안에 있음
			// => 계속 공격
			return true;
		}
		return false;
	}
	bool ICombatHandler.SearchingNewTarget(out ITargetableCombatant newTarget)
	{
		// TODO:: Operation 과 Unit 의 적 탐지 및 선택 로직 수정.....
		// Unit 에도 NearbySearcher 가 필요해....
		if (ThisCombatHandler.IsOperationCombatState && ThisCombatHandler.HasOperationCurrentTarget)
		{

		}
			
		newTarget = null;
		var detectingList = Faction.DetectedList.TargetableType;
		if (detectingList == null || detectingList.Count() == 0) return false;

		Vector3 thisPosition = ThisCombatHandler.Position;
		float minDistance = float.MaxValue;
		float maxActionRange = Mathf.Max(sqrCombatActionRange, sqrCombatAttackStartRange.y);
		foreach (var targetable in detectingList)
		{
			if (targetable.IsNullRef()) continue;

			Vector3 distance = targetable.Position - thisPosition;
			float sqrDistance = distance.sqrMagnitude;
			if (sqrDistance < minDistance && sqrDistance <= maxActionRange)
			{
				minDistance = sqrDistance;
				newTarget = targetable;
			}
		}
		return newTarget != null;
	}
	void ICombatHandler.ChangeCombatTarget(in ITargetableCombatant newTarget)
	{
		if (newTarget == null)
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

		OnChangeCurrentCombatTarget?.Invoke(ThisCombatHandler.CurrentTarget);
	}
	void SetCombatTarget(in ITargetableCombatant newTarget)
	{
		ThisCombatHandler.CurrentTarget = newTarget;
		if (!ThisCombatHandler.HasCurrentTarget) return;

		float sqrDistance = (ThisCombatHandler.CurrentTarget.Position - ThisCombatHandler.Position).sqrMagnitude;
		float sqrCombatAttackStartMinRange = combatAttackStartRange.x;
		float sqrCombatAttackStartMaxRange = combatAttackStartRange.y;
		float sqrCombatAttackLimitMinRange = combatAttackLimitRange.x;
		float sqrCombatAttackLimitMaxRange = combatAttackLimitRange.y;
		isTargetInStartAttackRange = sqrCombatAttackStartMinRange <= sqrDistance && sqrDistance <= sqrCombatAttackStartMaxRange;
		isTargetInLimitAttackRange = sqrCombatAttackLimitMinRange <= sqrDistance && sqrDistance <= sqrCombatAttackLimitMaxRange;
		isTargetInActionRange = sqrDistance <= sqrCombatActionRange;

		OnChangeCurrentCombatTarget?.Invoke(ThisCombatHandler.CurrentTarget);
	}
}


#if UNITY_EDITOR

public partial class UnitObject // RangeGizmos
{
	void OnDrawGizmos_Range()
	{
		// 중심 (XZ) 및 Y 고정
		Vector3 center = transform.position;
		center.y = transform.position.y;

		// 색 정의
		Color attackStartColor = new Color(1f, 0.2f, 0.2f, 0.9f); // 빨강 계열 
		Color attackLimitColor = new Color(1f, 0.6f, 0.1f, 0.9f); // 주황 계열 
		Color actionColor = new Color(0.2f, 1f, 0.3f, 0.9f);       // 녹색
		Color visionColor = new Color(0.2f, 0.8f, 1f, 0.9f);       // 시안

		// combatAttackStartRange : Vector2(minRadius, maxRadius) -> 점선으로 그리기
		if (combatAttackStartRange.x > 0f)
			DrawCircleXZ(center, combatAttackStartRange.x, attackStartColor);
		if (combatAttackStartRange.y > 0f)
			DrawCircleXZ(center, combatAttackStartRange.y, attackStartColor);

		// combatAttackLimitRange : Vector2(minRadius, maxRadius) -> 실선으로 그리기
		if (combatAttackLimitRange.x > 0f)
			DrawCircleXZ(center, combatAttackLimitRange.x, attackLimitColor);
		if (combatAttackLimitRange.y > 0f)
			DrawCircleXZ(center, combatAttackLimitRange.y, attackLimitColor);

		// action range (실선)
		// ThisCombatStats가 UnitObject의 필드에 정의되어 있다고 가정
		float combatActionRange = ThisCombatStats.ActionRange;
		if (combatActionRange > 0f)
			DrawCircleXZ(center, combatActionRange, actionColor);

		// vision range (실선)
		float combatVisionRange = ThisCombatStats.VisionRange;
		if (combatVisionRange > 0f)
			DrawCircleXZ(center, combatVisionRange, visionColor);
	}

	// XZ 평면에 원을 그림. dotted = true 면 Handles.DrawDottedLine을 사용한다.
	void DrawCircleXZ(Vector3 center, float radius, Color color)
	{
		if (radius <= 0f) return;

		Handles.color = color;
		Handles.DrawWireDisc(center, Vector3.up, radius);
	}
}
#endif