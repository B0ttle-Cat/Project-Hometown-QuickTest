using UnityEngine;

public interface IVisionRangeSearcher : INearbySearcher
{

}
public class VisionRangeSearching : NearbySearching
{
	protected override void OnDrawGizmos()
	{
		debugColor = Color.greenYellow;
		base.OnDrawGizmos();
	}
}
