using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

namespace StrategyManagerModule
{
	public partial class StrategyEscapeProcess : MonoBehaviour, IStrategyProcess
	{
		internal static StrategyEscapeProcess EscapeProcess;
		public IStrategyProcess ThisProcess => this;
		public List<ProcessOverrider> OverriderList { get; set; } = new List<ProcessOverrider>();
		void IStrategyProcess.OnInit() 
		{
			EscapeProcess = this;
		}
		void IStrategyProcess.OnStart()
		{
		}

		void IStrategyProcess.OnStop()
		{
		}
		void IStrategyProcess.Update()
		{
			if (Keyboard.current.escapeKey.wasPressedThisFrame)
			{
				OnPressedEscapeKey();
			}
		}

		private void OnPressedEscapeKey()
		{
			if (ThisProcess.TryGetProcessOverrider<ProcessOverrider_OnPressedEscapeKey>(out var hack))
			{
				hack.InvokeOverrider();
				return;
			}

			StrategyManager.GameUI.OpenGameSystemMenu();
		}
	}

}