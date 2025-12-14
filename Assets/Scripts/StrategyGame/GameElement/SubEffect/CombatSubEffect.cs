using static StrategyGamePlayData;

public abstract class CombatSubEffect : SubEffectObject, ICombatOffense
{
    ICombatOffense ICombatOffense.ThisOffense => this;
    public int FactionID { get; }
    public StrategyGamePlayData.IStatsValueControl StatsValue => this;

    int IStatsValueControl.GetStatsValue(StrategyGamePlayData.StatsType type)
    {
        return 0;
    }

    void IStatsValueControl.SetValueInMainStats(StrategyGamePlayData.StatsType type, int value)
    {

    }
}
