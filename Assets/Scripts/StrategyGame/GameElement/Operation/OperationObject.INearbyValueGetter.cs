using System.Collections.Generic;

using UnityEngine;

[RequireComponent(typeof(NearbySearcher))]
public partial class OperationObject : INearbyValueGetter
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

	partial void InitNearby()
	{
		Searcher = GetComponent<NearbySearcher>();
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
	HashSet<INearbyElement> INearbyValueGetter.GetIgnoreList()
	{
		return ignoreNearbyList ??= new HashSet<INearbyElement>();
	}
}
