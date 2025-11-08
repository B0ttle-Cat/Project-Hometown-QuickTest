using System.Linq;

using UnityEngine;

using static StrategyGamePlayData;

public partial class UnitObject : MonoBehaviour
{
	private UnitData.Profile profile;
	private UnitData.Stats stats;
	private UnitData.Skill skill;
	private UnitData.ConnectSector sector;

	private StatsGroup skillBuffGroup;

	public UnitData.Profile Profile { get => profile; set => profile = value; }
	public UnitData.Stats Stats { get => stats; set => stats = value; }
	public UnitData.Skill Skill { get => skill; set => skill = value; }
	public UnitData.ConnectSector Sector { get => sector; set => sector = value; }

	public ref readonly UnitData.Profile.Data ProfileData => ref Profile.ReadonlyData();
	public ref readonly UnitData.Stats.Data StatsData => ref Stats.ReadonlyData();
	public ref readonly UnitData.Skill.Data SkillData => ref Skill.ReadonlyData();
	public ref readonly UnitData.ConnectSector.Data SectorData => ref Sector.ReadonlyData();

	public string UnitName => ProfileData.unitName;
	public int UnitID => ProfileData.unitID;
	public int FactionID => ProfileData.factionID;
	public Faction Faction
	{
		get => StrategyManager.Collector.FindFaction(FactionID);
	}

	public StatsList MainStatsList => StatsData.GetStatsList();
	public StatsGroup SkillBuffGroup => skillBuffGroup ??= new StatsGroup();

	public void Init(string unitName = "", int unitID = -1)
	{
		profile = new UnitData.Profile(new UnitData.Profile.Data()
		{
			unitName = unitName,
			unitID = unitID,
			factionID = -1,
			weaponType = WeaponType.일반,
			protectType = ProtectionType.일반,
		});
	}
	public void Init(StrategyStartSetterData.UnitData data) // UnitData
	{
		string unitName = data.unitName;
		int unitID = data.unitID;
		int factionID = StrategyManager.Collector.FactionNameToID(data.factionName);
		UnitProfileObject profile = data.unitProfile;

		Profile = new UnitData.Profile(new()
		{
			unitName = unitName,
			unitID = unitID,
			factionID = factionID,
			weaponType = profile.weaponType,
			protectType = profile.protectType,
		});
		Stats = new UnitData.Stats(new()
		{
			stats = new StatsList(profile.ConvertStatsValues())
		});
		Skill = new UnitData.Skill(new()
		{
			skillDatas = data.skillData.Select(i=>new UnitData.Skill.SkillData(i.x, i.y)).ToArray()
		});
		Sector = new UnitData.ConnectSector(new(data.connectSectorName));
		InitOther(profile);
	}
}
public partial class UnitObject : MonoBehaviour // Other
{
	private SkillProfile[] connectSkill;
	private CaptureTag captureTag;

	public int GetStateValue(StatsType type) => StatsData.GetValue(type);

	public SkillProfile[] ConnectSkill { get => connectSkill; set => connectSkill = value; }
	public CaptureTag CaptureTag { get => captureTag; set => captureTag = value; }

	public void InitOther(UnitProfileObject data)
	{
		ConnectSkill = data.connectSkill;
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
public partial class UnitObject : IStrategyElement
{
	public bool IsInCollector { get; set; }

	public void InStrategyCollector()
	{
	}

	public void OutStrategyCollector()
	{
	}

    void IStartGame.OnStartGame()
    {
    }

    void IStartGame.OnStopGame()
    {
    }
}
public partial class UnitObject : ISelectMouse
{
	public Vector3 clickCenter => transform.position;
	bool ISelectMouse.IsSelectMouse { get; set; }
    bool ISelectMouse.IsPointEnter { get; set; }
	Vector3 ISelectMouse.ClickCenter => clickCenter;


	void ISelectMouse.OnPointEnter()
	{
	}
	void ISelectMouse.OnPointExit()
	{
	}
	bool ISelectMouse.OnSelect()
	{
		return true;
	}
	bool ISelectMouse.OnDeselect()
	{
		return true;
	}
	void ISelectMouse.OnSingleSelect()
	{
	}

	void ISelectMouse.OnSingleDeselect()
	{
	}

	void ISelectMouse.OnFirstSelect()
	{
	}

	void ISelectMouse.OnLastDeselect()
	{
	}
}
