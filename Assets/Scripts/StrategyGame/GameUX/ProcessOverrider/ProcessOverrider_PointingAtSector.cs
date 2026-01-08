using System;

namespace StrategyManagerModule
{
    public record ProcessOverrider_PointingAtSector : ProcessOverriderAction<SectorObject>
    {
        public override IStrategyProcess OriginalProcess => StrategyManager.GameUX.PointingProcess;
        public ProcessOverrider_PointingAtSector(Action<SectorObject> action) : base(action)
        {
        }
	}
}