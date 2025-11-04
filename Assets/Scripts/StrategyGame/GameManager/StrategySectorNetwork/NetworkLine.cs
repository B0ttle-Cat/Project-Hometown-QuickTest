using Sirenix.OdinInspector;

using UnityEngine;

using static StrategyStartSetterData;
using static StrategyStartSetterData.NetworkData;

public class NetworkLine : NetworkItem
{

	[SerializeField, ReadOnly]
	private string nodeNameA;
	[SerializeField, ReadOnly]
	private string nodeNameB;

	[SerializeField, ReadOnly]
	private Vector3 pointA;
	[SerializeField, ReadOnly]
	private Vector3 pointB;

	[SerializeField]
	private ConnectDir connectDir;
	[SerializeField]
	private WaypointUtility.Waypoint[] waypoint;

    public string NodeNameA { get => nodeNameA; set => nodeNameA = value; }
    public string NodeNameB { get => nodeNameB; set => nodeNameB = value; }
    public ConnectDir ConnectDir { get => connectDir; set => connectDir = value; }

    public override void Setup(object nodeData)
	{
		if (nodeData is not NetworkData data) return;

		if (!StrategyManager.Collector.TryFindSector(data.sectorA, out var sectorA)) return;
		if (!StrategyManager.Collector.TryFindSector(data.sectorB, out var sectorB)) return;

		pointA = sectorA.transform.position;
		pointB = sectorB.transform.position;

		ConnectDir = data.connectDir;
		waypoint = data.waypoint;

		transform.position = (pointA + pointB) * 0.5f;
	}

	public Vector3[] GetLinePoint()
	{
		return WaypointUtility.GetLineWithWaypoints(pointA, pointB, waypoint);
	}
	public float Distance()
	{
		var points = GetLinePoint();
		int length = points.Length;

		float distance = 0f;
		Vector3 prev = pointA;
		Vector3 next = pointA;
		for (int i = 0 ; i < length ; i++)
        {
			next = points[i];
			distance += Vector3.Distance(prev, next);
			prev = next;
		}
		next = pointB;
		distance += Vector3.Distance(prev, next);
		return distance;
	}
}
