using System;

using Sirenix.OdinInspector;

using UnityEngine;

using static StrategyGamePlayData;

[CreateAssetMenu(fileName = "UnitProfileObject", menuName = "Scriptable Objects/StrategyGame/UnitProfileObject")]
public class UnitProfileObject : ScriptableObject
{
	public GameObject unitPrefab;
	[InlineButton("PushData"), InlineButton("PullData")]
	public UnitKey unitKey;
#if UNITY_EDITOR
	private void PullData()
	{
		if (unitPrefab == null) return;
		if(Enum.TryParse(typeof(UnitKey), unitPrefab.name, out var tryKey))
		{
			unitKey = (UnitKey)tryKey;
		}
		else
		{
			unitKey = UnitKey.None;
		}
		if (unitPrefab.TryGetComponent<UnitObject>(out var unit))
		{
			var profileData = unit.ProfileData;
			weaponType = profileData.weaponType;
			protectType = profileData.protectType;

			var statsData = unit.StatsData;
			유닛_인력 = statsData.GetValue(StatsType.유닛_인력);
			유닛_물자 = statsData.GetValue(StatsType.유닛_물자);
			유닛_전력 = statsData.GetValue(StatsType.유닛_전력);

			유닛_최대내구도 = statsData.GetValue(StatsType.유닛_최대내구도);
			유닛_현재내구도 = statsData.GetValue(StatsType.유닛_현재내구도);

			유닛_공격력 = statsData.GetValue(StatsType.유닛_공격력);
			유닛_방어력 = statsData.GetValue(StatsType.유닛_방어력);
			유닛_치유력 = statsData.GetValue(StatsType.유닛_치유력);
			유닛_회복력 = statsData.GetValue(StatsType.유닛_회복력);
			유닛_이동속도 = statsData.GetValue(StatsType.유닛_이동속도);
			유닛_점령점수 = statsData.GetValue(StatsType.유닛_점령점수);

			유닛_치명공격력 = statsData.GetValue(StatsType.유닛_치명공격력);
			유닛_치명공격배율 = statsData.GetValue(StatsType.유닛_치명공격배율);
			유닛_치명방어력 = statsData.GetValue(StatsType.유닛_치명방어력);

			유닛_관통레벨 = statsData.GetValue(StatsType.유닛_관통레벨);
			유닛_장갑레벨 = statsData.GetValue(StatsType.유닛_장갑레벨);
			유닛_EMP저항레벨 = statsData.GetValue(StatsType.유닛_EMP저항레벨);

			유닛_상태이상적용레벨 = statsData.GetValue(StatsType.유닛_상태이상적용레벨);
			유닛_상태이상저항레벨 = statsData.GetValue(StatsType.유닛_상태이상저항레벨);

			유닛_공격명중기회 = statsData.GetValue(StatsType.유닛_공격명중기회);
			유닛_공격회피기회 = statsData.GetValue(StatsType.유닛_공격회피기회);
			유닛_치명명중기회 = statsData.GetValue(StatsType.유닛_치명명중기회);
			유닛_치명회피기회 = statsData.GetValue(StatsType.유닛_치명회피기회);

			유닛_명중피격수 = statsData.GetValue(StatsType.유닛_명중피격수);
			유닛_연속공격횟수 = statsData.GetValue(StatsType.유닛_연속공격횟수);
			유닛_조준지연시간 = statsData.GetValue(StatsType.유닛_조준지연시간);
			유닛_연속공격지연시간 = statsData.GetValue(StatsType.유닛_연속공격지연시간);
			유닛_재공격지연시간 = statsData.GetValue(StatsType.유닛_재공격지연시간);

			유닛_공격소모_물자 = statsData.GetValue(StatsType.유닛_공격소모_물자);
			유닛_공격소모_전력 = statsData.GetValue(StatsType.유닛_공격소모_전력);

			유닛_공격범위 = statsData.GetValue(StatsType.유닛_공격범위);
			유닛_행동범위 = statsData.GetValue(StatsType.유닛_행동범위);
			유닛_시야범위 = statsData.GetValue(StatsType.유닛_시야범위);
		}
	}
	private void PushData()
	{
		if (unitPrefab == null) return;
		if (unitPrefab.TryGetComponent<UnitObject>(out var unit))
		{
			var profileData = unit.ProfileData;
			profileData.unitKey = unitKey;
			profileData.weaponType = weaponType;
			profileData.protectType = protectType;
			unit.Profile.SetData(profileData);

			var statsData = unit.StatsData;
			statsData.SetValue(StatsType.유닛_인력, 유닛_인력);
			statsData.SetValue(StatsType.유닛_물자, 유닛_물자);
			statsData.SetValue(StatsType.유닛_전력, 유닛_전력);

			statsData.SetValue(StatsType.유닛_최대내구도, 유닛_최대내구도);
			statsData.SetValue(StatsType.유닛_현재내구도, 유닛_현재내구도);

			statsData.SetValue(StatsType.유닛_공격력, 유닛_공격력);
			statsData.SetValue(StatsType.유닛_방어력, 유닛_방어력);
			statsData.SetValue(StatsType.유닛_치유력, 유닛_치유력);
			statsData.SetValue(StatsType.유닛_회복력, 유닛_회복력);
			statsData.SetValue(StatsType.유닛_이동속도, 유닛_이동속도);
			statsData.SetValue(StatsType.유닛_점령점수, 유닛_점령점수);

			statsData.SetValue(StatsType.유닛_치명공격력, 유닛_치명공격력);
			statsData.SetValue(StatsType.유닛_치명공격배율, 유닛_치명공격배율);
			statsData.SetValue(StatsType.유닛_치명방어력, 유닛_치명방어력);

			statsData.SetValue(StatsType.유닛_관통레벨, 유닛_관통레벨);
			statsData.SetValue(StatsType.유닛_장갑레벨, 유닛_장갑레벨);
			statsData.SetValue(StatsType.유닛_EMP저항레벨, 유닛_EMP저항레벨);

			statsData.SetValue(StatsType.유닛_상태이상적용레벨, 유닛_상태이상적용레벨);
			statsData.SetValue(StatsType.유닛_상태이상저항레벨, 유닛_상태이상저항레벨);

			statsData.SetValue(StatsType.유닛_공격명중기회, 유닛_공격명중기회);
			statsData.SetValue(StatsType.유닛_공격회피기회, 유닛_공격회피기회);
			statsData.SetValue(StatsType.유닛_치명명중기회, 유닛_치명명중기회);
			statsData.SetValue(StatsType.유닛_치명회피기회, 유닛_치명회피기회);

			statsData.SetValue(StatsType.유닛_명중피격수, 유닛_명중피격수);
			statsData.SetValue(StatsType.유닛_연속공격횟수, 유닛_연속공격횟수);
			statsData.SetValue(StatsType.유닛_조준지연시간, 유닛_조준지연시간);
			statsData.SetValue(StatsType.유닛_연속공격지연시간, 유닛_연속공격지연시간);
			statsData.SetValue(StatsType.유닛_재공격지연시간, 유닛_재공격지연시간);

			statsData.SetValue(StatsType.유닛_공격소모_물자, 유닛_공격소모_물자);
			statsData.SetValue(StatsType.유닛_공격소모_전력, 유닛_공격소모_전력);

			statsData.SetValue(StatsType.유닛_공격범위, 유닛_공격범위);
			statsData.SetValue(StatsType.유닛_행동범위, 유닛_행동범위);
			statsData.SetValue(StatsType.유닛_시야범위, 유닛_시야범위);
			unit.Stats.SetData(statsData);
			//UnityEditor.PrefabUtility.SavePrefabAsset(unitPrefab);
		}
	}
#endif
	public WeaponType weaponType;
	public ProtectionType protectType;

	[Header("Stats")]
	public int 유닛_인력;
	public int 유닛_물자;
	public int 유닛_전력;
	[Space]
	public int 유닛_최대내구도;
	public int 유닛_현재내구도;

	public int 유닛_공격력;
	public int 유닛_방어력;
	public int 유닛_치유력;
	public int 유닛_회복력;
	public int 유닛_이동속도;
	public int 유닛_점령점수;

	public int 유닛_치명공격력;
	public int 유닛_치명공격배율;
	public int 유닛_치명방어력;

	[Space]
	public int 유닛_관통레벨;
	public int 유닛_장갑레벨;
	public int 유닛_EMP저항레벨;

	public int 유닛_상태이상적용레벨;
	public int 유닛_상태이상저항레벨;

	[Space]
	public int 유닛_공격명중기회;
	public int 유닛_공격회피기회;
	public int 유닛_치명명중기회;
	public int 유닛_치명회피기회;
	[Space]
	public int 유닛_명중피격수;
	public int 유닛_연속공격횟수;
	public int 유닛_조준지연시간;
	public int 유닛_연속공격지연시간;
	public int 유닛_재공격지연시간;
	[Space]
	public int 유닛_공격소모_물자;
	public int 유닛_공격소모_전력;
	[Space]
	public int 유닛_공격범위;
	public int 유닛_행동범위;
	public int 유닛_시야범위;

	[Space]
	public SkillProfile[] connectSkill;

	public StatsValue[] ConvertStatsValues()
	{
		return new StatsValue[]
		{
			new (StatsType.유닛_인력, 유닛_인력),
			new (StatsType.유닛_물자, 유닛_물자),
			new (StatsType.유닛_전력, 유닛_전력),

			new (StatsType.유닛_최대내구도, 유닛_최대내구도),
			new (StatsType.유닛_현재내구도, 유닛_현재내구도),
			new (StatsType.유닛_공격력, 유닛_공격력),
			new (StatsType.유닛_방어력, 유닛_방어력),
			new (StatsType.유닛_치유력, 유닛_치유력),
			new (StatsType.유닛_회복력, 유닛_회복력),
			new (StatsType.유닛_이동속도, 유닛_이동속도),
			new (StatsType.유닛_점령점수, 유닛_점령점수),
			new (StatsType.유닛_치명공격력, 유닛_치명공격력),
			new (StatsType.유닛_치명공격배율, 유닛_치명공격배율),
			new (StatsType.유닛_치명방어력, 유닛_치명방어력),

			new (StatsType.유닛_관통레벨, 유닛_관통레벨),
			new (StatsType.유닛_장갑레벨, 유닛_장갑레벨),
			new (StatsType.유닛_EMP저항레벨, 유닛_EMP저항레벨),

			new (StatsType.유닛_공격명중기회, 유닛_공격명중기회),
			new (StatsType.유닛_공격회피기회, 유닛_공격회피기회),
			new (StatsType.유닛_치명명중기회, 유닛_치명명중기회),
			new (StatsType.유닛_치명회피기회, 유닛_치명회피기회),

			new (StatsType.유닛_명중피격수, 유닛_명중피격수),
			new (StatsType.유닛_연속공격횟수, 유닛_연속공격횟수),
			new (StatsType.유닛_조준지연시간, 유닛_조준지연시간),
			new (StatsType.유닛_연속공격지연시간, 유닛_연속공격지연시간),
			new (StatsType.유닛_재공격지연시간, 유닛_재공격지연시간),

			new (StatsType.유닛_공격소모_물자, 유닛_공격소모_물자),
			new (StatsType.유닛_공격소모_전력, 유닛_공격소모_전력),

			new (StatsType.유닛_공격범위, 유닛_공격범위),
			new (StatsType.유닛_행동범위, 유닛_행동범위),
			new (StatsType.유닛_시야범위, 유닛_시야범위)
		};
	}
}
