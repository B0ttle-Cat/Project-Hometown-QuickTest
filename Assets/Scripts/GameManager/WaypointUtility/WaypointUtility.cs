using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;

public static class WaypointUtility
{
	[System.Serializable]
	public struct Waypoint
	{
		public Vector3 point;
		// 앞뒤로 얼마나 곡선을 그리는지? (0 = 직선, 1 = 최대 곡선)
		[Range(0f, 1f)]
		public float width;
	}

	public record WaypointLine
	{
		[SerializeField,ReadOnly]
		private readonly int networkID;
		[SerializeField,ReadOnly]
		private readonly int startNodeID;
		[SerializeField,ReadOnly]
		private readonly int lastNodeID;
		[SerializeField,ReadOnly]
		private readonly Vector3[] points;
		[SerializeField,ReadOnly]
		private readonly float distance;
		public int NetworkID => networkID;
		public (int start, int last) Tips => (startNodeID, lastNodeID); 
		public Vector3[] Points => points;
		public float Distance => distance;

		public WaypointLine(int id, NetworkNode startNode, NetworkNode lastNode, Waypoint[] waypoints, int samplesPerSegment = 10)
		{
			networkID = id;
			startNodeID = startNode.NetworkID;
			lastNodeID = lastNode.NetworkID; 
			points = GetLineWithWaypoints(startNode.Position, lastNode.Position, waypoints, samplesPerSegment);
			distance = _Distance();
		}

		private float _Distance()
		{
			var points = this.points;
			int length = this.points.Length;
			if (length < 0) return 0f;

			float distance = 0f;
			Vector3 prev = this.points[0];
			Vector3 next = prev;
			for (int i = 0 ; i < length ; i++)
			{
				next = this.points[i];
				distance += Vector3.Distance(prev, next);
				prev = next;
			}
			return distance;
		}
	}

	public static Vector3[] GetLineWithWaypoints(Vector3 start, Vector3 last, Waypoint[] waypoints, int samplesPerSegment = 10)
	{
		var result = new List<Vector3>();
		if (waypoints == null || waypoints.Length == 0)
		{
			result.Add(start);
			result.Add(last);
			return result.ToArray();
		}

		// 전체 포인트 배열
		var points = new List<Vector3> { start };
		foreach (var wp in waypoints) points.Add(wp.point);
		points.Add(last);

		// 전체 width 배열
		var widths = new List<float>();
		// 시작점 width = 첫 웨이포인트 width
		widths.Add(waypoints.Length > 0 ? Mathf.Clamp01(waypoints[0].width) : 1f);
		for (int i = 0 ; i < waypoints.Length ; i++)
			widths.Add(Mathf.Clamp01(waypoints[i].width));
		// 마지막점 width = 마지막 웨이포인트 width
		widths.Add(waypoints.Length > 0 ? Mathf.Clamp01(waypoints[waypoints.Length - 1].width) : 1f);

		// 슬라이딩 윈도우로 Catmull-Rom 계산
		for (int i = 0 ; i < points.Count - 1 ; i++)
		{
			Vector3 P0 = i == 0 ? points[i] : points[i - 1];
			Vector3 P1 = points[i];
			Vector3 P2 = points[i + 1];
			Vector3 P3 = (i + 2 < points.Count) ? points[i + 2] : points[i + 1];

			// 현재 segment의 width = P1과 P2 사이
			float width = (widths[i] +  widths[i + 1]) * 0.5f;

			for (int s = 0 ; s <= samplesPerSegment ; s++)
			{
				float t = s / (float)samplesPerSegment;

				Vector3 catmullPoint = 0.5f * ((2f * P1) +
											   (-P0 + P2) * t +
											   (2f * P0 - 5f * P1 + 4f * P2 - P3) * t * t +
											   (-P0 + 3f * P1 - 3f * P2 + P3) * t * t * t);

				Vector3 linearPoint = Vector3.Lerp(P1, P2, t);
				Vector3 finalPoint = Vector3.Lerp(linearPoint, catmullPoint, width);

				if (result.Count == 0 || result[result.Count - 1] != finalPoint)
					result.Add(finalPoint);
			}
		}

		return result.ToArray();
	}

}
