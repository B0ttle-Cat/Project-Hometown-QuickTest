using UnityEngine;

public class CaptureTag : MonoBehaviour , IStrategyStartGame
{
	[Header("Info")]
	public int factionID;
	public int pointValue;

    void IStrategyStartGame.OnStartGame()
	{
		StrategyManager.Collector.Add(this);
	}

    void IStrategyStartGame.OnStopGame()
	{
		StrategyManager.Collector.Remove(this);
	}
}
