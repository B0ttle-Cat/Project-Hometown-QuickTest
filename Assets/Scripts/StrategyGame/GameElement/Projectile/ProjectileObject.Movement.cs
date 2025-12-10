using UnityEngine;

[RequireComponent(typeof(ProjectileMovement))]
public partial class ProjectileObject : IProjectileMovement
{
	private ProjectileMovement movement;
	public IProjectileMovement ThisMovement => movement;
	public int OrderElementID => ThisMovement.OrderElementID;
	public int TargetElementID => ThisMovement.TargetElementID;
	public Vector3 StartPosition => ThisMovement.StartPosition;
	public Vector3 TargetPosition => ThisMovement.TargetPosition;
    public Vector3 PrevPosition => ThisMovement.PrevPosition;
	public Vector3 CurrentPosition => ThisMovement.CurrentPosition;
	public float MoveSpeed => ThisMovement.MoveSpeed;
	public Vector3 MoveDiraction => ThisMovement.MoveDiraction;
    public bool RawDataUpdateFlag => ThisMovement.RawDataUpdateFlag;

    partial void InitMovement()
	{
		movement = GetComponent<ProjectileMovement>();
		movement.Init(StatsData);
	}
	partial void DeinitMovment()
	{
		movement.Deinit();
		movement = null;
	}

    public void SetTarget(IUnitCombatController order, ITargetableCombatant target)
    {
        ThisMovement.SetTarget(order, target);
    }

    public void ApplyJobResult(in ProjectileMovement.MovementJobData movementJobData)
    {
        ThisMovement.ApplyJobResult(movementJobData);
    }

    public void InitPureMovementData(out ProjectileMovement.MovementJobData movementJobData)
    {
        ThisMovement.InitPureMovementData(out movementJobData);
    }

    public void UpdatePureMovementData(ref ProjectileMovement.MovementJobData movementJobData)
    {
        ThisMovement.UpdatePureMovementData(ref movementJobData);
    }
}
