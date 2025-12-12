using System;
using System.Collections.Generic;

using Pathfinding;

using Sirenix.OdinInspector;

using UnityEngine;

[RequireComponent(typeof(Seeker))]
[RequireComponent(typeof(SimpleSmoothModifier))]
public partial class OperationObject : IMovement , ITargetableCombatant
{
	[FoldoutGroup("INodeMovement"), ShowInInspector, ReadOnly]
	private Vector3 movePosition;
	[FoldoutGroup("INodeMovement"), ShowInInspector, ReadOnly]
	private Vector3 moveVelocity = Vector3.zero;
	[FoldoutGroup("INodeMovement"), ShowInInspector, ReadOnly]
	private float smoothTime = 0f;
	[FoldoutGroup("INodeMovement"), ShowInInspector]
	private float moveSpeed;
	[FoldoutGroup("INodeMovement"), ShowInInspector, ReadOnly]
	private float initLength = 0f;
	[FoldoutGroup("INodeMovement"), ShowInInspector, ReadOnly]
	private float totalLength = 0f;
	[FoldoutGroup("INodeMovement"), ShowInInspector, ReadOnly]
	private float tempLength = 0f;
	private Seeker seeker;
	private int movementIndex;
    private Vector3[] initPath;
	private List<Vector3> movePath;
	private List<Vector3> tempMovePath;
	private Queue<Vector3> findingPoints;
	private Action onMovePathUpdate;
	private Action<float> onMoveProgress;
    private Action onStartMove;
    private Action onEndedMove;

	private Collider collider;

    public IMovement ThisMovement => this;

	public Seeker ThisSeeker => seeker;
	Vector3 IMovement.CurrentPosition
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
	Vector3 IMovement.CurrentVelocity => moveVelocity;
	Vector3 IMovement.NextMovePosition { get; set; }
	public float CurrentRadius => 0f;
	float IMovement.SmoothTime => smoothTime;
	float IMovement.MaxSpeed => moveSpeed;
	int IMovement.MovementIndex { get => movementIndex; set => movementIndex = value; }
    Vector3[] IMovement.InitPath { get => initPath; set => initPath = value; }
    List<Vector3> IMovement.MovePath { get => movePath; set => movePath = value; }
	List<Vector3> IMovement.TempMovePath { get => tempMovePath; set => tempMovePath = value; }
	Queue<Vector3> IMovement.FindingPoints { get => findingPoints; set => findingPoints = value; }
	float IMovement.InitLength { get => initLength; set => initLength = value; }
	float IMovement.TotalLength { get => totalLength; set => totalLength = value; }
	float IMovement.TempLength { get => tempLength; set => tempLength = value; }
	Action IMovement.OnChangeMovePath { get => onMovePathUpdate; set => onMovePathUpdate = value; }
	Action<float> IMovement.OnChangeMoveProgress { get => onMoveProgress; set => onMoveProgress = value; }
	Action IMovement.OnStartMove { get => onStartMove; set => onStartMove = value; }
	Action IMovement.OnEndedMove { get => onEndedMove; set => onEndedMove = value; }
	Vector3 ITargetableCombatant.Position => movePosition;
	Vector3 ITargetableCombatant.HitTargetPosition => movePosition + Vector3.up;
	Collider IHitableCombatant.HitCollider => null;

    partial void InitMovement()
	{
		initPath = new Vector3[0];
		movePath = new List<Vector3>();
		tempMovePath = null;
		findingPoints = new Queue<Vector3>();
		totalLength = 0;
		tempLength = 0;

		InitPositionAndVelocity(out movePosition, out moveVelocity);
		UpdateMovementTransform();

		seeker = GetComponent<Seeker>();
		if (seeker == null) seeker = gameObject.AddComponent<Seeker>();
	}
 	private void InitPositionAndVelocity(out Vector3 position, out Vector3 velocity)
	{
		int count = 0;
		position = Vector3.zero;
		velocity = Vector3.zero;
		foreach (var item in GetAllUnitObj)
		{
			position += item.ThisMovement.CurrentPosition;
			velocity += item.ThisMovement.CurrentVelocity;
			++count;
		}
		if (count > 1)
		{
			float rate = 1f / count;
			position *= rate;
			velocity *= rate;
		}
	}
	void UpdateMovementTransform()
	{
		transform.position = movePosition;
		if (moveVelocity.sqrMagnitude > 0.1f)
			transform.LookAt(movePosition + moveVelocity.normalized);
		transform.hasChanged = false;
	}
	public void OnStayUpdate(in float deltaTime)
	{
		foreach (var unit in GetAllUnitObj)
		{
			unit.ThisNodeMovement.OnStayUpdate(in deltaTime);
		}
	}

	void IMovement.OnMoveStart()
	{
		moveVelocity = Vector3.zero;
		smoothTime = 0.5f;

		foreach (var unit in GetAllUnitObj)
		{
			unit.ThisNodeMovement.OnMoveStart();
		}
	}
	void IMovement.OnMoveStop()
	{
		moveVelocity = Vector3.zero;
		smoothTime = 0.5f;

		foreach (var unit in GetAllUnitObj)
		{
			unit.ThisNodeMovement.OnMoveStop();
		}
	}
}
public partial class OperationObject : INodeMovement
{
	public INodeMovement ThisNodeMovement => this;
	bool INodeMovement.IsMovableState()
	{
		return FsmFlag.HasFlag(FSMFlag.NodeMovement);
	}
	void INodeMovement.SetPositionAndVelocity(in Vector3 position, in Vector3 delteMove, in Vector3 velocity, in float deltaTime)
	{
		movePosition = position;
		moveVelocity = velocity;
		if (ThisMovement.HasTampPath)
		{
			tempLength -= delteMove.magnitude;
			if (tempLength < 0) tempLength = 0f;
		}

		UpdateMovementTransform();

		foreach (var unit in GetAllUnitObj)
		{
			unit.ThisNodeMovement.SetPositionAndVelocity(in position, in delteMove, in velocity, in deltaTime);
		}
	}
}