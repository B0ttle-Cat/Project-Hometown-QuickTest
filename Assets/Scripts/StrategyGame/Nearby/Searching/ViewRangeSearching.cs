using Sirenix.OdinInspector;

using UnityEngine;

public interface IViewRangeSearcher : INearbySearcher
{

}
public class ViewRangeSearching : NearbySearching
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

		UnityEditor.Handles.color = new Color(0.2f, 0.8f, 1f, 0.9f);
		UnityEditor.Handles.DrawWireDisc(center, Vector3.up, range, debugThickness);
	}
#endif
}
