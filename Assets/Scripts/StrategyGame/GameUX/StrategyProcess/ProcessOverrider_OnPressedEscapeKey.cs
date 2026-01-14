using System;

namespace StrategyManagerModule
{
    public record ProcessOverrider_OnPressedEscapeKey : ProcessOverriderAction
	{
		public override IStrategyProcess OriginalProcess => StrategyEscapeProcess.EscapeProcess;
		public ProcessOverrider_OnPressedEscapeKey(Action action) : base(action)
        {
        }
    }

}