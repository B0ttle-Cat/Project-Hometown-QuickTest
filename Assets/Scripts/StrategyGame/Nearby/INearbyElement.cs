using UnityEngine;

public interface ITargetableCombatant
{
	// IStrategyElement 가 선언된 class 에서만 사용 할수 있도록 강제 하도록 위함
	public IStrategyElement ThisElement { get; }
	public int FactionID { get; }
	public Vector3 Position { get; }
}


public interface INearbyElement	: ITargetableCombatant
{
	// INearbyElement 에 추가로 필요한 값
	public float Radius { get; }
}