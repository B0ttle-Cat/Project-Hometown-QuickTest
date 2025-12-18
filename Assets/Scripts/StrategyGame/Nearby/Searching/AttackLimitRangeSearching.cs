using UnityEngine;

public interface IAttackLimitRangeSearcher : INearbySearcher
{

}
public class AttackLimitRangeSearching : NearbySearching
{
	protected override void OnDrawGizmos()
	{
		debugColor = Color.orangeRed;
		base.OnDrawGizmos();
	}
}
