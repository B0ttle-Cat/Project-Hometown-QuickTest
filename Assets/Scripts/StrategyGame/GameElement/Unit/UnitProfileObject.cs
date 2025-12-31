using System;

using Sirenix.OdinInspector;

using UnityEditor;

using UnityEngine;

using static StrategyGamePlayData;

using Object = UnityEngine.Object;

[CreateAssetMenu(fileName = "ProjectileProfileObject", menuName = "Scriptable Objects/StrategyGame/ProjectileProfileObject")]
public class UnitProfileObject : ScriptableObject
{
	[InlineButton("CreatePrefab","New",ShowIf = "@prefab == null")]
	public GameObject prefab;
	[InlineButton("PushData"), InlineButton("PullData")]
	public UnitKey unitKey;
	public string displayName;

	[BoxGroup("Sprite Image")]
	[HorizontalGroup("Sprite Image/H"), HideLabel, PreviewField(100)]
	public Sprite unitFullBodySprite;
	[HorizontalGroup("Sprite Image/H"), HideLabel, PreviewField(100)]
	public Sprite unitPortraitSprite;

	[Title("Stats"), InlineProperty, HideLabel]
	public UnitStatsData stats;

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
			var profileData = unit.InstanceData;
			displayName = profileData.displayName;

			var objStats = unit.StatsData;
			stats = new UnitStatsData(objStats);
		}
	}
	private void PushData()
	{
		if (prefab == null) return;
		if (prefab.TryGetComponent<UnitObject>(out var unit))
		{
			var profileData = unit.InstanceData;
			profileData.unitKey = unitKey;
			profileData.displayName = displayName;

			unit.Init(this);
		}
	}

	[Button]
	void SetTestStatsValue()
	{
		stats = new UnitStatsData();
		// --- 💸 Cost (비용) ---
		// 유닛_인력, 유닛_물자, 유닛_전력은 '배치 비용'에 해당합니다.
		stats.GetCost.DeploymentCostPersonnel = 1;
		stats.GetCost.DeploymentCostMaterial = 1;
		stats.GetCost.DeploymentCostElectric = 1;

		// 유닛_공격소모_물자, 유닛_공격소모_전력은 '공격 비용'에 해당합니다.
		stats.GetCost.AttackCostMaterial = 1;
		stats.GetCost.AttackCostElectric = 1;

		// --- 🛡️ Common (공통) ---
		stats.GetCommon.MaxDurability = 1000;      // 유닛_최대내구도
		stats.GetCommon.HealingPower = 10;         // 유닛_치유력
		stats.GetCommon.RecoveryPower = 1;         // 유닛_회복력
		stats.GetCommon.MovementSpeed = 1.00f;     // 유닛_이동속도 (float 형변환 필요)
		stats.GetCommon.CaptureScore = 1;          // 유닛_점령점수

		// --- 🔭 Range (범위) ---
		// Vector4(x: LimitMin, y: StartMin, z: StartMax, w: LimitMax)
		stats.GetRange.AttackRange = new Vector4(
			0f,     // 유닛_공격범위_종료최소 (x)
			0f,     // 유닛_공격범위_시작최소 (y)
			8.00f,  // 유닛_공격범위_시작최대 (z)
			10.00f  // 유닛_공격범위_종료최대 (w)
		);
		stats.GetRange.ActionRange = 11.00f;       // 유닛_행동범위
		stats.GetRange.VisionRange = 15.00f;       // 유닛_시야범위

		// --- ⚙️ Cycle (공격 주기) ---
		stats.GetCycle.AimDelayTime = 1.00f;       // 유닛_조준지연시간
		stats.GetCycle.ContinuousAttackDelayTime = 0.10f; // 유닛_연속공격지연시간
		stats.GetCycle.ReattackDelayTime = 0.50f;    // 유닛_재공격지연시간
		stats.GetCycle.ReloadTime = 3.00f;         // 유닛_재장전시간

		// --- 彈 Ammo (탄약) ---
		stats.GetAmmo.AmmunitionCapacity = 8;      // 유닛_탄용량
		stats.GetAmmo.ConcurrentAttackCount = 1;   // 유닛_동시공격개수
		stats.GetAmmo.ContinuousAttackCount = 3;   // 유닛_연속공격횟수

		// --- 💥 Offense (공격) ---
		stats.GetOffense.projectileKey = ProjectileKey.일반탄_소형;

		stats.GetOffense.AttackPower = 10;           // 유닛_공격력
		stats.GetOffense.CriticalAttackPower = 30;   // 유닛_치명공격력
		stats.GetOffense.CriticalDamageRatio = 200;  // 유닛_치명피해율

		stats.GetOffense.PenetrationLevel = 1;       // 유닛_관통레벨
		stats.GetOffense.EMPImpactLevel = 1;         // 유닛_EMP충격레벨
		stats.GetOffense.StatusPotencyLevel = 1;     // 유닛_상태이상적용레벨

		stats.GetOffense.HitChanceScore = 70;        // 유닛_공격명중기회
		stats.GetOffense.CriticalChanceScore = 30;   // 유닛_치명명중기회

		// --- 🛡️ Defense (방어) ---
		stats.GetDefense.protectType = ProtectionType.일반;

		stats.GetDefense.AntiAttackPower = 1;        // 유닛_방어력
		stats.GetDefense.AntiCriticalAttackPower = 10; // 유닛_치명방어력

		stats.GetDefense.AntiPenetrationLevel = 1;   // 유닛_장갑레벨 (Anti-Penetration)
		stats.GetDefense.AntiEMPImpactLevel = 1;     // 유닛_EMP방호레벨
		stats.GetDefense.AntiStatusPotencyLevel = 1; // 유닛_상태이상저항레벨

		stats.GetDefense.AntiHitChanceScore = 10;    // 유닛_공격회피기회 (Anti-Hit Chance)
		stats.GetDefense.AntiCriticalChanceScore = 20; // 유닛_치명회피기회 (Anti-Critical Chance)
	}
#endif
}
