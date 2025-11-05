using System;

using Unity.Collections;

using UnityEngine;

[Serializable]
public struct NetworkNode : IEquatable<NetworkNode>
{
	[SerializeField, ReadOnly]
	private Vector3 position;
	[SerializeField, ReadOnly]
	private string nodeName;

	public Vector3 Position { get => position; }
	public string NodeName { get => nodeName; }

	public NetworkNode(SectorObject sector)
	{
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
   
	public override bool Equals(object obj)
    {
        return obj is NetworkNode node && Equals(node);
    }
    public bool Equals(NetworkNode other)
    {
        return nodeName == other.nodeName;
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(nodeName);
    }
    public bool IsEmpty => string.IsNullOrWhiteSpace(nodeName);
}
