using UnityEngine;

using static StrategyGamePlayData;
using static StrategyStartSetterData;

public partial class UnitObject : MonoBehaviour
{
	private UnitBaseData unitBaseData;
	private UnitProfileData unitProfileData;

	public UnitBaseData UnitBaseData { get => unitBaseData; set => unitBaseData = value; }
	public UnitProfileData UnitProfileData { get => unitProfileData; set => unitProfileData = value; }

	public string UnitName => unitBaseData.GetData().unitName;
	public int UnitID => unitBaseData.GetData().unitID;
	public int FactionID => unitBaseData.GetData().factionID;
	public Faction Faction
	{
		get => StrategyGameManager.Collector.FindFaction(FactionID);
	}

	public void Init(string unitName = "", int unitID = -1)
	{
		unitBaseData = new UnitBaseData(new UnitBaseData.Data()
		{
			unitName = unitName,
			unitID = unitID,
			factionID = -1,
			weaponType = UnitBaseData.WeaponType.None,
			protectType = UnitBaseData.ProtectionType.None,
		});
	}
	public void Init(UnitData data) // UnitData
	{
		UnitBaseData.SetData(new UnitBaseData.Data()
		{
			unitName = data.unitName,
			unitID = data.unitID,
			factionID = StrategyGameManager.Collector.FactionNameToID(data.factionName),
			weaponType = (UnitBaseData.WeaponType)data.weaponType,
			protectType = (UnitBaseData.ProtectionType)data.protectType,
		});

		var profile = data.unitProfile;
		UnitProfileData = new UnitProfileData(new UnitProfileData.Data()
		{
			manpower = profile.manpower,
			durability = profile.durability,

			attack = profile.attack,
			defense = profile.defense,
			heal = profile.heal,

			piercingLevel = profile.piercingLevel,
			protectingLevel = profile.protectingLevel,

			EMPProtectionLevel = profile.EMPProtectionLevel,

			attackHitPoint = profile.attackHitPoint,
			attackMissPoint = profile.attackMissPoint,

			criticalHitPoint = profile.criticalHitPoint,
			criticalMissPoint = profile.criticalMissPoint,

			attackDelay = profile.attackDelay,
			firingCount = profile.firingCount,
			firingDelay = profile.firingDelay,

			electricPerAttack = profile.electricPerAttack,
			suppliePerAttack = profile.suppliePerAttack,

			attackange = profile.attackange,
			actionRange = profile.actionRange,
			viewRange = profile.viewRange,

			moveSpeed = profile.moveSpeed,

			occupationPoint = profile.occupationPoint,
		});
	}
}
public partial class UnitObject : MonoBehaviour // ProfileData
{
	private SkillProfile[] connectSkill;
	private OccupationTag occupationTag;

	public int Manpower => UnitProfileData.GetData().manpower;
	public int Durability => UnitProfileData.GetData().durability;

	public int Attack => UnitProfileData.GetData().attack;
	public int Defense => UnitProfileData.GetData().defense;
	public int Heal => UnitProfileData.GetData().heal;

	public int PiercingLevel => UnitProfileData.GetData().piercingLevel;
	public int ProtectingLevel => UnitProfileData.GetData().protectingLevel;

	public int EMPProtectionLevel => UnitProfileData.GetData().EMPProtectionLevel;

	public int AttackHitPoint => UnitProfileData.GetData().attackHitPoint;
	public int AttackMissPoint => UnitProfileData.GetData().attackMissPoint;

	public int CriticalHitPoint => UnitProfileData.GetData().criticalHitPoint;
	public int CriticalMissPoint => UnitProfileData.GetData().criticalMissPoint;

	public float AttackDelay => UnitProfileData.GetData().attackDelay;
	public int FiringCount => UnitProfileData.GetData().firingCount;
	public float FiringDelay => UnitProfileData.GetData().firingDelay;

	public int ElectricPerAttack => UnitProfileData.GetData().electricPerAttack;
	public int SuppliePerAttack => UnitProfileData.GetData().suppliePerAttack;

	public float Attackange => UnitProfileData.GetData().attackange;
	public float ActionRange => UnitProfileData.GetData().actionRange;
	public float ViewRange => UnitProfileData.GetData().viewRange;

	public float MoveSpeed => UnitProfileData.GetData().moveSpeed;
	public int OccupationPoint => UnitProfileData.GetData().occupationPoint;

	public SkillProfile[] ConnectSkill { get => connectSkill; set => connectSkill = value; }
	public OccupationTag OccupationTag { get => occupationTag; set => occupationTag = value; }

	public void Init(UnitProfile data)
	{
		ConnectSkill = data.connectSkill;

		if (OccupationPoint > 0)
		{
			if (OccupationTag == null) OccupationTag = GetComponentInChildren<OccupationTag>();
			if (OccupationTag == null) OccupationTag = gameObject.AddComponent<OccupationTag>();

			OccupationTag.factionID = FactionID;
			OccupationTag.pointValue = data.occupationPoint;
		}
		else
		{
			if (OccupationTag != null)
			{
				Destroy(OccupationTag);
				OccupationTag = null;
			}
		}
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
