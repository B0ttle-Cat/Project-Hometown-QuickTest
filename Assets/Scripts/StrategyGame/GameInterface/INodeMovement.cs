using UnityEngine;

public interface INodeMovement
{
	public struct MovementPlan
	{
		public int startNodeID;
		public int nextNodeID;

		public int endedNodeID;
	}


	public INodeMovement ThisMovement { get; }

	public Vector3 CurrentPosition { get; set; }
	public float MovementSpeed { get; set; }
	public Vector3 MoveDelta { get; set; }
}
