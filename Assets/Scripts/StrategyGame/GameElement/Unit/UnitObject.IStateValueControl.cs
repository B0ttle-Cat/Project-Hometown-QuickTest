using UnityEngine;

using static StrategyGamePlayData;
using static StrategyGamePlayData.UnitData.Skill;
public partial class UnitObject : IStatsValueControl
{
	public IStatsValueControl StatsValue => this;

	private StatsGroup skillBuffGroup;
	public StatsList MainStatsList => StatsData.GetStatsList();
	public StatsGroup SkillBuffGroup => skillBuffGroup ??= new StatsGroup();

	partial void InitProfileObject(UnitProfileObject profileObj)
	{
		if (profileObj == null) return;

		var mainStatsList = new StatsList(profileObj.ConvertStatsValues());
		mainStatsList.SetValue(StatsType.유닛_현재내구도, mainStatsList.GetValue(StatsType.유닛_최대내구도));
		Stats = new UnitData.Stats(new()
		{
			stats = mainStatsList
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

	int IStatsValueControl.GetStatsValue(StatsType type)
	{
		if (StrategyManager.IsNotReadyScene) return 0;
		int value = MainStatsList.GetValueInt(type) + SkillBuffGroup.GetValueInt(type);
		return value;
	}
	float IStatsValueControl.GetStatsValuePercent(StatsType type)
	{
		return StatsValue.GetStatsValue(type) * 0.01f;
	}
	void IStatsValueControl.SetValueInMainStats(StatsType type, int value)
	{
		if (StrategyManager.IsNotReadyScene) return;
		MainStatsList.SetValue(type, value);
	}
	void IStatsValueControl.SetValueInMainStatsPercent(StatsType type, float valuePercent)
	{
		StatsValue.SetValueInMainStats(type, Mathf.RoundToInt(valuePercent * 100f));
	}
}
public partial class UnitObject : ICombatCommon, ICombatOffense, ICombatDefance
{
	public ICombatCommon ThisCombatStats => this;
	public ICombatOffense ThisOffense => this;
	public ICombatDefance ThisDefance => this;

	public void TakeDamage(int damage, CombatUtility.DamageFlag flag)
	{
		int currentDurability = ThisCombatStats.CurrentDurability;
		currentDurability -= damage;
		ThisCombatStats.SetValueInMainStats(StatsType.유닛_현재내구도, currentDurability);

		// Show Demage Effect


		if (currentDurability <= 0)
		{
			DamageDeath();
		}
	}
}