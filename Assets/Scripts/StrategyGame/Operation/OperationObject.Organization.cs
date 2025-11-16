using System;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;

using static StrategyGamePlayData;

public partial class OperationObject // Organization
{
	[ShowInInspector]
	private Dictionary<UnitKey, OrganizationInfo> unitOrganization;
	public event Action<OperationObject> OnChangeUnitList;
	private List<int> allUnitID;
	private List<UnitObject> allUnitObj;
	private List<Transform> allUnitTr;
	public List<int> GetAllUnitID => allUnitID ??= new List<int>();
	public List<UnitObject> GetAllUnitObj => allUnitObj ??= new List<UnitObject>();
	public List<Transform> GetAllUnitTr => allUnitTr ??= new List<Transform>();
	[Serializable]
	public class OrganizationInfo
	{
		[ShowInInspector]
		private HashSet<int> unitIDs;
		public HashSet<int> UnitIDList => unitIDs;

		public OrganizationInfo()
		{
			unitIDs = new HashSet<int>();
		}
		public bool Add(UnitObject unitObject)
		{
			if (unitObject == null) return false;

			int unitID = unitObject.UnitID;
			if (unitID < 0) return false;

			if (unitIDs.Contains(unitID)) return false;

			return unitIDs.Add(unitID);
		}
		public bool Remove(UnitObject unitObject)
		{
			if (unitObject == null) return false;

			int unitID = unitObject.UnitID;
			if (unitID < 0) return false;

			return unitIDs.Remove(unitID);
		}
	}

	partial void InitOrganization(in List<int> unitList)
	{
		unitOrganization = new Dictionary<UnitKey, OrganizationInfo>();
		allUnitID = new List<int>();
		allUnitObj = new List<UnitObject>();
		allUnitTr = new List<Transform>();

		int length = unitList.Count;
		bool isChange = false;
		for (int i = 0 ; i < length ; i++)
		{
			int unitID = unitList[i];
			if (!StrategyManager.Collector.TryFindUnit(unitID, out var unitObj)) continue;

			if (AddUnitObject(unitObj, false))
			{
				isChange = true;
			}
		}
		if (isChange)
		{
			ChangeUnitListUpdate();
		}
	}
	partial void DeInitOrganization()
	{
		RelaseAndDestroyAllUnit();
		unitOrganization = null;
		allUnitID = null;
		allUnitObj = null;
		allUnitTr = null;
	}

	public bool HasUnitType(in UnitKey unitKey)
	{
		return unitOrganization.ContainsKey(unitKey);
	}
	public bool AddUnitObject(UnitObject unitObject, bool callback = true)
	{
		if (unitObject == null) return false;
		if (factionID != unitObject.ProfileData.factionID) return false;

		UnitKey unitKey = unitObject.ProfileData.unitKey;

		bool onChange = false;
		if (!unitOrganization.TryGetValue(unitKey, out var unitList))
		{
			unitList = new OrganizationInfo();
			unitOrganization.Add(unitKey, unitList);
		}

		if (unitList.Add(unitObject))
		{
			allUnitID.Add(unitObject.UnitID);
			allUnitObj.Add(unitObject);
			allUnitTr.Add(unitObject.transform);
			if(unitObject is IOperationBelonger belonger){
				belonger.SetOperationBelong(this);
			}
			onChange = true;
		}

		if (onChange)
		{
			if(callback) ChangeUnitListUpdate();
			return true;
		}
		return false;
	}
	public bool RemoveUnitObject(UnitObject unitObject, bool callback = true)
	{
		if (unitObject == null) return false;

		UnitKey unitKey = unitObject.ProfileData.unitKey;

		bool onChange = false;
		if (unitOrganization.TryGetValue(unitKey, out var unitList))
		{
			if (unitList.Remove(unitObject))
			{
				allUnitID.Remove(unitObject.UnitID);
				allUnitObj.Remove(unitObject);
				allUnitTr.Remove(unitObject.transform);
				if (unitObject is IOperationBelonger belonger)
				{
					belonger.RelaseOperationBelong();
				}
				onChange = true;
			}
		}

		if (onChange)
		{
			if (callback) ChangeUnitListUpdate();
			return true;
		}
		return false;
	}
	public void RelaseAllUnit(bool withDestroy = false)
	{
		if (allUnitObj != null)
		{
			var tempList = allUnitObj.ToArray();
			int length = tempList.Length;
			for (int i = 0 ; i < length ; i++)
			{
				var unit = tempList[i];
				if (unit is IOperationBelonger belonger)
				{
					belonger.RelaseOperationBelong();
				}
				if (withDestroy)
				{
					StrategyElementUtility.Destroy(unit);
				}
				tempList[i] = null;
			}
			allUnitObj.Clear();
		}
		unitOrganization?.Clear();
		allUnitTr?.Clear();
		allUnitID?.Clear();
	}
	public void RelaseAndDestroyAllUnit()
	{
		RelaseAllUnit(true);
	}
	private void ChangeUnitListUpdate()
	{
		if (OnChangeUnitList != null)
		{
			OnChangeUnitList.Invoke(this);
		}
	}
}