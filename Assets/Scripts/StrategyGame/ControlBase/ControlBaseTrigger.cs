using System.Collections.Generic;

using UnityEngine;

public class ControlBaseTrigger : MonoBehaviour
{
	private List<Collider> triggingList;

	public List<Collider> TriggingList { get => triggingList; set => triggingList = value; }

	private void Awake()
	{
		triggingList = new List<Collider>();
	}

	void OnTriggerEnter(Collider other)
	{
		triggingList.Add(other);
	}

	void OnTriggerExit(Collider other)
	{
		triggingList.Remove(other);
	}
}
