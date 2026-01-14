using System.Collections.Generic;

using UnityEngine;

namespace StrategyManagerModule
{
	[DefaultExecutionOrder(-1)]
	public partial class StrategyGameUX : MonoBehaviour, IStrategyStartGame
	{
		HashSet<IStrategyProcess> hashSetProcess;
		HashSet<IStrategyProcess> ProcessList => hashSetProcess ??= new HashSet<IStrategyProcess>();

		private void Awake()
		{
			hashSetProcess ??= new HashSet<IStrategyProcess>();
			IStrategyProcess[] strategyProcess =  GetComponentsInChildren<IStrategyProcess>(true);
			int length = strategyProcess.Length;
			for (int i = 0 ; i < length ; i++)
			{
				var process = strategyProcess[i];
				process.OnInit();
				hashSetProcess.Add(process);
			}
		}
		private void OnDestroy()
		{
			if (hashSetProcess == null) return;
			hashSetProcess.Clear();
			hashSetProcess = null;
		}
		void IStrategyStartGame.OnStartGame()
		{
			if (hashSetProcess == null) return;
			foreach (var process in hashSetProcess)
			{
				process.OnStart();
			}
		}
		void IStrategyStartGame.OnStopGame()
		{
			if (hashSetProcess == null) return;
			foreach (var process in hashSetProcess)
			{
				process.OnStop();
			}
			hashSetProcess.Clear();
			hashSetProcess = null;
		}
		public void UXUpdate()
		{
			if (hashSetProcess == null || hashSetProcess.Count == 0) return;
			foreach (var item in ProcessList)
			{
				item.Update();
			}
		}
	}
}