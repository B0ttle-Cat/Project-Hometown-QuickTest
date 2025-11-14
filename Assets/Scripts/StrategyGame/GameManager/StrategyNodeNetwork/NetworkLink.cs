using System;

using Sirenix.OdinInspector;

using UnityEngine;

[Serializable]
public record NetworkLink : IEquatable<NetworkLink>
{
	[ShowInInspector, ReadOnly]
	private readonly int networkID;
	private readonly Vector3 position;
	[ShowInInspector, ReadOnly, HorizontalGroup, LabelText("A"), LabelWidth(20)]
	private readonly int startNodeID;
	[ShowInInspector, ReadOnly, HorizontalGroup, LabelText("B"), LabelWidth(20)]
	private readonly int lastNodeID;

	private readonly Vector3 start;
	private readonly Vector3 ended;

	[ShowInInspector, ReadOnly, HorizontalGroup, HideLabel]
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
