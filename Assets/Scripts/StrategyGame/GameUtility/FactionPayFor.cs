using static StrategyGamePlayData;

public static class FactionPayFor
{
    private static bool IsAlive(this Faction faction)
    {
		return faction != null && faction.FactionID >= 0;
	}
	private static bool IsNotAlive(this Faction faction)
	{
		return !faction.IsAlive();
	}
	public static bool PayFor_UnitCounter(this Faction faction, int countingValue)
	{
		if (faction.IsNotAlive()) return false;

		var value = faction.FactionStats.GetValue(StatsType.세력_병력_현재보유량) + countingValue;
		faction.FactionStats.SetValue(value);

		return true;
	}
}