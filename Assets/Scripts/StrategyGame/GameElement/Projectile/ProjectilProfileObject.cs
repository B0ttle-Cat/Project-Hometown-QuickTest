using Sirenix.OdinInspector;

using UnityEditor;

using UnityEngine;

using static StrategyGamePlayData;

[CreateAssetMenu(fileName = "ProjectileProfileObject", menuName = "Scriptable Objects/StrategyGame/ProjectileProfileObject")]
public class ProjectileProfileObject : ScriptableObject
{
	[InlineButton("CreatePrefab","New",ShowIf = "@prefab == null")]
	public GameObject prefab;

	public string displayName;

	[InlineButton("PushData"), InlineButton("PullData")]
	public ProjectileKey projectileKey;

	// 단일 Stats 관리
	[SerializeField, InlineProperty, HideLabel]
	public ProjectileStatsData statsData = new ProjectileStatsData();

#if UNITY_EDITOR
	private void CreatePrefab()
	{
		string basePath = "Assets/Resources/Prefabs/ProjectileObject/_ProjectileObject.prefab";
		string newPrefabPath = $"Assets/Resources/Prefabs/ProjectileObject/{projectileKey}.prefab";

		GameObject basePrefab = prefab != null
			? PrefabUtility.GetCorrespondingObjectFromOriginalSource(prefab)
			: AssetDatabase.LoadAssetAtPath<GameObject>(basePath);

		if (basePrefab == null)
		{
			Debug.LogError($"Base prefab not found at {basePath}");
			return;
		}

		if (prefab != null && prefab.name == projectileKey.ToString())
		{
			Debug.Log($"Prefab '{projectileKey}' already exists. Creation skipped.");
			return;
		}

		GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
		instance.name = projectileKey.ToString();

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

		Object.DestroyImmediate(instance);
	}

	private void PullData()
	{
		if (prefab == null)
		{
			Debug.LogWarning("Prefab is null. Cannot pull data.");
			return;
		}

		if (!prefab.TryGetComponent<ProjectileObject>(out var obj))
		{
			Debug.LogWarning("Prefab does not contain ProjectileObject component.");
			return;
		}

		if (obj.StatsData == null)
		{
			Debug.LogWarning("ProjectileObject.StatsData is null. Cannot pull data.");
			return;
		}

		// prefab → profile(statsData)
		statsData = obj.StatsData.Copy();

		EditorUtility.SetDirty(this);
		Debug.Log($"Pulled StatsData from prefab '{prefab.name}'.");
	}

	private void PushData()
	{
		if (prefab == null)
		{
			Debug.LogWarning("Prefab is null. Cannot push data.");
			return;
		}

		if (!prefab.TryGetComponent<ProjectileObject>(out var obj))
		{
			Debug.LogWarning("Prefab does not contain ProjectileObject component.");
			return;
		}

		// profile(statsData) → prefab
		obj.Init(this);

		EditorUtility.SetDirty(prefab);
		Debug.Log($"Pushed StatsData to prefab '{prefab.name}'.");
	}
	private void Reset() { SetTestStatsValue(); }
	[Button]
	private void SetTestStatsValue()
	{
		statsData = new ProjectileStatsData(
			weaponType : WeaponType.일반,
			moveStartSpeed : 10f,
			isShiftSpeed : false,
			moveMaxSpeed : 20f,
			moveSpeedCurve : AnimationCurve.Linear(0, 0, 1, 1),
			timeFromStartToMaxSpeed : 2f,
			homingEnabled : false,
			homingActivationDelay : 0.0f,
			homingTurnSpeed : 180f,
			homingTurnSpeedWhenMaxSpeed : 180f,
			homingLimitAngle : 180f,
			homingLimitDistance : float.PositiveInfinity,
			cepEnabled : false,
			cepRadius : 3f,
			cepProbability : 0.9f,
			cepReapply : false,
			cepReapplyMinMaxTime : new Vector2(1f, 3f),
			lifeTime : 10f,
			destroyDelayAfterHit : 0.1f,
			collisionRadius : 0.1f,
			hitDamageMultiplier : 1f,
			hitEffectsFlag : StatusEffectsFlag.None,
			hitEffectsTimeMultiplier : 1f,
			piercingEnable : false,
			piercingMinMaxCount : new Vector2Int(1, 1),
			piercingFalloffCurve : AnimationCurve.Linear(0, 1, 1, 0),
			explosionEnabled : false,
			explosionMinMaxRadius : new Vector2(1f, 5f),
			explosionFalloffCurve : AnimationCurve.Linear(0, 1, 1, 0)
		);
	}
#endif
}
