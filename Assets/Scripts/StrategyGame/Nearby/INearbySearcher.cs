using System.Collections.Generic;

using UnityEngine;

public interface INearbyValueGetter
{
	GameObject gameObject { get; }
	INearbySearcher Searcher { get; set; }
	float SearcherRange { get; set; }
	public HashSet<INearbyElement> GetIgnoreList();
}
public interface INearbySearcher
{
	INearbyValueGetter ValueGetter { get; }
	void Init(INearbyValueGetter valueGetter);
	void DeInit();
	T GetNearbyItemType<T>() where T : class, INearbyElement;
	IEnumerable<T> GetNearbyItemsType<T>() where T : class, INearbyElement;
}