using System;

using Sirenix.OdinInspector;

using UnityEngine;

public abstract class KeyPairAssets<T> : ScriptableObject
{
	public KeyPairAssets<T>[] chains;

	[Serializable]
	private struct KeyPairAssetsStruct
	{
		[HorizontalGroup, HideLabel, SuffixLabel("Key", overlay: true)]
		public string key;
		[HorizontalGroup, HideLabel, SuffixLabel("KeyValue Target", overlay: true)]
		public T asset;

        public KeyPairAssetsStruct(string key, T asset)
        {
            this.key = key;
            this.asset = asset;
        }
    }

	[TitleGroup("Key KeyValue")]
	[HorizontalGroup("Key KeyValue/H")]
	public string prefix;
	[TitleGroup("Key KeyValue")]
	[HorizontalGroup("Key KeyValue/H")]
	public string suffix;

	[SerializeField]
	[ListDrawerSettings(ShowPaging = false, ShowFoldout = false)]
	private KeyPairAssetsStruct[] KeyPairTargetList;


#if UNITY_EDITOR
	private bool autoUpdateAsset = false;

#endif
	public virtual bool TryGetAsset(string key, out T result)
	{
		result = default;
		if (string.IsNullOrWhiteSpace(key)) return false;

		string _key = key.Trim();
		if(_TryGetAsset(in _key, out result))
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
	public virtual T GetAsset(string key)
	{
		return TryGetAsset(key, out T result) ? result : default;
	}
	protected bool _TryGetAsset(in string key, out T value)
	{
		int length = KeyPairTargetList == null ? 0 : KeyPairTargetList.Length;
		for (int i = 0 ; i < length ; i++)
		{
			string listKey = $"{prefix.Trim()}{KeyPairTargetList[i].key.Trim()}{suffix.Trim()}";

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
}
