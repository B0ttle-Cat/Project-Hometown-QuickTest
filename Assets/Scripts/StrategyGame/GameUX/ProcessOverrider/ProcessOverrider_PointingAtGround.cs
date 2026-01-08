using System;

using UnityEngine;

namespace StrategyManagerModule
{
    public record ProcessOverrider_PointingAtGround : ProcessOverriderAction<Vector3>
	{
		public override IStrategyProcess OriginalProcess => StrategyManager.GameUX.PointingProcess;
		public ProcessOverrider_PointingAtGround(Action<Vector3> action) : base(action)
        {
        }
    }
}