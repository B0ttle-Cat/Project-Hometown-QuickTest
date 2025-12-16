using UnityEngine;

public interface IHitableCombatant : IStrategyElementDestroyer
{
	public Transform transform { get; }
	public Collider HitCollider { get; }
}
