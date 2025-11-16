using System.Collections.Generic;
using System.Linq;

using Pathfinding;

using UnityEngine;

public interface INodeMovement
{
	INodeMovement ThisMovement { get; }
	INodeMovement ParentMovement => null;
	Seeker ThisSeeker { get; }
	Vector3 CurrentPosition { get; }
	Vector3 CurrentVelocity { get; }
	float SmoothTime { get; }
	float MaxSpeed { get; }
	int MovementIndex { get; set; }
	List<Vector3> MovePath { get; set; }
	List<Vector3> TempMovePath { get; set; }
	Queue<Vector3> FindingPoints { get; set; }
	bool HasPath => MovePath != null && MovePath.Count > 0;
	bool HasTampPath => TempMovePath != null && TempMovePath.Count > 0;
	bool EmptyPath => !HasPath && !HasTampPath;
	float TotalLength { get; set; }
	float SectionLength { get; set; }
	float TempLength { get; set; }
	void SetMovePath(params SectorObject[] waypointSectors) => SetMovePath(true, waypointSectors);
	void SetMovePath(bool clearPath, params SectorObject[] waypointSectors)
	{
		SetMovePath(clearPath, waypointSectors.Select(i => i.transform.position).ToArray());
	}
	void SetMovePath(bool clearPath, params Vector3[] waypoints)
	{
		if (ThisSeeker == null) return;
		if (waypoints == null || waypoints.Length == 0) return;

		MovePath ??= new List<Vector3>();
		FindingPoints ??= new Queue<Vector3>();

		if (clearPath)
		{
			ClearMovePath();
		}
		bool isWait = FindingPoints.Count > 0;
		int length = waypoints.Length;
		for (int i = 0 ; i < length ; i++)
		{
			FindingPoints.Enqueue(waypoints[i]);
		}
		if (isWait) return;

		StartPath(CurrentPosition);
		void StartPath(Vector3 prevPoint)
		{
			if (!FindingPoints.TryDequeue(out var nextPoint)) return;

			ThisSeeker.StartPath(start: prevPoint, nextPoint, (path) =>
			{
				if (path.error) return;
				var abPath = path as ABPath;
				MovePath.AddRange(abPath.vectorPath);
				if (TempMovePath != null)
				{
					TempMovePath.Clear();
					TempMovePath = null;
					TempLength = 0;
				}
				TotalLength += abPath.GetTotalLength();
				StartPath(nextPoint);
			});
		}
	}
	void ClearMovePath()
	{
		TempMovePath = new List<Vector3>();
		TempMovePath.AddRange(MovePath);
		TempLength = TotalLength + SectionLength;

		TotalLength = 0;
		SectionLength = 0;
		if (MovePath != null) MovePath.Clear();
		if (FindingPoints != null) FindingPoints.Clear();
		if (ThisSeeker != null) ThisSeeker.CancelCurrentPathRequest();
	}

	bool FindNextMovementTarget(out Vector3 nextTarget)
	{
		if (EmptyPath)
		{
			nextTarget = CurrentPosition;
			return false;
		}
		Vector3 curr = CurrentPosition;
		List<Vector3> Path = HasTampPath ? TempMovePath : MovePath;

		while (Path.Count >= 2)
		{
			Vector3 prev = Path[0];
			Vector3 next = Path[1];
			Vector3 toNextDir = next - prev;
			Vector3 toMoveDir = next - curr;

			float dot = Vector3.Dot(toMoveDir, toNextDir);
			if (dot <= 0f)
			{
				RemoveAtFirst();
				continue;
			}
			float sqrMagnitude = toMoveDir.sqrMagnitude;
			if (Mathf.Approximately(sqrMagnitude, 0f))
			{
				RemoveAtFirst();
				continue;
			}
			break;
		}
		if (Path.Count == 0)
		{
			nextTarget = CurrentPosition;
			return false;
		}
		if (Path.Count == 1)
		{
			nextTarget = Path[0];
			RemoveAtFirst();
			return true;
		}
		nextTarget = Path[1];
		return true;

		void RemoveAtFirst()
		{
			if (Path.Count == 0) return;

			if (Path.Count >= 2)
			{
				float distance = Vector3.Distance(Path[0], Path[1]);
				TotalLength -= distance;
				SectionLength = distance;
			}
			Path.RemoveAt(0);
		}
	}
	Vector3 NextSmoothMovement(in Vector3 nextTarget, out Vector3 velocity, in float deltaTime)
	{
		Vector3 position = CurrentPosition;
		velocity = CurrentVelocity;
		float remainingDistance = HasTampPath? TempLength : TotalLength + SectionLength;
		if (remainingDistance <= 0f || Mathf.Approximately(remainingDistance, 0f))
		{
			velocity = Vector3.zero;
			return position;
		}
		Vector3 diraction = (nextTarget - position).normalized * remainingDistance;

		Vector3 nextPosition = Vector3.SmoothDamp(position, position + diraction, ref velocity, SmoothTime, MaxSpeed, deltaTime);

		float moveDelta = Vector3.Distance(position, nextPosition);
		if (Vector3.Distance(position, nextPosition) > remainingDistance)
		{
			nextPosition = nextTarget;
			velocity = Vector3.zero;
		}
		return nextPosition;
	}
	public void NextConstantSpeedMovement(ref Vector3 nextTarget, out Vector3 velocity, in float deltaTime)
	{
		Vector3 position = CurrentPosition;
		float remainingDistance = HasTampPath? TempLength : TotalLength + SectionLength;
		float maxSpeed = MaxSpeed;

		if (Mathf.Approximately(remainingDistance, 0f) || remainingDistance <= maxSpeed * deltaTime)
		{
			velocity = Vector3.zero;
		}
		Vector3 direction = (nextTarget - position).normalized;

		velocity = direction * maxSpeed;
		Vector3 nextPosition = position + velocity * deltaTime;

		if (Vector3.Distance(position, nextTarget) > remainingDistance)
		{
			nextPosition = nextTarget;
			velocity = Vector3.zero;
		}
		nextTarget = nextPosition;
	}


	void OnMoveStart();
	void OnMoveStop();
	void SetPositionAndVelocity(in Vector3 position, in Vector3 delteMove, in Vector3 velocity, in float deltaTime);
	void OnStayUpdate(in float deltaTime);
}
