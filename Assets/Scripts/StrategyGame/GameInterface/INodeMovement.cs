using System.Collections.Generic;

using UnityEngine;

public interface INodeMovement
{
	public struct MovementPlan
	{
		public int prevNodeID;
		public int nextNodeID;

		public Vector3[] path;
	}

	public INodeMovement ThisMovement { get; }
	Vector3 CurrentPosition { get; }
	Vector3 CurrentVelocity { get; }
	float SmoothTime { get; }
	float MaxSpeed { get; }
	int RecentVisitedNode { get; }
	LinkedList<MovementPlan> MovementPlanList { get; set; }
	int PrevVisitNide => EmptyPath ? RecentVisitedNode : MovementPlanList.First.Value.prevNodeID;
	int NextVisitNide => EmptyPath ? RecentVisitedNode : MovementPlanList.First.Value.nextNodeID;
	int LastVisitNide => EmptyPath ? RecentVisitedNode : MovementPlanList.Last.Value.nextNodeID;
	bool HasPath => MovementPlanList != null && MovementPlanList.Count > 0;
	bool EmptyPath => !HasPath;

	public void SetMovePath(params SectorObject[] waypoint) => SetMovePath(true, waypoint);
	public void SetMovePath(bool clearPath, params SectorObject[] waypoint)
	{
		MovementPlanList ??= new LinkedList<MovementPlan>();
		if (clearPath) ClearMovePath();

		int length = waypoint.Length;
		for (int i = 0 ; i < length ; i++)
		{
			var sector = waypoint[i];
			if (sector == null) continue;

			int lastIndex = LastVisitNide;
			int nextIndex = StrategyManager.SectorNetwork.SectorToNodeIndex(sector);
			if (lastIndex == nextIndex) continue;

			List<int> nodePath = new List<int>();
			StrategyManager.SectorNetwork.FindShortestPath(lastIndex, nextIndex, nodePath);
			StrategyManager.SectorNetwork.NodePathToVectorPath(in nodePath, out List<Vector3> pointPath);
			MovementPlanList.AddLast(new MovementPlan()
			{
				prevNodeID = lastIndex,
				nextNodeID = nextIndex,
				path = pointPath.ToArray(),
			});
		}
	}
	public void ClearMovePath()
	{
		if (MovementPlanList != null)
			MovementPlanList.Clear();
	}

	public Vector3 SmoothDampMove(Vector3 target, out Vector3 velocity, in float deltaTime)
	{
		Vector3 position = CurrentPosition;
		velocity = CurrentVelocity;
		float smoothTime = SmoothTime;
		return Vector3.SmoothDamp(position, target, ref velocity, smoothTime, MaxSpeed, deltaTime);
	}

	void OnMoveStart();
	void OnExitFirstNode();
	void OnEnterLastNode();
	void OnMoveEnded();
	void OnSetPositionAndVelocity(in Vector3 position, in Vector3 velocity);
}
