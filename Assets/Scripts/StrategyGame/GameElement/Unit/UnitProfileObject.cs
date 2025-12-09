using System;

using Sirenix.OdinInspector;

using UnityEditor;

using UnityEngine;

using static StrategyGamePlayData;
using static StrategyGamePlayData.UnitData.Skill;

using Object = UnityEngine.Object;

[CreateAssetMenu(fileName = "ProjectileProfileObject", menuName = "Scriptable Objects/StrategyGame/ProjectileProfileObject")]
public class UnitProfileObject : ScriptableObject
{
	[InlineButton("CreatePrefab","New",ShowIf = "@prefab == null")]
	public GameObject prefab;
	[InlineButton("PushData"), InlineButton("PullData")]
	public UnitKey unitKey;
	public string displayName;
#if UNITY_EDITOR
	private void CreatePrefab()
	{
		string basePath = "Assets/Resources/Prefabs/UnitObject/_UnitObject.prefab";
		string newPrefabPath = $"Assets/Resources/Prefabs/UnitObject/{unitKey}.prefab";

		// 이미 prefab 이 존재한다면, 그걸 원본으로 사용
		GameObject basePrefab = prefab != null
			? PrefabUtility.GetCorrespondingObjectFromOriginalSource(prefab)
			: AssetDatabase.LoadAssetAtPath<GameObject>(basePath);

		if (basePrefab == null)
		{
			Debug.LogError($"Base prefab not found at {basePath}");
			return;
		}

		// prefab이 이미 있고 이름이 같은 경우 생성 중단
		if (prefab != null && prefab.name == unitKey.ToString())
		{
			Debug.Log($"Prefab '{unitKey}' already exists. Creation skipped.");
			return;
		}

		// 인스턴스 생성
		GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
		instance.name = unitKey.ToString();

		// Prefab 저장 (Variant로)
		GameObject variant = PrefabUtility.SaveAsPrefabAsset(instance, newPrefabPath);
		if (variant != null)
		{
			prefab = variant;
			EditorUtility.SetDirty(this);
			Debug.Log($"Created prefab variant: {newPrefabPath}");
		}
		else
		{
			Debug.LogError($"Failed to create prefab at {newPrefabPath}");
		}

		// 임시 오브젝트 정리
		Object.DestroyImmediate(instance);
	}

	private void PullData()
	{
		if (prefab == null) return;
		if (Enum.TryParse(typeof(UnitKey), prefab.name, out var tryKey))
		{
			unitKey = (UnitKey)tryKey;
		}
		else
		{
			unitKey = UnitKey.None;
		}
		if (prefab.TryGetComponent<UnitObject>(out var unit))
		{
			var profileData = unit.ProfileData;
			displayName = profileData.displayName;
			protectType = profileData.protectType;

			var statsData = unit.StatsData;
			유닛_인력 = statsData.GetValue(StatsType.유닛_인력);
			유닛_물자 = statsData.GetValue(StatsType.유닛_물자);
			유닛_전력 = statsData.GetValue(StatsType.유닛_전력);

			유닛_최대내구도 = statsData.GetValue(StatsType.유닛_최대내구도);

			유닛_공격력 = statsData.GetValue(StatsType.유닛_공격력);
			유닛_방어력 = statsData.GetValue(StatsType.유닛_방어력);
			유닛_치유력 = statsData.GetValue(StatsType.유닛_치유력);
			유닛_회복력 = statsData.GetValue(StatsType.유닛_회복력);
			유닛_이동속도 = statsData.GetValue(StatsType.유닛_이동속도_c);
			유닛_점령점수 = statsData.GetValue(StatsType.유닛_점령점수);

			유닛_치명공격력 = statsData.GetValue(StatsType.유닛_치명공격력);
			유닛_치명공격백분율 = statsData.GetValue(StatsType.유닛_치명공격백분율);
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
			유닛_조준지연시간 = statsData.GetValue(StatsType.유닛_조준지연시간_c);
			유닛_연속공격지연시간 = statsData.GetValue(StatsType.유닛_연속공격지연시간_c);
			유닛_재공격지연시간 = statsData.GetValue(StatsType.유닛_재공격지연시간_c);

			유닛_탄용량 = statsData.GetValue(StatsType.유닛_탄용량);
			유닛_잔탄수 = statsData.GetValue(StatsType.유닛_사용탄수);
			유닛_재장전시간 = statsData.GetValue(StatsType.유닛_재장전시간_c);

			유닛_공격소모_물자 = statsData.GetValue(StatsType.유닛_공격소모_물자);
			유닛_공격소모_전력 = statsData.GetValue(StatsType.유닛_공격소모_전력);

			유닛_공격범위_종료최소 = statsData.GetValue(StatsType.유닛_공격범위_종료최소_c);
			유닛_공격범위_시작최소 = statsData.GetValue(StatsType.유닛_공격범위_시작최소_c);
			유닛_공격범위_시작최대 = statsData.GetValue(StatsType.유닛_공격범위_시작최대_c);
			유닛_공격범위_종료최대 = statsData.GetValue(StatsType.유닛_공격범위_종료최대_c);
			유닛_행동범위 = statsData.GetValue(StatsType.유닛_행동범위_c);
			유닛_시야범위 = statsData.GetValue(StatsType.유닛_시야범위_c);
		}
	}
	private void PushData()
	{
		if (prefab == null) return;
		if (prefab.TryGetComponent<UnitObject>(out var unit))
		{
			var profileData = unit.ProfileData;
			profileData.unitKey = unitKey;
			profileData.displayName = displayName;
			profileData.protectType = protectType;
			unit.Profile.SetData(profileData);

			var statsData = unit.StatsData;
			statsData.SetValue(StatsType.유닛_인력, 유닛_인력);
			statsData.SetValue(StatsType.유닛_물자, 유닛_물자);
			statsData.SetValue(StatsType.유닛_전력, 유닛_전력);

			statsData.SetValue(StatsType.유닛_최대내구도, 유닛_최대내구도);

			statsData.SetValue(StatsType.유닛_공격력, 유닛_공격력);
			statsData.SetValue(StatsType.유닛_방어력, 유닛_방어력);
			statsData.SetValue(StatsType.유닛_치유력, 유닛_치유력);
			statsData.SetValue(StatsType.유닛_회복력, 유닛_회복력);
			statsData.SetValue(StatsType.유닛_이동속도_c, 유닛_이동속도);
			statsData.SetValue(StatsType.유닛_점령점수, 유닛_점령점수);

			statsData.SetValue(StatsType.유닛_치명공격력, 유닛_치명공격력);
			statsData.SetValue(StatsType.유닛_치명공격백분율, 유닛_치명공격백분율);
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
			statsData.SetValue(StatsType.유닛_조준지연시간_c, 유닛_조준지연시간);
			statsData.SetValue(StatsType.유닛_연속공격지연시간_c, 유닛_연속공격지연시간);
			statsData.SetValue(StatsType.유닛_재공격지연시간_c, 유닛_재공격지연시간);

			statsData.SetValue(StatsType.유닛_탄용량, 유닛_탄용량);
			statsData.SetValue(StatsType.유닛_사용탄수, 유닛_잔탄수);
			statsData.SetValue(StatsType.유닛_재장전시간_c, 유닛_재장전시간);

			statsData.SetValue(StatsType.유닛_공격소모_물자, 유닛_공격소모_물자);
			statsData.SetValue(StatsType.유닛_공격소모_전력, 유닛_공격소모_전력);

			statsData.SetValue(StatsType.유닛_공격범위_종료최소_c, 유닛_공격범위_종료최소);
			statsData.SetValue(StatsType.유닛_공격범위_시작최소_c, 유닛_공격범위_시작최소);
			statsData.SetValue(StatsType.유닛_공격범위_시작최대_c, 유닛_공격범위_시작최대);
			statsData.SetValue(StatsType.유닛_공격범위_종료최대_c, 유닛_공격범위_종료최대);
			statsData.SetValue(StatsType.유닛_행동범위_c, 유닛_행동범위);
			statsData.SetValue(StatsType.유닛_시야범위_c, 유닛_시야범위);
			unit.Stats.SetData(statsData);
			//UnityEditor.PrefabUtility.SavePrefabAsset(prefab);
		}
	}
#endif

	public ProjectileKey projectileKey;
	public ProtectionType protectType;

	[FoldoutGroup("StatsData")] public int 유닛_인력;
	[FoldoutGroup("StatsData")] public int 유닛_물자;
	[FoldoutGroup("StatsData")] public int 유닛_전력;
	[Space]
	[FoldoutGroup("StatsData")] public int 유닛_최대내구도;
	[FoldoutGroup("StatsData")] public int 유닛_현재내구도;

	[FoldoutGroup("StatsData")] public int 유닛_공격력;
	[FoldoutGroup("StatsData")] public int 유닛_방어력;
	[FoldoutGroup("StatsData")] public int 유닛_치유력;
	[FoldoutGroup("StatsData")] public int 유닛_회복력;
	[FoldoutGroup("StatsData")] public int 유닛_이동속도;
	[FoldoutGroup("StatsData")] public int 유닛_점령점수;

	[FoldoutGroup("StatsData")] public int 유닛_치명공격력;
	[FoldoutGroup("StatsData")] public int 유닛_치명공격백분율;
	[FoldoutGroup("StatsData")] public int 유닛_치명방어력;

	[Space]
	[FoldoutGroup("StatsData")] public int 유닛_관통레벨;
	[FoldoutGroup("StatsData")] public int 유닛_장갑레벨;
	[FoldoutGroup("StatsData")] public int 유닛_EMP저항레벨;

	[FoldoutGroup("StatsData")] public int 유닛_상태이상적용레벨;
	[FoldoutGroup("StatsData")] public int 유닛_상태이상저항레벨;

	[Space]
	[FoldoutGroup("StatsData")] public int 유닛_공격명중기회;
	[FoldoutGroup("StatsData")] public int 유닛_공격회피기회;
	[FoldoutGroup("StatsData")] public int 유닛_치명명중기회;
	[FoldoutGroup("StatsData")] public int 유닛_치명회피기회;
	[Space]
	[FoldoutGroup("StatsData")] public int 유닛_명중피격수;
	[FoldoutGroup("StatsData")] public int 유닛_연속공격횟수;
	[FoldoutGroup("StatsData")] public int 유닛_조준지연시간;
	[FoldoutGroup("StatsData")] public int 유닛_연속공격지연시간;
	[FoldoutGroup("StatsData")] public int 유닛_재공격지연시간;
	[Space]
	[FoldoutGroup("StatsData")] public int 유닛_탄용량;
	[FoldoutGroup("StatsData")] public int 유닛_잔탄수;
	[FoldoutGroup("StatsData")] public int 유닛_재장전시간;

	[Space]
	[FoldoutGroup("StatsData")] public int 유닛_공격소모_물자;
	[FoldoutGroup("StatsData")] public int 유닛_공격소모_전력;
	[Space]
	[FoldoutGroup("StatsData")] public int 유닛_공격범위_종료최소;
	[FoldoutGroup("StatsData")] public int 유닛_공격범위_시작최소;
	[FoldoutGroup("StatsData")] public int 유닛_공격범위_시작최대;
	[FoldoutGroup("StatsData")] public int 유닛_공격범위_종료최대;
	[FoldoutGroup("StatsData")] public int 유닛_행동범위;
	[FoldoutGroup("StatsData")] public int 유닛_시야범위;

	[Space]
	public SkillData[] personalSkills;

	public StatsValue[] ConvertStatsValues()
	{
		return new StatsValue[]
		{
			new (StatsType.유닛_인력, 유닛_인력),
			new (StatsType.유닛_물자, 유닛_물자),
			new (StatsType.유닛_전력, 유닛_전력),

			new (StatsType.유닛_최대내구도, 유닛_최대내구도),
			new (StatsType.유닛_공격력, 유닛_공격력),
			new (StatsType.유닛_방어력, 유닛_방어력),
			new (StatsType.유닛_치유력, 유닛_치유력),
			new (StatsType.유닛_회복력, 유닛_회복력),
			new (StatsType.유닛_이동속도_c, 유닛_이동속도),
			new (StatsType.유닛_점령점수, 유닛_점령점수),
			new (StatsType.유닛_치명공격력, 유닛_치명공격력),
			new (StatsType.유닛_치명공격백분율, 유닛_치명공격백분율),
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
			new (StatsType.유닛_조준지연시간_c, 유닛_조준지연시간),
			new (StatsType.유닛_연속공격지연시간_c, 유닛_연속공격지연시간),
			new (StatsType.유닛_재공격지연시간_c, 유닛_재공격지연시간),

			new(StatsType.유닛_탄용량    , 유닛_탄용량    ),
			new(StatsType.유닛_사용탄수    , 유닛_잔탄수    ),
			new(StatsType.유닛_재장전시간_c, 유닛_재장전시간),

			new (StatsType.유닛_공격소모_물자, 유닛_공격소모_물자),
			new (StatsType.유닛_공격소모_전력, 유닛_공격소모_전력),

			new (StatsType.유닛_공격범위_종료최소_c, 유닛_공격범위_종료최소),
			new (StatsType.유닛_공격범위_시작최소_c, 유닛_공격범위_시작최소),
			new (StatsType.유닛_공격범위_시작최대_c, 유닛_공격범위_시작최대),
			new (StatsType.유닛_공격범위_종료최대_c, 유닛_공격범위_종료최대),
			new (StatsType.유닛_행동범위_c, 유닛_행동범위),
			new (StatsType.유닛_시야범위_c, 유닛_시야범위)
		};
	}

	[Button]
	private void SetTestStatsValue()
	{
		유닛_인력 = 1;
		유닛_물자 = 1;
		유닛_전력 = 1;

		유닛_최대내구도 = 1000;
		유닛_현재내구도 = 900;

		유닛_공격력 = 10;
		유닛_방어력 = 1;
		유닛_치유력 = 10;
		유닛_회복력 = 1;
		유닛_이동속도 = 1_00;
		유닛_점령점수 = 1;

		유닛_치명공격력 = 30;
		유닛_치명공격백분율 = 200;
		유닛_치명방어력 = 10;

		유닛_관통레벨 = 1;
		유닛_장갑레벨 = 1;
		유닛_EMP저항레벨 = 1;

		유닛_상태이상적용레벨 = 1;
		유닛_상태이상저항레벨 = 1;

		유닛_공격명중기회 = 70;
		유닛_공격회피기회 = 10;
		유닛_치명명중기회 = 30;
		유닛_치명회피기회 = 20;

		유닛_명중피격수 = 1;
		유닛_연속공격횟수 = 3;
		유닛_조준지연시간 = 1_00;
		유닛_연속공격지연시간 = 0_10;
		유닛_재공격지연시간 = 0_50;

		유닛_탄용량 = 8;
		유닛_잔탄수 = 8;
		유닛_재장전시간 = 3_00;

		유닛_공격소모_물자 = 1;
		유닛_공격소모_전력 = 1;

		유닛_공격범위_종료최소 = 0;
		유닛_공격범위_시작최소 = 0;
		유닛_공격범위_시작최대 = 8_00;
		유닛_공격범위_종료최대 = 1_000;
		유닛_행동범위 = 11_00;
		유닛_시야범위 = 15_00;
	}
}
