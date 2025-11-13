using System;

using Unity.Collections;

using UnityEngine;

[Serializable]
public record NetworkNode : IEquatable<NetworkNode>
{
	[SerializeField, ReadOnly]
	private readonly int networkID;
	[SerializeField, ReadOnly]
	private readonly Vector3 position;
	[SerializeField, ReadOnly]
	private readonly string nodeName;

    public int NetworkID { get => networkID; }
	public Vector3 Position { get => position; }
	public string NodeName { get => nodeName; }

	public NetworkNode(int id, SectorObject sector)
	{
		networkID = id;
		if (sector != null)
		{
			nodeName = sector.SectorName;
			position = sector.transform.position;
		}
		else
		{
			nodeName = "";
			position = Vector3.zero;
		}
	}
   
    public override int GetHashCode()
    {
        return HashCode.Combine(nodeName);
    }
    public bool IsEmpty => string.IsNullOrWhiteSpace(nodeName);

}
