using System.Collections.Generic;

using StrategyManagerModule;

using UnityEngine;

using static StrategyGamePlayData;

public static class StrategyElementFactory
{
	#region UnitObject
	public static UnitObject Instantiate(in StrategyStartSetterData.UnitData setterData, bool enterThis = true)
	{
		var unitKey = setterData.unitKey;
		int factionId = setterData.factionID;
		Vector3 position = setterData.position;
		Quaternion rotation = Quaternion.Euler(setterData.rotation);

		UnitObject newUnit = Instantiate(unitKey, factionId, position, rotation, false);
		newUnit.Init(setterData);
		SetOperationBelong(setterData.belongedOperation);
		if (enterThis) AddCollector(newUnit);

		return newUnit;

		void SetOperationBelong(int belongedOperation)
		{
			var operation = StrategyManager.Collector.Find<OperationObject>(belongedOperation);
			if (operation == null) return;

			operation.AddUnitObject(newUnit);
		}
	}
	public static UnitObject Instantiate(UnitKey unitKey, int factionID = -1, Vector3? position = null, Quaternion? rotation = null, bool enterThis = true)
	{
		if (StrategyManager.Key2Unit.TryGetAsset(unitKey, out var info))
		{
			return Instantiate(info.UnitProfileObject, factionID, position, rotation, enterThis);
		}
		return null;
	}
	public static UnitObject Instantiate(UnitProfileObject profile, int factionID = -1, Vector3? position = null, Quaternion? rotation = null, bool enterThis = true)
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
		unitObject.Init(profile, factionID);
		newObject.name = $"{profile.displayName}_{unitObject.UnitID:00}";
		if (StrategyManager.Collector.TryFind<Faction>(factionID, out var faction))
		{
			faction.API_UnitCounter(profile.stats.DeploymentCostPersonnel);
		}
		if (enterThis)
		{
			AddCollector(unitObject);
		}

		return unitObject;
	}
	private static void AddCollector(UnitObject unitObject)
	{
		unitObject.InitOther();
		StrategyManager.Collector.Add<UnitObject>(unitObject);
	}
	public static void Destroy(UnitObject unitObject)
	{
		if (unitObject == null) return;

		if (StrategyManager.Collector.TryFind<Faction>(unitObject.FactionID, out var faction))
		{
			faction.API_UnitCounter(-unitObject.StatsData.DeploymentCostPersonnel);
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
		float radius = newOperation.OperationRadius;


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
	public static void ReadyPoolCount(ProjectileKey projectileKey, int newCount)
	{

		if (StrategyManager.Key2Projectile.TryGetAsset(projectileKey, out var info))
		{
			GameObject prefab = info.ProjectileProfileObject.prefab;
			ProjectileObject projectilePrefab = prefab.GetComponent<ProjectileObject>();

			StrategyManager.Pooling.ReadyPoolCount<ProjectileObject>(prefab, newCount, NewInstantiateProjectile);
		
			async Awaitable<ProjectileObject[]> NewInstantiateProjectile(int instantCount)
			{
				return await GameObject.InstantiateAsync<ProjectileObject>(projectilePrefab, instantCount);
			}
		}
	}


	public static async Awaitable<ProjectileObject[]> Instantiate(StrategyStartSetterData.ProjectileData setterData, bool enterThis = true)
	{
		var projectilKey = setterData.projectilKey;
		int newCount = setterData.count;
		if (newCount == 0) return null;


		var newProjectiles = await Instantiate(null,null,projectilKey, newCount, false);

		for (int i = 0 ; i < newCount ; i++)
		{
			newProjectiles[i].Init(setterData[i]);
			if (enterThis)
			{
				ICombatHandler order = StrategyManager.Collector.Find<UnitObject>(setterData[i].orderInSetterIndex);
				ITargetableCombatant target = StrategyManager.Collector.Find<UnitObject>(setterData[i].targetInSetterIndex);
				AddCollector(order, target, newProjectiles[i]);
			}
		}
		return newProjectiles;
	}
	public static async Awaitable<ProjectileObject[]> Instantiate(ICombatHandler order, ITargetableCombatant target, ProjectileKey projectileKey, int newCount = 1, bool enterThis = true)
	{
		if (StrategyManager.Key2Projectile.TryGetAsset(projectileKey, out var info))
		{
			return await Instantiate(order, target, info.ProjectileProfileObject, newCount, enterThis);
		}
		return null;
	}

	public static async Awaitable<ProjectileObject[]> Instantiate(ICombatHandler order, ITargetableCombatant target, ProjectileProfileObject profile, int newCount = 1, bool enterThis = true)
	{
		GameObject prefab = profile.prefab;
		ProjectileObject projectilePrefab = prefab.GetComponent<ProjectileObject>();

		ProjectileObject[] newProjectiles = await StrategyManager.Pooling.Acquires<ProjectileObject>(prefab,newCount, NewInstantiateProjectile);

		for (int i = 0 ; i < newCount ; i++)
		{
			newProjectiles[i].Init();
			newProjectiles[i].Init(profile);
		}
		if (enterThis)
		{
			AddCollector(order, target, newProjectiles);
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
	private static void AddCollector(ICombatHandler order, ITargetableCombatant target, ProjectileObject projectile)
	{
		projectile.InitOther();
		if (order != null && target != null)
		{
			projectile.SetTarget(order, target);
		}
		StrategyManager.Pooling.Add<ProjectileObject>(projectile);
	}
	private static void AddCollector(ICombatHandler order, ITargetableCombatant target, ProjectileObject[] projectiles)
	{
		int addCout = projectiles.Length;
		for (int i = 0 ; i < addCout ; i++)
		{
			projectiles[i].InitOther();
			if (order != null && target != null)
			{
				projectiles[i].SetTarget(order, target);
			}
		}
		StrategyManager.Pooling.Add<ProjectileObject>(projectiles);
	}

	#endregion
}
