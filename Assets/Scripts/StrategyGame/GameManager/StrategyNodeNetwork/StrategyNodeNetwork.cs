using System;
using System.Collections.Generic;
using System.Linq;

using Sirenix.OdinInspector;
using Sirenix.Utilities;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

using static WaypointUtility;

public partial class StrategyNodeNetwork : MonoBehaviour, IStrategyStartGame
{
	[SerializeField,ReadOnly] private NetworkNode[] networkNodes = Array.Empty<NetworkNode>();
	[SerializeField,ReadOnly] private NetworkLine[] networkLines = Array.Empty<NetworkLine>();

	[ShowInInspector,ReadOnly] private Dictionary<NetworkNode, NetworkLine[]> nodeLinks = new();
	[ShowInInspector,ReadOnly] private Dictionary<NetworkLine, WaypointLine> waypointLines = new();

	public async Awaitable Init(NetworkNode[] nodeList, StrategyStartSetterData.SectorLinkData[] sectorLinkData)
	{
		Transform parent = transform;

		networkNodes = nodeList;
		nodeLinks = new Dictionary<NetworkNode, NetworkLine[]>(networkNodes.Length);

		int length = sectorLinkData.Length;
		networkLines = new NetworkLine[length];
		waypointLines = new Dictionary<NetworkLine, WaypointLine>(length);
		for (int i = 0 ; i < length ; i++)
		{
			var data = sectorLinkData[i];

			// Backward(A <- B) 일 경우 Forward(A -> B) 방향으로 전환
			// 이후 있을 계산의 통일성을 위해여 반전시킨다.
			if (data.connectDir == NetworkLine.ConnectDirType.Backward)
				data = data.ReverseDir;

			var line = new NetworkLine(data);

			var nodeA = networkNodes.FirstOrDefault(n => n.NodeName == line.NodeNameA);
			var nodeB = networkNodes.FirstOrDefault(n => n.NodeName == line.NodeNameB);

			Vector3 start = nodeA.Position;
			Vector3 end = nodeB.Position;

			var waypointLine = new WaypointLine(start, end, data.waypoint);
			networkLines[i] = line;
			waypointLines[line] = waypointLine;
		}

		length = networkNodes.Length;
		for (int i = 0 ; i < length ; i++)
		{
			NetworkNode node = networkNodes[i];
			var nodeName = node.NodeName;
			var links = networkLines.Where(line => line.NodeNameA == nodeName || line.NodeNameB == nodeName).ToArray();
			nodeLinks.Add(node, links);
		}

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

		if (networkLines != null)
		{
			networkLines = null;
		}

		if (buildConnectGroups != null)
		{
			buildConnectGroups.Clear();
			buildConnectGroups = null;
		}

		if (nodeLinks != null)
		{
			nodeLinks.Clear();
			nodeLinks = null;
		}

		if (waypointLines != null)
		{
			waypointLines.Clear();
			waypointLines = null;
		}
	}
#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		_OnDrawGizmos();
	}
#endif
}
public partial class StrategyNodeNetwork // BuildConnectGroups
{
	[Serializable]
	private struct ConnectGroup
	{
		public string[] nodeNames;
	}

	[ShowInInspector,ReadOnly]
	private Dictionary<ConnectConditions, ConnectGroup[]> buildConnectGroups = new();

	private void BuildConnectGroups(ConnectConditions targetCondition)
	{
		if (networkNodes == null || networkLines == null)
			return;

		// Dictionary 초기화 (해당 condition 만 갱신)
		buildConnectGroups ??= new Dictionary<ConnectConditions, ConnectGroup[]>();

		var nodeDict = networkNodes.ToDictionary(n => n.NodeName, n => n);

		// Union-Find 초기화
		var parent = networkNodes.ToDictionary(n => n.NodeName, n => n.NodeName);

		string Find(string name)
		{
			if (parent[name] != name)
				parent[name] = Find(parent[name]);
			return parent[name];
		}

		void Union(string a, string b)
		{
			var pa = Find(a);
			var pb = Find(b);
			if (pa != pb)
				parent[pa] = pb;
		}

		// 조건에 맞는 라인 연결
		foreach (var line in networkLines)
		{
			if (!LineMatchesCondition(line, targetCondition)) continue;

			if (nodeDict.ContainsKey(line.NodeNameA) && nodeDict.ContainsKey(line.NodeNameB))
				Union(line.NodeNameA, line.NodeNameB);
		}

		// 루트별 그룹화
		var groupMap = new Dictionary<string, List<string>>();
		foreach (var node in networkNodes)
		{
			string root = Find(node.NodeName);
			if (!groupMap.TryGetValue(root, out var list))
			{
				list = new List<string>();
				groupMap[root] = list;
			}
			list.Add(node.NodeName);
		}

		// ConnectGroup 배열 생성
		var groups = groupMap.Values
		.Select(nodes => new ConnectGroup
		{
			nodeNames = nodes.ToArray()
		})
		.ToArray();

		// 기존 값 덮어쓰기 (해당 condition만)
		buildConnectGroups[targetCondition] = groups;
	}
	private bool LineMatchesCondition(NetworkLine line, ConnectConditions condition)
	{
		return condition switch
		{
			ConnectConditions.None => true,
			ConnectConditions.Disconnected => line.ConnectDir is NetworkLine.ConnectDirType.Disconnected,
			ConnectConditions.Forward => line.ConnectDir is NetworkLine.ConnectDirType.Forward,
			ConnectConditions.Backward => line.ConnectDir is NetworkLine.ConnectDirType.Backward,
			ConnectConditions.ConnectedAny => line.ConnectDir is NetworkLine.ConnectDirType.Forward
										  or NetworkLine.ConnectDirType.Backward
										  or NetworkLine.ConnectDirType.Both,
			ConnectConditions.ConnectedBoth => line.ConnectDir is NetworkLine.ConnectDirType.Both,
			_ => false,
		};
	}
}

public partial class StrategyNodeNetwork // FindShortestPath
{
	// 내부 핵심 Dijkstra 함수
	private bool FindShortestPathInternal(
		NetworkNode start, NetworkNode target, out List<NetworkNode> path,
		Func<PathContext, PathResult> pathFunction,
		ConnectConditions connectConditions)
	{
		path = new List<NetworkNode>();
		if (start.IsEmpty || target.IsEmpty)
			return false;

		// 동일 노드면 바로 반환
		if (start.NodeName == target.NodeName)
		{
			path.Add(start);
			return true;
		}

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

		// 🔹 2. start 노드가 속한 그룹 찾기
		ConnectGroup? startGroup = null;

		foreach (var group in groupArray)
		{
			if (group.nodeNames.Contains(start.NodeName))
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
		if (!startGroup.Value.nodeNames.Contains(target.NodeName))
			return false;

		// 🔹 4. 그룹 내 노드만 딕셔너리로 구성
		var nodeDict = networkNodes
		.Where(n => startGroup.Value.nodeNames.Contains(n.NodeName))
		.ToDictionary(n => n.NodeName);

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

			if(!nodeLinks.TryGetValue(current, out var linkLins)) continue;

			foreach (var line in linkLins)
			{
				if (!LineMatchesCondition(line, connectConditions)) continue;

				string neighborName = line.NodeNameA == current.NodeName ? line.NodeNameB : line.NodeNameA;
				if (!nodeDict.TryGetValue(neighborName, out var neighbor)) continue;
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
		if (networkNodes == null || networkLines == null) return;
		if (networkNodes.Length == 0 || networkLines.Length == 0) return;

		foreach (var line in networkLines)
		{
			if (line.IsEmpty) continue;

			// 실제 노드 찾기
			var nodeA = networkNodes.FirstOrDefault(n => n.NodeName == line.NodeNameA);
			var nodeB = networkNodes.FirstOrDefault(n => n.NodeName == line.NodeNameB);
			if (nodeA.IsEmpty || nodeB.IsEmpty) continue;

			Vector3 posA = nodeA.Position;
			Vector3 posB = nodeB.Position;

			// 연결선 색상
			Color color = line.ConnectDir switch
			{
				NetworkLine.ConnectDirType.Both => Color.green,
				NetworkLine.ConnectDirType.Forward => Color.cyan,
				NetworkLine.ConnectDirType.Backward => Color.magenta,
				_ => Color.gray
			};

			Handles.color = color;
			Handles.DrawAAPolyLine(3f, posA, posB);

			// 화살표 표시
			DrawArrow(posA, posB, line.ConnectDir);

			// 웨이포인트가 있으면 노란색 경로 표시
			if (waypointLines != null && waypointLines.TryGetValue(line, out var wline))
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
				NetworkLine.ConnectDirType.Both => "↔",
				NetworkLine.ConnectDirType.Forward => "→",
				NetworkLine.ConnectDirType.Backward => "←",
				_ => "|"
			};
			Vector3 mid = (posA + posB) * 0.5f;
			DrawLabel(mid, Vector3.zero, $"{line.NodeNameA} {arrowText} {line.NodeNameB}", color);
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

	private void DrawArrow(Vector3 from, Vector3 to, NetworkLine.ConnectDirType type)
	{
		Vector3 dir = (to - from).normalized;
		Vector3 mid = Vector3.Lerp(from, to, 0.5f);
		float size = Vector3.Distance(from, to) * 0.05f;

		switch (type)
		{
			case NetworkLine.ConnectDirType.Forward:
			Handles.ConeHandleCap(0, mid - dir * size * 0.5f, Quaternion.LookRotation(dir), size, EventType.Repaint);
			break;
			case NetworkLine.ConnectDirType.Backward:
			Handles.ConeHandleCap(0, mid + dir * size * 0.5f, Quaternion.LookRotation(-dir), size, EventType.Repaint);
			break;
			case NetworkLine.ConnectDirType.Both:
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
		public NetworkLine Line;         // 현재-다음 노드 연결 라인
		public float AccumulatedCost;    // 현재까지 경로의 총 비용
		public PathContext(NetworkNode from, NetworkNode to, NetworkLine line, float accumulatedCost)
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
			if (context.Line.IsEmpty)
			{
				// Line이 없는 경우 이동 불가
				return new PathResult(false, 0f);
			}

			StrategyManager.SectorNetwork.waypointLines.TryGetValue(context.Line, out var line);

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