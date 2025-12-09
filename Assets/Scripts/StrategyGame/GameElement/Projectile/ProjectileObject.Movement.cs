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
	void IProjectileMovement.MovmentUpdate(in float deltaTime) => ThisMovement.MovmentUpdate(deltaTime);
	void IProjectileMovement.SetTarget(IUnitCombatController order, ITargetableCombatant target) => ThisMovement.SetTarget(order, target);
}
