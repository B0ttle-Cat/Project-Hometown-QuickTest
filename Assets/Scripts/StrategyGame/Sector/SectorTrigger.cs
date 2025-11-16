using System.Collections.Generic;

using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SectorTrigger : MonoBehaviour
{
	private Collider thisCollider;

	private HashSet<Collider> colliderList;
	private List<CaptureTag> captureTagList;

	public HashSet<Collider> ColliderList => colliderList;
	public List<CaptureTag> CaptureTagList => captureTagList;

	private void Awake()
	{
		thisCollider = GetComponent<Collider>();
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

	public bool OverlapTrigger(in Vector3 point)
	{
		var result = thisCollider.ClosestPoint(point);
		float sqrDistance = Vector3.SqrMagnitude(result - point);
		return Mathf.Approximately(sqrDistance, 0f);
	}
}
