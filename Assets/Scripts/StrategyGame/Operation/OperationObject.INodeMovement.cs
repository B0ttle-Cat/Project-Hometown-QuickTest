using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;
public partial class OperationObject : INodeMovement
{
	private Vector3 velocity;
	private float smoothTime;
	public INodeMovement ThisMovement => this;
	[FoldoutGroup("INodeMovement"), ShowInInspector, ReadOnly]
	Vector3 INodeMovement.CurrentPosition => position;
	[FoldoutGroup("INodeMovement"), ShowInInspector, ReadOnly]
	Vector3 INodeMovement.CurrentVelocity => velocity;
	[FoldoutGroup("INodeMovement"), ShowInInspector, ReadOnly]
	float INodeMovement.SmoothTime => smoothTime;
	[FoldoutGroup("INodeMovement"), ShowInInspector, ReadOnly]
	float INodeMovement.MaxSpeed => moveSpeed;
	[FoldoutGroup("INodeMovement"), ShowInInspector, ReadOnly]
	int INodeMovement.RecentlyVisitedNode => 0;
	LinkedList<INodeMovement.MovementPlan> INodeMovement.MovementPlanList { get; set; }

	void INodeMovement.OnMoveStart()
	{
		velocity = Vector3.zero;
		smoothTime = 0.5f;
	}
	void INodeMovement.OnExitFirstNode()
	{
		smoothTime = 0f;
	}
	void INodeMovement.OnEnterLastNode()
	{
		smoothTime = 0.5f;
	}
	void INodeMovement.OnMoveEnded()
	{
		velocity = Vector3.zero;
		smoothTime = 0.5f;
	}
	void INodeMovement.OnSetPositionAndVelocity(in Vector3 position, in Vector3 velocity)
	{
		Vector3 delteMove = position - this.position;
		this.position = position;
		this.velocity = velocity;

		foreach (var unit in GetAllUnitObj)
		{
			unit.ThisMovement.OnSetPositionAndVelocity(position, velocity);
		}
	}
}