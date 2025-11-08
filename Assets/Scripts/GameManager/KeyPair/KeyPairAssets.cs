using System;

using Sirenix.OdinInspector;

using UnityEngine;

public abstract class KeyPairAssets<TKey,TValue> : ScriptableObject
{
	[PropertyOrder(-10)]
	public KeyPairAssets<TKey,TValue>[] chains;

	[Serializable]
	protected struct KeyPairAssetsStruct
	{
		[HorizontalGroup, HideLabel, SuffixLabel("Key", overlay: true)]
		public TKey key;
		//[HorizontalGroup, HideLabel, SuffixLabel("KeyValue Target", overlay: true)]
		[HideIf("@true")]
		public TValue asset;

#if UNITY_EDITOR
		private bool IsString() => asset is string;
		[ShowIf("IsString"), ShowInInspector, HorizontalGroup, HideLabel]
		public TValue asset_string { get => asset; set => asset =value; }
		[HideIf("IsString"), ShowInInspector, HorizontalGroup, HideLabel, PreviewField]
		public TValue asset_preview { get => asset; set => asset = value; }
#endif
		public KeyPairAssetsStruct(TKey key, TValue asset)
        {
            this.key = key;
            this.asset = asset;
        }
    }

	[TitleGroup("Key KeyValue")]
	[HorizontalGroup("Key KeyValue/H")]
	[SerializeField]
	protected string prefix;
	[TitleGroup("Key KeyValue")]
	[HorizontalGroup("Key KeyValue/H")]
	[SerializeField]
	protected string suffix;

	[SerializeField]
	[ListDrawerSettings(ShowFoldout = false)]
	protected KeyPairAssetsStruct[] KeyPairTargetList;


#if UNITY_EDITOR
	protected bool autoUpdateAsset = false;
#endif
	public TValue this[TKey key] => GetAsset(key);

	public virtual bool TryGetAsset(TKey key, out TValue result)
	{
		result = default;

		string keyString = key.ToString();
		if (_TryGetAsset(in keyString, out result))
		{
			return true;
		}
		Debug.LogWarning($"{name}({GetType().Name}) 에서 해당하는 키({key})를 찾을 수 없습니다.");
#if UNITY_EDITOR
		if (autoUpdateAsset)
		{
			int newSize = KeyPairTargetList.Length +1;
			Array.Resize(ref KeyPairTargetList, newSize);
			KeyPairTargetList[^1] = new KeyPairAssetsStruct(key, default);
		}
#endif
		return false;
	}
	public virtual TValue GetAsset(TKey key)
	{
		return TryGetAsset(key, out TValue result) ? result : default;
	}
	protected bool _TryGetAsset(in string key, out TValue value)
	{
		int length = KeyPairTargetList == null ? 0 : KeyPairTargetList.Length;
		for (int i = 0 ; i < length ; i++)
		{
			string keyString = KeyPairTargetList[i].key.ToString();
			string listKey = $"{prefix.Trim()}{keyString.Trim()}{suffix.Trim()}";

			if (listKey.Equals(key))
			{
				value = KeyPairTargetList[i].asset;
				return true;
			}
		}
		if (chains == null)
		{
			value = default;
			return false;
		}
		length = chains.Length;
		for (int i = 0 ; i < length ; i++)
		{
			var next = chains[i];
			if(next != null && next._TryGetAsset(key, out value))
			{
				return true; 
			}
		}
		value = default;
		return false;
	}

	protected void AddAsset(TKey key, TValue value)
	{
		int newSize = KeyPairTargetList.Length + 1;
		Array.Resize(ref KeyPairTargetList, newSize);
		KeyPairTargetList[^1] = new KeyPairAssetsStruct(key,value);
	}
}
public abstract class KeyPairAssets<TValue> : KeyPairAssets<string, TValue>
{
	public override bool TryGetAsset(string key, out TValue result)
	{
		result = default;
		if (string.IsNullOrWhiteSpace(key)) return false;

		string _key = key.Trim();
		if (_TryGetAsset(in _key, out result))
		{
			return true;
		}
		Debug.LogWarning($"{name}({GetType().Name}) 에서 해당하는 키({key})를 찾을 수 없습니다.");
#if UNITY_EDITOR
		if (autoUpdateAsset)
		{
			int newSize = KeyPairTargetList.Length +1;
			Array.Resize(ref KeyPairTargetList, newSize);
			KeyPairTargetList[^1] = new KeyPairAssetsStruct(key, default);
		}
#endif
		return false;
	}
}