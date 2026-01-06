using UnityEngine;

namespace StrategyManagerModule
{
	public partial class StrategyGameUX : MonoBehaviour, IStrategyStartGame
	{
		void IStrategyStartGame.OnStartGame()
		{
			var strategyProcesses = GetComponentsInChildren<IStrategyProcess>(true);
			if (strategyProcesses == null)
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
			var strategyProcesses = GetComponentsInChildren<IStrategyProcess>(true);
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

}