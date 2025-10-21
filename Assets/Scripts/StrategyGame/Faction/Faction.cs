using System;

using Sirenix.OdinInspector;

using UnityEngine;

[Serializable]
public partial class Faction
{
	public Faction(StrategyStartSetterData.FactionData data)
	{
		factionID = data.factionID;
		factionName = data.factionName;
		factionColor = data.factionColor;
		factionIcon = data.factionIcon;
		defaultUnitPrefab = data.defaultUnitPrefab;

		MaxPersonnel = data.maxPersonnel;

		captureSpeed = 1f;
		suppletionSpeed = 1f;
	}

	[ShowInInspector]
	public string FactionName { get => factionName; set => factionName = value; }
	[ShowInInspector]
	public int FactionID { get => factionID; }
	public Color FactionColor { get => factionColor; set => factionColor = value; }
	public Sprite FactionIcon { get => factionIcon; set => factionIcon = value; }
	public GameObject DefaultUnitPrefab { get => defaultUnitPrefab; set => defaultUnitPrefab = value; }

	public int MaxPersonnel { get => maxPersonnel; set => maxPersonnel = value; }
	public float SuppletionSpeed { get => suppletionSpeed; set => suppletionSpeed = value; }
	public float CaptureSpeed { get => captureSpeed; set => captureSpeed = value; }

	private string factionName;
	private readonly int factionID;

	private Color factionColor;
	private Sprite factionIcon;
	private GameObject defaultUnitPrefab;

	private int maxPersonnel;
	private float captureSpeed;
	private float suppletionSpeed;


	public static bool TryFindFaction(string factionName, out Faction find)
	{
		return StrategyManager.Collector.TryFindElement<Faction>(f => f.factionName == factionName, out find);
	}
	public static bool FindFaction(int factionID, out Faction find)
	{
		return StrategyManager.Collector.TryFindElement<Faction>(f => f.factionID == factionID, out find);
	}
}
public partial class Faction : IStrategyElement
{
	public bool IsInCollector { get; set; }
	public void InStrategyCollector()
	{
	}
	public void OutStrategyCollector()
	{
	}
}
