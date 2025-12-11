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
    public bool ResetJobDataFlag => ThisMovement.ResetJobDataFlag;

    partial void InitMovement()
	{
		movement = GetComponent<ProjectileMovement>();
		movement.Init(StatsData, OnTransformUpdate);
	}
	partial void DeinitMovment()
	{
		movement.Deinit();
		movement = null;
	}

    void OnTransformUpdate()
    {
        RuntimeData.StartPosition = StartPosition;
        RuntimeData.TargetPosition = TargetPosition;
        RuntimeData.OrderUnitID = OrderElementID;
        RuntimeData.TargetUnitID = TargetElementID;
        RuntimeData.Position = transform.position;
        RuntimeData.Rotation = transform.rotation;
        RuntimeData.Velocity = MoveDiraction * MoveSpeed;
	}

    public void SetTarget(IUnitCombatController order, ITargetableCombatant target)
    {
        ThisMovement.SetTarget(order, target);
    }

    public void ApplyJobResult(in ProjectileMovement.MovementJobData movementJobData)
    {
        ThisMovement.ApplyJobResult(movementJobData);
    }

    public void InitMovementJobData(out ProjectileMovement.MovementJobData movementJobData)
    {
        ThisMovement.InitMovementJobData(out movementJobData);
    }

    public void UpdateMovementJobData(ref ProjectileMovement.MovementJobData movementJobData)
    {
        ThisMovement.UpdateMovementJobData(ref movementJobData);
    }
}
