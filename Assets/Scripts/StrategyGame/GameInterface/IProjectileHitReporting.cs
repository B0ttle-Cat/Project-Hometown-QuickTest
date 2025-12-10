
using UnityEngine;

public interface IProjectileHitReporting
{
	public void HitReporting(Collider hit)
	{
		var hitObject = hit.gameObject;
		IStrategyElement strategyElement = hitObject.GetComponentInParent<IStrategyElement>();
		if (strategyElement != null)
		{
			switch (strategyElement)
			{
				case ITargetableCombatant targetable:
				HitTargetable(targetable);
				return;
				default:
				HitOtherElement(strategyElement);
				return;
			}
		}
		HitOtherObject(hitObject);
	}
	public void HitOtherObject(GameObject gameObject);
	public void HitTargetable(ITargetableCombatant targetable);
	public void HitOtherElement(IStrategyElement hit);
}