using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;

public partial class UnitObject : INodeMovement
{												
    private Vector3 moveVelocity;
	private float smoothTime;
	public INodeMovement ThisMovement => this;
    [FoldoutGroup("INodeMovement"), ShowInInspector, ReadOnly]
    Vector3 INodeMovement.CurrentPosition => transform.position;
	[FoldoutGroup("INodeMovement"), ShowInInspector, ReadOnly]
	Vector3 INodeMovement.CurrentVelocity => moveVelocity;
	[FoldoutGroup("INodeMovement"), ShowInInspector, ReadOnly]
	float INodeMovement.SmoothTime => smoothTime;
	[FoldoutGroup("INodeMovement"), ShowInInspector, ReadOnly]
	float INodeMovement.MaxSpeed => StrategyManager.IsNotReady ? default
		: GetStateValue(StrategyGamePlayData.StatsType.유닛_이동속도);
	[HideInEditorMode, FoldoutGroup("INodeMovement"), ShowInInspector, ReadOnly]
	int INodeMovement.RecentVisitedNode => StrategyManager.IsNotReady ? default
		: StrategyManager.SectorNetwork.NameToIndex(SectorData.ConnectSectorName);
	LinkedList<INodeMovement.MovementPlan> INodeMovement.MovementPlanList { get; set; }
	
    void INodeMovement.OnMoveStart()
	{
        moveVelocity = Vector3.zero;
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
        moveVelocity = Vector3.zero;
		smoothTime = 0.5f;
	}
    void INodeMovement.OnSetPositionAndVelocity(in Vector3 position, in Vector3 velocity)
    {
        transform.position = position;
        moveVelocity = velocity;
	}
}