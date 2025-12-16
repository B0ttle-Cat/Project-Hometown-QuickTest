using System;
using System.Collections.Generic;

using UnityEngine;

[RequireComponent(typeof(NearbySearching))]
public partial class OperationObject : INearbySearcher, INearbySearcherAPI
{

	public NearbySearching nearbySearcher;
	private Vector3 searchCenterPosition;
	private float searchViewRange;

	public INearbySearcher ThisSearcher => this;
	public INearbySearcherAPI SearcherAPI => nearbySearcher;
	Vector3 INearbySearcher.SearchCenter => searchCenterPosition;
	float INearbySearcher.SearchRange => searchViewRange + OperationRadius;
	int INearbySearcher.FactionID => factionID;


	partial void InitNearby(in float baseRadius)
    {
		if(!TryGetComponent<NearbySearching>(out nearbySearcher))
        {
			nearbySearcher = gameObject.AddComponent<NearbySearching>();
        }

		SearcherAPI.Init(this);
		StrategyManager.Collector.Add<INearbySearcher>(this);
	}

    partial void DeInitNearby()
    {
		if (nearbySearcher != null)
		{
			nearbySearcher.Deinit();
			nearbySearcher = null;
		}
		StrategyManager.Collector.Remove<INearbySearcher>(this);
	}

    void INearbySearcherAPI.Init(INearbySearcher searcher)
    {
		SearcherAPI.Init(searcher);
    }

    INearbyElement INearbySearcherAPI.GetNearbyItem(Func<INearbyElement, bool> func)
    {
        return SearcherAPI.GetNearbyItem(func);
    }

    IEnumerable<INearbyElement> INearbySearcherAPI.GetNearbyItems(Func<INearbyElement, bool> func)
    {
        return SearcherAPI.GetNearbyItems(func);
    }

    T INearbySearcherAPI.GetNearbyItemType<T>(Func<T, bool> func)
    {
        return SearcherAPI.GetNearbyItemType(func);
    }

    IEnumerable<T> INearbySearcherAPI.GetNearbyItemsType<T>(Func<T, bool> func)
    {
        return SearcherAPI.GetNearbyItemsType(func);
    }

    void INearbySearcherAPI.UpdateNearby(HashSet<INearbyElement> allElements)
    {
		SearcherAPI.UpdateNearby(allElements);
    }
}
