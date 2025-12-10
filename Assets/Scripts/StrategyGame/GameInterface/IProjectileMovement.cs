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
	public void SetTarget(IUnitCombatController order, ITargetableCombatant target);
	public void ApplyJobResult(in MovementJobData movementJobData);
	public bool RawDataUpdateFlag { get; }
	public void InitPureMovementData(out MovementJobData movementJobData);
	public void UpdatePureMovementData(ref MovementJobData movementJobData);
}

