using System;

namespace StrategyManagerModule
{
    public record ProcessOverrider_OnPressedEscapeKey : ProcessOverriderAction
	{
		public override IStrategyProcess OriginalProcess => StrategyManager.GameUX.EscapeProcess;
		public ProcessOverrider_OnPressedEscapeKey(Action action) : base(action)
        {
        }
    }

}