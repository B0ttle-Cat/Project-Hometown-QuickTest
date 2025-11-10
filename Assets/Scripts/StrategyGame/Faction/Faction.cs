using System;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;

using static StrategyGamePlayData;

[Serializable]
public partial class Faction : IEquatable<Faction> , IDisposable
{
	public Faction(in StrategyStartSetterData.FactionData data)
	{
		factionName = data.factionName;
		factionColor = data.factionColor;
		factionIcon = data.factionIcon;
		defaultUnitPrefab = data.defaultUnitPrefab;

		factionStats = new StatsList(new StatsValue(StatsType.세력_점령속도비율, data.captureSpeed),
			new StatsValue(StatsType.세력_병력_최대허용량, data.maxOperationPoint),
			new StatsValue(StatsType.세력_병력_편제요구량, data.requireOperationPoint),
			new StatsValue(StatsType.세력_병력_현재보유량, data.currentOperationPoint),
			new StatsValue(StatsType.세력_물자_최대허용량, data.maxMaterialPoint),
			new StatsValue(StatsType.세력_물자_현재보유량, data.currentMaterialPoint),
			new StatsValue(StatsType.세력_전력_최대허용량, data.maxElectricPoint),
			new StatsValue(StatsType.세력_전력_현재보유량, data.currentElectricPoint)
			);
		availableUnitKeyList = data.AvailableUnitKeyList();
	}
	public void Dispose()
	{
		factionIcon = null;
		defaultUnitPrefab = null;
		availableUnitKeyList = null;
	}

	private string factionName;
	private int factionID;

	private Color factionColor;
	private Sprite factionIcon;
	private GameObject defaultUnitPrefab;

	private StatsList factionStats;
	private List<UnitKey> availableUnitKeyList;

	[ShowInInspector]
	public string FactionName => factionName; 
	[ShowInInspector]
	public int FactionID => factionID; 
	public Color FactionColor => factionColor; 
	public Sprite FactionIcon => factionIcon; 
	public GameObject DefaultUnitPrefab => defaultUnitPrefab;
	public StatsList FactionStats => factionStats;
	public List<UnitKey> AvailableUnitKeyList => availableUnitKeyList; 


	public static bool TryFindFaction(string factionName, out Faction find)
	{
		return StrategyManager.Collector.TryFindElement<Faction>(f => f.factionName == factionName, out find);
	}
	public static bool FindFaction(int factionID, out Faction find)
	{
		return StrategyManager.Collector.TryFindElement<Faction>(f => f.factionID == factionID, out find);
	}
    public override bool Equals(object obj)
    {
        return Equals(obj as Faction);
    }
    public bool Equals(Faction other)
    {
        return other is not null &&
               factionName == other.factionName &&
               factionID == other.factionID;
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(factionName, factionID);
    }


    public static bool operator ==(Faction left, Faction right)
    {
        return EqualityComparer<Faction>.Default.Equals(left, right);
    }
    public static bool operator !=(Faction left, Faction right)
    {
        return !(left == right);
    }
}
public partial class Faction : IStrategyElement
{
	public IStrategyElement ThisElement => this;
	public bool IsInCollector { get; set; }
    int IStrategyElement.ID { get => factionID; set => factionID = value; }

    public void InStrategyCollector()
	{
	}
	public void OutStrategyCollector()
	{
	}

    void IStrategyStartGame.OnStartGame()
    {
    }

    void IStrategyStartGame.OnStopGame()
    {
    }
}
