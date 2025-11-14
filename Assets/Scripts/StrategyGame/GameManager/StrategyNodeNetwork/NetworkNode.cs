using System;

using Sirenix.OdinInspector;

using UnityEngine;

[Serializable]
public record NetworkNode : IEquatable<NetworkNode>
{
	[ShowInInspector, ReadOnly]
	private readonly int networkID;
	[ShowInInspector, ReadOnly]
	private readonly Vector3 position;
	[ShowInInspector, ReadOnly]
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
}
