using UnityEngine;


public interface IHitableCombatant : IStrategyElementDestroyer
{
	public Transform transform { get; }
	public Collider HitCollider { get; }
}

public interface ITargetableCombatant : IHitableCombatant
{
	// IStrategyElement 가 선언된 class 에서만 사용 할수 있도록 강제 하도록 위함
	public int FactionID { get; }
	public Vector3 Position { get; }
	public Vector3 HitTargetPosition { get; }
}


public interface INearbyElement	: ITargetableCombatant
{
	// INearbyElement 에 추가로 필요한 값
	public float Radius { get; }
}