using UnityEngine;

using static StrategyGamePlayData;

public static class StrategyElementUtility
{
	public static UnitObject Instantiate(UnitKey unitKey, int factionID = -1, Transform parent = null)
	{
		if (!StrategyManager.Key2UnitInfo.TryGetAsset(unitKey, out var info))
		{
			return null;
		}
		var profile = info.UnitProfileObject;
		if (profile == null) return null;
		var prefab = profile.unitPrefab;
		if(prefab == null) return null;

		var newObject = GameObject.Instantiate(prefab, parent);

		if(!newObject.TryGetComponent<UnitObject>(out UnitObject unitObject))
		{
			GameObject.Destroy(newObject);
			return null;
		}

		unitObject.Init(profile, factionID);
		StrategyManager.Collector.AddElement<UnitObject>(unitObject);

		return unitObject;
	}
	public static TroopsObject Instantiate(in ISectorController.SpawnTroopsInfo troopsInfo)
	{
		var newGameObject = new GameObject();
		newGameObject.SetActive(false);
		TroopsObject troopsObject = newGameObject.AddComponent<TroopsObject>();

		var influencer = new GameObject("Influencer");
		var followers = new GameObject("Followers");
		influencer.transform.SetParent(newGameObject.transform);
		followers.transform.SetParent(newGameObject.transform);

		troopsObject.influencer = influencer.transform;
		troopsObject.followers = followers.transform;

		newGameObject.SetActive(true);
		troopsObject.Init(in troopsInfo);
		StrategyManager.Collector.AddElement<TroopsObject>(troopsObject);
		return troopsObject;
	}
}
