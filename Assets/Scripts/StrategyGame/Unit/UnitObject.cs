using Sirenix.OdinInspector;

using UnityEngine;

using static StrategyGamePlayData;
using static StrategyGamePlayData.UnitData.Skill;

public partial class UnitObject : MonoBehaviour
{
	private UnitData.Profile profile;
	private UnitData.Stats stats;
	private UnitData.Skill skill;
	private UnitData.ConnectSector sector;
	private CaptureTag captureTag;

	[ShowInInspector, ReadOnly]
	public UnitData.Profile Profile { get => profile; set => profile = value; }
	[ShowInInspector, ReadOnly]
	public UnitData.Stats Stats { get => stats; set => stats = value; }
	[ShowInInspector, ReadOnly]
	public UnitData.Skill Skill { get => skill; set => skill = value; }
	[ShowInInspector, ReadOnly]
	public UnitData.ConnectSector Sector { get => sector; set => sector = value; }
	[ShowInInspector, ReadOnly]
	public CaptureTag CaptureTag { get => captureTag; set => captureTag = value; }
	public ref readonly UnitData.Profile.Data ProfileData => ref Profile.ReadonlyData();
	public ref readonly UnitData.Stats.Data StatsData => ref Stats.ReadonlyData();
	public ref readonly UnitData.Skill.Data SkillData => ref Skill.ReadonlyData();
	public ref readonly UnitData.ConnectSector.Data SectorData => ref Sector.ReadonlyData();
	public string UnitName => ProfileData.displayName;
	public int UnitID => ProfileData.unitID;
	public int FactionID => ProfileData.factionID;
	public Faction Faction
	{
		get => StrategyManager.Collector.FindFaction(FactionID);
	}



	public void Init(string displayName = "", int factionID = -1)
	{
		int unitID = profile != null ? UnitID : -1;
		factionID = (factionID == -1 && profile != null) ? FactionID : factionID;

		profile = new UnitData.Profile(new UnitData.Profile.Data()
		{
			unitKey = UnitKey.None,
			displayName = displayName,
			unitID = unitID,
			factionID = factionID,
			weaponType = WeaponType.일반,
			protectType = ProtectionType.일반,
		});
		sector = new UnitData.ConnectSector(new());
	}
	public void Init(UnitProfileObject data, int factionID = -1)
	{
		int unitID = profile != null ? UnitID : -1;
		factionID = (factionID == -1 && profile != null) ? FactionID : factionID;

		profile = new UnitData.Profile(new UnitData.Profile.Data()
		{
			unitKey = data.unitKey,
			displayName = data.displayName,
			unitID = unitID,
			factionID = factionID,
			weaponType = data.weaponType,
			protectType = data.protectType,
		});
		sector = new UnitData.ConnectSector(new());

		InitOther(data);
	}
	public void Init(in StrategyStartSetterData.UnitData data) // UnitData
	{
		int unitID = profile != null ? UnitID : -1;
		int factionID = StrategyManager.Collector.FactionNameToID(data.factionName);
		UnitProfileObject profileObj = data.unitProfile;

		profile = new UnitData.Profile(new()
		{
			unitKey = profileObj.unitKey,
			displayName = profileObj.displayName,
			unitID = unitID,
			factionID = factionID,
			weaponType = profileObj.weaponType,
			protectType = profileObj.protectType,
		});
		sector = new UnitData.ConnectSector(new(data.connectSectorName));

		InitOther(profileObj);
	}
	private void InitOther(UnitProfileObject data)
	{
		Stats = new UnitData.Stats(new()
		{
			stats = new StatsList(data.ConvertStatsValues())
		});
		Skill = new UnitData.Skill(new()
		{
			skillDatas = data.personalSkills.Clone() as SkillData[]
		});

		var 유닛_점령점수 = StatsData.GetValue(StatsType.유닛_점령점수);
		if (유닛_점령점수 > 0)
		{
			if (CaptureTag == null) CaptureTag = GetComponentInChildren<CaptureTag>();
			if (CaptureTag == null) CaptureTag = gameObject.AddComponent<CaptureTag>();

			CaptureTag.factionID = FactionID;
			CaptureTag.pointValue = data.유닛_점령점수;
		}
		else
		{
			if (CaptureTag != null)
			{
				Destroy(CaptureTag);
				CaptureTag = null;
			}
		}
	}
}
public partial class UnitObject // StateValue
{
	private StatsGroup skillBuffGroup;
	public  StatsList MainStatsList => StatsData.GetStatsList();
	public StatsGroup SkillBuffGroup => skillBuffGroup ??= new StatsGroup();
	public int GetStateValue(StatsType type) => MainStatsList.GetValueInt(type) + SkillBuffGroup.GetValueInt(type);
}

public partial class UnitObject // TroopBelong
{
	private TroopsObject troopBelong;
	[ShowInInspector]
	public TroopsObject TroopBelong { get => troopBelong; private set => troopBelong = value; }

	public void SetTroopBelong(TroopsObject troopObject)
	{
		TroopBelong = troopObject;
	}
}