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
	void Deinit();
	INearbyElement GetNearbyItem(Func<INearbyElement, bool> func);
	IEnumerable<INearbyElement> GetNearbyItems(Func<INearbyElement, bool> func = null);
	T GetNearbyItemType<T>(Func<T, bool> func = null) where T : class, IStrategyElement;
	IEnumerable<T> GetNearbyItemsType<T>(Func<T, bool> func = null) where T : class, IStrategyElement;
	void UpdateNearby(IEnumerable<INearbyElement> searchingElementList);
	HashSet<INearbyElement> EnterRageThisFrame();
	HashSet<INearbyElement> ExitRageThisFrame();
	public int NearbyCount();
	public bool HasNearby(INearbyElement nearby);
}