using System.Collections.Generic;

using UnityEngine.InputSystem;

namespace StrategyManagerModule
{
	public partial class StrategyEscapeProcess : IStrategyProcess
	{
		public IStrategyProcess ThisProcess => this;
		public List<ProcessOverrider> OverriderList { get; set; } = new List<ProcessOverrider>();

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

    public partial class StrategyGameUX
	{
		private IStrategyProcess escapeProcess;
		public IStrategyProcess EscapeProcess
		{
			get
			{
				if (escapeProcess.IsNullRef())
				{
					escapeProcess = new StrategyEscapeProcess();
					ProcessList.Add(escapeProcess);
				}
				return escapeProcess;
			}
			private set
			{
				pointingProcess = value;
			}
		}



	}

}