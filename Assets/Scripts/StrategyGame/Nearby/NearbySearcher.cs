using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class NearbySearcher : MonoBehaviour, INearbySearcher
{
	private INearbySearcherValueGetter valueGetter;
	[SerializeField]
	private float baseRadius;
	private List<INearbyElement> nearbyElements;

	private List<(INearbyElement element, float sqrDist)> tempList;

	public INearbySearcherValueGetter ValueGetter { get => valueGetter; private set => valueGetter = value; }
	public float Range => ValueGetter == null ? 0 : ValueGetter.SearcherRange;
	public float BaseRadius { get => baseRadius; set => baseRadius = value; }
	private HashSet<INearbyElement> IgnoreSet => ValueGetter == null ? null : ValueGetter.GetIgnoreList();
	void INearbySearcher.Init(INearbySearcherValueGetter valueGetter)
	{
		if (valueGetter == null) return;
		ValueGetter = valueGetter;
		nearbyElements = new List<INearbyElement>();
		tempList = new List<(INearbyElement, float)>();
	}
	void INearbySearcher.DeInit()
	{
		if (ValueGetter == null) return;
		ValueGetter = null;
		nearbyElements = null;
		tempList = null;
	}
	INearbyElement INearbySearcher.GetNearbyItem(Func<INearbyElement, bool> func)
	{
		if (ValueGetter == null) return null;

		foreach (var item in nearbyElements)
		{
			if (func == null || func.Invoke(item))
			{
				return item;
			}
		}
		return null;
	}
	IEnumerable<INearbyElement> INearbySearcher.GetNearbyItems(Func<INearbyElement, bool> func)
	{
		if (ValueGetter == null) return Enumerable.Empty<INearbyElement>();
		if (func == null)
			return nearbyElements;
		else
			return nearbyElements.Where(t => (func.Invoke(t)));
	}
	T INearbySearcher.GetNearbyItemType<T>(Func<T, bool> func)
	{
		if (ValueGetter == null) return null;

		foreach (var item in nearbyElements)
		{
			if (item is T t && (func == null || func.Invoke(t)))
			{
				return t;
			}
		}
		return null;
	}
	IEnumerable<T> INearbySearcher.GetNearbyItemsType<T>(Func<T, bool> func)
	{
		if (ValueGetter == null) return Enumerable.Empty<T>();
		if (func == null)
			return nearbyElements.Where(n => n is not null and T).Select(n => n as T);
		else
			return nearbyElements.Where(n => n is not null and T).Select(n => n as T).Where(t => (func.Invoke(t)));
	}
	void INearbySearcher.UpdateNearby(HashSet<INearbyElement> allElements)
	{
		if (ValueGetter == null) return;
		nearbyElements.Clear();

		if (allElements == null || allElements.Count == 0) return;

		Vector3 position = transform.position;
		float sqrRange = (Range + BaseRadius) * (Range + BaseRadius);

		tempList.Clear();

		foreach (var item in allElements)
		{
			if (item == null) continue;
			if (IgnoreSet != null && IgnoreSet.Contains(item)) continue;

			Vector3 delta = position - item.Position;
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
}
