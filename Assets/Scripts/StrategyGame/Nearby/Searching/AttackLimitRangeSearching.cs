using Sirenix.OdinInspector;

using UnityEngine;

public interface IAttackLimitRangeSearcher : INearbySearcher
{

}
public class AttackLimitRangeSearching : NearbySearching
{
#if UNITY_EDITOR
	[ShowInInspector]
	public float debugThickness { get; set; } = 2;
	public Color debugColor { get; set; } = new Color(1f, 0.6f, 0.1f, 0.9f);
	protected override void OnDrawGizmos()
	{
		if (thisSearcher.IsNullRef()) return;
		float range = SearchRange;
		if (range <= 0) return;
		Vector3 center = SearchCenter;

		UnityEditor.Handles.color = debugColor;
		UnityEditor.Handles.DrawWireDisc(center, Vector3.up, range, debugThickness);
	}
#endif
}
