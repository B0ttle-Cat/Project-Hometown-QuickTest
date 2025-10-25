using System.Collections.Generic;

using UnityEngine;

public class ControlBaseTrigger : MonoBehaviour
{
	private HashSet<Collider> colliderList;
	private List<CaptureTag> captureTagList;

	public HashSet<Collider> ColliderList => colliderList;
	public List<CaptureTag> CaptureTagList => captureTagList;

	private void Awake()
	{
		colliderList = new HashSet<Collider>();
		captureTagList = new List<CaptureTag>();
	}
    private void OnDestroy()
    {
		ClearList(colliderList);
		ClearList(captureTagList);
		colliderList = null;
		captureTagList = null;
		void  ClearList<T>(ICollection<T> list)
		{
			if (list == null) return;
			list.Clear();
		}
    }

    void OnTriggerEnter(Collider other)
	{
		if (colliderList.Add(other))
		{
			var unit = other.GetComponentInParent<CaptureTag>();
			captureTagList.Add(unit);
		}
	}

	void OnTriggerExit(Collider other)
	{
		if (colliderList.Remove(other))
		{
			var unit = other.GetComponentInParent<CaptureTag>();
			captureTagList.Remove(unit);
		}
	}
}
