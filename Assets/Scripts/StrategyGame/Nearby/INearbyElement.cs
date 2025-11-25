using UnityEngine;

public interface INearbyElement
{
	// IStrategyElement 가 선언된 class 에서만 사용 할수 있도록 강제 하도록 위함
	public IStrategyElement ThisElement { get; }
	public int FactionID { get; }

	// INearbyElement 에 필요한 값
	public float Radius { get; }
	public Vector3 Position { get; }
}