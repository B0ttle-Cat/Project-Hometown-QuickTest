using Sirenix.OdinInspector;

using StrategyManagerModule;

using UnityEngine;

using static StrategyGamePlayData;

public partial class UnitObject : MonoBehaviour
{
	private UnitData.Profile profile;
	private UnitData.Stats stats;
	private UnitData.Skill skill;
	private UnitData.ConnectSector sector;
	private CaptureTag captureTag;
	[SerializeField]
	private UnitDebugRender debugRender;
#if UNITY_EDITOR
	[ShowInInspector, ToggleGroup("EditUnitData")]
	bool EditUnitData { get; set; } = false;
#endif
	[ShowInInspector, ToggleGroup("EditUnitData", GroupName = "UnitData"), HideReferenceObjectPicker]
	public UnitData.Profile Profile { get => profile; set => profile = value; }
	[ShowInInspector, ToggleGroup("EditUnitData"), HideReferenceObjectPicker]
	public UnitData.Stats Stats { get => stats; set => stats = value; }
	[ShowInInspector, ToggleGroup("EditUnitData"), HideReferenceObjectPicker]
	public UnitData.Skill Skill { get => skill; set => skill = value; }
	[ShowInInspector, ToggleGroup("EditUnitData"), HideReferenceObjectPicker]
	public UnitData.ConnectSector Sector { get => sector; set => sector = value; }
	[ShowInInspector, ToggleGroup("EditUnitData"), HideReferenceObjectPicker]
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


		int durability = Mathf.Min(data.durability, MainStatsList.GetValue(StatsType.유닛_최대내구도));
		if (durability < 1) durability = 1;
		MainStatsList.SetValue(StatsType.유닛_현재내구도, durability);
		
		sector = new UnitData.ConnectSector(new(data.visiteSectorID));
	}


	public void InitOther()
	{
		InitLife();		   
		InitDebugRender();
		InitMovement();
		InitOperationObject();
		InitVisibility();
		InitFSM();
		InitCombat();
		InitAttack();
	}
	partial void InitProfileObject(UnitProfileObject profileObj);
	private void InitDebugRender()
	{
		if (debugRender == null || Faction == null) return;
		debugRender.SetColor(Faction.FactionColor);
	}
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
