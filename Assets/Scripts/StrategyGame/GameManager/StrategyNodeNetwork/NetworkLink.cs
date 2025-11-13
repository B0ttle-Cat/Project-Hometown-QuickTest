using System;

using Sirenix.OdinInspector;

using UnityEngine;

[Serializable]
public record NetworkLink : IEquatable<NetworkLink>
{
	[SerializeField, ReadOnly]
	private readonly int networkID;
	[SerializeField, ReadOnly]
	private readonly Vector3 position;
	[SerializeField, ReadOnly]
	private readonly int startNodeID;
	[SerializeField, ReadOnly]
	private readonly int lastNodeID;

	[SerializeField, ReadOnly]
	private readonly Vector3 start;
	[SerializeField, ReadOnly]
	private readonly Vector3 ended;

	[SerializeField]
	private readonly ConnectDirType connectDir;

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
	public int NetworkID => networkID;
	public Vector3 Position { get => position; }
	public int StartNodeID { get => startNodeID; }
	public int LastNodeID { get => lastNodeID; }
	public ConnectDirType ConnectDir { get => connectDir; }

	public NetworkLink(int id, NetworkNode startNode, NetworkNode lastNode, ConnectDirType connectDir)
	{
		networkID = id;

		startNodeID = startNode.NetworkID;
		lastNodeID = lastNode.NetworkID;

		start = startNode.Position;
		ended = lastNode.Position;
		position = (start + ended) * 0.5f;
		position = Vector3.zero;

		this.connectDir = connectDir;
	}

    public override int GetHashCode()
    {
        return HashCode.Combine(networkID);
    }
}
