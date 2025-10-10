using UnityEngine;

using static StrategyStartSetterData;

public partial class UnitObject : MonoBehaviour
{
	private string unitName;
	private int unitID;
	private int factionID;
	public string UnitName { get => unitName; set => unitName = value; }
	public int UnitID { get => unitID; set => unitID = value; }
	public Faction OccupyingFaction
	{
		get => StrategyGameManager.Collector.FindFaction(factionID);
		set => factionID = value == null ? -1 : value.FactionID;
	}

	public void Init(string unitName = "" , int unitID = -1)
	{
		this.unitName = unitName;
		this.unitID = unitID;
	}
	public void Init(UnitData data) // UnitData
	{
		unitName = data.unitName;
		unitID = data.unitID;
		factionID = StrategyGameManager.Collector.FactionNameToID(data.factionName);
	}
}
public partial class UnitObject : MonoBehaviour // ProfileData
{
	private int manpower;
	private int healthPoint;     // 보유한 채력량
	private int suppliePoint;    // 보유한 물자량
	private int electricPoint;   // 보유한 전력량
	private int attack;
	private int defense;
	private int speed;
	private int range;
	private int vision;
	private SkillProfile[] connectSkill;
	private OccupationPoint occupationPoint;

	public int Manpower { get => manpower; set => manpower = value; }
	public int HealthPoint { get => healthPoint; set => healthPoint = value; }
	public int SuppliePoint { get => suppliePoint; set => suppliePoint = value; }
	public int ElectricPoint { get => electricPoint; set => electricPoint = value; }
	public int Attack { get => attack; set => attack = value; }
	public int Defense { get => defense; set => defense = value; }
	public int Speed { get => speed; set => speed = value; }
	public int Range { get => range; set => range = value; }
	public int Vision { get => vision; set => vision = value; }
	public SkillProfile[] ConnectSkill { get => connectSkill; set => connectSkill = value; }
	public OccupationPoint OccupationPoint { get => occupationPoint; set => occupationPoint = value; }

	public void Init(UnitProfile data)
	{
		Manpower = data.manpower;

		HealthPoint = data.healthPoint;
		SuppliePoint = data.suppliePoint;
		ElectricPoint = data.electricPoint;

		Attack = data.attack;
		Defense = data.defense;
		Speed = data.speed;
		Range = data.range;
		Vision = data.vision;

		ConnectSkill = data.connectSkill;

		if (OccupationPoint == null) OccupationPoint = GetComponentInChildren<OccupationPoint>();
		if (OccupationPoint == null) OccupationPoint = gameObject.AddComponent<OccupationPoint>();

		OccupationPoint.factionID = factionID;
		OccupationPoint.pointValue = data.occupationPoint;
	}
}
public partial class UnitObject : IStrategyElement
{
	public bool IsInCollector { get; set; }

	public void InStrategyCollector()
	{
	}

	public void OutStrategyCollector()
	{
	}
}
