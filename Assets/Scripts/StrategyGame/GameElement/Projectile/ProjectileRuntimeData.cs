using System;

using UnityEngine;

[Serializable]
public record ProjectileRuntimeData // RuntimeData
{
	[SerializeField] private int orderUnitID;
	[SerializeField] private int targetUnitID;
	[SerializeField] private Vector3 startPosition;
	[SerializeField] private Vector3 targetPosition;

	[SerializeField] private Vector3 position;
	[SerializeField] private Quaternion rotation;
	[SerializeField] private Vector3 velocity;

	[SerializeField] private float lifeTime;
	[SerializeField] private int piercingPoint;
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
		piercingPoint = 0;
	}
	public int OrderUnitID => orderUnitID;
	public int TargetUnitID => targetUnitID;
	public Vector3 StartPosition => startPosition;
	public Vector3 TargetPosition => targetPosition;

	public Vector3 Position => position;
	public Quaternion Rotation => rotation;
	public Vector3 Velocity => velocity;

	public float LifeTime => lifeTime;
	public int PiercingPoint => piercingPoint;

}
