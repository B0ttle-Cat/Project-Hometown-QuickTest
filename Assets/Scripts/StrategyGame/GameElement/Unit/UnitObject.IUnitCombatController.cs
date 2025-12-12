using System;

using UnityEngine;

public partial class UnitObject : ICombatHandler, INearbyElement, ITargetableCombatant
{
	public ICombatHandler ThisCombatHandler => this;
	bool ICombatHandler.IsCombatState => FSMController.CurrentStateType is UnitMainFSMType.Chasing or UnitMainFSMType.Fighting;
	bool ICombatHandler.IsRootCombatState { get => isOperationCombatState; set => isOperationCombatState = value; }

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
	float INearbyElement.Radius => ThisMovement.CurrentRadius;

	Vector3 ICombatHandler.Position => ThisMovement.CurrentPosition;
	Vector3 ICombatHandler.AttackStartPosition => ThisMovement.CurrentPosition + Vector3.up;
	Vector2 ICombatHandler.AttackStartRange => combatAttackStartRange;
	Vector2 ICombatHandler.AttackLimitRange => combatAttackLimitRange;

	ITargetableCombatant ICombatHandler.CurrentTarget { get => currentCombatTarget; set => currentCombatTarget = value; }
	ITargetableCombatant ICombatHandler.RootCurrentTarget { get => rootCurrentCombatTarget; set => rootCurrentCombatTarget = value; }
	bool ICombatHandler.TargetInStartAttackRange => ThisCombatHandler.HasCurrentTarget && isTargetInStartAttackRange;
	bool ICombatHandler.TargetInLimitAttackRange => ThisCombatHandler.HasCurrentTarget && isTargetInLimitAttackRange;
	bool ICombatHandler.TargetInActionRange => ThisCombatHandler.HasCurrentTarget && isTargetInActionRange;
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

		if (currentCombatTarget != null)
		{
			float sqrDistance = (currentCombatTarget.Position - ThisCombatHandler.Position).sqrMagnitude;

			isTargetInStartAttackRange = sqrCombatAttackStartMinRange <= sqrDistance && sqrDistance <= sqrCombatAttackStartMaxRange;
			isTargetInLimitAttackRange = sqrCombatAttackLimitMinRange <= sqrDistance && sqrDistance <= sqrCombatAttackLimitMaxRange;
			isTargetInActionRange = sqrDistance <= sqrCombatActionRange;
		}
	}
	bool ICombatHandler.IsKeepingTargetAllowed()
	{
		var currentTarget = currentCombatTarget;
		if (currentTarget == null) return false;

		Vector3 distance = currentTarget.Position - ThisCombatHandler.Position;
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
		newTarget = null;
		var detectingList = Faction.DetectingList;
		if (detectingList == null || detectingList.Count == 0) return false;

		Vector3 thisPosition = ThisCombatHandler.Position;
		float minDistance = float.MaxValue;
		float maxActionRange = Mathf.Max(sqrCombatActionRange, sqrCombatAttackStartRange.y);
		foreach (var item in detectingList)
		{
			if (item is not ITargetableCombatant targetable) continue;
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
		else if (currentCombatTarget == null || currentCombatTarget.ThisElement.ID != newTarget.ThisElement.ID)
		{
			SetCombatTarget(newTarget);
		}
	}
	void ClearCombatTarget()
	{
		if (currentCombatTarget == null) return;

		currentCombatTarget = null;

		OnChangeCurrentCombatTarget?.Invoke(null);
	}
	void SetCombatTarget(in ITargetableCombatant newTarget)
	{
		currentCombatTarget = newTarget;
		if (currentCombatTarget == null) return;

		float sqrDistance = (currentCombatTarget.Position - ThisCombatHandler.Position).sqrMagnitude;
		float sqrCombatAttackStartMinRange = combatAttackStartRange.x;
		float sqrCombatAttackStartMaxRange = combatAttackStartRange.y;
		float sqrCombatAttackLimitMinRange = combatAttackLimitRange.x;
		float sqrCombatAttackLimitMaxRange = combatAttackLimitRange.y;
		isTargetInStartAttackRange = sqrCombatAttackStartMinRange <= sqrDistance && sqrDistance <= sqrCombatAttackStartMaxRange;
		isTargetInLimitAttackRange = sqrCombatAttackLimitMinRange <= sqrDistance && sqrDistance <= sqrCombatAttackLimitMaxRange;
		isTargetInActionRange = sqrDistance <= sqrCombatActionRange;

		OnChangeCurrentCombatTarget?.Invoke(currentCombatTarget);
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

		// 색 정의 (원하시면 변경)
		Color attackStartColor = new Color(1f, 0.2f, 0.2f, 0.9f); // 빨강 계열 (점선)
		Color attackLimitColor = new Color(1f, 0.6f, 0.1f, 0.9f); // 주황 계열 (실선)
		Color actionColor = new Color(0.2f, 1f, 0.3f, 0.9f);       // 녹색 (실선)
		Color visionColor = new Color(0.2f, 0.8f, 1f, 0.9f);       // 시안 (실선)

		const int segments = 128;

		// combatAttackStartRange : Vector2(minRadius, maxRadius) -> 점선으로 그리기 (둘 다)
		if (combatAttackStartRange.x > 0f)
			DrawCircleXZ(center, combatAttackStartRange.x, attackStartColor, dotted: true, segments: segments);
		if (combatAttackStartRange.y > 0f)
			DrawCircleXZ(center, combatAttackStartRange.y, attackStartColor, dotted: true, segments: segments);

		// combatAttackLimitRange : Vector2(minRadius, maxRadius) -> 실선으로 그리기 (둘 다)
		if (combatAttackLimitRange.x > 0f)
			DrawCircleXZ(center, combatAttackLimitRange.x, attackLimitColor, dotted: false, segments: segments);
		if (combatAttackLimitRange.y > 0f)
			DrawCircleXZ(center, combatAttackLimitRange.y, attackLimitColor, dotted: false, segments: segments);

		// action range (실선)
		float combatActionRange = ThisCombatStats.ActionRange;
		if (combatActionRange > 0f)
			DrawCircleXZ(center, combatActionRange, actionColor, dotted: false, segments: segments);

		// vision range (실선)
		float combatVisionRange = ThisCombatStats.VisionRange;
		if (combatVisionRange > 0f)
			DrawCircleXZ(center, combatVisionRange, visionColor, dotted: false, segments: segments);
	}

	// XZ 평면에 원을 그림. dotted = true 면 점선(세그먼트/갭) 형태로 그린다.
	void DrawCircleXZ(Vector3 center, float radius, Color color, bool dotted = false, int segments = 64)
	{
		if (radius <= 0f || segments < 8) return;

		Gizmos.color = color;
		int dashSegments = segments/24;       // 점선에서 그릴 연속 세그먼트 수
		int gapSegments = segments/32;        // 점선에서 건너뛸 세그먼트 수

		Vector3 prev = Vector3.zero;
		Vector3 first = Vector3.zero;
		bool hasPrev = false;

		for (int i = 0 ; i <= segments ; i++)
		{
			float t = (float)i / segments;
			float ang = t * Mathf.PI * 2f;
			Vector3 p = new Vector3(Mathf.Cos(ang) * radius, 0f, Mathf.Sin(ang) * radius) + center;
			// y는 center.y 로 이미 설정됨

			if (!hasPrev)
			{
				first = p;
				prev = p;
				hasPrev = true;
				continue;
			}

			if (dotted)
			{
				// 점선 패턴: segments을 작은 조각으로 나누어 일부만 그림
				int patternIndex = i % (dashSegments + gapSegments);
				if (patternIndex < dashSegments)
				{
					Gizmos.DrawLine(prev, p);
				}
			}
			else
			{
				Gizmos.DrawLine(prev, p);
			}

			prev = p;
		}

		// 닫힌 루프 보장 (끝-시작 연결)
		if (!dotted)
		{
			Gizmos.DrawLine(prev, first);
		}
		else
		{
			// 점선일 경우에도 마지막 구간이 그려져야 하면 이미 처리됐음.
		}
	}
}
#endif