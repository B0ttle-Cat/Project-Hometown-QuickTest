using UnityEngine;


public interface INearbyElement : IStrategyElement
{
	public int FactionID { get; }
	public Vector3 Position { get; }
	public float Radius { get; }
}