using System;
using System.Collections.Generic;

using StrategyManagerModule;

using UnityEngine;

using static StrategyGamePlayData;

using Random = UnityEngine.Random;

public static class StrategyElementFactory
{
	#region UnitObject
	public static UnitObject Instantiate(StrategyStartSetterData.UnitData setterData, Action<UnitObject> beforeCallback = null)
	{
		var unitKey = setterData.unitKey;
		if (StrategyManager.Key2Unit.TryGetAsset(unitKey, out var info))
		{
			int factionId = setterData.factionID;
			Vector3 position = setterData.position;
			Quaternion rotation = Quaternion.Euler(setterData.rotation);
			return Instantiate(info.UnitProfileObject, factionId, setterData.belongedOperation, position, rotation, (newUnit) =>
			{
				newUnit.Init(setterData);
				beforeCallback?.Invoke(newUnit);
			});
		}
		return null;
	}
	public static UnitObject Instantiate(UnitKey unitKey, int factionID = -1, int belongedOperation = -1,
		Vector3? position = null, Quaternion? rotation = null, Action<UnitObject> beforeCallback = null)
	{
		if (StrategyManager.Key2Unit.TryGetAsset(unitKey, out var info))
		{
			return Instantiate(info.UnitProfileObject, factionID, belongedOperation, position, rotation, beforeCallback);
		}
		return null;
	}
	public static UnitObject Instantiate(UnitProfileObject profile, int factionID = -1, int belongedOperation = -1,
		Vector3? position = null, Quaternion? rotation = null, Action<UnitObject> beforeCallback = null)
	{
		if (profile.IsNullRef()) return null;
		var prefab = profile.prefab;
		if (prefab.IsNullRef()) return null;

		var newObject = GameObject.Instantiate(prefab, position ?? Vector3.zero, rotation ?? Quaternion.identity);

		if (!newObject.TryGetComponent<UnitObject>(out UnitObject unitObject))
		{
			GameObject.Destroy(newObject);
			return null;
		}

		unitObject.Init(profile, factionID);
		unitObject.name = $"{profile.displayName}";
		StrategyManager.Collector.Add<UnitObject>(unitObject, () =>
		{
			beforeCallback?.Invoke(unitObject);
			SetOperationBelong(belongedOperation);
			unitObject.InitOther();
			unitObject.name = $"{unitObject.name}_{unitObject.UnitID:00}";
		});
		if (StrategyManager.Collector.TryFind<Faction>(factionID, out var faction))
		{
			faction.API_UnitCounter(profile.stats.DeploymentCostPersonnel);
		}
		return unitObject;

		void SetOperationBelong(int belongedOperation)
		{
			if (belongedOperation < 0) return;

			var operation = StrategyManager.Collector.Find<OperationObject>(belongedOperation);
			if (operation.IsNullRef()) return;

			operation.ThisOrganization.AddUnitObject(unitObject);
		}
	}
	public static void Destroy(UnitObject unitObject)
	{
		if (unitObject.IsNullRef()) return;
		if (StrategyManager.IsNotReadyScene) return;

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
		if (sector.IsNullRef()) return null;
		return Instantiate(sector, new SpawnTroopsInfo(factionID, new (UnitKey key, int count)[0]), teamName);
	}
	public static OperationObject Instantiate(SectorObject sector, in SpawnTroopsInfo spawnTroopsInfo, string teamName = "")
	{
		int factionID = spawnTroopsInfo.factionID;
		var organizations = spawnTroopsInfo.organizations;
		int length = organizations == null ? 0 : organizations.Length;
		Vector3 sectorCenter = sector.transform.position;


		var newObject = new GameObject();
		var newOperation = newObject.AddComponent<OperationObject>();
		StrategyManager.Collector.Add<OperationObject>(newOperation, BeforeCallback);
		void BeforeCallback()
		{
			newObject.name = $"OperationObject_{newOperation.OperationID}";
			newObject.transform.position = sectorCenter;
			if (string.IsNullOrWhiteSpace(teamName))
			{
				teamName = $"{newOperation.OperationID}";
			}
			newOperation.Init(factionID, teamName);
			newOperation.InitOther();
		}
		int belongedOperation = newOperation.OperationID;
		float radius = newOperation.OperationRadius;

		List<int> spawnUnitIds = new List<int>(length);
		for (int i = 0 ; i < length ; i++)
		{
			(UnitKey unitKey, int count) = organizations[i];
			if (unitKey == UnitKey.None || count <= 0) continue;
			for (int ii = 0 ; ii < count ; ii++)
			{
				Vector3 randomValue = Random.onUnitSphere * radius;
				var position = sectorCenter + new Vector3(randomValue.x, 0f, randomValue.z);
				var rotation = Quaternion.Euler(0,randomValue.y * 180f,0);
				UnitObject unit = Instantiate(unitKey, factionID, belongedOperation, position, rotation);
				spawnUnitIds.Add(unit.UnitID);
			}
		}
		newOperation.InitUnit(in spawnUnitIds);

		return newOperation;
	}

	public static void Destroy(OperationObject operation)
	{
		if (operation.IsNullRef()) return;
		if (StrategyManager.IsNotReadyScene) return;

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
	public static async Awaitable<ProjectileObject[]> Instantiate(StrategyStartSetterData.ProjectileData setterData,
		Action<ProjectileObject, int> beforeCallback = null)
	{
		int newCount = setterData.count;
		if (newCount > 0 &&  StrategyManager.Key2Projectile.TryGetAsset(setterData.projectilKey, out var info))
		{
			return await Instantiate(info.ProjectileProfileObject, null, null, newCount, (newProjectil, index) =>
			{
				var data = setterData[index];
				newProjectil.Init(data);
				//ICombatHandler order = StrategyManager.Collector.Find<UnitObject>(data.orderInSetterIndex);
				//ITargetableCombatant target = StrategyManager.Collector.Find<UnitObject>(data.targetInSetterIndex);
				beforeCallback?.Invoke(newProjectil, index);
			});
		}
		return null;
	}
	public static async Awaitable<ProjectileObject[]> Instantiate(ProjectileKey projectileKey, ICombatHandler order, ITargetableCombatant target, int newCount = 1,
		Action<ProjectileObject, int> beforeCallback = null)
	{
		if (newCount > 0 && StrategyManager.Key2Projectile.TryGetAsset(projectileKey, out var info))
		{
			return await Instantiate(info.ProjectileProfileObject, order, target, newCount, beforeCallback);
		}
		return null;
	}
	public static async Awaitable<ProjectileObject[]> Instantiate(ProjectileProfileObject profile, ICombatHandler order, ITargetableCombatant target, int newCount = 1, Action<ProjectileObject, int> beforeCallback = null)
	{
		GameObject prefab = profile.prefab;
		ProjectileObject projectilePrefab = prefab.GetComponent<ProjectileObject>();

		ProjectileObject[] newProjectiles = await StrategyManager.Pooling.Acquires<ProjectileObject>(prefab,newCount, NewInstantiateProjectile);
		StrategyManager.Pooling.Add<ProjectileObject>(newProjectiles, (newProjectile, index) =>
		{
			newProjectile.Init();
			newProjectile.Init(profile);
			beforeCallback?.Invoke(newProjectile, index);
			newProjectile.InitOther();
			if (order != null && target != null)
			{
				newProjectile.SetTarget(order, target);
			}
		});
		return newProjectiles;

		async Awaitable<ProjectileObject[]> NewInstantiateProjectile(int instantCount)
		{
			return await GameObject.InstantiateAsync<ProjectileObject>(projectilePrefab, instantCount);
		}
	}
	
	public static void Destroy(ProjectileObject projectile)
	{
		if (projectile.IsNullRef()) return;
		if (StrategyManager.IsNotReadyScene) return;

		projectile.DeInit();
		StrategyManager.Pooling.Release(projectile);
	}
	#endregion
}
