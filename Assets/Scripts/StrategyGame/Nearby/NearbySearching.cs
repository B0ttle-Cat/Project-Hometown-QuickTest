using System;
using System.Collections.Generic;
using System.Linq;

using Sirenix.OdinInspector;

using UnityEditor;

using UnityEngine;

public class NearbySearching : MonoBehaviour, INearbySearcherAPI
{
	protected INearbySearcher thisSearcher;
	protected Vector3 SearchCenter => thisSearcher.SearchCenter;
	protected float SearchRange => thisSearcher.SearchRange;
	protected float SearchMinRange => thisSearcher.SearchMinRange;
	public const float CutMinRange = 0.0001f;
	protected int FactionID => thisSearcher.FactionID;

	[ShowInInspector]
	protected INearbyElement[] currentNearbyElements;
	protected List<(INearbyElement element, float sqrDist)> tempList;
	protected HashSet<INearbyElement> enterRangeThisFrameList;
	protected HashSet<INearbyElement> exitRangeThisFrameList;
	protected int nearbyCount;

	public void Init(INearbySearcher searcher)
	{
		thisSearcher = searcher;
		currentNearbyElements = new INearbyElement[0];
		tempList = new();
		nearbyCount = 0;
		enterRangeThisFrameList = new();
		exitRangeThisFrameList = new();
	}

	public void Deinit()
	{
		thisSearcher = null;
		currentNearbyElements = null;
		enterRangeThisFrameList = null;
		exitRangeThisFrameList = null;
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
	HashSet<INearbyElement> INearbySearcherAPI.EnterRageThisFrame()
	{
		if (thisSearcher.IsNullRef()) return null;
		return OnEnterRageThisFrame();
	}
	HashSet<INearbyElement> INearbySearcherAPI.ExitRageThisFrame()
	{
		if (thisSearcher.IsNullRef()) return null;
		return OnOutRageThisFrame();
	}
	int INearbySearcherAPI.NearbyCount()
	{
		return currentNearbyElements.Length;
	}

	bool INearbySearcherAPI.HasNearby(INearbyElement nearby)
	{
		if(nearby.IsNullRef()) return false;
		int length = currentNearbyElements.Length;
        for (int i = 0 ; i < length ; i++)
        {
			var item = currentNearbyElements[i];
			if (item == nearby) return true;
		}
		return false;
    }
	#endregion

	protected virtual INearbyElement OnGetNearbyItem(Func<INearbyElement, bool> func)
	{
		for (int i = 0 ; i < nearbyCount ; i++)
		{
			INearbyElement item = currentNearbyElements[i];
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
			return currentNearbyElements.Take(nearbyCount).Where(item => item.IsNotNullRef());
		else
			return currentNearbyElements.Take(nearbyCount).Where(item => item.IsNotNullRef() && (func.Invoke(item)));
	}

	protected virtual T OnGetNearbyItemType<T>(Func<T, bool> func) where T : class, IStrategyElement
	{
		for (int i = 0 ; i < nearbyCount ; i++)
		{
			INearbyElement item = currentNearbyElements[i];
			if (item.IsNullRef() || item is not T t) continue;

			if (func == null || func.Invoke(t))
			{
				return t;
			}
		}
		return null;
	}

	protected virtual IEnumerable<T> OnGetNearbyItemsType<T>(Func<T, bool> func) where T : class, IStrategyElement
	{
		return currentNearbyElements.Take(nearbyCount)
			.Where(item => item.IsNotNullRef() && item is T t && (func == null || func.Invoke(t)))
			.Select(item => (T)item);
	}

	private void OnUpdateNearby(IEnumerable<INearbyElement> searchingElementList)
	{
		enterRangeThisFrameList.Clear();
		exitRangeThisFrameList.Clear();
		for (int i = 0 ; i < nearbyCount ; i++)
		{
			// 이전에 근처에 있던 모든 요소를 '이번 프레임에 나갔을 수 있는 목록'에 추가합니다.
			exitRangeThisFrameList.Add(currentNearbyElements[i]);
		}

		nearbyCount = 0;

		if (searchingElementList == null || searchingElementList.Count() == 0) return;

		Vector3 center = SearchCenter;
		float searchRange = SearchRange;
		float searchMinRange = SearchMinRange;
		if (CutMinRange > searchMinRange)
			searchMinRange = 0f;

		tempList.Clear();
		foreach (var item in searchingElementList)
		{
			if (NearbyCheck(in item, in center, in searchRange, in searchMinRange, out float sqrDist))
			{ 
				tempList.Add((item, sqrDist));
			}
		}
		nearbyCount = tempList.Count;

		if (nearbyCount == 0)
		{
			if (currentNearbyElements.Length > 0)
			{
				Array.Resize(ref currentNearbyElements, 0);
			}
			return;
		}
		if (currentNearbyElements.Length < nearbyCount)
		{
			Array.Resize(ref currentNearbyElements, nearbyCount);
		}

		tempList.Sort((a, b) => a.sqrDist.CompareTo(b.sqrDist));

		for (int i = 0 ; i < nearbyCount ; i++)
		{
			var item= tempList[i].element;
			currentNearbyElements[i] = item;

			if (!exitRangeThisFrameList.Remove(item))
			{
				enterRangeThisFrameList.Add(item);
			}
		}
		tempList.Clear();
	}
	protected virtual bool NearbyCheck(in INearbyElement item, in Vector3 center, in float searchRange, in float searchMinRange, out float sqrDist)
	{
		sqrDist = 0;
		if (item.IsNullRef()) return false;

		if (FactionID == item.FactionID) return false;

		float targetRadius = item.Radius;

		float radius = searchRange + targetRadius;
		float minRadius = searchMinRange - targetRadius;

		Vector3 delta = center - item.Position;
		sqrDist = delta.sqrMagnitude;

		if (sqrDist > radius * radius) return false;
		if (minRadius > 0f && sqrDist < minRadius * minRadius) return false;

		return true;
	}

	protected virtual HashSet<INearbyElement> OnEnterRageThisFrame()
	{
		return enterRangeThisFrameList;
	}
	protected virtual HashSet<INearbyElement> OnOutRageThisFrame()
	{
		return exitRangeThisFrameList;
	}

#if UNITY_EDITOR
	protected virtual void OnDrawGizmos()
	{
		if (thisSearcher.IsNullRef()) return;
		float range = SearchRange;
		if (range <= 0) return;
		Vector3 center = SearchCenter;

		UnityEditor.Handles.color = Color.red;
		UnityEditor.Handles.DrawWireDisc(center, Vector3.up, range);
	}
#endif
}
