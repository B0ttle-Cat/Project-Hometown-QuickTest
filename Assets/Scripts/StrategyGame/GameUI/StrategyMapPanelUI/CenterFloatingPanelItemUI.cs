using System.Collections.Generic;
using System.Linq;

using Sirenix.OdinInspector;

using UnityEngine;

public class CenterFloatingPanelItemUI : FloatingPanelItemUI
{
	public enum CenterMode
	{
		MinMax = 0,
		Aaverage,
		Avoid_MinMax,
		//Avoid_Aaverage,
	}

	[SerializeField] private CenterMode mode;
	[SerializeField] private Vector2 pivot;
	[SerializeField] private Vector2 offset;

	public Vector2 Pivot => pivot;
	public Vector2 Offset => offset;
	protected override Transform MapTarget => targetsGroup == null ? null : targetsGroup.FirstOrDefault();

	[ShowInInspector, ReadOnly]
	public HashSet<Transform> targetsGroup;

	protected override void Reset()
	{
		base.Reset();
		pivot = new Vector2(0.5f, 1f);
		offset = Vector2.zero;
	}

	protected override void Awake()
	{
		base.Awake();
		targetsGroup = new HashSet<Transform>();
	}

	public void SetTargetInMap(Transform[] otherTargets)
	{
		if (otherTargets == null || otherTargets.Length == 0) return;
		foreach (var item in otherTargets)
			SetTargetInMap(item);
	}

	public override void SetTargetInMap(Transform mapTarget = null)
	{
		if (mapTarget != null && targetsGroup.Add(mapTarget))
			InitTarget(mapTarget);
	}

	public void RemoveTargetInMap(Transform[] otherTargets)
	{
		if (otherTargets == null || otherTargets.Length == 0) return;
		foreach (var item in otherTargets)
			RemoveTargetInMap(item);
	}

	public override void RemoveTargetInMap(Transform mapTarget = null)
	{
		if (mapTarget == null)
		{
			foreach (var item in targetsGroup)
			{
				if (item != null) ReleaseTarget(item);
			}
			targetsGroup.Clear();
		}
		else if (targetsGroup.Remove(mapTarget))
		{
			ReleaseTarget(mapTarget);
		}
	}

	private (Rect screenRect, Vector2 aaverage, int count) GetScreenRectAndAverage(Camera camera)
	{
		if (targetsGroup == null || targetsGroup.Count == 0)
			return (default, Vector2.zero, 0);

		Vector2 sum = Vector2.zero;
		int count = 0;
		Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
		Vector2 max = new Vector2(float.MinValue, float.MinValue);

		foreach (var item in targetsGroup)
		{
			if (!item.gameObject.activeInHierarchy) continue;

			Vector3 screen = camera.WorldToScreenPoint(item.position);
			if (screen.z < 0) continue; // 카메라 뒤쪽이면 무시

			Vector2 s = new Vector2(screen.x, screen.y);
			sum += s;

			min = Vector2.Min(min, s);
			max = Vector2.Max(max, s);

			count++;
		}

		Vector2 avg = count > 0 ? sum / count : Vector2.zero;
		Rect rect = new Rect(min, max - min);

		return (rect, avg, count);
	}


protected override void OnUpdate()
{
	Camera camera = StrategyManager.MainCamera;
	if (camera == null) return;

	rectTransform.pivot = Pivot;

	Vector2 screenPosition = mode switch
	{
		CenterMode.MinMax        => GetScreenRectCenter(camera, useAverage: false),
		CenterMode.Aaverage      => GetScreenRectCenter(camera, useAverage: true),
		CenterMode.Avoid_MinMax  => GetAvoidScreenPosition(camera, useAverage: false),
		//CenterMode.Avoid_Aaverage=> GetAvoidScreenPosition(camera, useAverage: true),
		_ => Vector2.zero
	};

	rectTransform.position = screenPosition + Offset;
}

	private Vector2 GetScreenRectCenter(Camera camera, bool useAverage)
	{
		var (rect, avg, count) = GetScreenRectAndAverage(camera);
		if (count == 0) return Vector2.zero;

		return useAverage ? avg : rect.center;
	}

	private Vector2 GetAvoidScreenPosition(Camera camera, bool useAverage)
	{
		var (rect, avg, count) = GetScreenRectAndAverage(camera);
		if (count == 0) return Vector2.zero;

		Vector2 center = useAverage ? avg : rect.center;

		// OffsetFloatingPanelItemUI와 동일한 계산
		float halfWidth = rect.width * 0.5f;
		float halfHeight = rect.height * 0.5f;
		Vector2 pivotNormal = (Pivot - Vector2.one * 0.5f) * 2f;

		Vector2 screenPosition = center + (-pivotNormal * new Vector2(halfWidth, halfHeight));
		return screenPosition;
	}

}
