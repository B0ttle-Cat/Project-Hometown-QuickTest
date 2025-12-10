using Sirenix.OdinInspector;

using StrategyManagerModule;

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
		get => StrategyManager.IsNotReadyScene ? null : StrategyManager.Collector.Find<Faction>(FactionID);
	}

	public void Init(UnitProfileObject data, int factionID = -1)
	{

		profile = new UnitData.Profile(new UnitData.Profile.Data()
		{
			unitKey = UnitKey.None,
			displayName = data.displayName,
			unitID = unitElementID,
			factionID = factionID,
			projectileKey = data.projectileKey,
			protectType = data.protectType,
		});
		InitProfileObject(data);
	}
	public void Init(in StrategyStartSetterData.UnitData data) // UnitData
	{
		//UnitProfileObject profileObj = data.GetUnitProfile;
		if (profile == null)
		{
			if (StrategyManager.Key2Unit.TryGetAsset(data.unitKey, out var info) && info.UnitProfileObject != null)
			{

				profile = new UnitData.Profile(new()
				{
					unitKey = data.unitKey,
					displayName = info.UnitProfileObject.displayName,
					unitID = unitElementID,
					factionID = data.factionID,
					projectileKey = info.UnitProfileObject.projectileKey,
					protectType = info.UnitProfileObject.protectType,
				});
			}
			else
			{
				profile = new UnitData.Profile(new()
				{
					unitKey = data.unitKey,
					displayName = "",
					unitID = unitElementID,
					factionID = -1,
					projectileKey = ProjectileKey.None,
					protectType = ProtectionType.일반,
				});
			}
		}
		else
		{
			ref UnitData.Profile.Data refData = ref profile.RefData();
			refData.factionID = data.factionID;
		}

		sector = new UnitData.ConnectSector(new(data.visiteSectorID));
	}


	public void InitOther()
	{
		InitDebugRender();
		InitMovement();
		InitOperationObject();
		InitVisibility();
		InitFSM();
		InitCombat();
		InitAttack();
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
	partial void InitAttack();
	public void Deinit()
	{
		DeselectSelf();
		DeinitFSM();
		DeinitCombat();
		DeinitMovement();
		DeinitAttack();
	}
	partial void DeselectSelf();
	partial void DeinitFSM();
	partial void DeinitCombat();
	partial void DeinitMovement();
	partial void DeinitAttack();
#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		OnDrawGizmos_Range();
	}
#endif
}
public partial class UnitObject : IStateValueControl
{
	private StatsGroup skillBuffGroup;
	public StatsList MainStatsList => StatsData.GetStatsList();
	public StatsGroup SkillBuffGroup => skillBuffGroup ??= new StatsGroup();
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
	public float GetStateValuePercent(StatsType type)
	{
		if (StrategyManager.IsNotReadyScene) return 0;
		float value = MainStatsList.GetValueInt(type) + SkillBuffGroup.GetValueInt(type);
		return value * 0.01f;
	}
	public int GetStateValue(StatsType type)
	{
		if (StrategyManager.IsNotReadyScene) return 0;
		int value = MainStatsList.GetValueInt(type) + SkillBuffGroup.GetValueInt(type);
		return value;
	}
	public void SetValueInMainState(StatsType type, int value)
	{
		if (StrategyManager.IsNotReadyScene) return;
		StatsData.SetValue(type, value);
	}
}
