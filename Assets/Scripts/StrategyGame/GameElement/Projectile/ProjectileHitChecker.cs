using UnityEngine;

public class ProjectileHitChecker : MonoBehaviour
{
	public void HitCheck()
	{
		Physics.SphereCastAll(transform.position, 0.5f, Vector3.forward);	
	}
}
