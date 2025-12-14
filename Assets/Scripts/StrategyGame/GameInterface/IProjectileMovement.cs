using UnityEngine;

using static ProjectileMovement;

public interface IProjectileMovement
{
	IProjectileMovement ThisMovement { get; }
	int OrderElementID { get; }
	int TargetElementID { get; }
	Vector3 StartPosition { get; }
	Vector3 TargetPosition { get; }
	Vector3 PrevPosition { get; }
	Vector3 CurrentPosition { get; }
	float MoveSpeed { get; }
	Vector3 MoveDiraction { get; }
	//(ProjectileKey Key, MovmentConstantData Data) GetConstantData();
	public void SetTarget(ICombatHandler order, ITargetableCombatant target);
	public void ReleaseTarget();
	public void ApplyJobResult(in MovementJobData movementJobData);
	public void InitMovementJobData(out MovementJobData movementJobData);
	public void UpdateMovementJobData(ref MovementJobData movementJobData);
}

