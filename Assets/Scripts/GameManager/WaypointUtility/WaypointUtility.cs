using System;
using System.Collections.Generic;
using System.Linq;

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

	[Serializable]
	public record WaypointLine
	{
		[ShowInInspector,ReadOnly]
		private readonly int networkID;
		private readonly int startNodeID;
		private readonly int lastNodeID;
		private readonly Vector3[] points;
		private readonly Vector3[] reversPoint;
		[ShowInInspector,ReadOnly]
		private readonly float length;
		public int NetworkID => networkID;
		public (int start, int last) Tips => (startNodeID, lastNodeID);
		public Vector3[] Points => points;
		public Vector3[] ReversPoint => reversPoint;
		public float Length => length;

		public WaypointLine(int id, NetworkNode startNode, NetworkNode lastNode, Waypoint[] waypoints, int samplesPerSegment = 10)
		{
			networkID = id;
			startNodeID = startNode.NetworkID;
			lastNodeID = lastNode.NetworkID;
			points = GetLineWithWaypoints(startNode.Position, lastNode.Position, waypoints, samplesPerSegment);
			reversPoint = points.Reverse().ToArray();
			length = _Length();

			float _Length()
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
	}

	public static Vector3[] GetLineWithWaypoints(Vector3 start, Vector3 last, Waypoint[] waypoints, int samplesPerSegment = 10)
	{
		var result = new List<Vector3>(samplesPerSegment);
		float segmentPersamples = 1f / samplesPerSegment;
		if (waypoints == null || waypoints.Length == 0)
		{
			for (int i = 0 ; i <= samplesPerSegment ; i++)
			{
				float t = i * segmentPersamples;
				result.Add(Vector3.Lerp(start, last, t));
			}
			return result.ToArray();
		}
		int waypointsCount = waypoints.Length;

		// 전체 포인트 배열
		var points = new List<Vector3> { start };
		foreach (var wp in waypoints) points.Add(wp.point);
		points.Add(last);

		// 전체 width 배열
		var widths = new List<float>();
		// 시작점 width = 첫 웨이포인트 width
		widths.Add(waypointsCount > 0 ? Mathf.Clamp01(waypoints[0].width) : 1f);
		for (int i = 0 ; i < waypointsCount ; i++)
			widths.Add(Mathf.Clamp01(waypoints[i].width));
		// 마지막점 width = 마지막 웨이포인트 width
		widths.Add(waypointsCount > 0 ? Mathf.Clamp01(waypoints[waypointsCount - 1].width) : 1f);

		// 슬라이딩 윈도우로 Catmull-Rom 계산
		int pointCount = points.Count;
		var tempResult = new List<Vector3>(samplesPerSegment * pointCount);
		for (int i = 0 ; i < pointCount - 1 ; i++)
		{
			Vector3 P0 = i == 0 ? points[i] : points[i - 1];
			Vector3 P1 = points[i];
			Vector3 P2 = points[i + 1];
			Vector3 P3 = (i + 2 < pointCount) ? points[i + 2] : points[i + 1];

			// 현재 segment의 width = P1과 P2 사이
			float width = (widths[i] +  widths[i + 1]) * 0.5f;

			for (int s = 0 ; s <= samplesPerSegment ; s++)
			{
				float t = s * segmentPersamples;

				Vector3 catmullPoint = 0.5f * ((2f * P1) +
											   (-P0 + P2) * t +
											   (2f * P0 - 5f * P1 + 4f * P2 - P3) * t * t +
											   (-P0 + 3f * P1 - 3f * P2 + P3) * t * t * t);

				Vector3 linearPoint = Vector3.Lerp(P1, P2, t);
				Vector3 finalPoint = Vector3.Lerp(linearPoint, catmullPoint, width);

				if (tempResult.Count == 0 || tempResult[tempResult.Count - 1] != finalPoint)
					tempResult.Add(finalPoint);
			}
		}
		int tempResultCount = tempResult.Count;

		float[] cumulativeLength = new float[tempResultCount];
		cumulativeLength[0] = 0f;
		for (int i = 1 ; i < tempResultCount ; i++)
		{
			cumulativeLength[i] = cumulativeLength[i - 1] + Vector3.Distance(tempResult[i - 1], tempResult[i]);
		}

		float totalLength = cumulativeLength[^1];

		for (int i = 0 ; i <= samplesPerSegment ; i++)
		{
			float targetLength = totalLength * segmentPersamples * i;

			// targetLength에 맞는 segment 찾기
			int index = 0;
			while (index < tempResultCount - 1 && cumulativeLength[index + 1] < targetLength)
				index++;

			// 두 점 사이 보간
			float segmentLength = cumulativeLength[index + 1] - cumulativeLength[index];
			float localT = segmentLength > 0f ? (targetLength - cumulativeLength[index]) / segmentLength : 0f;
			result.Add(Vector3.Lerp(tempResult[index], tempResult[index + 1], localT));
		}

		return result.ToArray();
	}

}
