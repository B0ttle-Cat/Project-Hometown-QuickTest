using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Sirenix.OdinInspector;




#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

using static NetworkLink;
using static WaypointUtility;

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

	[SerializeField,ReadOnly,FoldoutGroup("NetworkInfo")] private NetworkNode[] networkNodes = Array.Empty<NetworkNode>();
	[SerializeField,ReadOnly,FoldoutGroup("NetworkInfo")] private NetworkLink[] networkLinks = Array.Empty<NetworkLink>();
	[SerializeField,ReadOnly,FoldoutGroup("NetworkInfo")] private WaypointLine[] networkLines = Array.Empty<WaypointLine>();
	[SerializeField,ReadOnly,FoldoutGroup("NetworkInfo")] private PointInfo[] allPointInfos = Array.Empty<PointInfo>();

	[ShowInInspector,ReadOnly,FoldoutGroup("NetworkInfo")] private Dictionary<SectorObject, NetworkNode> sectorToNode = new();
	[ShowInInspector,ReadOnly,FoldoutGroup("NetworkInfo")] private Dictionary<NetworkNode, SectorObject> nodeToSector = new();
	[ShowInInspector,ReadOnly,FoldoutGroup("NetworkInfo")] private Dictionary<NetworkNode, NetworkLink[]> nodeToLink = new();
	[ShowInInspector,ReadOnly,FoldoutGroup("NetworkInfo")] private Dictionary<NetworkLink, WaypointLine> linkToLine = new();

	public async Awaitable Init(NetworkNode[] nodeList, SectorObject[] sectorList, StrategyStartSetterData.SectorLinkData[] sectorLinkData)
	{
		Transform parent = transform;

		networkNodes = nodeList;
		sectorToNode = new Dictionary<SectorObject, NetworkNode>();
		nodeToSector = new Dictionary<NetworkNode, SectorObject>();
		int length = networkNodes.Length;
		for (int i = 0 ; i < length ; i++)
		{
			sectorToNode.Add(sectorList[i], networkNodes[i]);
			nodeToSector.Add(networkNodes[i], sectorList[i]);
		}

		nodeToLink = new Dictionary<NetworkNode, NetworkLink[]>(networkNodes.Length);

		length = sectorLinkData.Length;
		networkLinks = new NetworkLink[length];
		networkLines = new WaypointLine[length];
		linkToLine = new Dictionary<NetworkLink, WaypointLine>(length);
		for (int i = 0 ; i < length ; i++)
		{
			var data = sectorLinkData[i];

			// Backward(A <- B) 일 경우 Forward(A -> B) 방향으로 전환
			// 이후 있을 계산의 통일성을 위해여 반전시킨다.
			if (data.connectDir == NetworkLink.ConnectDirType.Backward)
				data = data.ReverseDir;

			if (!StrategyManager.Collector.TryFindSector(data.sectorA, out var sectorA)) continue;
			if (!StrategyManager.Collector.TryFindSector(data.sectorB, out var sectorB)) continue;
			var startNode = SectorToNode(sectorA);
			var lastNode = SectorToNode(sectorB);

			var line = new NetworkLink(i,startNode, lastNode, data.connectDir);
			var waypointLine = new WaypointLine(i, startNode, lastNode,data.waypoint);

			networkLinks[i] = line;
			networkLines[i] = waypointLine;
			linkToLine[line] = waypointLine;
		}

		length = networkNodes.Length;
		for (int i = 0 ; i < length ; i++)
		{
			NetworkNode node = networkNodes[i];
			var networkID = node.NetworkID;
			var links = networkLinks.Where(line => line.StartNodeID == networkID || line.LastNodeID == networkID).ToArray();
			nodeToLink.Add(node, links);
		}

		List<PointInfo> pointInfos = new List<PointInfo>();
		length = networkLines.Length;
		for (int i = 0 ; i < length ; i++)
		{
			var line = networkLines[i];
			var points = line.Points;
			int pointCount = points.Length;
			for (int ii = 0 ; ii < pointCount ; ii++)
			{
				Vector3 point = points[ii];

				int closetype = 0;
				if (ii < pointCount / 2) closetype = -1;
				else if (pointCount % 2 == 1 && ii == pointCount / 2) closetype = 0;
				else closetype = 1;

				pointInfos.Add(new PointInfo(point, line.NetworkID, closetype switch
				{
					-1 => line.Tips.start,
					1 => line.Tips.last,
					0 => FindClosestNodeWithoutLine(point, out var closestNode) ? closestNode.NetworkID : line.Tips.start,
					_ => line.Tips.start,
				}));
			}
		}
		allPointInfos = pointInfos.ToArray();

		pathResultCaches = new List<PathResultCache>();

		isInit = true;
	}
	void IStrategyStartGame.OnStartGame()
	{

	}
	void IStrategyStartGame.OnStopGame()
	{
		if (networkNodes != null)
		{
			networkNodes = null;
		}
		if (networkLinks != null)
		{
			networkLinks = null;
		}
		if (networkLines != null)
		{
			networkLines = null;
		}
		if (allPointInfos != null)
		{
			allPointInfos = null;
		}

		if (sectorToNode != null)
		{
			sectorToNode.Clear();
			sectorToNode = null;
		}
		if (nodeToSector != null)
		{
			nodeToSector.Clear();
			nodeToSector = null;
		}
		if (nodeToLink != null)
		{
			nodeToLink.Clear();
			nodeToLink = null;
		}
		if (linkToLine != null)
		{
			linkToLine.Clear();
			linkToLine = null;
		}
	}
#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		_OnDrawGizmos();
	}
#endif

	public int SectorToNodeIndex(SectorObject sector)
	{
		if (sector == null) return -1;
		NetworkNode node = SectorToNode(sector);
		if (node == null) return -1;
		return node.NetworkID;
	}
	public NetworkNode SectorToNode(SectorObject sector)
	{
		if (sector == null) return null;
		if (!sectorToNode.TryGetValue(sector, out var node)) return null;
		return node;
	}
	public SectorObject NodeIndexToSector(int nodeIndex)
	{
		if (nodeIndex < 0 || nodeIndex >= networkNodes.Length) return null;
		var node = networkNodes[nodeIndex];
		return NodeToSector(node);
	}
	public SectorObject NodeToSector(NetworkNode node)
	{
		if (node == null) return null;
		if (!nodeToSector.TryGetValue(node, out var sector)) return null;
		return sector;
	}
	public int NameToIndex(string nodeName)
	{
		if (string.IsNullOrWhiteSpace(nodeName)) return -1;

		int length = networkNodes.Length;
		for (int i = 0 ; i < length ; i++)
		{
			var node = networkNodes[i];
			if (node.NodeName == nodeName)
			{
				return i;
			}
		}
		return -1;
	}
	public string IndexToName(int index)
	{
		if (index < 0 || index >= networkNodes.Length) return "";
		return networkNodes[index].NodeName;
	}
	public NetworkNode IndexToNode(int index)
	{
		if (index < 0 || index >= networkNodes.Length) return default;
		return networkNodes[index];
	}
	public SectorObject IndexToSector(int index)
	{
		return NodeToSector(IndexToNode(index));
	}
	public int NodeToIndex(NetworkNode node)
	{
		int length = networkNodes.Length;
		for (int i = 0 ; i < length ; i++)
		{
			if (networkNodes[i].Equals(node))
			{
				return i;
			}
		}
		return -1;
	}
	public NetworkNode NameToNode(string nodeName)
	{
		if (string.IsNullOrWhiteSpace(nodeName)) return null;

		int length = networkNodes.Length;
		for (int i = 0 ; i < length ; i++)
		{
			var node = networkNodes[i];
			if (node.NodeName == nodeName)
			{
				return node;
			}
		}
		return default;
	}
	public string NodeToName(NetworkNode node) => node.NodeName;

	public bool TryGetLink(in NetworkNode start, in NetworkNode end, out NetworkLink link, out bool isRevers)
	{
		link = default;
		isRevers = false;
		if (!nodeToLink.TryGetValue(start, out var links)) return false;

		int length = links.Length;
		for (int i = 0 ; i < length ; i++)
		{
			var _link = links[i];

			if (_link.StartNodeID.Equals(end.NetworkID))
			{
				link = _link;
				isRevers = true;
				return true;
			}
			else if (_link.LastNodeID.Equals(end.NetworkID))
			{
				link = _link;
				isRevers = false;
				return true;
			}
		}
		return false;
	}
	public bool TryGetLine(in NetworkLink link, out WaypointLine line)
	{
		return linkToLine.TryGetValue(link, out line);
	}

	public bool FindClosestNodeWithoutLine(Vector3 position, out NetworkNode closestNode)
	{
		int minIndex = -1;
		float minDistance = float.MaxValue;
		object lockObj = new object();

		Parallel.For(0, networkNodes.Length,
			() => (Index: -1, Distance: float.MaxValue),
			(index, state, local) =>
			{
				var node = networkNodes[index];
				float sqrMagnitude = Vector3.SqrMagnitude(position - node.Position);

				if (sqrMagnitude < local.Distance)
				{
					local = (index, sqrMagnitude);
				}
				return local;
			},
			local =>
			{
				lock (lockObj)
				{
					if (local.Distance < minDistance)
					{
						minDistance = local.Distance;
						minIndex = local.Index;
					}
				}
			}
		);

		if (minIndex < 0)
		{
			closestNode = null;
			return false;
		}

		closestNode = networkNodes[minIndex];
		return true;
	}

	internal bool FindClosestNode(Vector3 position, out NetworkNode closestNode)
	{
		int minIndex = -1;
		float minDistance = float.MaxValue;
		object lockObj = new object();

		Parallel.For(0, allPointInfos.Length,
			() => (Index: -1, Distance: float.MaxValue),
			(index, state, local) =>
			{
				var node = allPointInfos[index];
				float sqrMagnitude = Vector3.SqrMagnitude(position - node.point);

				if (sqrMagnitude < local.Distance)
				{
					local = (index, sqrMagnitude);
				}
				return local;
			},
			local =>
			{
				lock (lockObj)
				{
					if (local.Distance < minDistance)
					{
						minDistance = local.Distance;
						minIndex = local.Index;
					}
				}
			}
		);

		if (minIndex < 0)
		{
			closestNode = null;
			return false;
		}

		var pointInfo = allPointInfos[minIndex];
		closestNode = networkNodes[pointInfo.closetNodeID];
		return true;
	}

}
public partial class StrategyNodeNetwork // ConnectDirType & ConnectConditions
{
	private bool IsAvailableLink(NetworkNode start, NetworkLink line, ConnectConditions condition)
	{
		return IsAvailableLink(GetConnectDirType(start, line), condition);
	}
	private bool IsAvailableLink(NetworkLink.ConnectDirType connectDir, ConnectConditions condition)
	{
		return condition switch
		{
			ConnectConditions.None => true,
			ConnectConditions.Disconnected => connectDir
				is NetworkLink.ConnectDirType.Disconnected,
			ConnectConditions.Forward => connectDir
				is NetworkLink.ConnectDirType.Forward
				or NetworkLink.ConnectDirType.Both,
			ConnectConditions.Backward => connectDir
				is NetworkLink.ConnectDirType.Backward
				or NetworkLink.ConnectDirType.Both,
			ConnectConditions.ConnectedAny => connectDir
				is NetworkLink.ConnectDirType.Forward
				or NetworkLink.ConnectDirType.Backward
				or NetworkLink.ConnectDirType.Both,
			ConnectConditions.ConnectedBoth => connectDir
				is NetworkLink.ConnectDirType.Both,
			_ => false,
		};
	}
	private ConnectDirType GetConnectDirType(NetworkNode start, NetworkLink line)
	{
		return line.ConnectDir switch
		{
			ConnectDirType.Forward => start.NetworkID == line.StartNodeID ? ConnectDirType.Forward : ConnectDirType.Backward,
			ConnectDirType.Backward => start.NetworkID == line.LastNodeID ? ConnectDirType.Backward : ConnectDirType.Forward,
			_ => line.ConnectDir,
		};
	}
}

public partial class StrategyNodeNetwork // FindShortestPath
{
	public struct SerchBuffer
	{
		public int parentID;
		public int nodeID;
		public float cost;
		public int deep;
		public SerchBuffer(int parentID, int nodeID, float cost, int deep)
		{
			this.parentID = parentID;
			this.nodeID = nodeID;
			this.cost = cost;
			this.deep = deep;
		}
	}
	public class SerchBufferComparer : IComparer<SerchBuffer>
	{
		public int Compare(SerchBuffer x, SerchBuffer y)
		{
			int c = x.cost.CompareTo(y.cost);
			if (c != 0) return c;

			// cost가 같을 때 중복 방지용
			// nodeID 로 구분
			return x.nodeID.CompareTo(y.nodeID);
		}
	}

	public readonly struct PathResultCache
	{
		private readonly int startID;
		private readonly int targetID;
		private readonly ConnectConditions conditions;
		private readonly NetworkNode[] pathResult;
		public readonly NetworkNode[] Path => pathResult;
		public PathResultCache(int startID, int targetID, ConnectConditions conditions, NetworkNode[] pathResult)
		{
			this.startID = startID;
			this.targetID = targetID;
			this.conditions = conditions;
			this.pathResult = pathResult;
		}

		public bool FindIt(int startID, int targetID, ConnectConditions conditions)
		{
			return this.startID == startID && this.targetID == targetID && this.conditions == conditions;
		}
	}

	private List<PathResultCache> pathResultCaches;
	// 내부 핵심 Dijkstra 함수
	private bool FindShortestPathInternal(
		in NetworkNode startNode, in NetworkNode targetNode, out List<NetworkNode> pathResult,
		Func<PathContext, PathResult> pathFunction,
		ConnectConditions connectConditions)
	{
		pathResult = new List<NetworkNode>();
		if (startNode == null || targetNode == null)
			return false;

		int startID = startNode.NetworkID;
		int targetID = targetNode.NetworkID;


		// 동일 노드면 바로 반환
		if (startID == targetID)
		{
			pathResult.Add(startNode);
			return true;
		}

		int findIndex = pathResultCaches.FindIndex(i => i.FindIt(startID, targetID, connectConditions));
		if (findIndex >= 0)
		{
			var findPathResult = pathResultCaches[findIndex].Path;
			if (findPathResult == null || findPathResult.Length == 0) return false;

			pathResult.AddRange(findPathResult);
			return true;
		}

		bool pathFuncIsNull = pathFunction == null;

		// current cost 
		HashSet<int> visited = new HashSet<int>();
		SortedSet<SerchBuffer> nextSearchList = new SortedSet<SerchBuffer>(new SerchBufferComparer());
		Dictionary<int, int> childToParent = new Dictionary<int, int>();

		NetworkNode thisNode =  startNode;
		NetworkLink[] thisLinks = nodeToLink[thisNode];

		visited.Add(startID);
		SerchingChild(thisNode, thisLinks, 0, 0);
		while (nextSearchList.Count > 0)
		{
			SerchBuffer min = nextSearchList.Min;
			int thisID = min.nodeID;
			int parentID = min.parentID;
			childToParent.Add(thisID, parentID);
			if (thisID == targetID)
			{
				break;
			}
			nextSearchList.Remove(min);
			thisNode = networkNodes[thisID];
			thisLinks = nodeToLink[thisNode];
			SerchingChild(thisNode, thisLinks, min.cost, min.deep);
		}
		void SerchingChild(NetworkNode parent, NetworkLink[] links, float cost, int deep)
		{
			int parentID = parent.NetworkID;

			foreach (var link in links)
			{
				int childID = link.StartNodeID != parentID ? link.StartNodeID : link.LastNodeID;
				if (visited.Contains(childID))
				{
					// 이미 방문한 노드
					continue;
				}
				if (!IsAvailableLink(parent, link, connectConditions))
				{
					// 이동 불가능 방향
					continue;
				}

				NetworkNode child = networkNodes[childID];
				PathResult result = PathResult.Default;
				if (!pathFuncIsNull)
				{
					try
					{
						result = pathFunction(new PathContext(parent, child, link, cost));
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
					}
				}

				// 이동 불가능 조건
				if (!result.Result) continue;

				visited.Add(childID);
				nextSearchList.Add(new SerchBuffer(parentID, childID, result.cost, deep + 1));
			}
		}
		if (childToParent.Count == 0)
		{
			pathResultCaches.Add(new PathResultCache(startID, targetID, connectConditions, null));
			return false;
		}

		int chlidID = nextSearchList.Min.nodeID;
		Stack<int> path = new Stack<int>();
		path.Push(chlidID);
		while (childToParent.TryGetValue(chlidID, out chlidID))
		{
			path.Push(chlidID);
		}

		while (path.TryPop(out int pathID))
		{
			pathResult.Add(networkNodes[pathID]);
		}
		pathResultCaches.Add(new PathResultCache(startID, targetID, connectConditions, pathResult.ToArray()));

		return true;
	}

	private bool FindShortestPathInternal(
		int start, int target, out List<NetworkNode> path,
		Func<PathContext, PathResult> pathFunction,
		ConnectConditions connectConditions)
	{
		NetworkNode startNode = IndexToNode(start);
		NetworkNode targetNode = IndexToNode(target);
		return FindShortestPathInternal(in startNode, in targetNode, out path, pathFunction, connectConditions);
	}

	// ----------------- 동기 / 비동기 공개 API -----------------
	public bool FindShortestPath(
		NetworkNode start, NetworkNode target, List<NetworkNode> path,
		Func<PathContext, PathResult> pathFunction = null,
		ConnectConditions connectConditions = ConnectConditions.Forward)
	{
		bool result = FindShortestPathInternal(start, target, out var tempPath, pathFunction, connectConditions);

		path.Clear();
		path.AddRange(tempPath);
		return result;
	}

	public async Awaitable<bool> FindShortestPathAsync(
		NetworkNode start, NetworkNode target, List<NetworkNode> path,
		Func<PathContext, PathResult> pathFunction = null,
		ConnectConditions connectConditions = ConnectConditions.Forward)
	{
		await Awaitable.BackgroundThreadAsync();
		bool result = FindShortestPathInternal(start, target, out var tempPath, pathFunction, connectConditions);
		await Awaitable.MainThreadAsync();

		path.Clear();
		path.AddRange(tempPath);

		return result;
	}
	public bool FindShortestPath(
		int start, int target, List<int> path,
		Func<PathContext, PathResult> pathFunction = null,
		ConnectConditions connectConditions = ConnectConditions.Forward)
	{
		bool result = FindShortestPathInternal(start, target, out var tempPath, pathFunction, connectConditions);

		path.Clear();
		path.AddRange(tempPath
			.Select(n => NodeToIndex(n))
			.Where(i => i >= 0));
		return result;
	}

	public async Awaitable<bool> FindShortestPathAsync(
		int start, int target, List<int> path,
		Func<PathContext, PathResult> pathFunction = null,
		ConnectConditions connectConditions = ConnectConditions.Forward)
	{
		await Awaitable.BackgroundThreadAsync();
		bool result = FindShortestPathInternal(start, target, out var tempPath, pathFunction, connectConditions);
		await Awaitable.MainThreadAsync();

		path.Clear();
		path.AddRange(tempPath
			.Select(n => NodeToIndex(n))
			.Where(i => i >= 0));

		return result;
	}

	public bool IsConnectedNode(int start, int target, ConnectConditions connectConditions = ConnectConditions.Forward)
	{
		return FindShortestPath(start, target, new List<int>(), null, connectConditions);
	}
	public bool IsConnectedNode(NetworkNode start, NetworkNode target, ConnectConditions connectConditions = ConnectConditions.Forward)
	{
		return FindShortestPath(start, target, new List<NetworkNode>(), null, connectConditions);
	}


	public void NodePathToVectorPath(in List<int> nodePath, out List<Vector3> path)
	{
		NodePathToVectorPath(nodePath.Select(n => IndexToNode(n)).ToList(), out path);
	}
	public void NodePathToVectorPath(in List<NetworkNode> nodePath, out List<Vector3> path)
	{
		int length = nodePath == null ? 0 : nodePath.Count;
		if (length < 2)
		{
			path = new List<Vector3>();
			return;
		}
		path = new List<Vector3>();
		NetworkNode prevNode = nodePath[0];
		NetworkNode nextNode = default;
		for (int i = 1 ; i < length ; i++)
		{
			nextNode = nodePath[i];
			if (!TryGetLink(in prevNode, in nextNode, out var link, out bool isRevers)) continue;
			if (!TryGetLine(in link, out var pointLine)) continue;
			prevNode = nextNode;

			if(i == 1)
			{
				path.AddRange(isRevers ? pointLine.Points : pointLine.ReversPoint);
			}
			else
			{
				path.AddRange((isRevers ? pointLine.Points : pointLine.ReversPoint)[1..^0]);
			}
		}
	}
	// ----------------- NodeDistance Comparer -----------------
	private class NodeDistanceComparer : IComparer<(float dist, NetworkNode node)>
	{
		public int Compare((float dist, NetworkNode node) a, (float dist, NetworkNode node) b)
		{
			int cmp = a.dist.CompareTo(b.dist);
			return cmp != 0 ? cmp : a.node.NetworkID.CompareTo(b.node.NetworkID);
		}
	}
}

public partial class StrategyNodeNetwork // OnDrawGizmos
{
#if UNITY_EDITOR
	private List<NetworkNode> testPathfinding;
	private List<Vector3> testPositions;
	[Button]
	private void TestPathfinding(
		[ValueDropdown("FindSectorObjectInScene")] SectorObject start,
		[ValueDropdown("FindSectorObjectInScene")] SectorObject last,
		ConnectConditions connectConditions)
	{
		if (pathResultCaches != null) pathResultCaches.Clear();
		testPathfinding = new List<NetworkNode>();
		testPositions = new List<Vector3>();
		if (FindShortestPath(SectorToNode(start), SectorToNode(last), testPathfinding, null, connectConditions))
		{

			NodePathToVectorPath(testPathfinding, out testPositions);
		}

	}
	private ValueDropdownList<SectorObject> FindSectorObjectInScene()
	{
		ValueDropdownList<SectorObject> list = new ValueDropdownList<SectorObject>();
		if (isInit)
		{
			var items = FindObjectsByType<SectorObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID);
			foreach (var item in items)
			{
				string name = $"{item.name} + ({SectorToNodeIndex(item)})";

				list.Add(item.name, item);
			}
		}
		return list;
	}

	private void _OnDrawGizmos()
	{
		if (!Selection.Contains(gameObject)) return;
		if (networkNodes == null || networkLinks == null) return;
		if (networkNodes.Length == 0 || networkLinks.Length == 0) return;

		foreach (var line in networkLinks)
		{
			if (line == null) continue;

			// 실제 노드 찾기
			var nodeA = networkNodes.FirstOrDefault(n => n.NetworkID == line.StartNodeID);
			var nodeB = networkNodes.FirstOrDefault(n => n.NetworkID == line.LastNodeID);
			if (nodeA == null || nodeB == null) continue;

			Vector3 posA = nodeA.Position;
			Vector3 posB = nodeB.Position;

			// 연결선 색상
			Color color = line.ConnectDir switch
			{
				NetworkLink.ConnectDirType.Both => Color.green,
				NetworkLink.ConnectDirType.Forward => Color.cyan,
				NetworkLink.ConnectDirType.Backward => Color.magenta,
				_ => Color.gray
			};

			Handles.color = color;
			Handles.DrawAAPolyLine(3f, posA, posB);

			// 화살표 표시
			DrawArrow(posA, posB, line.ConnectDir);

			// 웨이포인트가 있으면 노란색 경로 표시
			if (linkToLine != null && linkToLine.TryGetValue(line, out var wline))
			{
				var points = wline.Points;
				if (points != null && points.Length > 1)
				{
					Handles.color = Color.yellow;
					Handles.DrawAAPolyLine(2f, points);
				}
			}

			// 이름 시각화
			string arrowText = line.ConnectDir switch
			{
				NetworkLink.ConnectDirType.Both => "↔",
				NetworkLink.ConnectDirType.Forward => "→",
				NetworkLink.ConnectDirType.Backward => "←",
				_ => "|"
			};
			Vector3 mid = (posA + posB) * 0.5f;
			DrawLabel(mid, Vector3.zero, $"{line.StartNodeID} {arrowText} {line.LastNodeID}", color);
		}

		// 노드 표시
		Gizmos.color = Color.white;
		foreach (var node in networkNodes)
		{
			if (node == null) continue;

			Gizmos.DrawSphere(node.Position, 0.3f);
			DrawLabel(node.Position, Vector3.up * 0.5f, node.NodeName, Color.white);
		}


		if (testPathfinding == null) return;
		NetworkNode prev = null;
		Gizmos.color = Color.blue;
		foreach (NetworkNode node in testPathfinding)
		{
			if (node == null) continue;

			if (prev == null)
			{
				prev = node;
				Gizmos.DrawSphere(node.Position, 1);
			}
			else
			{
				Gizmos.DrawSphere(node.Position, 1f);
				Gizmos.DrawLine(prev.Position + Vector3.up, node.Position + Vector3.up);

				prev = node;
			}
		}
		if (testPositions == null) return;
		Gizmos.color = Color.yellow;
		foreach (Vector3 point in testPositions)
		{
			Gizmos.DrawSphere(point + Vector3.up, .5f);
		}
	}

	private void DrawArrow(Vector3 from, Vector3 to, NetworkLink.ConnectDirType type)
	{
		Vector3 dir = (to - from).normalized;
		Vector3 mid = Vector3.Lerp(from, to, 0.5f);
		float size = Vector3.Distance(from, to) * 0.05f;

		switch (type)
		{
			case NetworkLink.ConnectDirType.Forward:
			Handles.ConeHandleCap(0, mid - dir * size * 0.5f, Quaternion.LookRotation(dir), size, EventType.Repaint);
			break;
			case NetworkLink.ConnectDirType.Backward:
			Handles.ConeHandleCap(0, mid + dir * size * 0.5f, Quaternion.LookRotation(-dir), size, EventType.Repaint);
			break;
			case NetworkLink.ConnectDirType.Both:
			Handles.ConeHandleCap(0, mid - dir * size * 0.5f, Quaternion.LookRotation(dir), size, EventType.Repaint);
			Handles.ConeHandleCap(0, mid + dir * size * 0.5f, Quaternion.LookRotation(-dir), size, EventType.Repaint);
			break;
		}
	}

	public void DrawLabel(Vector3 worldPos, Vector3 offset, string text, Color? color = null, GUIStyle style = null)
	{
		if (SceneView.currentDrawingSceneView == null)
			return;

		Camera cam = SceneView.currentDrawingSceneView.camera;
		if (cam == null)
			return;

		worldPos += cam.transform.TransformDirection(offset); // 약간 위로 띄우기

		// 3D -> 2D 화면 좌표 변환
		Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

		// 카메라 뒤쪽에 있으면 표시 안 함
		if (screenPos.z < 0)
			return;

		// GUI 좌표계는 Y가 반대라서 변환 필요
		screenPos.y = SceneView.currentDrawingSceneView.position.height - screenPos.y;

		Handles.BeginGUI();

		Color prev = GUI.color;
		GUI.color = color ?? Color.white;

		style ??= EditorStyles.boldLabel;
		Vector2 size = style.CalcSize(new GUIContent(text));

		// 중심 정렬된 라벨
		Rect rect = new Rect(screenPos.x - size.x / 2f, screenPos.y - size.y / 2f, size.x, size.y);
		GUI.Label(rect, text, style);

		GUI.color = prev;
		Handles.EndGUI();
	}
#endif
}


public partial class StrategyNodeNetwork // NetworkPathFunctions
{
	public enum ConnectConditions : byte
	{
		None = 0,           // 조건 없음
		Disconnected = 1,   // 연결되지 않은 Link
		Forward = 2,        // 정방향 연결
		Backward = 3,       // 역방향 연결
		ConnectedAny = 4,   // 방향 상관없이 연결
		ConnectedBoth = 5   // 양방향 연결
	}
	public struct PathContext
	{
		public NetworkNode From;         // 현재 노드
		public NetworkNode To;           // 다음 노드
		public NetworkLink Link;         // 현재-다음 노드 연결 라인
		public float AccumulatedCost;    // 현재까지 경로의 총 비용
		public PathContext(NetworkNode from, NetworkNode to, NetworkLink link, float accumulatedCost)
		{
			From = from;
			To = to;
			Link = link;
			AccumulatedCost = accumulatedCost;
		}
	}
	public struct PathResult
	{
		public bool Result;   // true: 포함, false: 제외
		public float cost;    // 이번 이동 비용
		public PathResult(bool result)
		{
			this.Result = result;
			this.cost = 1f;
		}
		public PathResult(bool result = true, float cost = 1f)
		{
			this.Result = result;
			this.cost = cost;
		}

		public static readonly PathResult Default = new(true);
		public static PathResult UseCost(float cost) => new(true, cost);

		// ------------------- 조합 연산 -------------------
		/// <summary>Any / OR: 하나라도 Result=true이면 포함, 비용은 모두 합산</summary>
		public static PathResult Any(IEnumerable<PathResult> results)
		{
			bool anyTrue = false;
			float totalCost = 0f;

			foreach (var r in results)
			{
				if (r.Result) anyTrue = true;
				totalCost += r.cost;
			}

			return new PathResult(anyTrue, totalCost);
		}

		/// <summary>All / AND: 모든 Result=true여야 포함, 비용은 모두 합산</summary>
		public static PathResult All(IEnumerable<PathResult> results)
		{
			bool allTrue = true;
			float totalCost = 0f;

			foreach (var r in results)
			{
				if (!r.Result) allTrue = false;
				totalCost += r.cost;
			}

			return new PathResult(allTrue, totalCost);
		}

		/// <summary>Not / NOT: Result 반전, 비용 그대로</summary>
		public static PathResult Not(PathResult result)
		{
			return new PathResult(!result.Result, result.cost);
		}
	}

	public static Func<PathContext, PathResult> Any(params Func<PathContext, PathResult>[] funcs)
	{
		return context =>
		{
			return PathResult.Any(funcs.Select(f => f(context)));
		};
	}
	public static Func<PathContext, PathResult> All(params Func<PathContext, PathResult>[] funcs)
	{
		return context =>
		{
			return PathResult.All(funcs.Select(f => f(context)));
		};
	}
	public static Func<PathContext, PathResult> Not(Func<PathContext, PathResult> func)
	{
		return context =>
		{
			var result = func(context);
			return PathResult.Not(result);
		};
	}
	public static Func<PathContext, PathResult> DistanceCost(float threshold = float.PositiveInfinity)
	{
		return context =>
		{
			if (context.Link == null)
			{
				// Line이 없는 경우 이동 불가
				return new PathResult(false, 0f);
			}

			StrategyManager.SectorNetwork.linkToLine.TryGetValue(context.Link, out var line);

			float cost = line.Distance;
			if (float.IsPositiveInfinity(threshold) || context.AccumulatedCost + cost < threshold)
			{
				return new PathResult(true, cost);
			}
			return new PathResult(false, cost);
		};
	}

#if UNITY_EDITOR
	private static void TestUsingCode()
	{
		List<NetworkNode> nodePath = new List<NetworkNode>();
		StrategyManager.SectorNetwork.FindShortestPath(default, default, nodePath,
			DistanceCost(1000), ConnectConditions.Forward);
	}
#endif
}