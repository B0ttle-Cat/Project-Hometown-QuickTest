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

	public class SectorNetwork
	{
		public SectorObject sector;
		public Vector3 position;
		public List<Neighbor> neighbors;

        public SectorNetwork(SectorObject sector)
        {
            this.sector = sector;
			position = sector.transform.position;
			neighbors = new List<Neighbor>();
		}

        public readonly struct Neighbor
		{
			public readonly SectorObject sector;
            public Neighbor(SectorObject sector)
            {
				this.sector = sector;
			}
        }

		public void AddNeighbor(SectorObject sectorObject)
		{
			neighbors.Add(new Neighbor(sectorObject));
		}
	}
	private Dictionary<SectorObject, SectorNetwork> sectorNetworkList;

	public async Awaitable Init(List<SectorObject> sectorList, StrategyStartSetterData.SectorLinkData[] sectorLinkData)
	{
		AstarData data = ActiveAstarPath.data;
		thisPointGraph = data.AddGraph<PointGraph>();
		thisPointGraph.Scan();
		sectorNetworkList = new Dictionary<SectorObject, SectorNetwork>(sectorList.Count);

		ActiveAstarPath.AddWorkItem(() => {
			int nodeLength = sectorList.Count;
			PointNode[] pointNodes = new PointNode[nodeLength];
			for (int i = 0 ; i < nodeLength ; i++)
			{
				var sector = sectorList[i];
				sectorNetworkList.Add(sector, new SectorNetwork(sector));
				pointNodes[i] = thisPointGraph.AddNode((Int3)sector.transform.position);
			}
			int linkLength = sectorLinkData.Length;
			for (int i = 0 ; i < linkLength ; i++)
			{
				StrategyStartSetterData.SectorLinkData link = sectorLinkData[i];
				if (link.connectDir == ConnectDirType.Disconnected) continue;
				if (link.connectDir == ConnectDirType.Backward) link = link.ReverseDir;

				OffMeshLinks.Directionality directionality = link.connectDir == ConnectDirType.Both ? OffMeshLinks.Directionality.TwoWay : OffMeshLinks.Directionality.OneWay;

				string sectorAName = link.sectorA;
				string sectorBName = link.sectorB;
				int indexA = sectorList.FindIndex(s=>s.gameObject.name == sectorAName);
				int indexB = sectorList.FindIndex(s=>s.gameObject.name == sectorBName);
				SectorObject sectorA = sectorList[indexA];
				SectorObject sectorB = sectorList[indexB];

				sectorNetworkList[sectorA].AddNeighbor(sectorB);
				sectorNetworkList[sectorB].AddNeighbor(sectorA);

				Vector3[] waypoint = WaypointUtility.GetLineWithWaypoints(sectorA.transform.position, sectorB.transform.position, link.waypoint);
				int pointCount = waypoint.Length;
				if (pointCount == 2)
				{
					// waypoint가 시작/끝 만 있는 경우
					PointNode prev = pointNodes[indexA];
					PointNode next = pointNodes[indexB];
					uint cost = (uint)(next.position - prev.position).costMagnitude;
					GraphNode.Connect(prev, next, cost, directionality);
				}
				else if (pointCount > 2)
				{
					PointNode prev = pointNodes[indexA];
					PointNode last = pointNodes[indexB];
					for (int ii = 1 ; ii < pointCount - 1 ; ii++)
					{
						var point = waypoint[ii];
						var next = thisPointGraph.AddNode((Int3)point);
						var cost = (uint)(next.position - prev.position).costMagnitude;
						GraphNode.Connect(prev, next, cost, directionality);
						prev = next;
					}
					var _cost = (uint)(last.position - prev.position).costMagnitude;
					GraphNode.Connect(prev, last, _cost, directionality);
				}
			}
		});

		AstarPath.active.FlushWorkItems();

		isInit = true;
	}
	
	public bool GetSectorNetwork(SectorObject sector, out SectorNetwork item)
	{
		return sectorNetworkList.TryGetValue(sector, out item);
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