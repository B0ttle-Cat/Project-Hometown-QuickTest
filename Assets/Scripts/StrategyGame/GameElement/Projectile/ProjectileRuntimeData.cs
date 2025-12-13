using System;

using Sirenix.OdinInspector;

using StrategyManagerModule;

using UnityEngine;

[Serializable]
public record ProjectileRuntimeData // RuntimeData
{
	[TitleGroup("Projectile Runtime Data")]
	[SerializeField, BoxGroup("Projectile Runtime Data/Targeting")] private int orderUnitID;
	[SerializeField, BoxGroup("Projectile Runtime Data/Targeting")] private int targetUnitID;
	[SerializeField, BoxGroup("Projectile Runtime Data/Targeting")] private Vector3 startPosition;
	[SerializeField, BoxGroup("Projectile Runtime Data/Targeting")] private Vector3 targetPosition;
	[SerializeField, BoxGroup("Projectile Runtime Data/Targeting")]private Vector3 endedPosition;

	[SerializeField, BoxGroup("Projectile Runtime Data/Transform")] private Vector3 position;
	[SerializeField, BoxGroup("Projectile Runtime Data/Transform")] private Quaternion rotation;
	[SerializeField, BoxGroup("Projectile Runtime Data/Transform")] private Vector3 moveDiraction;
	[SerializeField, BoxGroup("Projectile Runtime Data/Transform")] private float moveSpeed;

	[SerializeField, BoxGroup("Projectile Runtime Data/Runtime Stats")] private float lifeTime;
	[SerializeField, BoxGroup("Projectile Runtime Data/Runtime Stats")] private int piercingCount;
	public ProjectileRuntimeData(ProjectileProfileObject profile)
	{
		if (profile == null || profile.statsData == null) return;

		OrderUnitID = -1;
		TargetUnitID = -1;
		StartPosition = Vector3.positiveInfinity;
		TargetPosition = Vector3.positiveInfinity;
		EndedPosition = Vector3.positiveInfinity;
		Position = Vector3.positiveInfinity;
		Rotation = Quaternion.identity;
		moveDiraction = Vector3.zero;
		moveSpeed = 0f;
		LifeTime = profile.statsData.LifeTime;
		PiercingCount = 0;
	}
	public ProjectileRuntimeData(StrategyStartSetterData.ProjectileData.Info setterInfo)
	{
		OrderUnitID = setterInfo.orderInSetterIndex;
		TargetUnitID = setterInfo.targetInSetterIndex;
		StartPosition = setterInfo.startPosition;
		TargetPosition = setterInfo.targetPosition;
		EndedPosition = setterInfo.endedPosition;
		Position = setterInfo.position;
		Rotation = setterInfo.rotation;
		Velocity = setterInfo.velocity;
		LifeTime = setterInfo.lifeTime;
		PiercingCount = setterInfo.piercingCount;
	}

    public int OrderUnitID { get => orderUnitID; set => orderUnitID = value; }
    public int TargetUnitID { get => targetUnitID; set => targetUnitID = value; }
    public Vector3 StartPosition { get => startPosition; set => startPosition = value; }
    public Vector3 TargetPosition { get => targetPosition; set => targetPosition = value; }
	public Vector3 EndedPosition { get => endedPosition; set => endedPosition = value; }
	public Vector3 Position { get => position; set => position = value; }
    public Quaternion Rotation { get => rotation; set => rotation = value; }
	public Vector3 MoveDiraction { get => moveDiraction; set => moveDiraction = value; }
	public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }

	public Vector3 Velocity { get => moveDiraction * moveSpeed; set
		{
			moveDiraction = value;
			moveSpeed = moveDiraction.magnitude;
			moveDiraction = moveDiraction.normalized;
		}
	}

	public float LifeTime { get => lifeTime; set => lifeTime = value; }
    public int PiercingCount { get => piercingCount; set => piercingCount = value; }
}
