using UnityEngine;

public class CaptureTag : MonoBehaviour
{
	[Header("Info")]
	public int factionID;
	public int pointValue;

    private async void Awake()
    {
		while(StrategyManager.IsNotReadyManager)
		{
			await Awaitable.NextFrameAsync();
			if (destroyCancellationToken.IsCancellationRequested) return;
		}
		StrategyManager.Collector.AddOther(this);
	}
    private void OnDestroy()
    {
		StrategyManager.Collector.RemoveOther(this);
	}
}
