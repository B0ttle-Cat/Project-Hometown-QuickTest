using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Sirenix.OdinInspector;
using Sirenix.Utilities;

using Unity.VisualScripting;


#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

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

	[SerializeField,ReadOnly] private NetworkNode[] networkNodes = Array.Empty<NetworkNode>();
	[SerializeField,ReadOnly] private NetworkLink[] networkLinks = Array.Empty<NetworkLink>();
	[SerializeField,ReadOnly] private WaypointLine[] networkLines = Array.Empty<WaypointLine>();
	[SerializeField,ReadOnly] private PointInfo[] allPointInfos = Array.Empty<PointInfo>();

	[ShowInInspector,ReadOnly] private Dictionary<SectorObject, NetworkNode> sectorToNode = new();
	[ShowInInspector,ReadOnly] private Dictionary<NetworkNode, SectorObject> nodeToSector = new();
	[ShowInInspector,ReadOnly] private Dictionary<NetworkNode, NetworkLink[]> nodeToLink = new();
	[ShowInInspector,ReadOnly] private Dictionary<NetworkLink, WaypointLine> linkToNode = new();

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
		linkToNode = new Dictionary<NetworkLink, WaypointLine>(length);
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
			linkToNode[line] = waypointLine;
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

		buildConnectGroups = new Dictionary<ConnectConditions, ConnectGroup[]>();
		BuildConnectGroups(ConnectConditions.Forward);
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
		if (linkToNode != null)
		{
			linkToNode.Clear();
			linkToNode = null;
		}

		if (buildConnectGroups != null)
		{
			buildConnectGroups.Clear();
			buildConnectGroups = null;
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
		if(string.IsNullOrWhiteSpace(nodeName)) return -1;

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

	public bool TryGetLink(in NetworkNode start, in NetworkNode end, out NetworkLink link)
	{
		link = default;
		if (!nodeToLink.TryGetValue(start, out var links)) return false;

		int length = links.Length;
		for (int i = 0 ; i < length ; i++)
		{
			if (links[i].LastNodeID.Equals(end.NodeName))
			{
				link = links[i];
				return true;
			}
		}
		return false;
	}
	public bool TryGetLine(in NetworkLink link, out WaypointLine line)
	{
		return linkToNode.TryGetValue(link, out line);
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
public partial class StrategyNodeNetwork // BuildConnectGroups
{
	[Serializable]
	private struct ConnectGroup
	{
		public int[] nodeNetworkID;
	}

	[ShowInInspector,ReadOnly]
	private Dictionary<ConnectConditions, ConnectGroup[]> buildConnectGroups = new();

	private void BuildConnectGroups(ConnectConditions targetCondition)
	{
		if (networkNodes == null || networkLinks == null)
			return;

		// Dictionary 초기화 (해당 condition 만 갱신)
		buildConnectGroups ??= new Dictionary<ConnectConditions, ConnectGroup[]>();

		var nodeDict = networkNodes.ToDictionary(n => n.NetworkID, n => n);

		// Union-Find 초기화
		var parent = networkNodes.ToDictionary(n => n.NetworkID, n => n.NetworkID);

		int Find(int id)
		{
			if (parent[id] != id)
				parent[id] = Find(parent[id]);
			return parent[id];
		}

		void Union(int a, int b)
		{
			var pa = Find(a);
			var pb = Find(b);
			if (pa != pb)
				parent[pa] = pb;
		}

		// 조건에 맞는 라인 연결
		foreach (var line in networkLinks)
		{
			if (!LineMatchesCondition(line, targetCondition)) continue;

			if (nodeDict.ContainsKey(line.StartNodeID) && nodeDict.ContainsKey(line.LastNodeID))
				Union(line.StartNodeID, line.LastNodeID);
		}

		// 루트별 그룹화
		var groupMap = new Dictionary<int, List<int>>();
		foreach (var node in networkNodes)
		{
			int root = Find(node.NetworkID);
			if (!groupMap.TryGetValue(root, out var list))
			{
				list = new List<int>();
				groupMap[root] = list;
			}
			list.Add(node.NetworkID);
		}

		// ConnectGroup 배열 생성
		var groups = groupMap.Values
		.Select(nodes => new ConnectGroup
		{
			nodeNetworkID = nodes.ToArray()
		})
		.ToArray();

		// 기존 값 덮어쓰기 (해당 condition만)
		buildConnectGroups[targetCondition] = groups;
	}
	private bool LineMatchesCondition(NetworkLink line, ConnectConditions condition)
	{
		return condition switch
		{
			ConnectConditions.None => true,
			ConnectConditions.Disconnected => line.ConnectDir is NetworkLink.ConnectDirType.Disconnected,
			ConnectConditions.Forward => line.ConnectDir is NetworkLink.ConnectDirType.Forward,
			ConnectConditions.Backward => line.ConnectDir is NetworkLink.ConnectDirType.Backward,
			ConnectConditions.ConnectedAny => line.ConnectDir is NetworkLink.ConnectDirType.Forward
										  or NetworkLink.ConnectDirType.Backward
										  or NetworkLink.ConnectDirType.Both,
			ConnectConditions.ConnectedBoth => line.ConnectDir is NetworkLink.ConnectDirType.Both,
			_ => false,
		};
	}


	public bool IsConnectedNode(ConnectConditions connectConditions, int startID, int endedID)
	{
		// 🔹 1. 해당 조건의 그룹이 미리 계산되어 있는지 확인
		if (!buildConnectGroups.TryGetValue(connectConditions, out var groupArray) || groupArray == null)
		{
			BuildConnectGroups(connectConditions);
			if (!buildConnectGroups.TryGetValue(connectConditions, out groupArray) || groupArray == null)
			{
				Debug.LogWarning($"[FindShortestPathInternal] No ConnectGroup found for condition {connectConditions}");
				return false;
			}
		}

		// 🔹 2. startPoint 노드가 속한 그룹 찾기
		ConnectGroup? startGroup = null;

		foreach (var group in groupArray)
		{
			if (group.nodeNetworkID.Contains(startID))
			{
				startGroup = group;
				break;
			}
		}

		if (startGroup is null)
		{
			Debug.LogWarning($"[FindShortestPathInternal] Start node {networkNodes[startID].NodeName} not found in any ConnectGroup for {connectConditions}");
			return false;
		}

		// 🔹 3. 타겟 노드가 같은 그룹에 없으면 탐색 불필요
		if (!startGroup.Value.nodeNetworkID.Contains(endedID))
			return false;

		return true;
	}
}

public partial class StrategyNodeNetwork // FindShortestPath
{
	// 내부 핵심 Dijkstra 함수
	private bool FindShortestPathInternal(
		in NetworkNode start, in NetworkNode target, out List<NetworkNode> path,
		Func<PathContext, PathResult> pathFunction,
		ConnectConditions connectConditions)
	{
		path = new List<NetworkNode>();
		if (start.IsEmpty || target.IsEmpty)
			return false;

		// 동일 노드면 바로 반환
		if (start.NetworkID == target.NetworkID)
		{
			path.Add(start);
			return true;
		}

		IsConnectedNode(connectConditions, start.NetworkID, target.NetworkID);

		// 🔹 1. 해당 조건의 그룹이 미리 계산되어 있는지 확인
		if (!buildConnectGroups.TryGetValue(connectConditions, out var groupArray) || groupArray == null)
		{
			BuildConnectGroups(connectConditions);
			if (!buildConnectGroups.TryGetValue(connectConditions, out groupArray) || groupArray == null)
			{
				Debug.LogWarning($"[FindShortestPathInternal] No ConnectGroup found for condition {connectConditions}");
				return false;
			}
		}

		// 🔹 2. startPoint 노드가 속한 그룹 찾기
		ConnectGroup? startGroup = null;

		foreach (var group in groupArray)
		{
			if (group.nodeNetworkID.Contains(start.NetworkID))
			{
				startGroup = group;
				break;
			}
		}

		if (startGroup is null)
		{
			Debug.LogWarning($"[FindShortestPathInternal] Start node {start.NodeName} not found in any ConnectGroup for {connectConditions}");
			return false;
		}

		// 🔹 3. 타겟 노드가 같은 그룹에 없으면 탐색 불필요
		if (!startGroup.Value.nodeNetworkID.Contains(target.NetworkID))
			return false;

		// 🔹 4. 그룹 내 노드만 딕셔너리로 구성
		var nodeDict = networkNodes
		.Where(n => startGroup.Value.nodeNetworkID.Contains(n.NetworkID))
		.ToDictionary(n => n.NetworkID);

		bool pathFuncIsNull = pathFunction == null;

		var distances = new Dictionary<NetworkNode, float>();
		var previous = new Dictionary<NetworkNode, NetworkNode>();
		var visited = new HashSet<NetworkNode>();
		var pq = new SortedSet<(float dist, NetworkNode node)>(new NodeDistanceComparer());

		foreach (var node in nodeDict.Values)
			distances[node] = float.MaxValue;

		distances[start] = 0f;
		pq.Add((0f, start));

		// 🔹 5. Dijkstra
		while (pq.Count > 0)
		{
			var currentPair = pq.Min;
			pq.Remove(currentPair);

			var current = currentPair.node;
			if (visited.Contains(current)) continue;
			visited.Add(current);

			if (current.NodeName == target.NodeName) break;

			if (!nodeToLink.TryGetValue(current, out var linkLins)) continue;

			foreach (var line in linkLins)
			{
				if (!LineMatchesCondition(line, connectConditions)) continue;

				int neighborID = line.StartNodeID == current.NetworkID ? line.LastNodeID : line.StartNodeID;
				if (!nodeDict.TryGetValue(neighborID, out var neighbor)) continue;
				if (visited.Contains(neighbor)) continue;

				PathResult result = PathResult.Default;
				if (!pathFuncIsNull)
				{
					try
					{
						result = pathFunction(new PathContext(current, neighbor, line, distances[current]));
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						return false;
					}
				}

				if (!result.Result) continue;

				float newDist = distances[current] + result.cost;
				if (newDist < distances[neighbor])
				{
					distances[neighbor] = newDist;
					previous[neighbor] = current;
					pq.Add((newDist, neighbor));
				}
			}
		}

		// 🔹 6. 경로 복원
		if (!previous.ContainsKey(target))
			return false;

		var nodeTrace = target;
		while (!nodeTrace.IsEmpty)
		{
			path.Add(nodeTrace);
			previous.TryGetValue(nodeTrace, out nodeTrace);
		}
		path.Reverse();

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
			if (!TryGetLink(in prevNode, in nextNode, out var link)) continue;
			if (!TryGetLine(in link, out var pointLine)) continue;

			path.AddRange(pointLine.Points);
		}
	}
	// ----------------- NodeDistance Comparer -----------------
	private class NodeDistanceComparer : IComparer<(float dist, NetworkNode node)>
	{
		public int Compare((float dist, NetworkNode node) a, (float dist, NetworkNode node) b)
		{
			int cmp = a.dist.CompareTo(b.dist);
			return cmp != 0 ? cmp : a.node.NodeName.CompareTo(b.node.NodeName);
		}
	}
}

public partial class StrategyNodeNetwork // OnDrawGizmos
{
#if UNITY_EDITOR
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
			if (nodeA.IsEmpty || nodeB.IsEmpty) continue;

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
			if (linkToNode != null && linkToNode.TryGetValue(line, out var wline))
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
		foreach (var node in networkNodes)
		{
			if (node.IsEmpty) continue;

			Gizmos.color = Color.white;
			Gizmos.DrawSphere(node.Position, 0.3f);
			DrawLabel(node.Position, Vector3.up * 0.5f, node.NodeName, Color.white);
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
		Disconnected = 1,   // 연결되지 않은 Line
		Forward = 2,        // 정방향 연결
		Backward = 3,       // 역방향 연결
		ConnectedAny = 4,   // 방향 상관없이 연결
		ConnectedBoth = 5   // 양방향 연결
	}
	public struct PathContext
	{
		public NetworkNode From;         // 현재 노드
		public NetworkNode To;           // 다음 노드
		public NetworkLink Line;         // 현재-다음 노드 연결 라인
		public float AccumulatedCost;    // 현재까지 경로의 총 비용
		public PathContext(NetworkNode from, NetworkNode to, NetworkLink line, float accumulatedCost)
		{
			From = from;
			To = to;
			Line = line;
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
			if (context.Line == null)
			{
				// Line이 없는 경우 이동 불가
				return new PathResult(false, 0f);
			}

			StrategyManager.SectorNetwork.linkToNode.TryGetValue(context.Line, out var line);

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