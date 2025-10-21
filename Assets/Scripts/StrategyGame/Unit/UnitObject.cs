using UnityEngine;

using static StrategyGamePlayData;

public partial class UnitObject : MonoBehaviour
{
	private UnitData.Profile profile;
	private UnitData.Stats stats;

	public UnitData.Profile Profile { get => profile; set => profile = value; }
	public UnitData.Stats Stats { get => stats; set => stats = value; }

	public UnitData.Profile.Data ProfileData => Profile.GetData();
	public UnitData.Stats.Data StatsData => Stats.GetData();

	public string UnitName => profile.GetData().unitName;
	public int UnitID => profile.GetData().unitID;
	public int FactionID => profile.GetData().factionID;
	public Faction Faction
	{
		get => StrategyManager.Collector.FindFaction(FactionID);
	}

	public void Init(string unitName = "", int unitID = -1)
	{
		profile = new UnitData.Profile(new UnitData.Profile.Data()
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
		string unitName = data.unitName;
		int unitID = data.unitID;
		int factionID = StrategyManager.Collector.FactionNameToID(data.factionName);
		UnitProfileObject profile = data.unitProfile;

		Profile = new UnitData.Profile(new UnitData.Profile.Data()
		{
			unitName = unitName,
			unitID = unitID,
			factionID = factionID,
			weaponType = profile.weaponType,
			protectType = profile.protectType,
		});
		Stats = new UnitData.Stats(new UnitData.Stats.Data()
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
				(StatsType.유닛_치명공격력, profile.유닛_치명공격력, SymbolType.Number),
				(StatsType.유닛_치명공격배율, profile.유닛_치명공격배율, SymbolType.Number),
				(StatsType.유닛_치명방어력, profile.유닛_치명방어력, SymbolType.Number),

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

				(StatsType.유닛_공격소모_물자, profile.유닛_공격소모_물자, SymbolType.Number),
				(StatsType.유닛_공격소모_전력, profile.유닛_공격소모_전력, SymbolType.Number),

				(StatsType.유닛_공격범위, profile.유닛_공격범위, SymbolType.Number),
				(StatsType.유닛_행동범위, profile.유닛_행동범위, SymbolType.Number),
				(StatsType.유닛_시야범위, profile.유닛_시야범위, SymbolType.Number)
				)
		});

		InitOther(profile);
	}		  
}
public partial class UnitObject : MonoBehaviour // Other
{
	private SkillProfile[] connectSkill;
	private CaptureTag captureTag;

	public int GetStateValue(StatsType type, SymbolType symbol) => Stats.GetData().GetValue(type, symbol);

	public SkillProfile[] ConnectSkill { get => connectSkill; set => connectSkill = value; }
	public CaptureTag CaptureTag { get => captureTag; set => captureTag = value; }

	public void InitOther(UnitProfileObject data)
	{
		ConnectSkill = data.connectSkill;
		var 유닛_점령점수 = Stats.GetData().GetValue(StatsType.유닛_점령점수, SymbolType.Number);
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
}
