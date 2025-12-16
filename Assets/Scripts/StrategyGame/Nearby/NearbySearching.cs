using System;
using System.Collections.Generic;
using System.Linq;

using Sirenix.OdinInspector;

using UnityEngine;

public class NearbySearching : MonoBehaviour, INearbySearcherAPI
{
	INearbySearcher thisSearcher;
	Vector3 SearchCenter => thisSearcher.SearchCenter;
	float SearchRange => thisSearcher.SearchRange;
	int FactionID => thisSearcher.FactionID;
	[ShowInInspector]
	private List<INearbyElement> nearbyElements;

	private List<(INearbyElement element, float sqrDist)> tempList;

	public void Init(INearbySearcher searcher)
	{
		thisSearcher = searcher;
		nearbyElements = new List<INearbyElement>();
		tempList = new List<(INearbyElement, float)>();
	}

	public void Deinit()
	{
		thisSearcher = null;
		nearbyElements = null;
		tempList = null;
	}

	
	INearbyElement INearbySearcherAPI.GetNearbyItem(Func<INearbyElement, bool> func)
	{
		if (thisSearcher.IsNullRef()) return null;

		foreach (var item in nearbyElements)
		{
			if (func == null || func.Invoke(item))
			{
				return item;
			}
		}
		return null;
	}
	IEnumerable<INearbyElement> INearbySearcherAPI.GetNearbyItems(Func<INearbyElement, bool> func)
	{
		if (thisSearcher.IsNullRef()) return null;

		if (func == null)
			return nearbyElements;
		else
			return nearbyElements.Where(t => (func.Invoke(t)));
	}
	T INearbySearcherAPI.GetNearbyItemType<T>(Func<T, bool> func)
	{
		if (thisSearcher.IsNullRef()) return null;

		foreach (var item in nearbyElements)
		{
			if (item is T t && (func == null || func.Invoke(t)))
			{
				return t;
			}
		}
		return null;
	}
	IEnumerable<T> INearbySearcherAPI.GetNearbyItemsType<T>(Func<T, bool> func)
	{
		if (func == null)
			return nearbyElements.Where(n => n is not null and T).Select(n => n as T);
		else
			return nearbyElements.Where(n => n is not null and T).Select(n => n as T).Where(t => (func.Invoke(t)));
	}
	void INearbySearcherAPI.UpdateNearby(HashSet<INearbyElement> allElements)
	{
		if (thisSearcher.IsNullRef()) return;

		nearbyElements.Clear();

		if (allElements == null || allElements.Count == 0) return;

		Vector3 center = SearchCenter;
		float sqrRange = SearchRange;
		sqrRange *= sqrRange;

		tempList.Clear();

		foreach (var item in allElements)
		{
			if (item == null) continue;

			if (FactionID == item.FactionID) continue;

			Vector3 delta = center - item.Position;
			float sqrDist = delta.sqrMagnitude;

			if (sqrDist <= sqrRange)
			{
				tempList.Add((item, sqrDist));
			}
		}

		int tempCount = tempList.Count;
		if (tempCount == 0) return;

		tempList.Sort((a, b) => a.sqrDist.CompareTo(b.sqrDist));	

		for (int i = 0 ; i < tempCount ; i++)
		{
			nearbyElements.Add(tempList[i].element);
		}

		tempList.Clear();
	}
	

	void OnDrawGizmos()
	{
		if (thisSearcher.IsNullRef()) return;
		float range = SearchRange;
		if (range <= 0) return;
		Vector3 center = SearchCenter;

		Gizmos.color = Color.red;
		float step = 2f * Mathf.PI / 10;
		// 첫 점
		Vector3 prev = center + new Vector3(Mathf.Cos(0f) * range, 0f, Mathf.Sin(0f) * range);

		for (int i = 1 ; i <= 10 ; i++)
		{
			float angle = i * step;
			Vector3 curr = center + new Vector3(Mathf.Cos(angle) * range, 0f, Mathf.Sin(angle) * range);
			Gizmos.DrawLine(prev, curr);
			prev = curr;
		}
	}
}
