using Sirenix.OdinInspector;

using UnityEngine;

public interface IActionRangeSearcher : INearbySearcher
{

}

public class ActionRangeSearching : NearbySearching
{
#if UNITY_EDITOR
	[ShowInInspector]
	public float debugThickness { get; set; } = 2;
	[ShowInInspector]
	public Color debugColor { get; set; } = new Color(0.2f, 1f, 0.3f, 0.9f);
	protected override void OnDrawGizmos()
	{
		if (thisSearcher.IsNullRef()) return;
		float range = SearchRange;
		float minRange = SearchMinRange;
		Vector3 center = SearchCenter;


		UnityEditor.Handles.color = debugColor;
		UnityEditor.Handles.DrawWireDisc(center, Vector3.up, range, debugThickness);
		if(minRange >= CutMinRange)
		{
			UnityEditor.Handles.DrawWireDisc(center, Vector3.up, minRange, debugThickness);
		}
	}
#endif
}
