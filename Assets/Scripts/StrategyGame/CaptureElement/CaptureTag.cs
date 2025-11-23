using UnityEngine;

public class CaptureTag : MonoBehaviour
{
	[Header("Info")]
	public int factionID;
	public int pointValue;

    private async void Awake()
    {
		while(!StrategyManager.IsReadyManager)
		{
			await Awaitable.NextFrameAsync();
		}
		StrategyManager.Collector.AddOther(this);
	}
    private void OnDestroy()
    {
		StrategyManager.Collector.RemoveOther(this);
	}
}
