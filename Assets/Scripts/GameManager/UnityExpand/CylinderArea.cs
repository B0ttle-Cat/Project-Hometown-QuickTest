using Sirenix.OdinInspector;

using UnityEngine;

public class CylinderArea : MonoBehaviour
{
	public enum AxisDirection { X, Y, Z }

	[Header("Cylinder Settings")]
	public float radius = 1f;
	public float height = 2f;
	[Range(0f,1f)]
	public float heightPivot = 0.5f;                        // axisDirection 방향 Pivot
	public AxisDirection axisDirection = AxisDirection.Y;   // 기준 방향

	Vector3 Axis
	{
		get
		{
			return axisDirection switch
			{
				AxisDirection.X => transform.right,
				AxisDirection.Y => transform.up,
				AxisDirection.Z => transform.forward,
				_ => transform.up,
			};
		}
	}
	Vector3 Center => transform.TransformPoint((heightPivot - 0.5f) * height * Vector3.down);
	float ScaledHeight => Mathf.Abs(height) * GetAxisScale();
	float ScaledRadius => Mathf.Abs(radius) * GetRadiusScale();

	float GetAxisScale()
	{
		switch (axisDirection)
		{
			case AxisDirection.X: return Mathf.Abs(transform.lossyScale.x);
			case AxisDirection.Y: return Mathf.Abs(transform.lossyScale.y);
			case AxisDirection.Z: return Mathf.Abs(transform.lossyScale.z);
		}
		return 1f;
	}
	float GetRadiusScale()
	{
		var s = transform.lossyScale;
		switch (axisDirection)
		{
			case AxisDirection.X: return Mathf.Max(Mathf.Abs(s.y), Mathf.Abs(s.z));
			case AxisDirection.Y: return Mathf.Max(Mathf.Abs(s.x), Mathf.Abs(s.z));
			case AxisDirection.Z: return Mathf.Max(Mathf.Abs(s.x), Mathf.Abs(s.y));
		}
		return 1f;
	}
    #region IsOverlap

    /// <summary>
    /// 점이 CylinderArea 안에 있는지 확인
    /// </summary>
    public bool IsOverlap(Vector3 point)
	{
		Vector3 localPoint = point - Center;
		Vector3 axis = Axis.normalized;
		float halfHeight = ScaledHeight * 0.5f;

		// 축 방향 거리
		float d = Vector3.Dot(localPoint, axis);
		if (d < -halfHeight || d > halfHeight) return false;

		// 축 평면상의 거리
		Vector3 radial = localPoint - axis * d;
		return radial.sqrMagnitude <= ScaledRadius * ScaledRadius;
	}

	/// <summary>
	/// Collider와 겹치는지 확인.
	/// 캡슐과 Box에 둘다 동시에 충돌하는 부분을 원통으로 고려한다.
	/// </summary>
	public int GetOverlapCollider(Collider[] colliders, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		if (colliders.Length == 0) return 0;

		Vector3 center = Center;
		float height = ScaledHeight;
		float radius = ScaledRadius;

		Vector3 axis = Axis.normalized;
		float halfHeight = height * 0.5f;
		Vector3 top = center + axis * halfHeight;
		Vector3 bottom = center - axis * halfHeight;
		int count = Physics.OverlapCapsuleNonAlloc(top, bottom, radius, colliders, layerMask, queryTriggerInteraction);

		if (count == 0) return count;

		Vector3 halfExtents = axisDirection switch
		{
			AxisDirection.X => new Vector3(halfHeight, radius, radius),
			AxisDirection.Y => new Vector3(radius, halfHeight, radius),
			AxisDirection.Z => new Vector3(radius, radius, halfHeight),
			_ => new Vector3(radius, halfHeight, radius)
		};
		Quaternion rot = transform.rotation;
		count = Physics.OverlapBoxNonAlloc(center, halfExtents, colliders, rot, layerMask, queryTriggerInteraction);
		
		return count;
	}
	#endregion


#if UNITY_EDITOR
	[SerializeField, HorizontalGroup, LabelWidth(100)]
	private Color gizmoColor = Color.cyan;
	[SerializeField, ShowIf("@checkOverlapCollider"), HorizontalGroup(width: 0.25f), HideLabel]
	private Color overlapGizmoColor = Color.red;
	[SerializeField, ShowIf("@checkOverlapCollider"), HorizontalGroup(width: 0.25f), HideLabel]
	private Color highlightGizmoColor = Color.yellow;
	[SerializeField]
	private bool checkOverlapCollider;

	Color TestCollider(out Collider[] overlappingColliders)
	{
		overlappingColliders = new Collider[32];

		if (!checkOverlapCollider) return gizmoColor;
		bool isOverlapping = false;

		// 1) 주변 Collider 검사
		int count = Physics.OverlapSphereNonAlloc(Center, Mathf.Max(ScaledRadius, ScaledHeight) * 2f, overlappingColliders);
		if (count > 0)
		{
			int overlapCount = GetOverlapCollider(overlappingColliders, -1);
			if (overlapCount > 0)
			{
				int overlappingCollidersLength = overlappingColliders.Length;

				for (int i = overlapCount ; i < overlappingCollidersLength ; i++)
				{
					overlappingColliders[i] = null;
				}
				isOverlapping = true;
			}
		}

		return isOverlapping ? overlapGizmoColor : gizmoColor;
	}

	void OnDrawGizmos()
	{
		Collider[] colliders;
		Gizmos.color = TestCollider(out colliders);

		var center = Center;
		var axis = Axis.normalized;
		var h = ScaledHeight * 0.5f;
		var r = ScaledRadius;

		var p1 = center + axis * h;
		var p2 = center - axis * h;

		DrawCircle(p1, axis, r);
		DrawCircle(p2, axis, r);

		Vector3 dir = Vector3.Cross(axis, Vector3.up);
		if (dir.sqrMagnitude < 0.001f)
			dir = Vector3.Cross(axis, Vector3.right);
		dir.Normalize();

		var dir2 = Vector3.Cross(axis, dir).normalized;
		Gizmos.DrawLine(p1 + dir * r, p2 + dir * r);
		Gizmos.DrawLine(p1 - dir * r, p2 - dir * r);
		Gizmos.DrawLine(p1 + dir2 * r, p2 + dir2 * r);
		Gizmos.DrawLine(p1 - dir2 * r, p2 - dir2 * r);

		// 3) 겹친 콜라이더 노랑 표시
		Gizmos.color = highlightGizmoColor;
		foreach (var col in colliders)
		{
			if (col == null) continue;
			Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
		}
	}

	void DrawCylinderGizmo(CylinderArea cyl, Color col)
	{
		Gizmos.color = col;
		var center = cyl.Center;
		var axis = cyl.Axis.normalized;
		var h = cyl.ScaledHeight * 0.5f;
		var r = cyl.ScaledRadius;

		var p1 = center + axis * h;
		var p2 = center - axis * h;

		DrawCircle(p1, axis, r);
		DrawCircle(p2, axis, r);

		Vector3 dir = Vector3.Cross(axis, Vector3.up);
		if (dir.sqrMagnitude < 0.001f)
			dir = Vector3.Cross(axis, Vector3.right);
		dir.Normalize();

		Vector3 dir2 = Vector3.Cross(axis, dir).normalized;
		Gizmos.DrawLine(p1 + dir * r, p2 + dir * r);
		Gizmos.DrawLine(p1 - dir * r, p2 - dir * r);
		Gizmos.DrawLine(p1 + dir2 * r, p2 + dir2 * r);
		Gizmos.DrawLine(p1 - dir2 * r, p2 - dir2 * r);
	}
	void DrawCircle(Vector3 center, Vector3 normal, float radius)
	{
		const int seg = 32;
		var axis = normal.normalized;

		Vector3 ref1 = Vector3.Cross(axis, Vector3.up);
		if (ref1.sqrMagnitude < 0.001f)
			ref1 = Vector3.Cross(axis, Vector3.right);
		ref1.Normalize();

		var ref2 = Vector3.Cross(axis, ref1).normalized;

		Vector3 prev = center + ref1 * radius;
		for (int i = 1 ; i <= seg ; i++)
		{
			float t = (float)i / seg * Mathf.PI * 2f;
			Vector3 next = center + (ref1 * Mathf.Cos(t) + ref2 * Mathf.Sin(t)) * radius;
			Gizmos.DrawLine(prev, next);
			prev = next;
		}
	}
#endif
}
