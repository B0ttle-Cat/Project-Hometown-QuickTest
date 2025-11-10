using System;

using Sirenix.OdinInspector;

using UnityEngine;

using static StrategyStartSetterData;

[Serializable]
public struct NetworkLink: IEquatable<NetworkLink>
{
	[SerializeField, ReadOnly]
	private Vector3 position;
	[SerializeField, ReadOnly]
	private string nodeNameA;
	[SerializeField, ReadOnly]
	private string nodeNameB;

	[SerializeField, ReadOnly]
	private Vector3 start;
	[SerializeField, ReadOnly]
	private Vector3 ended;

	[SerializeField]
	private ConnectDirType connectDir;

	public enum ConnectDirType
	{
		[InspectorName("A ↔ B")]
		Both,
		[InspectorName("A → B")]
		Forward,
		[InspectorName("A ← B")]
		Backward,
		[InspectorName("A | B")]
		Disconnected
	}

	public Vector3 Position { get => position; }
	public string NodeNameA { get => nodeNameA; }
	public string NodeNameB { get => nodeNameB; }
	public ConnectDirType ConnectDir { get => connectDir; }

	public NetworkLink(SectorLinkData data)
	{
		if (data.connectDir == ConnectDirType.Backward)
			data = data.ReverseDir;

		nodeNameA = "";
		nodeNameB = "";

		start = Vector3.zero;
		ended = Vector3.zero;
		position = Vector3.zero;

		connectDir = data.connectDir;
		
		if (StrategyManager.Collector.TryFindSector(data.sectorA, out var sectorA))
		{
			if (StrategyManager.Collector.TryFindSector(data.sectorB, out var sectorB))
			{
				nodeNameA = sectorA.SectorName;
				nodeNameB = sectorB.SectorName;

				start = sectorA.transform.position;
				ended = sectorB.transform.position;
	
				connectDir = data.connectDir;
				position = (start + ended) * 0.5f;
			}
		}
	}

    public override bool Equals(object obj)
    {
        return obj is NetworkLink line && Equals(line);
    }

    public bool Equals(NetworkLink other)
    {
        return nodeNameA == other.nodeNameA &&
               nodeNameB == other.nodeNameB &&
               connectDir == other.connectDir;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(nodeNameA, nodeNameB, connectDir);
    }

    public bool IsEmpty => string.IsNullOrWhiteSpace(nodeNameA) || string.IsNullOrWhiteSpace(nodeNameB);
}
