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

	[ShowInInspector, FoldoutGroup("UnitData"), ReadOnly]
	public UnitData.Profile Profile { get => profile; set => profile = value; }
	[ShowInInspector, FoldoutGroup("UnitData"), ReadOnly]
	public UnitData.Stats Stats { get => stats; set => stats = value; }
	[ShowInInspector, FoldoutGroup("UnitData"), ReadOnly]
	public UnitData.Skill Skill { get => skill; set => skill = value; }
	[ShowInInspector, FoldoutGroup("UnitData"), ReadOnly]
	public UnitData.ConnectSector Sector { get => sector; set => sector = value; }
	[ShowInInspector, FoldoutGroup("UnitData"), ReadOnly]
	public CaptureTag CaptureTag { get => captureTag; set => captureTag = value; }
	public ref readonly UnitData.Profile.Data ProfileData => ref Profile.ReadonlyData();
	public ref readonly UnitData.Stats.Data StatsData => ref Stats.ReadonlyData();
	public ref readonly UnitData.Skill.Data SkillData => ref Skill.ReadonlyData();
	public ref readonly UnitData.ConnectSector.Data SectorData => ref Sector.ReadonlyData();
	public string UnitName => ProfileData.displayName;
	public int UnitID => ProfileData.unitID;
	public int FactionID => ProfileData.factionID;
	[ShowInInspector, FoldoutGroup("UnitData"), ReadOnly]
	public Faction Faction
	{
		get => StrategyManager.IsNotReadyScene ? null : StrategyManager.Collector.FindFaction(FactionID);
	}

	public void Init(string displayName = "", int factionID = -1)
	{
		factionID = (factionID == -1 && profile != null) ? FactionID : factionID;

		profile = new UnitData.Profile(new UnitData.Profile.Data()
		{
			unitKey = UnitKey.None,
			displayName = displayName,
			unitID = unitElementID,
			factionID = factionID,
			weaponType = WeaponType.일반,
			protectType = ProtectionType.일반,
		});
		sector = new UnitData.ConnectSector(new());
		InitOther(null);
	}
	public void Init(UnitProfileObject data, int factionID = -1)
	{
		factionID = (factionID == -1 && profile != null) ? FactionID : factionID;

		profile = new UnitData.Profile(new UnitData.Profile.Data()
		{
			unitKey = data.unitKey,
			displayName = data.displayName,
			unitID = unitElementID,
			factionID = factionID,
			weaponType = data.weaponType,
			protectType = data.protectType,
		});
		sector = new UnitData.ConnectSector(new());
		InitOther(data);
	}
	public void Init(in StrategyStartSetterData.UnitData data) // UnitData
	{
		int factionID = StrategyManager.Collector.FactionNameToID(data.factionName);

		UnitProfileObject profileObj = data.unitProfile;

		profile = new UnitData.Profile(new()
		{
			unitKey = profileObj.unitKey,
			displayName = profileObj.displayName,
			unitID = unitElementID,
			factionID = factionID,
			weaponType = profileObj.weaponType,
			protectType = profileObj.protectType,
		});
		sector = new UnitData.ConnectSector(new(data.visiteSectorName));
		InitOther(profileObj);
	}

	private void InitOther(UnitProfileObject profileObj)
	{
		InitProfileObject(profileObj);
		InitMovement();
		InitOperationObject();
		InitVisibility();
		InitFSM();
		InitCombat();
	}
	partial void InitProfileObject(UnitProfileObject profileObj);
	partial void InitMovement();
	partial void InitVisibility();
	partial void InitOperationObject();
	partial void InitFSM();
	partial void InitCombat();
	public void Deinit()
	{
		DeselectSelf();
		DeinitFSM();
		DeinitCombat();
	}
	partial void DeselectSelf();
	partial void DeinitFSM();
	partial void DeinitCombat();
}
public partial class UnitObject // StateValue
{
	private StatsGroup skillBuffGroup;
	public StatsList MainStatsList => StatsData.GetStatsList();
	public StatsGroup SkillBuffGroup => skillBuffGroup ??= new StatsGroup();
	public int GetStateValue(StatsType type) => StrategyManager.IsNotReadyScene ? 0 : MainStatsList.GetValueInt(type) + SkillBuffGroup.GetValueInt(type);
	partial void InitProfileObject(UnitProfileObject profileObj)
	{
		if (profileObj == null) return;

		Stats = new UnitData.Stats(new()
		{
			stats = new StatsList(profileObj.ConvertStatsValues())
		});
		Skill = new UnitData.Skill(new()
		{
			skillDatas = profileObj.personalSkills == null ? new SkillData[0] : profileObj.personalSkills.Clone() as SkillData[]
		});

		var 유닛_점령점수 = StatsData.GetValue(StatsType.유닛_점령점수);
		if (유닛_점령점수 > 0)
		{
			if (CaptureTag == null) CaptureTag = GetComponentInChildren<CaptureTag>();
			if (CaptureTag == null) CaptureTag = gameObject.AddComponent<CaptureTag>();

			CaptureTag.factionID = FactionID;
			CaptureTag.pointValue = profileObj.유닛_점령점수;
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
