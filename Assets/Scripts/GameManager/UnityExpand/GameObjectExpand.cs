using UnityEngine;

public static class GameObjectExpand
{
	public static void SetDeactive(this GameObject gameObject, bool value)
	{
		gameObject.SetActive(!value);
	}
}
