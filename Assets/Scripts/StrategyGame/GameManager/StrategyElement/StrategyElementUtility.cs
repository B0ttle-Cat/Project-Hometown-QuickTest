using System.Collections.Generic;

using UnityEngine;

using static StrategyGamePlayData;

public static class StrategyElementUtility
{
	#region UnitObject
	public static UnitObject Instantiate(in StrategyStartSetterData.UnitData setterUnitData)
	{
		var unitProfile = setterUnitData.unitProfile;
		int factionId = StrategyManager.Collector.FactionNameToID(setterUnitData.factionName);
		Vector3 position = setterUnitData.position;
		Quaternion rotation = Quaternion.Euler(setterUnitData.rotation);

		return Instantiate(factionID: factionId, profile: unitProfile, position: position, rotation: rotation);
	}
	public static UnitObject Instantiate(UnitKey unitKey, int factionID = -1, Vector3? position = null, Quaternion? rotation = null)
	{
		if (StrategyManager.Key2UnitInfo.TryGetAsset(unitKey, out var info))
		{
			return Instantiate(info.UnitProfileObject, factionID, position, rotation);
		}
		return null;
	}
	public static UnitObject Instantiate(UnitProfileObject profile, int factionID = -1, Vector3? position = null, Quaternion? rotation = null)
	{
		if (profile == null) return null;
		var prefab = profile.unitPrefab;
		if (prefab == null) return null;

		var newObject = GameObject.Instantiate(prefab, position ?? Vector3.zero, rotation ?? Quaternion.identity);

		if (!newObject.TryGetComponent<UnitObject>(out UnitObject unitObject))
		{
			GameObject.Destroy(newObject);
			return null;
		}

		StrategyManager.Collector.AddElement<UnitObject>(unitObject);
		unitObject.Init(profile, factionID);
		newObject.name = $"{profile.displayName}_{unitObject.UnitID:00}";
		return unitObject;
	}
	public static void Destroy(UnitObject unitObject)
	{
		if (unitObject == null) return;

		unitObject.Deinit();
		StrategyManager.Collector.RemoveElement<UnitObject>(unitObject);
		GameObject.Destroy(unitObject);
	}
	#endregion
	#region OperationObject 
	public static OperationObject Instantiate(SectorObject sector, in SpawnTroopsInfo spawnTroopsInfo)
	{
		int factionID = spawnTroopsInfo.factionID;
		var organizations = spawnTroopsInfo.organizations;
		int length = organizations.Length;
		if(length == 0) return null;
		Vector3 randomPosCenter = sector.transform.position;

		var newObject = new GameObject();
		var operationObject = newObject.AddComponent<OperationObject>();
		operationObject.Init(spawnTroopsInfo.factionID);

		StrategyManager.Collector.AddElement<OperationObject>(operationObject);
		newObject.gameObject.name = $"OperationObject_{operationObject.OperationID}";

		List<int> spawnUnitIds = new List<int>(length);
		for (int i = 0 ; i < length ; i++)
		{
			(UnitKey key, int count) = organizations[i];
			if (key == UnitKey.None || count <= 0) continue;
			UnitObject unit = Instantiate(key, factionID);
			unit.transform.position = randomPosCenter;
			spawnUnitIds.Add(unit.UnitID);
		}

		operationObject.Init(in spawnUnitIds);
		return operationObject;
	}
	public static void Destroy(OperationObject operation)
	{
		if (operation == null) return;

		operation.DeInit();
		StrategyManager.Collector.RemoveElement<OperationObject>(operation);
		operation.Dispose();
	}
	#endregion 
}
