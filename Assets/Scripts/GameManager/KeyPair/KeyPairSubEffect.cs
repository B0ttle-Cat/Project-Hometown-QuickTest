using System;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEditor;

using UnityEngine;

using static StrategyGamePlayData;

[CreateAssetMenu(fileName = "KeyPairSubEffect", menuName = "Scriptable Objects/KeyPairAssets/KeyPairSubEffect")]
public class KeyPairSubEffect : KeyPairAssets<StrategyGamePlayData.SubEffectKey, KeyPairSubEffect.SubEffectInfo>
{
	public static KeyPairSubEffect Load(string name)
	{
		string path = $"{BaseResourcesPath()}/{nameof(KeyPairSubEffect)}/{name}";
		var load = Resources.Load<KeyPairSubEffect>(path);
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
	Dictionary<StrategyGamePlayData.SubEffectKey, string> GeneralNames = new Dictionary<StrategyGamePlayData.SubEffectKey, string>
	{
		[SubEffectKey.폭발_소형] = nameof(SubEffectKey.폭발_소형),
		[SubEffectKey.폭발_중형] = nameof(SubEffectKey.폭발_중형),
		[SubEffectKey.폭발_대형] = nameof(SubEffectKey.폭발_대형),
		[SubEffectKey.EMP충격_소형] = nameof(SubEffectKey.EMP충격_소형),
		[SubEffectKey.EMP충격_중형] = nameof(SubEffectKey.EMP충격_중형),
		[SubEffectKey.EMP충격_대형] = nameof(SubEffectKey.EMP충격_대형),
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

		string basePath = $"Assets/Resources/ScriptableObject/SubEffectProfiles";
		if (!System.IO.Directory.Exists(basePath))
			System.IO.Directory.CreateDirectory(basePath);

		int updatedCount = 0;
		for (int i = 0 ; i < KeyPairTargetList.Length ; i++)
		{
			var pair = KeyPairTargetList[i];
			SubEffectInfo info = pair.asset;
			SubEffectKey key = info.SubEffectKey;
			string name = info.DisplayName;

			// 이미 존재하면 패스
			if (info.SubEffectProfileObject != null)
				continue;

			// 검색 경로
			string assetName = key.ToString();
			string assetPath = $"{basePath}/{assetName}.asset";

			// 기존 파일이 있는지 검사
			SubEffectProfileObject profile = null;

			profile = AssetDatabase.LoadAssetAtPath<SubEffectProfileObject>(assetPath);

			// 없다면 새로 생성
			if (profile == null)
			{
				profile = ScriptableObject.CreateInstance<SubEffectProfileObject>();
				profile.name = assetName;
				profile.subEffectKey = key;
				profile.displayName = name;

				AssetDatabase.CreateAsset(profile, assetPath);
				AssetDatabase.SaveAssets();
				Debug.Log($"[SetupUnitProfiles] Created new SubEffectProfileObject: {assetPath}");
			}

			info = new SubEffectInfo(key, name, profile);
			KeyPairTargetList[i] = new KeyPairAssetsStruct(key, info);
			updatedCount++;
		}

		EditorUtility.SetDirty(this);
		AssetDatabase.SaveAssets();

		Debug.Log($"[{name}] SetupUnitProfiles completed. Updated {updatedCount} entries.");
	}
#endif
	[Serializable]
	public struct SubEffectInfo
	{
		[ShowIf("@subEffectProfileObject == null"), SerializeField]
		private StrategyGamePlayData.SubEffectKey subEffectKey;
		[ShowIf("@subEffectProfileObject == null"), SerializeField]
		private string displayName;
		[SerializeField]
		private SubEffectProfileObject subEffectProfileObject;

		public readonly string DisplayName => SubEffectProfileObject == null ? displayName : SubEffectProfileObject.displayName;
		public readonly SubEffectKey SubEffectKey => SubEffectProfileObject == null ? subEffectKey : SubEffectProfileObject.subEffectKey;
		public readonly SubEffectProfileObject SubEffectProfileObject => subEffectProfileObject;

		public SubEffectInfo(SubEffectKey key, string displayName) : this()
		{
			this.displayName = displayName;
			this.subEffectKey = key;
			subEffectProfileObject = null;
		}
		public SubEffectInfo(SubEffectKey key, string displayName, SubEffectProfileObject profileObject) : this()
		{
			this.displayName = displayName;
			this.subEffectKey = key;
			this.subEffectProfileObject = profileObject;
		}
	}
}