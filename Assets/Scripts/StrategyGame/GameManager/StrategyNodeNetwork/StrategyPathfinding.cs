using System;
using System.Collections.Generic;

using Pathfinding;

using UnityEngine;

using static NetworkLink;

public partial class StrategyPathfinding : MonoBehaviour, IStrategyStartGame
{
	[SerializeField]
	private Transform groundParent;
	[SerializeField]
	private LayerMask groundLayerMask;



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
	private void Awake()
	{

	}

	AstarPath ActiveAstarPath => AstarPath.active;
	PointGraph thisPointGraph;
	RecastGraph thisRecastGraph;

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
		sectorNetworkList = new Dictionary<SectorObject, SectorNetwork>(sectorList.Count);

		AstarData data = ActiveAstarPath.data;
		thisPointGraph = data.AddGraph<PointGraph>();
		thisPointGraph.name = "MainPointGraph";
		thisPointGraph.Scan();

		thisRecastGraph = data.AddGraph<RecastGraph>();
		thisRecastGraph.name = "MainRecastGraph";

		// groundParent 기준으로 모든 Collider Bounds 합산
		Bounds combined = CalculateCombinedBounds(groundParent);

		thisRecastGraph.useTiles = true;
		thisRecastGraph.forcedBoundsCenter = combined.center;
		thisRecastGraph.forcedBoundsSize = combined.size;

		thisRecastGraph.collectionSettings.rasterizeTerrain = false;
		thisRecastGraph.collectionSettings.rasterizeMeshes = false;
		thisRecastGraph.collectionSettings.rasterizeColliders = true;
		thisRecastGraph.collectionSettings.layerMask = groundLayerMask;

		thisRecastGraph.drawGizmos = false;

		// 필요한 기본 설정(필요 최소만 기입)
		thisRecastGraph.cellSize = 0.15f;
		thisRecastGraph.tileSizeX = 128;
		thisRecastGraph.tileSizeZ = 128;
		thisRecastGraph.maxEdgeLength = 20;

		thisRecastGraph.walkableHeight = 2f;
		thisRecastGraph.walkableClimb = 0.5f;
		thisRecastGraph.characterRadius = 0.4f;
		thisRecastGraph.maxSlope = 30f;

		thisRecastGraph.Scan();

		bool wating = true;
		ActiveAstarPath.AddWorkItem(() =>
		{
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
			wating = false;
		});

		AstarPath.active.FlushWorkItems();

		while (wating)
		{
			await Awaitable.NextFrameAsync();
		}
	}


	private Bounds CalculateCombinedBounds(Transform parent)
	{
		bool initialized = false;
		Bounds result = default;

		Collider[] colliders = parent.GetComponentsInChildren<Collider>(true);

		for (int i = 0 ; i < colliders.Length ; i++)
		{
			Collider c = colliders[i];

			// LayerMask 필터
			int layer = c.gameObject.layer;
			if ((groundLayerMask.value & (1 << layer)) == 0)
				continue;

			Bounds b = c.bounds;

			if (!initialized)
			{
				result = b;
				initialized = true;
			}
			else
			{
				result.Encapsulate(b);
			}
		}

		// col 없는 경우 문제 방지 위해 최소 사이즈
		if (!initialized)
			result = new Bounds(parent.position, Vector3.one);

		return result;
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
				if (thisPointGraph != null)
				{
					thisPointGraph.Clear();
					AstarPath.active.data.RemoveGraph(thisPointGraph);
					thisPointGraph = null;
				}

				if(thisRecastGraph != null)
				{
					AstarPath.active.data.RemoveGraph(thisRecastGraph);
					thisRecastGraph = null;
				}
			});
			ActiveAstarPath.FlushWorkItems();
		}
	}

	internal void FindNodePath(Seeker thisSeeker, Vector3 prevPoint, Vector3 nextPoint, OnPathDelegate findPath)
	{
		GraphMask graphMask = GraphMask.FromGraphName("MainPointGraph");
		thisSeeker.StartPath(prevPoint, nextPoint, findPath, graphMask);
	}
	internal void FindNavPath(Seeker thisSeeker, Vector3 prevPoint, Vector3 nextPoint, OnPathDelegate findPath)
	{
		GraphMask graphMask = GraphMask.FromGraphName("MainRecastGraph");
		thisSeeker.StartPath(prevPoint, nextPoint, findPath);
	}
}