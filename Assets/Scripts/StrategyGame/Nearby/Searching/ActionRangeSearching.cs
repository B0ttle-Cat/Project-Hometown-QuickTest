using Sirenix.OdinInspector;

using UnityEngine;

public interface IActionRangeSearcher : INearbySearcher
{

}

public class ActionRangeSearching : NearbySearching
{
#if UNITY_EDITOR
	[ShowInInspector]
	private float debugThickness = 5;
	protected override void OnDrawGizmos()
	{
		if (thisSearcher.IsNullRef()) return;
		float range = SearchRange;
		if (range <= 0) return;
		Vector3 center = SearchCenter;

		UnityEditor.Handles.color = new Color(0.2f, 1f, 0.3f, 0.9f);       // 녹색 (실선);
		UnityEditor.Handles.DrawWireDisc(center, Vector3.up, range, debugThickness);
	}
#endif
}
