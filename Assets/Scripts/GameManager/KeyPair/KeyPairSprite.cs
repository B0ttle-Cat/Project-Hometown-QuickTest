using UnityEngine;

[CreateAssetMenu(fileName = "KeyPairSprite", menuName = "Scriptable Objects/KeyPairAssets/KeyPairSprite")]
public class KeyPairSprite : KeyPairAssets<Sprite>
{
	public static KeyPairSprite Load(Language.Type type, string name)
	{
		string path = $"{nameof(KeyPairSprite)}/{type.ToString()}/{name}";
		Debug.Log(path);
		return Resources.Load<KeyPairSprite>(path);
	}
}
