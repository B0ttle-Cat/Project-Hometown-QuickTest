using UnityEngine;

public interface ITargetableCombatant : IHitableCombatant , INearbyElement
{
	// IStrategyElement 가 선언된 class 에서만 사용 할수 있도록 강제 하도록 위함
	public Vector3 HitTargetPosition { get; }
}
