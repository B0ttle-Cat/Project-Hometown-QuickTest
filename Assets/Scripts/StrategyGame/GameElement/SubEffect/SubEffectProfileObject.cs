using Sirenix.OdinInspector;

using UnityEditor;

using UnityEngine;

[CreateAssetMenu(fileName = "SubEffectProfileObject", menuName = "Scriptable Objects/StrategyGame/subEffectProfileObject")]
public class SubEffectProfileObject : ScriptableObject
{
	[InlineButton("CreatePrefab","New",ShowIf = "@prefab == null")]
	public GameObject prefab;
    public string displayName;
	public StrategyGamePlayData.SubEffectKey subEffectKey;
#if UNITY_EDITOR
	private void CreatePrefab()
	{
		string basePath = "Assets/Resources/Prefabs/SubEffectObject/_SubEffectObject.prefab";
		string newPrefabPath = $"Assets/Resources/Prefabs/SubEffectObject/{subEffectKey}.prefab";

		GameObject basePrefab = prefab != null
			? PrefabUtility.GetCorrespondingObjectFromOriginalSource(prefab)
			: AssetDatabase.LoadAssetAtPath<GameObject>(basePath);

		if (basePrefab == null)
		{
			Debug.LogError($"Base prefab not found at {basePath}");
			return;
		}

		if (prefab != null && prefab.name == subEffectKey.ToString())
		{
			Debug.Log($"Prefab '{subEffectKey}' already exists. Creation skipped.");
			return;
		}

		GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
		instance.name = subEffectKey.ToString();

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
#endif

}
