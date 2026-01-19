using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

namespace StrategyManagerModule
{
	public partial class StrategyGameUX
	{
		public bool OnKey_ShowDetail;
	}
	public class StrategyShowDetailProcess : MonoBehaviour, IStrategyProcess
	{
		internal static StrategyShowDetailProcess ShowDetailProcess;
		public IStrategyProcess ThisProcess => this;
		public List<ProcessOverrider> OverriderList { get; } = new List<ProcessOverrider>();

		void IStrategyProcess.OnInit() { 
			ShowDetailProcess = this;
		}
		void IStrategyProcess.OnStart()
		{
			StrategyManager.GameUX.OnKey_ShowDetail = false;
		}
		void IStrategyProcess.OnStop() { }
		void IStrategyProcess.Update()
		{
			if (ThisProcess.TryGetProcessOverrider<ProcessOverrider_OnShowDetailKey>(out var hack))
			{
				hack.InvokeOverrider();
				return;
			}

			StrategyManager.GameUX.OnKey_ShowDetail = Keyboard.current.altKey.isPressed;
		}
	}

	public record ProcessOverrider_OnShowDetailKey : ProcessOverriderFunc<bool>
	{
		public override IStrategyProcess OriginalProcess => StrategyShowDetailProcess.ShowDetailProcess;
		public ProcessOverrider_OnShowDetailKey(Func<bool> action, Action<bool> result) : base(action, result)
		{
		}
	}
}