using System;

using UnityEngine;

public class StrategyTime : MonoBehaviour
{
	public double unscaledGamePlayTime;
	public double gamePlayTime;

	public void Awake()
	{
		unscaledGamePlayTime = 0f;
		gamePlayTime = 0f;
	}

	public void TimeUpdate()
	{
		unscaledGamePlayTime += Time.unscaledDeltaTime;
		gamePlayTime += Time.deltaTime;
	}

	internal void Init()
	{
		var data = StrategyManager.PreparedData.GetData();
		unscaledGamePlayTime = data.unscaleGamePlayTime;
		gamePlayTime = data.gamePlayTime;
	}

	[Serializable]
	public struct GmaePlayTimer	: IDisposable
	{
		public double endedTime;
		public bool HasTime => endedTime > 0;
		public bool IsEnd => StrategyManager.Time.gamePlayTime > endedTime;
		public float duration => (float)(endedTime - StrategyManager.Time.gamePlayTime);
		public GmaePlayTimer(double endedTime)
		{
			this.endedTime = endedTime;
		}
		public GmaePlayTimer(float duration)
		{
			endedTime = StrategyManager.Time.gamePlayTime + duration;
		}
		public void Dispose()
		{
			endedTime = 0;
		}
	}
}
