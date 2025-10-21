using UnityEngine;

using static StrategyGamePlayData;

public partial class UnitObject : MonoBehaviour
{
	private UnitData.Profile profileData;
	private UnitData.Stats statsData;

	public UnitData.Profile ProfileData { get => profileData; set => profileData = value; }
	public UnitData.Stats StatsData { get => statsData; set => statsData = value; }

	public string UnitName => profileData.GetData().unitName;
	public int UnitID => profileData.GetData().unitID;
	public int FactionID => profileData.GetData().factionID;
	public Faction Faction
	{
		get => StrategyManager.Collector.FindFaction(FactionID);
	}

	public void Init(string unitName = "", int unitID = -1)
	{
		profileData = new UnitData.Profile(new UnitData.Profile.Data()
		{
			unitName = unitName,
			unitID = unitID,
			factionID = -1,
			weaponType = WeaponType.None,
			protectType = ProtectionType.None,
		});
	}
	public void Init(StrategyStartSetterData.UnitData data) // UnitData
	{
		var initData = new UnitData.Profile.Data()
		{
			unitName = data.unitName,
			unitID = data.unitID,
			factionID = StrategyManager.Collector.FactionNameToID(data.factionName),
			weaponType = (WeaponType)data.weaponType,
			protectType = (ProtectionType)data.protectType,
		};

		if (profileData == null)
		{
			profileData = new UnitData.Profile(initData);
		}
		else
		{
			ProfileData.SetData(initData);
		}

		var profile = data.unitProfile;
		StatsData = new UnitData.Stats(new UnitData.Stats.Data()
		{
			statsList = new StatsList(
				(StatsType.유닛_인력, profile.유닛_인력, SymbolType.Number),
				(StatsType.유닛_물자, profile.유닛_물자, SymbolType.Number),
				(StatsType.유닛_전력, profile.유닛_전력, SymbolType.Number),

				(StatsType.유닛_최대내구도, profile.유닛_최대내구도, SymbolType.Number),
				(StatsType.유닛_현재내구도, profile.유닛_현재내구도, SymbolType.Number),
				(StatsType.유닛_공격력, profile.유닛_공격력, SymbolType.Number),
				(StatsType.유닛_방어력, profile.유닛_방어력, SymbolType.Number),
				(StatsType.유닛_치유력, profile.유닛_치유력, SymbolType.Number),
				(StatsType.유닛_회복력, profile.유닛_회복력, SymbolType.Number),
				(StatsType.유닛_이동속도, profile.유닛_이동속도, SymbolType.Number),
				(StatsType.유닛_점령점수, profile.유닛_점령점수, SymbolType.Number),

				(StatsType.유닛_관통레벨, profile.유닛_관통레벨, SymbolType.Number),
				(StatsType.유닛_장갑레벨, profile.유닛_장갑레벨, SymbolType.Number),
				(StatsType.유닛_EMP저항레벨, profile.유닛_EMP저항레벨, SymbolType.Number),

				(StatsType.유닛_공격명중기회, profile.유닛_공격명중기회, SymbolType.Number),
				(StatsType.유닛_공격회피기회, profile.유닛_공격회피기회, SymbolType.Number),
				(StatsType.유닛_치명명중기회, profile.유닛_치명명중기회, SymbolType.Number),
				(StatsType.유닛_치명회피기회, profile.유닛_치명회피기회, SymbolType.Number),

				(StatsType.유닛_명중피격수, profile.유닛_명중피격수, SymbolType.Number),
				(StatsType.유닛_연속공격횟수, profile.유닛_연속공격횟수, SymbolType.Number),
				(StatsType.유닛_조준지연시간, profile.유닛_조준지연시간, SymbolType.Number),
				(StatsType.유닛_연속공격지연시간, profile.유닛_연속공격지연시간, SymbolType.Number),
				(StatsType.유닛_재공격지연시간, profile.유닛_재공격지연시간, SymbolType.Number),

				(StatsType.유닛_공격범위, profile.유닛_공격범위, SymbolType.Number),
				(StatsType.유닛_행동범위, profile.유닛_행동범위, SymbolType.Number),
				(StatsType.유닛_시야범위, profile.유닛_시야범위, SymbolType.Number)
				)
		});
	}
}
public partial class UnitObject : MonoBehaviour // ProfileData
{
	private SkillProfile[] connectSkill;
	private CaptureTag captureTag;

	public int 유닛_인력 => StatsData.GetData().GetValue(StatsType.유닛_인력, SymbolType.Number);
	public int 유닛_최대내구도 => StatsData.GetData().GetValue(StatsType.유닛_최대내구도, SymbolType.Number);
	public int 유닛_현재내구도 => StatsData.GetData().GetValue(StatsType.유닛_현재내구도, SymbolType.Number);
	public int 유닛_공격력 => StatsData.GetData().GetValue(StatsType.유닛_공격력, SymbolType.Number);
	public int 유닛_방어력 => StatsData.GetData().GetValue(StatsType.유닛_방어력, SymbolType.Number);
	public int 유닛_치유력 => StatsData.GetData().GetValue(StatsType.유닛_치유력, SymbolType.Number);
	public int 유닛_회복력 => StatsData.GetData().GetValue(StatsType.유닛_회복력, SymbolType.Number);
	public int 유닛_관통레벨 => StatsData.GetData().GetValue(StatsType.유닛_관통레벨, SymbolType.Number);
	public int 유닛_장갑레벨 => StatsData.GetData().GetValue(StatsType.유닛_장갑레벨, SymbolType.Number);
	public int 유닛_EMP저항레벨 => StatsData.GetData().GetValue(StatsType.유닛_EMP저항레벨, SymbolType.Number);
	public int 유닛_공격명중기회 => StatsData.GetData().GetValue(StatsType.유닛_공격명중기회, SymbolType.Number);
	public int 유닛_공격회피기회 => StatsData.GetData().GetValue(StatsType.유닛_공격회피기회, SymbolType.Number);
	public int 유닛_치명명중기회 => StatsData.GetData().GetValue(StatsType.유닛_치명명중기회, SymbolType.Number);
	public int 유닛_치명회피기회 => StatsData.GetData().GetValue(StatsType.유닛_치명회피기회, SymbolType.Number);
	public int 유닛_공격지연시간 => StatsData.GetData().GetValue(StatsType.유닛_재공격지연시간, SymbolType.Number);
	public int 유닛_연속공격횟수 => StatsData.GetData().GetValue(StatsType.유닛_연속공격횟수, SymbolType.Number);
	public int 유닛_연속공격지연시간 => StatsData.GetData().GetValue(StatsType.유닛_연속공격지연시간, SymbolType.Number);
	public int 유닛_공격소모_전력 => StatsData.GetData().GetValue(StatsType.유닛_공격소모_전력, SymbolType.Number);
	public int 유닛_공격소모_물자 => StatsData.GetData().GetValue(StatsType.유닛_공격소모_물자, SymbolType.Number);
	public int 유닛_공격범위 => StatsData.GetData().GetValue(StatsType.유닛_공격범위, SymbolType.Number);
	public int 유닛_행동범위 => StatsData.GetData().GetValue(StatsType.유닛_행동범위, SymbolType.Number);
	public int 유닛_시야범위 => StatsData.GetData().GetValue(StatsType.유닛_시야범위, SymbolType.Number);
	public int 유닛_이동속도 => StatsData.GetData().GetValue(StatsType.유닛_이동속도, SymbolType.Number);
	public int 유닛_점령점수 => StatsData.GetData().GetValue(StatsType.유닛_점령점수, SymbolType.Number);


	public SkillProfile[] ConnectSkill { get => connectSkill; set => connectSkill = value; }
	public CaptureTag CaptureTag { get => captureTag; set => captureTag = value; }

	public void Init(UnitProfile data)
	{
		ConnectSkill = data.connectSkill;

		if (유닛_점령점수 > 0)
		{
			if (CaptureTag == null) CaptureTag = GetComponentInChildren<CaptureTag>();
			if (CaptureTag == null) CaptureTag = gameObject.AddComponent<CaptureTag>();

			CaptureTag.factionID = FactionID;
			CaptureTag.pointValue = data.capturePoint;
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
}
