using System;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEditor;

using UnityEngine;

using static StrategyGamePlayData;

[CreateAssetMenu(fileName = "KeyPairProjectile", menuName = "Scriptable Objects/KeyPairAssets/KeyPairProjectile")]
public class KeyPairProjectile : KeyPairAssets<StrategyGamePlayData.ProjectileKey, KeyPairProjectile.ProjectileInfo>
{
	public static KeyPairProjectile Load(string name)
	{
		string path = $"{nameof(KeyPairProjectile)}/{name}";
		var load = Resources.Load<KeyPairProjectile>(path);
		return load;
	}
#if UNITY_EDITOR
	#region SetGeneralType
	[ButtonGroup(order: -5)]
	void ListClear()
	{
		Clear();
	}
	[ButtonGroup]
	void SetGeneralType()
	{
		foreach (var item in GeneralNames)
		{
			AddAsset(item.Key, new(item.Key, item.Value));
		}
	}
	Dictionary<StrategyGamePlayData.ProjectileKey, string> GeneralNames = new Dictionary<StrategyGamePlayData.ProjectileKey, string>
	{
		[ StrategyGamePlayData.ProjectileKey.일반탄_소형] =  "일반탄_소형" ,
		[ StrategyGamePlayData.ProjectileKey.일반탄_중형] =  "일반탄_중형" ,
		[ StrategyGamePlayData.ProjectileKey.일반탄_대형] =  "일반탄_대형" ,

		[ StrategyGamePlayData.ProjectileKey.관통탄_소형] =  "관통탄_소형" ,
		[ StrategyGamePlayData.ProjectileKey.관통탄_중형] =  "관통탄_중형" ,
		[ StrategyGamePlayData.ProjectileKey.관통탄_대형] =  "관통탄_대형" ,

		[ StrategyGamePlayData.ProjectileKey.관통특화탄_소형] =  "관통특화탄_소형" ,
		[ StrategyGamePlayData.ProjectileKey.관통특화탄_중형] =  "관통특화탄_중형" ,
		[ StrategyGamePlayData.ProjectileKey.관통특화탄_대형] =  "관통특화탄_대형" ,

		[ StrategyGamePlayData.ProjectileKey.폭발탄_소형] =  "폭발탄_소형" ,
		[ StrategyGamePlayData.ProjectileKey.폭발탄_중형] =  "폭발탄_중형" ,
		[ StrategyGamePlayData.ProjectileKey.폭발탄_대형] =  "폭발탄_대형" ,

		[ StrategyGamePlayData.ProjectileKey.관통특화탄_소형] =  "관통특화탄_소형" ,
		[ StrategyGamePlayData.ProjectileKey.관통특화탄_중형] =  "관통특화탄_중형" ,
		[ StrategyGamePlayData.ProjectileKey.관통특화탄_대형] =  "관통특화탄_대형" ,

		[ StrategyGamePlayData.ProjectileKey.폭발특화탄_소형] =  "폭발특화탄_소형" ,
		[ StrategyGamePlayData.ProjectileKey.폭발특화탄_중형] =  "폭발특화탄_중형" ,
		[ StrategyGamePlayData.ProjectileKey.폭발특화탄_대형] =  "폭발특화탄_대형" ,

		[ StrategyGamePlayData.ProjectileKey.에너지탄_소형] =  "에너지탄_소형" ,
		[ StrategyGamePlayData.ProjectileKey.에너지탄_중형] =  "에너지탄_중형" ,
		[ StrategyGamePlayData.ProjectileKey.에너지탄_대형] =  "에너지탄_대형" ,
	};
	#endregion
	#region SetUniqueType
	[ButtonGroup]
	void SetUniqueType()
	{
		foreach (var item in UniquelNames)
		{
			AddAsset(item.Key, new(item.Key, item.Value));
		}
	}
	Dictionary<StrategyGamePlayData.ProjectileKey, string> UniquelNames = new Dictionary<StrategyGamePlayData.ProjectileKey, string>
	{

	};
	#endregion
	[ButtonGroup("SetupProfiles", order: -4)]
	private void SetupProfiles()
	{
		if (KeyPairTargetList == null || KeyPairTargetList.Length == 0)
		{
			Debug.LogWarning($"{name}: KeyPairTargetList is empty.");
			return;
		}

		string basePath = $"Assets/Resources/ScriptableObject/ProjectileProfiles";
		if (!System.IO.Directory.Exists(basePath))
			System.IO.Directory.CreateDirectory(basePath);

		int updatedCount = 0;
		for (int i = 0 ; i < KeyPairTargetList.Length ; i++)
		{
			var pair = KeyPairTargetList[i];
			ProjectileInfo info = pair.asset;
			ProjectileKey key = info.ProjectileKey;
			string name = info.DisplayName;

			// 이미 존재하면 패스
			if (info.ProjectileProfileObject != null)
				continue;

			// 검색 경로
			string assetName = key.ToString();
			string assetPath = $"{basePath}/{assetName}.asset";

			// 기존 파일이 있는지 검사
			ProjectileProfileObject profile = null;

			profile = AssetDatabase.LoadAssetAtPath<ProjectileProfileObject>(assetPath);

			// 없다면 새로 생성
			if (profile == null)
			{
				profile = ScriptableObject.CreateInstance<ProjectileProfileObject>();
				profile.name = assetName;
				profile.projectileKey = key;
				profile.displayName = name;

				AssetDatabase.CreateAsset(profile, assetPath);
				AssetDatabase.SaveAssets();
				Debug.Log($"[SetupUnitProfiles] Created new ProjectileProfileObject: {assetPath}");
			}

			info = new ProjectileInfo(key, name, profile);
			KeyPairTargetList[i] = new KeyPairAssetsStruct(key, info);
			updatedCount++;
		}

		EditorUtility.SetDirty(this);
		AssetDatabase.SaveAssets();

		Debug.Log($"[{name}] SetupUnitProfiles completed. Updated {updatedCount} entries.");
	}
#endif
	[Serializable]
	public struct ProjectileInfo
	{
		[ShowIf("@projectileProfileObject == null"), SerializeField]
		private StrategyGamePlayData.ProjectileKey projectileKey;
		[ShowIf("@projectileProfileObject == null"), SerializeField]
		private string displayName;
		[SerializeField]
		private ProjectileProfileObject projectileProfileObject;

		public readonly string DisplayName => ProjectileProfileObject == null ? displayName : ProjectileProfileObject.displayName;
		public readonly ProjectileKey ProjectileKey => ProjectileProfileObject == null ? projectileKey : ProjectileProfileObject.projectileKey;
		public readonly ProjectileProfileObject ProjectileProfileObject => projectileProfileObject;

		public ProjectileInfo(ProjectileKey key, string displayName) : this()
		{
			this.displayName = displayName;
			this.projectileKey = key;
			projectileProfileObject = null;
		}
		public ProjectileInfo(ProjectileKey key, string displayName, ProjectileProfileObject profileObject) : this()
		{
			this.displayName = displayName;
			this.projectileKey = key;
			this.projectileProfileObject = profileObject;
		}
	}
}