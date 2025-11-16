using System.Collections.Generic;

using Pathfinding;

using Sirenix.OdinInspector;

using UnityEngine;

[RequireComponent(typeof(Seeker))]
public partial class UnitObject : INodeMovement
{
	private Vector3 operationMoveTarget;
	[FoldoutGroup("INodeMovement"), ShowInInspector, ReadOnly]
	private Vector3 movePosition;
	[FoldoutGroup("INodeMovement"), ShowInInspector, ReadOnly]
	private Vector3 moveVelocity = Vector3.zero;
	[FoldoutGroup("INodeMovement"), ShowInInspector, ReadOnly]
	private float smoothTime = 0f;
	private float totalLength = 0f;
	private float sectionLength = 0f;
	private float tempLength = 0f;

	private Seeker seeker;
	private int movementIndex;
	private List<Vector3> movePath;
	private List<Vector3> tempMovePath;
	private Queue<Vector3> findingPoints;

	public INodeMovement ThisMovement => this;
	public INodeMovement ParentMovement => operationObject;
	public Seeker ThisSeeker => seeker;
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
	List<Vector3> INodeMovement.MovePath { get => movePath; set => movePath = value; }
	List<Vector3> INodeMovement.TempMovePath { get => tempMovePath; set => tempMovePath = value; }
	Queue<Vector3> INodeMovement.FindingPoints { get => findingPoints; set => findingPoints = value; }
	float INodeMovement.TotalLength { get => totalLength; set => totalLength = value; }
	float INodeMovement.SectionLength { get => sectionLength; set => sectionLength = value; }
	float INodeMovement.TempLength { get => tempLength; set => tempLength = value; }

	partial void InitMovement()
	{
		movePath = new List<Vector3>();
		tempMovePath = null;
		findingPoints = new Queue<Vector3>();
		totalLength = 0;
		sectionLength = 0;
		tempLength = 0f;
		InitPositionAndVelocity(out movePosition, out moveVelocity);
		UpdateMovementTransform();

		seeker = GetComponent<Seeker>();
		if (seeker == null) seeker = gameObject.AddComponent<Seeker>();
	}
	private void InitPositionAndVelocity(out Vector3 position, out Vector3 velocity)
	{
		position = transform.position;
		velocity = Vector2.zero;
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
		if (HasOperationBelong)
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
	void OperationSetPositionAndVelocity(in Vector3 position, in float deltaTime)
	{
		if (!HasOperationBelong) return;
		Vector3 nextPosition = position;
		Vector3 currPosition = ThisMovement.CurrentPosition;
		Vector3 currVelocity = ThisMovement.CurrentVelocity;
		float smoothTime = ThisMovement.SmoothTime;
		float maxSpeed = ThisMovement.MaxSpeed;

		float distance = Vector3.Distance(currPosition, nextPosition);
		float oneDistance = maxSpeed * (smoothTime + 0.2f);
		if (distance > oneDistance)
		{
			operationMoveTarget = position;
			maxSpeed *= distance / oneDistance;
		}

		currPosition = Vector3.SmoothDamp(currPosition, operationMoveTarget, ref currVelocity, smoothTime, maxSpeed, deltaTime);
		Vector3 delteMove = currPosition - movePosition;
		movePosition = currPosition;
		moveVelocity = currVelocity;
		sectionLength -= delteMove.magnitude;
		if (sectionLength < 0) sectionLength = 0f;

		UpdateMovementTransform();
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
		OperationSetPositionAndVelocity(in operationMoveTarget, in deltaTime);
	}
}