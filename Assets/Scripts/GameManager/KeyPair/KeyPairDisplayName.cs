using UnityEngine;

[CreateAssetMenu(fileName = "KeyPairDisplayName", menuName = "Scriptable Objects/KeyPairAssets/KeyPairDisplayName")]
public class KeyPairDisplayName : KeyPairAssets<string>
{
	public override bool TryGetAsset(string key, out string result)
	{
		result = key;
		if (string.IsNullOrWhiteSpace(key)) return false;

		string _key = key.Trim();
		if(_TryGetAsset(in _key, out result))
		{
			return true; 
		}
		else
		{
			result = key;
			return false;
		}
	}
	public override string GetAsset(string key)
	{
		TryGetAsset(key, out var result);
		return result;
	}

	public static KeyPairDisplayName Load(Language.Type type, string name)
	{
		string path = $"{nameof(KeyPairDisplayName)}/{type.ToString()}/{name}";
		Debug.Log(path);
		return Resources.Load<KeyPairDisplayName>(path);
	}
}
