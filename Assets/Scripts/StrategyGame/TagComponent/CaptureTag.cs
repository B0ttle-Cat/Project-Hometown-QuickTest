using UnityEngine;

public class CaptureTag : MonoBehaviour , IStrategyStartGame
{
	[Header("Info")]
	public int factionID;
	public int pointValue;

	private bool isAdd = false;

    void IStrategyStartGame.OnStartGame()
	{
		isAdd = true;
		StrategyManager.Collector.Add(this);
	}

    void IStrategyStartGame.OnStopGame()
	{
		if (isAdd)
		{
			isAdd = false;
			StrategyManager.Collector.Remove(this);
		}
	}

    public void OnDestroy()
    {
		if (isAdd)
		{
			isAdd = false;
			StrategyManager.Collector.Remove(this);
		}
	}
}
