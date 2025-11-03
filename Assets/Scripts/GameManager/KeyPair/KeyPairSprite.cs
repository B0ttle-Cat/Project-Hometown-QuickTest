using System.Linq;

using Sirenix.OdinInspector;

using UnityEditor;

using UnityEngine;

[CreateAssetMenu(fileName = "KeyPairSprite", menuName = "Scriptable Objects/KeyPairAssets/KeyPairSprite")]
public class KeyPairSprite : KeyPairAssets<Sprite>
{
#if UNITY_EDITOR
	[InlineButton("AutoPairSprite"), PropertyOrder(-1), ShowInInspector]
	private Texture2D texture { get; set; }
	private void AutoPairSprite()
	{
		if (texture == null) return;

		string path = AssetDatabase.GetAssetPath(texture);
		Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);

		// Texture2D와 Sprite들을 모두 불러오므로 필터링 필요
		foreach (var obj in assets)
		{
			if (obj is Sprite sprite)
			{
				AddAsset(sprite.name, sprite);
			}
		}
		KeyPairTargetList = KeyPairTargetList.OrderBy(x => x.key).ToArray();
	}
#endif
	public static KeyPairSprite Load(Language.Type type, string name)
	{
		string path = $"{nameof(KeyPairSprite)}/{type.ToString()}/{name}";
		Debug.Log(path);
		return Resources.Load<KeyPairSprite>(path);
	}
}
