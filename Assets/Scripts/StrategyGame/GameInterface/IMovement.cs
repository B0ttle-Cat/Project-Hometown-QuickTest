using System;
using System.Collections.Generic;
using System.Linq;

using Pathfinding;

using UnityEngine;


public interface IMovement
{
	IMovement ThisMovement { get; }
	Seeker ThisSeeker { get; }
	Vector3 CurrentPosition { get; }
	Vector3 CurrentVelocity { get; }
	float CurrentRadius { get; }
	Vector3 NextMovePosition { get; set; }
	float SmoothTime { get; }
	float MaxSpeed { get; }
	int MovementIndex { get; set; }
	Vector3[] InitPath { get; set; }
	List<Vector3> MovePath { get; set; }
	List<Vector3> TempMovePath { get; set; }
	Queue<Vector3> FindingPoints { get; set; }
	bool HasPath => MovePath != null && MovePath.Count > 0;
	bool HasTampPath => TempMovePath != null && TempMovePath.Count > 0;
	bool EmptyPath => !HasPath && !HasTampPath;
	float InitLength { get; set; }
	float TotalLength { get; set; }
	float TempLength { get; set; }
	Action OnStartMove { get; set; }
	Action OnEndedMove { get; set; }
	Action OnChangeMovePath { get; set; }
	Action<float> OnChangeMoveProgress { get; set; }

	void ClearMovePath()
	{
		TempMovePath = new List<Vector3>();
		TempMovePath.AddRange(MovePath);
		TempLength = TotalLength;

		TotalLength = 0;
		if (MovePath != null) MovePath.Clear();
		if (FindingPoints != null) FindingPoints.Clear();
		if (ThisSeeker != null) ThisSeeker.CancelCurrentPathRequest();
	}
	float GetRemainingDistance()
	{
		List<Vector3> path = HasTampPath ? TempMovePath : MovePath;
		Vector3 currentPos = CurrentPosition;

		if (path == null || path.Count < 2)
			return 0f;

		float total = 0f;

		// 1. 현재 위치 → path[1]
		total += Vector3.Distance(currentPos, path[1]);

		// 2. path[1] → path[2] ... → path[last]
		for (int i = 1 ; i < path.Count - 1 ; i++)
		{
			total += Vector3.Distance(path[i], path[i + 1]);
		}

		return total;
	}
	Vector3 NextSmoothMovement(in Vector3 nextTarget, out Vector3 velocity, in float deltaTime)
	{
		Vector3 position = CurrentPosition;
		velocity = CurrentVelocity;
		float remainingDistance = GetRemainingDistance();
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
	void NextConstantSpeedMovement(ref Vector3 nextTarget, out Vector3 velocity, in float deltaTime)
	{
		Vector3 position = CurrentPosition;
		float remainingDistance = HasTampPath? TempLength : TotalLength;
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
	void MoveStart()
	{
		OnMoveStart();
		OnStartMove?.Invoke();
	}
	void MoveStop()
	{
		OnMoveStop();
		OnEndedMove?.Invoke();
	}
	void OnMoveStart();
	void OnMoveStop();
}
public interface INodeMovement : IMovement
{
	INodeMovement ThisNodeMovement => this;
	INodeMovement ParentMovement => null;

	void SetMovePath(params SectorObject[] waypointSectors) => SetMovePath(true, waypointSectors);
	void SetMovePath(bool clearPath, params SectorObject[] waypointSectors) => SetMovePath(clearPath, waypointSectors.Select(i => i.transform.position).ToArray());
	void SetMovePath(params Vector3[] waypoints) => SetMovePath(null, true, waypoints);
	void SetMovePath(Action callback, params Vector3[] waypoints) => SetMovePath(callback, true, waypoints);
	void SetMovePath(bool clearPath, params Vector3[] waypoints) => SetMovePath(null, clearPath, waypoints);
	void SetMovePath(Action callback, bool clearPath, params Vector3[] waypoints)
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

		StartPath(MovePath.Count == 0 ? CurrentPosition : MovePath[^1]);
		void StartPath(Vector3 prevPoint)
		{
			if (!FindingPoints.TryDequeue(out var nextPoint))
			{
				InitPath = MovePath.ToArray();
				InitLength = TotalLength;
				OnChangeMovePath?.Invoke();
				OnChangeMoveProgress?.Invoke(0);
				return;
			}
			StrategyManager.Pathfinding.FindNodePath(ThisSeeker, prevPoint, nextPoint, FindPath);
			void FindPath(Path path)
			{
				if (path.error)
				{
					Debug.LogError("Path Error:" + path.errorLog);
					return;
				}
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
			}
		}
	}
	bool FindNextMovementTarget()
	{
		if (EmptyPath)
		{
			NextMovePosition = CurrentPosition;
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
			NextMovePosition = CurrentPosition;
			return false;
		}
		if (Path.Count == 1)
		{
			NextMovePosition = Path[0];
			RemoveAtFirst();
			return true;
		}
		NextMovePosition = Path[1];
		return true;

		void RemoveAtFirst()
		{
			if (Path.Count == 0) return;

			if (Path.Count >= 2)
			{
				float distance = Vector3.Distance(Path[0], Path[1]);
				TotalLength -= distance;
			}
			Path.RemoveAt(0);
			OnChangeMoveProgress?.Invoke(1f - TotalLength / InitLength);
		}
	}
	bool IsMovableState();
	void SetPositionAndVelocity(in Vector3 position, in Vector3 delteMove, in Vector3 velocity, in float deltaTime);
	void OnStayUpdate(in float deltaTime);
}
public interface INavMovement : IMovement
{
	INavMovement ThisNavMovement => this;
	void SetMovePath(params Transform[] waypointTarget) => SetMovePath(true, waypointTarget);
	void SetMovePath(bool clearPath, params Transform[] waypointTarget) => SetMovePath(clearPath, waypointTarget.Select(i => i.transform.position).ToArray());
	void SetMovePath(params Vector3[] waypoints) => SetMovePath(null, true, waypoints);
	void SetMovePath(Action callback, params Vector3[] waypoints) => SetMovePath(callback, true, waypoints);
	void SetMovePath(bool clearPath, params Vector3[] waypoints) => SetMovePath(null, clearPath, waypoints);
	void SetMovePath(Action callback, bool clearPath, params Vector3[] waypoints)
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
			var waypoint = waypoints[i];
			if (float.IsInfinity(waypoint.x)) continue;
			FindingPoints.Enqueue(waypoint);
		}
		if (isWait) return;

		StartPath(MovePath.Count == 0 ? CurrentPosition : MovePath[^1]);
		void StartPath(Vector3 prevPoint)
		{
			if (!FindingPoints.TryDequeue(out var nextPoint))
			{
				InitPath = MovePath.ToArray();
				InitLength = TotalLength;
				OnChangeMovePath?.Invoke();
				OnChangeMoveProgress?.Invoke(0);
				callback?.Invoke();
				return;
			}
			StrategyManager.Pathfinding.FindNavPath(ThisSeeker, prevPoint, nextPoint, FindPath);
			void FindPath(Path path)
			{
				if (path.error)
				{
					if(TempMovePath == null)
					{
						Debug.LogError("Path Error:" + path.errorLog);
					}
					else
					{
						Debug.LogWarning("Path Warning:" + path.errorLog);
					}
					return;
				}
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
			}
		}
	}
	bool FindNextMovementTarget()
	{
		if (EmptyPath)
		{
			NextMovePosition = CurrentPosition;
			return false;
		}
		Vector3 curr = CurrentPosition;
		List<Vector3> Path = HasTampPath ? TempMovePath : MovePath;

		if (IsChangeTargetPositionCheck())
		{
			return true;
		}

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
			NextMovePosition = CurrentPosition;
			return false;
		}
		if (Path.Count == 1)
		{
			NextMovePosition = Path[0];
			RemoveAtFirst();
			return true;
		}
		NextMovePosition = Path[1];
		return true;

		void RemoveAtFirst()
		{
			if (Path.Count == 0) return;

			if (Path.Count >= 2)
			{
				float distance = Vector3.Distance(Path[0], Path[1]);
				TotalLength -= distance;
			}
			Path.RemoveAt(0);
			OnChangeMoveProgress?.Invoke(1f - TotalLength / InitLength);
		}
	}
	bool IsMovableState();
	bool IsChangeTargetPositionCheck();
	void SetPositionAndVelocity(in Vector3 position, in Vector3 delteMove, in Vector3 velocity, in float deltaTime);
	void OnStayUpdate(in float deltaTime);
}