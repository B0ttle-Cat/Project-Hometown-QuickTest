using UnityEngine;

public interface IAttackStartRangeSearcher : INearbySearcher
{

}
public class AttackStartRangeSearching : NearbySearching
{
	protected override void OnDrawGizmos()
	{
		debugColor = Color.orange;
		base.OnDrawGizmos();
	}

}
