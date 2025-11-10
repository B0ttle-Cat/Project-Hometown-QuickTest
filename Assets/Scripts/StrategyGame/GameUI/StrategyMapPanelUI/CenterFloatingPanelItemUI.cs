using System.Collections.Generic;
using System.Linq;

using Sirenix.OdinInspector;

using UnityEngine;

public class CenterFloatingPanelItemUI : FloatingPanelItemUI
{
	[SerializeField]
	private Vector2 pivot;
	[SerializeField]
	private Vector2 offset;
	public Vector2 Pivot => pivot;
	public Vector2 Offset => offset;
	protected override Transform MapTarget => targetsGroup == null ? null : targetsGroup.FirstOrDefault();

	[ShowInInspector,ReadOnly]
	public HashSet<Transform> targetsGroup;

	protected override void Reset()
	{
		base.Reset();
		pivot = new Vector2(0.5f, 1f);
		offset = Vector3.zero;
	}

	protected override void Awake()
	{
		base.Awake();
		targetsGroup = new HashSet<Transform>();
	}
	public void SetTargetInMap(params Transform[] mapTargets)
	{
		foreach (var item in mapTargets)
		{
			SetTargetInMap(item);
		}
	}
	public override void SetTargetInMap(Transform mapTarget = null)
	{
		rectTransform = rectTransform == null ? GetComponent<RectTransform>() : rectTransform;

		if (targetsGroup.Add(mapTarget))
		{
			InitTarget(mapTarget);
		}
	}
	public override void RemoveTargetInMap(Transform mapTarget = null)
	{
		if (mapTarget == null)
		{
			foreach (var item in targetsGroup)
			{
				if (item != null) ReleaseTarget(item);
			}
		}
		else if (targetsGroup.Remove(mapTarget))
		{
			ReleaseTarget(mapTarget);
		}
	}

	public Vector3 CenterPosition
	{
		get
		{
			Vector3 center = Vector3.zero;
			int count = 0;
			foreach (var item in targetsGroup)
			{
				center += item.position;
				count++;
			}
			if (count < 2) return center;
			else return center /= count;
		}
	}
	protected override void OnUpdate()
	{
		Camera camera = StrategyManager.MainCamera;
		if (camera == null) return;

		rectTransform.pivot = Pivot;
		Vector2 screenMapTarget = camera.WorldToScreenPoint(MapTarget.transform.position);
		rectTransform.position = screenMapTarget + Offset;
	}
}
