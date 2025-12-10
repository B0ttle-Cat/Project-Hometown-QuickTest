using System.Collections.Generic;

using StrategyManagerModule;

using UnityEngine;

using static StrategyGamePlayData;

public static class StrategyElementFactory
{
	#region UnitObject
	public static UnitObject Instantiate(in StrategyStartSetterData.UnitData setterUnitData)
	{
		var unitProfile = setterUnitData.GetUnitProfile;
		int factionId = setterUnitData.factionID;
		Vector3 position = setterUnitData.position;
		Quaternion rotation = Quaternion.Euler(setterUnitData.rotation);

		return Instantiate(factionID: factionId, profile: unitProfile, position: position, rotation: rotation);
	}
	public static UnitObject Instantiate(UnitKey unitKey, int factionID = -1, Vector3? position = null, Quaternion? rotation = null)
	{
		if (StrategyManager.Key2Unit.TryGetAsset(unitKey, out var info))
		{
			return Instantiate(info.UnitProfileObject, factionID, position, rotation);
		}
		return null;
	}
	public static UnitObject Instantiate(UnitProfileObject profile, int factionID = -1, Vector3? position = null, Quaternion? rotation = null)
	{
		if (profile == null) return null;
		var prefab = profile.prefab;
		if (prefab == null) return null;

		var newObject = GameObject.Instantiate(prefab, position ?? Vector3.zero, rotation ?? Quaternion.identity);

		if (!newObject.TryGetComponent<UnitObject>(out UnitObject unitObject))
		{
			GameObject.Destroy(newObject);
			return null;
		}

		StrategyManager.Collector.Add<UnitObject>(unitObject);
		unitObject.Init(profile, factionID);
		newObject.name = $"{profile.displayName}_{unitObject.UnitID:00}";

		if (StrategyManager.Collector.TryFind<Faction>(factionID, out var faction))
		{
			faction.API_UnitCounter(profile.유닛_인력);
		}
		return unitObject;
	}
	public static void Destroy(UnitObject unitObject)
	{
		if (unitObject == null) return;

		if (StrategyManager.Collector.TryFind<Faction>(unitObject.FactionID, out var faction))
		{
			faction.API_UnitCounter(-unitObject.StatsData.GetValue(StatsType.유닛_인력));
		}
		unitObject.Deinit();
		StrategyManager.Collector.Remove<UnitObject>(unitObject);
		GameObject.Destroy(unitObject.gameObject);
	}
	#endregion
	#region OperationObject 
	public static OperationObject Instantiate(in StrategyStartSetterData.OperationData setterOperationData)
	{
		int factionID = setterOperationData.factionID;
		string teamName = setterOperationData.teamName;
		int visiteSectorID = setterOperationData.visiteSectorID;

		var sector = StrategyManager.Collector.Find<SectorObject>(visiteSectorID);
		if (sector == null) return null;
		return Instantiate(sector, new SpawnTroopsInfo(factionID, null), teamName);
	}
	public static OperationObject Instantiate(SectorObject sector, in SpawnTroopsInfo spawnTroopsInfo, string teamName = "")
	{
		int factionID = spawnTroopsInfo.factionID;
		var organizations = spawnTroopsInfo.organizations;
		int length = organizations == null ? 0 : organizations.Length;
		Vector3 sectorCenter = sector.transform.position;
		float radius = 5f;


		var newObject = new GameObject();
		var newOperation = newObject.AddComponent<OperationObject>();

		StrategyManager.Collector.Add<OperationObject>(newOperation);
		newObject.name = $"OperationObject_{newOperation.OperationID}";
		newObject.transform.position = sectorCenter;
		if (string.IsNullOrWhiteSpace(teamName))
		{
			teamName = $"{newOperation.OperationID}";
		}
		newOperation.Init(spawnTroopsInfo.factionID, teamName);


		List<int> spawnUnitIds = new List<int>(length);
		for (int i = 0 ; i < length ; i++)
		{
			(UnitKey key, int count) = organizations[i];
			if (key == UnitKey.None || count <= 0) continue;
			for (int ii = 0 ; ii < count ; ii++)
			{
				UnitObject unit = Instantiate(key, factionID);
				Vector2 randomPos = Random.insideUnitCircle * radius;
				unit.transform.position = sectorCenter + new Vector3(randomPos.x, 0f, randomPos.y);
				spawnUnitIds.Add(unit.UnitID);
			}
		}


		newOperation.Init(in spawnUnitIds, in radius);
		return newOperation;
	}
	public static void Destroy(OperationObject operation)
	{
		if (operation == null) return;

		operation.DeInit();
		StrategyManager.Collector.Remove<OperationObject>(operation);
		GameObject.Destroy(operation.gameObject);
	}
	#endregion
	#region Projectile Object

	public static async Awaitable<ProjectileObject[]> Instantiate(StrategyStartSetterData.ProjectileData setterData)
	{
		var projectilKey = setterData.projectilKey;
		int newCount = setterData.count;
		if (newCount == 0) return null;

		var newProjectiles = await Instantiate(projectilKey, newCount);

		for (int i = 0 ; i < newCount ; i++)
		{
			newProjectiles[i].Init(setterData[i]);
		}
		return newProjectiles;
	}
	public static async Awaitable<ProjectileObject[]> Instantiate(ProjectileKey projectileKey, int newCount = 1)
	{
		if (StrategyManager.Key2Projectile.TryGetAsset(projectileKey, out var info))
		{
			return await Instantiate(info.ProjectileProfileObject, newCount);
		}
		return null;
	}

	public static async Awaitable<ProjectileObject[]> Instantiate(ProjectileProfileObject profile, int newCount = 1)
	{
		GameObject prefab = profile.prefab;
		ProjectileObject projectilePrefab = prefab.GetComponent<ProjectileObject>();

		ProjectileObject[] newProjectiles = await StrategyManager.Pooling.Acquires<ProjectileObject>(prefab,newCount, NewInstantiateProjectile);

		for (int i = 0 ; i < newCount ; i++)
		{
			newProjectiles[i].Init(profile);
		}
		return newProjectiles;

		async Awaitable<ProjectileObject[]> NewInstantiateProjectile(int instantCount)
		{
			return await GameObject.InstantiateAsync<ProjectileObject>(projectilePrefab, instantCount);
		}
	}
	public static void Destroy(ProjectileObject projectile)
	{
		if (projectile == null) return;

		projectile.DeInit();
		StrategyManager.Pooling.Release(projectile);
	}

	#endregion
}
