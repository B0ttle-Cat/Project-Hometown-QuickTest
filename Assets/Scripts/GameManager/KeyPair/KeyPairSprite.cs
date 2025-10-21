using UnityEngine;

[CreateAssetMenu(fileName = "KeyPairSprite", menuName = "Scriptable Objects/KeyPairAssets/KeyPairSprite")]
public class KeyPairSprite : KeyPairAssets<Sprite>
{
	public static KeyPairSprite Load(Language.Type type, string name)
	{
		return Resources.Load<KeyPairSprite>($"{nameof(KeyPairSprite)}/{type.ToString()}/{name}");
	}
}
