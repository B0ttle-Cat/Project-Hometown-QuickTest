using UnityEngine;

public class StrategyGameUX : MonoBehaviour, IStrategyStartGame
{
	IStrategyProcess[] strategyProcesses;
	public void Awake()
	{
		strategyProcesses = GetComponentsInChildren<IStrategyProcess>(true);
	}
	void IStrategyStartGame.OnStartGame()
	{
		if(strategyProcesses == null)
		{
			return;
		}
		foreach (var process in strategyProcesses)
		{
			process.OnStart();
		}
	}

	void IStrategyStartGame.OnStopGame()
	{
		if (strategyProcesses == null)
		{
			return;
		}
		foreach (var process in strategyProcesses)
		{
			process.OnStop();
		}
	}
}
