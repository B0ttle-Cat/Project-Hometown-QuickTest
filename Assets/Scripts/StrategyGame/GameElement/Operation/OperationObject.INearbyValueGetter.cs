using System.Collections.Generic;

using UnityEngine;

[RequireComponent(typeof(NearbySearcher))]
public partial class OperationObject : INearbySearcherValueGetter
{
	private float searcherRange;
	public INearbySearcher Searcher { get; set; }
	public float SearcherRange
	{
		get => searcherRange;
		set
		{
			if (Mathf.Approximately(searcherRange, value))
			{
				searcherRange = value;
			}
		}
	}
	private HashSet<INearbyElement> ignoreNearbyList;

	partial void InitNearby(in float baseRadius)
	{
		if (TryGetComponent<NearbySearcher>(out var nearbySearcher))
		{
			nearbySearcher = gameObject.AddComponent<NearbySearcher>();
		}
		nearbySearcher.BaseRadius = baseRadius;
		Searcher = nearbySearcher;
		Searcher.Init(this);
		ignoreNearbyList = new HashSet<INearbyElement>();
	}
	partial void DeInitNearby()
	{
		if (Searcher == null) return;
		Searcher.DeInit();
		Searcher = null;
		ignoreNearbyList?.Clear();
	}
	private void AddIgnoreNearbyList(INearbyElement item)
	{
		ignoreNearbyList ??= new HashSet<INearbyElement>();
		ignoreNearbyList.Add(item);
	}
	private void RemoveIgnoreNearbyList(INearbyElement item)
	{
		if (ignoreNearbyList == null) return;
		ignoreNearbyList.Remove(item);
	}
	HashSet<INearbyElement> INearbySearcherValueGetter.GetIgnoreList()
	{
		return ignoreNearbyList ??= new HashSet<INearbyElement>();
	}
}
