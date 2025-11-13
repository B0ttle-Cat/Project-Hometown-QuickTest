using System.Collections.Generic;
using System.Linq;

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
	int RecentlyVisitedNode { get; } // 최근 방문 노드
	LinkedList<MovementPlan> MovementPlanList { get; set; }
	int PrevVisitNide => EmptyPath ? SafeRecentlyVisitedNode() : MovementPlanList.First.Value.prevNodeID; // 이전 노드 
	int NextVisitNide => EmptyPath ? SafeRecentlyVisitedNode() : MovementPlanList.First.Value.nextNodeID; // 다음 노드
	int LastVisitNide => EmptyPath ? SafeRecentlyVisitedNode() : MovementPlanList.Last.Value.nextNodeID;  // 마지막 노드
	bool HasPath => MovementPlanList != null && MovementPlanList.Count > 0;
	bool EmptyPath => !HasPath;

	int SafeRecentlyVisitedNode()
	{
		if(FindRecentlyNode(out NetworkNode recentlyNode))
		{
			return recentlyNode.NetworkID;
		}
		return -1;
	}

	void SetMovePath(params SectorObject[] waypointSectors) => SetMovePath(true, waypointSectors);
	void SetMovePath(bool clearPath, params SectorObject[] waypointSectors)
	{
		var nodeIds = waypointSectors.Select(i => StrategyManager.SectorNetwork.SectorToNodeIndex(i)).ToArray();
		SetMovePath(clearPath, nodeIds);
	}
	void SetMovePath(params int[] waypointNodeID) => SetMovePath(true, waypointNodeID);
	void SetMovePath(bool clearPath, params int[] waypointNodeID)
	{
		MovementPlanList ??= new LinkedList<MovementPlan>();
		if (clearPath) ClearMovePath();

		int length = waypointNodeID.Length;
		for (int i = 0 ; i < length ; i++)
		{
			int lastIndex = LastVisitNide;
			int nextIndex = waypointNodeID[i];
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
	void ClearMovePath()
	{
		if (MovementPlanList != null)
			MovementPlanList.Clear();
	}
	Vector3 SmoothDampMove(Vector3 target, out Vector3 velocity, in float deltaTime)
	{
		Vector3 position = CurrentPosition;
		velocity = CurrentVelocity;
		float smoothTime = SmoothTime;
		return Vector3.SmoothDamp(position, target, ref velocity, smoothTime, MaxSpeed, deltaTime);
	}
	bool FindRecentlyNode(out NetworkNode recentlyNode)
	{
		var find = StrategyManager.SectorNetwork.IndexToNode(RecentlyVisitedNode);
		if(find != null)
		{
			recentlyNode = find;
			return true;
		}

		if (StrategyManager.SectorNetwork.FindClosestNode(CurrentPosition, out recentlyNode))
		{
			return recentlyNode != null;
		}
		return false;
	}

	void OnMoveStart();
	void OnExitFirstNode();
	void OnEnterLastNode();
	void OnMoveEnded();
	void OnSetPositionAndVelocity(in Vector3 position, in Vector3 velocity);
}
