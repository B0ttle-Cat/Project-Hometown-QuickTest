using System;
using System.Collections.Generic;

public interface INearbySearcherValueGetter
{
	// IStrategyElement 가 선언된 class 에서만 사용 할수 있도록 강제 하도록 위함
	public IStrategyElement ThisElement { get; }
	public int FactionID { get; }

	INearbySearcher Searcher { get; set; }
	float SearcherRange { get; set; }
	public HashSet<INearbyElement> GetIgnoreList();
}
public interface INearbySearcher
{
	INearbySearcherValueGetter ValueGetter { get; }
	void Init(INearbySearcherValueGetter valueGetter);
	void DeInit();
	INearbyElement GetNearbyItem(Func<INearbyElement, bool> func);
	IEnumerable<INearbyElement> GetNearbyItems(Func<INearbyElement, bool> func);
	T GetNearbyItemType<T>(Func<T,bool> func = null) where T : class, INearbyElement;
	IEnumerable<T> GetNearbyItemsType<T>(Func<T, bool> func = null) where T : class, INearbyElement;
	void UpdateNearby(HashSet<INearbyElement> allElements);
}