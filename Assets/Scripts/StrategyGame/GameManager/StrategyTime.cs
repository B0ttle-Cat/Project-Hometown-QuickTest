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
		if (!isActiveAndEnabled) return;
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
	public class GmaePlayTimer	: IDisposable
	{
        private double endedTime;
        private Action callback;

		public double EndedTime => endedTime;
		public bool HasTime => EndedTime > 0;
		public bool IsEnd => StrategyManager.Time.gamePlayTime > EndedTime;
		public float duration => (float)(EndedTime - StrategyManager.Time.gamePlayTime);

        public GmaePlayTimer(Action timerCallback)
		{
			if (timerCallback == null) return;
			callback += timerCallback;
		}
		public void SetEndedTime(double endedTime)
		{
			this.endedTime = endedTime;
		}
		public void SetDuration(float duration)
		{
			endedTime = StrategyManager.Time.gamePlayTime + duration;
		}
		public void TimeUpdate()
		{
			if (callback != null && IsEnd)
			{
				callback.Invoke();
				callback = null;
			}
		}
		public void Dispose()
		{
			endedTime = 0;
			callback = null;
		}
	}
}
