using static StrategyGamePlayData;

public readonly struct SpawnTroopsInfo
{
	public readonly int factionID;
	public readonly (UnitKey key, int count)[] organizations;
	public SpawnTroopsInfo(int factionID, params (UnitKey, int)[] organizations)
	{
		this.factionID = factionID;
		this.organizations = organizations;
	}
}
