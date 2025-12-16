using System;
using System.Collections.Generic;

using UnityEngine;

public interface INearbySearcher
{
	INearbySearcher ThisSearcher { get; }
	INearbySearcherAPI SearcherAPI { get; }
	Vector3 SearchCenter { get; }
	float SearchRange { get; }
	int FactionID { get; }
}
public interface INearbySearcherAPI
{
	void Init(INearbySearcher searcher);
	//void DeInit();
	INearbyElement GetNearbyItem(Func<INearbyElement, bool> func);
	IEnumerable<INearbyElement> GetNearbyItems(Func<INearbyElement, bool> func);
	T GetNearbyItemType<T>(Func<T, bool> func = null) where T : class, INearbyElement;
	IEnumerable<T> GetNearbyItemsType<T>(Func<T, bool> func = null) where T : class, INearbyElement;
	void UpdateNearby(HashSet<INearbyElement> allElements);
}