using System;
using System.Collections.Generic;
using System.Linq;

using Sirenix.OdinInspector;

using UnityEditor;

using UnityEngine;

public class NearbySearching : MonoBehaviour, INearbySearcherAPI
{
	INearbySearcher thisSearcher;
	Vector3 SearchCenter => thisSearcher.SearchCenter;
	float SearchRange => thisSearcher.SearchRange;
	int FactionID => thisSearcher.FactionID;
	[ShowInInspector]
	private INearbyElement[] nearbyElements;
	private List<(INearbyElement element, float sqrDist)> tempList;
	private int nearbyCount;

	public void Init(INearbySearcher searcher)
	{
		thisSearcher = searcher;
		nearbyElements = new INearbyElement[0];
		tempList = new();
		nearbyCount = 0;
	}

	public void Deinit()
	{
		thisSearcher = null;
		nearbyElements = null;
	}

	#region INearbySearcherAPI
	INearbyElement INearbySearcherAPI.GetNearbyItem(Func<INearbyElement, bool> func)
	{
		if (thisSearcher.IsNullRef() || func == null) return null;

		return OnGetNearbyItem(func);
	}
	IEnumerable<INearbyElement> INearbySearcherAPI.GetNearbyItems(Func<INearbyElement, bool> func)
	{
		if (thisSearcher.IsNullRef()) return null;

		return OnGetNearbyItems(func);
	}
	T INearbySearcherAPI.GetNearbyItemType<T>(Func<T, bool> func)
	{
		if (thisSearcher.IsNullRef()) return null;

		return OnGetNearbyItemType<T>(func);
	}
	IEnumerable<T> INearbySearcherAPI.GetNearbyItemsType<T>(Func<T, bool> func)
	{
		if (thisSearcher.IsNullRef()) return null;

		return OnGetNearbyItemsType<T>(func);
	}
	void INearbySearcherAPI.UpdateNearby(IEnumerable<INearbyElement> searchingElementList)
	{
		if (thisSearcher.IsNullRef()) return;

		OnUpdateNearby(searchingElementList);
	}
	#endregion

	protected virtual INearbyElement OnGetNearbyItem(Func<INearbyElement, bool> func)
	{
		for (int i = 0 ; i < nearbyCount ; i++)
		{
			INearbyElement item = nearbyElements[i];
			if (item.IsNullRef()) continue;

			if (func.Invoke(item))
			{
				return item;
			}
		}
		return null;
	}

	protected virtual IEnumerable<INearbyElement> OnGetNearbyItems(Func<INearbyElement, bool> func)
	{
		if (func == null)
			return nearbyElements.Take(nearbyCount).Where(item => item.IsNotNullRef());
		else
			return nearbyElements.Take(nearbyCount).Where(item => item.IsNotNullRef() && (func.Invoke(item)));
	}

	protected virtual T OnGetNearbyItemType<T>(Func<T, bool> func) where T : class, INearbyElement
	{
		for (int i = 0 ; i < nearbyCount ; i++)
		{
			INearbyElement item = nearbyElements[i];
			if (item.IsNullRef() || item is not T t) continue;

			if (func == null || func.Invoke(t))
			{
				return t;
			}
		}
		return null;
	}

	protected virtual IEnumerable<T> OnGetNearbyItemsType<T>(Func<T, bool> func) where T : class, INearbyElement
	{
		return nearbyElements.Take(nearbyCount)
			.Where(item => item.IsNotNullRef() && item is T t && (func == null || func.Invoke(t)))
			.Select(item => (T)item);
	}

	protected virtual void OnUpdateNearby(IEnumerable<INearbyElement> searchingElementList)
	{
		nearbyCount = 0;

		if (searchingElementList == null || searchingElementList.Count() == 0) return;

		Vector3 center = SearchCenter;
		float sqrRange = SearchRange;
		sqrRange *= sqrRange;

		tempList.Clear();

		foreach (var item in searchingElementList)
		{
			if (item.IsNullRef()) continue;

			if (FactionID == item.FactionID) continue;

			Vector3 delta = center - item.Position;
			float sqrDist = delta.sqrMagnitude;

			if (sqrDist <= sqrRange)
			{
				tempList.Add((item, sqrDist));
				nearbyCount++;
			}
		}

		if (nearbyCount == 0)
		{
			if (nearbyElements.Length > 0)
			{
				Array.Resize(ref nearbyElements, 0);
			}
			return;
		}
		if (nearbyElements.Length < nearbyCount)
		{
			Array.Resize(ref nearbyElements, nearbyCount);
		}

		tempList.Sort((a, b) => a.sqrDist.CompareTo(b.sqrDist));

		for (int i = 0 ; i < nearbyCount ; i++)
		{
			nearbyElements[i] = tempList[i].element;
		}
		tempList.Clear();
	}


#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		if (thisSearcher.IsNullRef()) return;
		float range = SearchRange;
		if (range <= 0) return;
		Vector3 center = SearchCenter;

		Handles.color = Color.red;
		Handles.DrawWireDisc(center, Vector3.up, range);
	}
#endif
}
