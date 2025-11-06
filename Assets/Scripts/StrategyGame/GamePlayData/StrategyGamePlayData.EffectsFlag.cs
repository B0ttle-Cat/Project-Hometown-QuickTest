using System;

public partial class StrategyGamePlayData // EffectsFlag
{
    [Flags]
	public enum EffectsFlag : int
	{
		None        = 0,
		감전        = 1 << 1,
		공포        = 1 << 2,
		방전        = 1 << 3,
		빙결        = 1 << 4,
		수면        = 1 << 5,
		실명        = 1 << 6,
		침수        = 1 << 7,
		혼란        = 1 << 8,
		화재        = 1 << 9,
		FX_10       = 1 << 10,
		FX_11       = 1 << 11,
		FX_12       = 1 << 12,
		FX_13       = 1 << 13,
		FX_14       = 1 << 14,
		FX_15       = 1 << 15,
		FX_16       = 1 << 16,
		FX_17       = 1 << 17,
		FX_18       = 1 << 18,
		FX_19       = 1 << 19,
		FX_20       = 1 << 20,
		FX_21       = 1 << 21,
		FX_22       = 1 << 22,
		FX_23       = 1 << 23,
		FX_24       = 1 << 24,
		FX_25       = 1 << 25,
		FX_26       = 1 << 26,
		FX_27       = 1 << 27,
		FX_28       = 1 << 28,
		FX_29       = 1 << 29,
		FX_30       = 1 << 30,
		FX_31       = 1 << 31,
	}

}
