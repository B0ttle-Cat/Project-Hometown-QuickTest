using UnityEngine;

public class CaptureTag : MonoBehaviour
{
	[Header("Info")]
	[SerializeField]
	private int factionID = -1;
	[SerializeField]
	private int pointValue = 0;

	public int FactionID => factionID;
	public int PointValue => pointValue;

    public void Init(int factionID, int pointValue)
	{
		this.factionID = factionID;
		this.pointValue = pointValue;
		StrategyManager.Collector.Add(this);

	}
	public void Deinit()
	{
		this.factionID = -1;
		this.pointValue = 0;
		StrategyManager.Collector.Remove(this);
	}
}
