using System;

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
		InitOther(null);
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


	private void InitOther(UnitProfileObject profileObj)
	{
		InitProfileObject(profileObj);
		operationObject = null;
		InitVisibility();
	}
	partial void InitProfileObject(UnitProfileObject profileObj);
	partial void InitVisibility();
	partial void InitOperationObject();
}
public partial class UnitObject // StateValue
{

	private StatsGroup skillBuffGroup;
	public StatsList MainStatsList => StatsData.GetStatsList();
	public StatsGroup SkillBuffGroup => skillBuffGroup ??= new StatsGroup();
	public int GetStateValue(StatsType type) => MainStatsList.GetValueInt(type) + SkillBuffGroup.GetValueInt(type);


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

public partial class UnitObject // OperationBelong
{
	[HideInEditorMode, FoldoutGroup("OperationBelong", VisibleIf = "IsOperationBelong"), InlineProperty, HideLabel]
	public OperationObject operationObject;
	public int OperationID => operationObject == null ? -1 : operationObject.OperationID;
	public bool IsOperationBelong => OperationID >= 0;

	partial void InitOperationObject()
	{
		operationObject = null;
	}
	public void SetOperationBelong(OperationObject operationObject)
	{
		this.operationObject = operationObject;
		if (operationObject == null) return;

		ThisVisibility.OnChangeVisible -= operationObject.ChangeVisibleUnit;
		ThisVisibility.OnChangeVisible += operationObject.ChangeVisibleUnit;

		ThisVisibility.OnChangeInvisible -= operationObject.ChangeInvisibleUnit;
		ThisVisibility.OnChangeInvisible += operationObject.ChangeInvisibleUnit;
		if (ThisVisibility.IsVisible)
		{
			operationObject.ChangeVisibleUnit(this);
		}
		else
		{
			operationObject.ChangeInvisibleUnit(this);
		}
	}
	public void RelaseOperationBelong()
	{
		if (operationObject != null)
		{
			ThisVisibility.OnChangeVisible -= operationObject.ChangeVisibleUnit;
			ThisVisibility.OnChangeInvisible -= operationObject.ChangeInvisibleUnit;
			operationObject = null;
		}
	}
}

public partial class UnitObject : IVisibilityEvent<UnitObject>
{
	private IVisibilityEvent<Component> ChildVisibility => childVisibility;
	private CameraVisibilityGroup childVisibility;
	public IVisibilityEvent<UnitObject> ThisVisibility => this;
	bool IVisibilityEvent<UnitObject>.IsVisible => ChildVisibility == null ? false : ChildVisibility.IsVisible;
	private Action<UnitObject> onChangeVisible;
	private Action<UnitObject> onChangeInvisible;
	event Action<UnitObject> IVisibilityEvent<UnitObject>.OnChangeVisible
	{
		add {onChangeVisible += value;}
		remove{onChangeVisible += value;}
	}

	event Action<UnitObject> IVisibilityEvent<UnitObject>.OnChangeInvisible
	{
		add {onChangeInvisible += value;}
		remove{onChangeInvisible += value;}
	}

	partial void InitVisibility()
	{
		if (childVisibility != null) return;
		childVisibility = GetComponentInChildren<CameraVisibilityGroup>();
		if (childVisibility == null) return;
		childVisibility.OnChangeVisible += CameraVisibilityGroup_OnChangeVisible;
		childVisibility.OnChangeInvisible += CameraVisibilityGroup_OnChangeInvisible;
		if (ChildVisibility.IsVisible)
		{
			CameraVisibilityGroup_OnChangeVisible(this);
		}
		else
		{
			CameraVisibilityGroup_OnChangeInvisible(this);
		}
	}
	private void CameraVisibilityGroup_OnChangeVisible(Component obj)
	{
		if (obj == null) return;
		if (obj is not UnitObject unit || unit != this) return;
		if (onChangeVisible == null) return;
		onChangeVisible.Invoke(unit);
	}
	private void CameraVisibilityGroup_OnChangeInvisible(Component obj)
	{
		if (obj == null) return;
		if (obj is not UnitObject unit || unit != this) return;
		if (onChangeInvisible == null) return;
		onChangeInvisible.Invoke(unit);
	}
}