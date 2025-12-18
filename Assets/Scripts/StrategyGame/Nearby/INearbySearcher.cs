using System;
using System.Collections.Generic;

using UnityEngine;

public interface INearbySearcher
{
	INearbySearcher ThisSearcher { get; }
	INearbySearcherAPI SearcherAPI { get; }
	int FactionID => -1;
	bool IsEnable => true;
	Vector3 SearchCenter => Vector3.zero;
	float SearchRange => 0f;
	float SearchMinRange => 0f;
}
public interface INearbySearcherAPI
{
	int FactionID { get; }
	bool IsEnable { get;}
	void Init(INearbySearcher searcher);
	void Deinit();
	INearbyElement GetNearbyItem(Func<INearbyElement, bool> func = null);
	IEnumerable<INearbyElement> GetNearbyItems(Func<INearbyElement, bool> func = null);
	T GetNearbyItemType<T>(Func<T, bool> func = null) where T : class, IStrategyElement;
	IEnumerable<T> GetNearbyItemsType<T>(Func<T, bool> func = null) where T : class, IStrategyElement;
	void OnNearbySearching(INearbySearcherAPI succession, bool immediately = false) => OnNearbySearching(succession.GetNearbyItems());
	void OnNearbySearching(IEnumerable<INearbyElement> searchingElementList, bool immediately = false);
	void ClearSearching();
	HashSet<INearbyElement> EnterRageThisFrame();
	HashSet<INearbyElement> ExitRageThisFrame();
	public int NearbyCount();
	public bool HasNearby(INearbyElement nearby);
	public bool HasNearbySomthing() => NearbyCount() > 0;
}