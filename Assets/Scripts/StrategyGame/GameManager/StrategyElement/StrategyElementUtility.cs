using System.Collections.Generic;

using UnityEngine;

using static StrategyGamePlayData;

public static class StrategyElementUtility
{
	public static UnitObject Instantiate(UnitKey unitKey, int factionID = -1)
	{
		if (!StrategyManager.Key2UnitInfo.TryGetAsset(unitKey, out var info))
		{
			return null;
		}
		var profile = info.UnitProfileObject;
		if (profile == null) return null;
		var prefab = profile.unitPrefab;
		if(prefab == null) return null;

		var newObject = GameObject.Instantiate(prefab);

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
		int factionID = troopsInfo.factionID;
		var organizations = troopsInfo.organizations;
		int length = organizations.Length;

		List<int> spawnUnitIds = new List<int>(length);
		for (int i = 0 ; i < length ; i++)
		{
			(UnitKey key, int count) = organizations[i];
			if (key == UnitKey.None || count <= 0) continue;
			UnitObject unit = Instantiate(key, factionID);
			spawnUnitIds.Add(unit.UnitID);
		}

        var troopsObject = new TroopsObject(troopsInfo.factionID, spawnUnitIds);
		StrategyManager.Collector.AddElement<TroopsObject>(troopsObject);
		return troopsObject;
	}
}
