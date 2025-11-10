using System.Collections.Generic;
using System.Linq;

using Sirenix.OdinInspector;

using UnityEngine;

public class CenterFloatingPanelItemUI : FloatingPanelItemUI
{
	protected override Transform MapTarget => targetsGroup == null ? null : targetsGroup.FirstOrDefault();

	[ShowInInspector,ReadOnly]
	public HashSet<Transform> targetsGroup;
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
		if(mapTarget == null)
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
}
