using System;

using Sirenix.OdinInspector;

using StrategyManagerModule;

using UnityEngine;

[Serializable]
public record ProjectileRuntimeData // RuntimeData
{
	[TitleGroup("Projectile Runtime Data")]
	[SerializeField,BoxGroup("Projectile Runtime Data/Targeting")] private int orderUnitID;
	[SerializeField,BoxGroup("Projectile Runtime Data/Targeting")] private int targetUnitID;
	[SerializeField,BoxGroup("Projectile Runtime Data/Targeting")] private Vector3 startPosition;
	[SerializeField,BoxGroup("Projectile Runtime Data/Targeting")] private Vector3 targetPosition;

	[SerializeField,BoxGroup("Projectile Runtime Data/Transform")] private Vector3 position;
	[SerializeField,BoxGroup("Projectile Runtime Data/Transform")] private Quaternion rotation;
	[SerializeField,BoxGroup("Projectile Runtime Data/Transform")] private Vector3 velocity;

	[SerializeField,BoxGroup("Projectile Runtime Data/Runtime Stats")] private float lifeTime;
	[SerializeField,BoxGroup("Projectile Runtime Data/Runtime Stats")] private int piercingCount;
	public ProjectileRuntimeData(StrategyStartSetterData.ProjectileData.Info setterInfo)
	{
		orderUnitID = setterInfo.orderInSetterIndex;
		targetUnitID = setterInfo.targetInSetterIndex;
		startPosition = setterInfo.startPosition;
		targetPosition = setterInfo.targetPosition;
		position = startPosition;
		rotation = Quaternion.identity;
		velocity = Vector3.zero;
		lifeTime = 0f;
		piercingCount = 0;
	}
	public int OrderUnitID => orderUnitID;
	public int TargetUnitID => targetUnitID;
	public Vector3 StartPosition => startPosition;
	public Vector3 TargetPosition => targetPosition;

	public Vector3 Position => position;
	public Quaternion Rotation => rotation;
	public Vector3 Velocity => velocity;

	public float LifeTime => lifeTime;
	public int PiercingPoint => piercingCount;

}
