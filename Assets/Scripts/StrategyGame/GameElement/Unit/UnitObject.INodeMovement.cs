using System;
using System.Collections.Generic;

using Pathfinding;
using Pathfinding.RVO;

using Sirenix.OdinInspector;

using UnityEngine;

[RequireComponent(typeof(Seeker))]
[RequireComponent(typeof(RVOController))]
public partial class UnitObject : INodeMovement
{
	private Vector3 operationMoveTarget;
	[FoldoutGroup("INodeMovement"), ShowInInspector, ReadOnly]
	private Vector3 movePosition;
	[FoldoutGroup("INodeMovement"), ShowInInspector, ReadOnly]
	private Vector3 moveVelocity = Vector3.zero;
	[FoldoutGroup("INodeMovement"), ShowInInspector, ReadOnly]
	private float smoothTime = 0f;
	[FoldoutGroup("INodeMovement"), ShowInInspector, ReadOnly]
	private float initLength = 0f;
	[FoldoutGroup("INodeMovement"), ShowInInspector, ReadOnly]
	private float totalLength = 0f;
	[FoldoutGroup("INodeMovement"), ShowInInspector, ReadOnly]
	private float sectionLength = 0f;
	[FoldoutGroup("INodeMovement"), ShowInInspector, ReadOnly]
	private float tempLength = 0f;

	private float moveRadius = 0.5f;

	private Seeker seeker;
	private int movementIndex;
	private Vector3[] initPath;
	private List<Vector3> movePath;
	private List<Vector3> tempMovePath;
	private Queue<Vector3> findingPoints;
	private Action onMovePathUpdate;
	private Action<float> onMoveProgress;
    private Action onEndedMove;
    private Action onStartMove;

	private RVOController rvoController;

    public INodeMovement ThisMovement => this;
	public INodeMovement ParentMovement => operationObject;
	public Seeker ThisSeeker => seeker;
	public RVOController RVO => rvoController;
	Vector3 INodeMovement.CurrentPosition
	{
		get
		{
			if (transform.hasChanged)
			{
				transform.hasChanged = false;
				movePosition = transform.position;
			}
			return movePosition;
		}
	}
	Vector3 INodeMovement.CurrentVelocity => moveVelocity;
	float INodeMovement.SmoothTime => smoothTime;
	float INodeMovement.MaxSpeed => GetStateValue(StrategyGamePlayData.StatsType.유닛_이동속도);
	int INodeMovement.MovementIndex { get => movementIndex; set => movementIndex = value; }
    Vector3[] INodeMovement.InitPath { get => initPath; set => initPath = value; }
	List<Vector3> INodeMovement.MovePath { get => movePath; set => movePath = value; }
	List<Vector3> INodeMovement.TempMovePath { get => tempMovePath; set => tempMovePath = value; }
	Queue<Vector3> INodeMovement.FindingPoints { get => findingPoints; set => findingPoints = value; }
	float INodeMovement.InitLength { get => initLength; set => initLength = value; }
	float INodeMovement.TotalLength { get => totalLength; set => totalLength = value; }
	float INodeMovement.SectionLength { get => sectionLength; set => sectionLength = value; }
	float INodeMovement.TempLength { get => tempLength; set => tempLength = value; }
	Action INodeMovement.OnChangeMovePath { get => onMovePathUpdate; set => onMovePathUpdate = value; }
	Action<float> INodeMovement.OnChangeMoveProgress { get => onMoveProgress; set => onMoveProgress = value; }
    Action INodeMovement.OnStartMove { get => onStartMove; set => onStartMove = value; }
    Action INodeMovement.OnEndedMove { get => onEndedMove; set => onEndedMove = value; }

	partial void InitMovement()
	{
		seeker = GetComponent<Seeker>();
		rvoController = GetComponent<RVOController>();
		moveRadius = rvoController.radius;

		movePath = new List<Vector3>();
		tempMovePath = null;
		findingPoints = new Queue<Vector3>();
		totalLength = 0;
		sectionLength = 0;
		tempLength = 0f;
		InitPositionAndVelocity(out movePosition, out moveVelocity);
		UpdateMovementTransform();

	}
	private void InitPositionAndVelocity(out Vector3 position, out Vector3 velocity)
	{
		position = transform.position;
		velocity = Vector2.zero;
	}

	bool INodeMovement.IsNodeMovableState()
	{
		return FsmFlag.HasFlag(FSMFlag.NodeMovement);
	}

	void INodeMovement.OnMoveStart()
	{
		operationMoveTarget = Vector3.zero;
		moveVelocity = Vector3.zero;
		smoothTime = 0.5f;
	}
	void INodeMovement.OnMoveStop()
	{
		moveVelocity = Vector3.zero;
		smoothTime = 0.5f;
	}
	void INodeMovement.SetPositionAndVelocity(in Vector3 position, in Vector3 delteMove, in Vector3 velocity, in float deltaTime)
	{
		if (HasOperation)
		{
			OperationSetPositionAndVelocity(in position, in deltaTime);
			return;
		}
		movePosition = position;
		moveVelocity = velocity;
		if (ThisMovement.HasTampPath)
		{
			tempLength -= delteMove.magnitude;
			if (tempLength < 0) tempLength = 0f;
		}
		else
		{
			sectionLength -= delteMove.magnitude;
			if (sectionLength < 0) sectionLength = 0f;
		}

		UpdateMovementTransform();
	}
	void OperationSetPositionAndVelocity(in Vector3 nextPosition, in float deltaTime)
	{
		if (!HasOperation) return;
		Vector3 currPosition = ThisMovement.CurrentPosition;
		Vector3 currVelocity = ThisMovement.CurrentVelocity;
		float smoothTime = ThisMovement.SmoothTime;
		float maxSpeed = ThisMovement.MaxSpeed;
		Vector3 operationLocalOffset = operationObject.transform.TransformVector(operationOffset);

		float distance = Vector3.Distance(currPosition, nextPosition + operationLocalOffset);
		float oneDistance = maxSpeed * (smoothTime + 0.2f);
		if (distance > oneDistance)
		{
			operationMoveTarget = nextPosition;
			maxSpeed *= distance / oneDistance;
		}

		Vector3 target = operationMoveTarget + operationLocalOffset;
		currPosition = Vector3.SmoothDamp(currPosition, target, ref currVelocity, smoothTime, maxSpeed, deltaTime);

		Vector3 delteMove = currPosition - transform.position;
		if (rvoController != null && rvoController.isActiveAndEnabled)
		{
			rvoController.SetTarget(target, currVelocity.magnitude, maxSpeed, target);
			delteMove = rvoController.CalculateMovementDelta(movePosition, deltaTime);
			movePosition += delteMove;
			moveVelocity = currVelocity;
		}
		else
		{
			movePosition = currPosition;
			moveVelocity = currVelocity;
		}

		sectionLength -= delteMove.magnitude;
		if (sectionLength < 0) sectionLength = 0f;
		UpdateMovementTransform();
	}
	void UpdateMovementTransform(bool skip = false)
	{
		transform.position = movePosition;
		if (moveVelocity.sqrMagnitude > 0.1f)
			transform.LookAt(movePosition + moveVelocity.normalized);
		transform.hasChanged = false;
	}
	public void OnStayUpdate(in float deltaTime)
	{
		if (HasOperation)
		{
			Vector3 operationPosition = operationObject.ThisMovement.CurrentPosition;
			OperationSetPositionAndVelocity(in operationPosition, in deltaTime);
		}
	}
}