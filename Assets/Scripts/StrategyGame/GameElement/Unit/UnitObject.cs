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
	[SerializeField]
	private UnitDebugRender debugRender;

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

		UnitProfileObject profileObj = data.GetUnitProfile;

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
		InitDebugRender();
		InitProfileObject(profileObj);
		InitMovement();
		InitOperationObject();
		InitVisibility();
		InitFSM();
		InitCombat();
	}
	private void InitDebugRender()
	{
		if (debugRender == null || Faction == null) return;
		debugRender.SetColor(Faction.FactionColor);
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
		DeinitMovement();
	}
	partial void DeselectSelf();
	partial void DeinitFSM();
	partial void DeinitCombat();
	partial void DeinitMovement();


#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		OnDrawGizmos_Range();
	}
#endif
}
public partial class UnitObject : IStateValueGetter
{
	private StatsGroup skillBuffGroup;
	public StatsList MainStatsList => StatsData.GetStatsList();
	public StatsGroup SkillBuffGroup => skillBuffGroup ??= new StatsGroup();
	public float GetStateValue(StatsType type)
	{
		if (StrategyManager.IsNotReadyScene) return 0;
		float value = MainStatsList.GetValueInt(type) + SkillBuffGroup.GetValueInt(type);
		value *= type switch
		{
			StatsType.유닛_이동속도_c => 0.01f,
			StatsType.유닛_조준지연시간_c => 0.01f,
			StatsType.유닛_연속공격지연시간_c => 0.01f,
			StatsType.유닛_재공격지연시간_c => 0.01f,
			StatsType.유닛_재장전시간_c => 0.01f,
			StatsType.유닛_공격범위_종료최소_c => 0.01f,
			StatsType.유닛_공격범위_시작최소_c => 0.01f,
			StatsType.유닛_공격범위_시작최대_c => 0.01f,
			StatsType.유닛_공격범위_종료최대_c => 0.01f,
			StatsType.유닛_행동범위_c => 0.01f,
			StatsType.유닛_시야범위_c => 0.01f,
			_ => 1f
		};
		return value;
	}
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
