using UnityEngine;

public interface IActionRangeSearcher : INearbySearcher
{

}

public class ActionRangeSearching : NearbySearching
{
	protected override void OnDrawGizmos()
	{
		debugColor = Color.cyan;
		base.OnDrawGizmos();
	}
}
