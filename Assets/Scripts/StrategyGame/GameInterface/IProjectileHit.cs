
using UnityEngine;

public interface IProjectileHit
{
	IProjectileHit ThisProjectileHit { get; }

	public const int MIN_ARRAY_CAPACITY = 16;
	void ProjectileOverlap(out int overlapCount, ref Collider[] overlaps);
	void ProjectileMoveCast(out int hitCount, ref RaycastHit[] raycastHits);
	public void HitReporting(Collider hit)
	{
		var hitObject = hit.gameObject;
		IStrategyElement strategyElement = hitObject.GetComponentInParent<IStrategyElement>();
		if (strategyElement != null)
		{
			switch (strategyElement)
			{
				case IHitableCombatant hitable:
				Hitable(hitable);
				return;
				default:
				HitOtherElement(strategyElement);
				return;
			}
		}
		HitOtherObject(hitObject);
	}
	public void HitOtherObject(GameObject gameObject);
	public void Hitable(IHitableCombatant hitable);
	public void HitOtherElement(IStrategyElement hit);
    void SendHitReporting(in int overlapCount, in Collider[] overlaps, in int hitCount, in RaycastHit[] raycastHits);
}