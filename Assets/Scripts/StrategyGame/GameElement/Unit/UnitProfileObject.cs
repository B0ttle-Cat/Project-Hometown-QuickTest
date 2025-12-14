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
			var profileData = unit.ProfileData;
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
			var profileData = unit.ProfileData;
			profileData.unitKey = unitKey;
			profileData.displayName = displayName;

			unit.Init(this);
		}
	}
#endif
}
