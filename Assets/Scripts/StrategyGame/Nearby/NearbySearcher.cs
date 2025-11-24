using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class NearbySearcher : MonoBehaviour, INearbySearcher
{
	private INearbyValueGetter valueGetter;
	[SerializeField]
	private float baseRadius;
	private List<INearbyElement> nearbyElements;
	// 정렬 용
	private List<(INearbyElement element, float sqrDist)> tempList;

    public INearbyValueGetter ValueGetter { get => valueGetter; private set => valueGetter = value; }
	public float Range => ValueGetter == null ? 0 : ValueGetter.SearcherRange;
	public float BaseRadius { get => baseRadius; set => baseRadius = value; }
	private HashSet<INearbyElement> IgnoreSet => ValueGetter == null ? null :ValueGetter.GetIgnoreList();
	void INearbySearcher.Init(INearbyValueGetter valueGetter)
	{
		if (valueGetter == null) return;
		ValueGetter = valueGetter;
		nearbyElements = new List<INearbyElement>(); ;
		tempList = new List<(INearbyElement, float)>(); 
	}
	void INearbySearcher.DeInit()
	{
		if (ValueGetter == null) return;
		ValueGetter = null;
		nearbyElements = null;
		tempList = null;
	}
	T INearbySearcher.GetNearbyItemType<T>()
	{
		if (ValueGetter == null) return null;

		foreach (var item in nearbyElements)
		{
			if (item is T t)
			{
				return t;
			}
		}
		return null;
	}
	IEnumerable<T> INearbySearcher.GetNearbyItemsType<T>()
	{
		if(ValueGetter == null) return Enumerable.Empty<T>();

		return nearbyElements.Where(n => n is not null and T).Select(n => n as T);
	}

	public void UpdateNearby(INearbyElement[] allElements)
	{
		if(ValueGetter == null) return;
		nearbyElements.Clear();
	
		if (allElements == null || allElements.Length == 0) return;
		
		Vector3 position = transform.position;
		float sqrRange = (Range + BaseRadius) * (Range + BaseRadius);
	
		tempList.Clear();
	
		int length = allElements.Length;
		for (int i = 0 ; i < length ; i++)
		{
			INearbyElement item = allElements[i];
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
