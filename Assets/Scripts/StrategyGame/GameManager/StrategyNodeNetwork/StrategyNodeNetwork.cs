using System;
using System.Collections.Generic;

using Pathfinding;





#if UNITY_EDITOR
#endif

using UnityEngine;

using static NetworkLink;

public partial class StrategyNodeNetwork : MonoBehaviour, IStrategyStartGame
{
	[Serializable]
	public readonly struct PointInfo
	{
		public readonly Vector3 point;
		public readonly int inLineID;
		public readonly int closetNodeID;

		public PointInfo(Vector3 point, int inLineID, int closetNodeID)
		{
			this.point = point;
			this.inLineID = inLineID;
			this.closetNodeID = closetNodeID;
		}
	}
	private bool isInit;

	private void Awake()
	{

	}

	AstarPath ActiveAstarPath => AstarPath.active;
	PointGraph thisPointGraph;

	public class SectorNode
	{
		public SectorObject sector;
		public Vector3 position;

		public NodeNetworkInfo[] nodeNetworkInfo;

		public struct NodeNetworkInfo
		{
			public SectorObject nextSector;
		}
	}

	public async Awaitable Init(List<SectorObject> sectorList, StrategyStartSetterData.SectorLinkData[] sectorLinkData)
	{
		AstarData data = ActiveAstarPath.data;
		thisPointGraph = data.AddGraph<PointGraph>();
		thisPointGraph.Scan();

		ActiveAstarPath.AddWorkItem(() =>
		{
			int nodeLength = sectorList.Count;
			PointNode[] pointNodes = new PointNode[nodeLength];
			for (int i = 0 ; i < nodeLength ; i++)
			{
				pointNodes[i] = thisPointGraph.AddNode((Int3)sectorList[i].transform.position);
			}
			int linkLength = sectorLinkData.Length;
			for (int i = 0 ; i < linkLength ; i++)
			{
				StrategyStartSetterData.SectorLinkData link = sectorLinkData[i];
				var sectorAName = link.sectorA;
				var sectorBName = link.sectorB;
				int indexA = sectorList.FindIndex(s=>s.gameObject.name == sectorAName);
				int indexB = sectorList.FindIndex(s=>s.gameObject.name == sectorBName);


				if (link.connectDir == ConnectDirType.Backward)
					link = link.ReverseDir;

				Vector3[] waypoint = WaypointUtility.GetLineWithWaypoints(sectorList[indexA].transform.position, sectorList[indexB].transform.position, link.waypoint);
				int pointCount = waypoint.Length;
				if (pointCount >= 2)
				{
					PointNode _prev = pointNodes[indexA];
					PointNode _next = pointNodes[indexB];
					for (int ii = 1 ; ii < pointCount - 1 ; ii++)
					{
						var point = waypoint[ii];
						var prev = _prev;
						var next = thisPointGraph.AddNode((Int3)point);
						var cost = (uint)(next.position - prev.position).costMagnitude;
						GraphNode.Connect(prev, next, cost);
						_prev = next;
					}
					var _cost = (uint)(_next.position - _prev.position).costMagnitude;
					GraphNode.Connect(_prev, _next, _cost);
				}
				else
				{
					PointNode prev = pointNodes[indexA];
					PointNode next = pointNodes[indexB];
					var cost = (uint)(next.position - prev.position).costMagnitude;
					GraphNode.Connect(prev, next, cost);
				}
			}
		});

		AstarPath.active.FlushWorkItems();

		isInit = true;
	}
	void IStrategyStartGame.OnStartGame()
	{

	}
	void IStrategyStartGame.OnStopGame()
	{
		if (thisPointGraph != null)
		{
			ActiveAstarPath.AddWorkItem(() =>
			{
				thisPointGraph.Clear();
			});
			ActiveAstarPath.FlushWorkItems();
		}
	}
}