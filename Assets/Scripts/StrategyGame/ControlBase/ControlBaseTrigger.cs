using System.Collections.Generic;

using UnityEngine;

public class ControlBaseTrigger : MonoBehaviour
{
	private List<Collider> triggingList;
	private List<CaptureTag> captureTagList;

	public List<Collider> TriggingList => triggingList;
	public List<CaptureTag> CaptureTagList => captureTagList;

	private void Awake()
	{
		triggingList = new List<Collider>();
		captureTagList = new List<CaptureTag>();
	}

	void OnTriggerEnter(Collider other)
	{
		triggingList.Add(other);
		var unit = other.GetComponentInParent<CaptureTag>();
		if(unit != null)
		{
			captureTagList.Add(unit);
		}
	}

	void OnTriggerExit(Collider other)
	{
		triggingList.Remove(other);
		var unit = other.GetComponentInParent<CaptureTag>();
		if (captureTagList.Remove(unit))
		{

		}
	}
}
