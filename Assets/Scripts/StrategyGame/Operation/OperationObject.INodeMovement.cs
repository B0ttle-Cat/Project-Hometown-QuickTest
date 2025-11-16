using System.Collections.Generic;

using Pathfinding;

using Sirenix.OdinInspector;

using UnityEngine;

[RequireComponent(typeof(Seeker))]
public partial class OperationObject : INodeMovement
{
	[FoldoutGroup("INodeMovement"), ShowInInspector]
	private Vector3 movePosition = Vector3.zero;
	[FoldoutGroup("INodeMovement"), ShowInInspector]
	private Vector3 moveVelocity = Vector3.zero;
	[FoldoutGroup("INodeMovement"), ShowInInspector]
	private int moveSpeed;
	[FoldoutGroup("INodeMovement"), ShowInInspector]
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
	float INodeMovement.MaxSpeed => moveSpeed;
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
	void INodeMovement.OnMoveStart()
	{
		moveVelocity = Vector3.zero;
		smoothTime = 0.5f;

		foreach (var unit in GetAllUnitObj)
		{
			unit.ThisMovement.OnMoveStart();
		}
	}
	void INodeMovement.OnMoveStop()
	{
		moveVelocity = Vector3.zero;
		smoothTime = 0.5f;

		foreach (var unit in GetAllUnitObj)
		{
			unit.ThisMovement.OnMoveStop();
		}
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
		else
		{
			sectionLength -= delteMove.magnitude;
			if (sectionLength < 0) sectionLength = 0f;
		}

		UpdateMovementTransform();

		foreach (var unit in GetAllUnitObj)
		{
			unit.ThisMovement.SetPositionAndVelocity(in position, in delteMove, in velocity, in deltaTime);
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
			unit.ThisMovement.OnStayUpdate(in deltaTime);
		}
	}
}