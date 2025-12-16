using System;
using System.Collections;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;

using static StrategyGamePlayData;

public partial class OperationObject // Organization
{
	[ShowInInspector]
	private OperationUnitList unitOrganizationList;
	public OperationUnitList UnitOrganizationList => unitOrganizationList;
	//public OperationUnitList UnitOrganizationList => UnitOrganizationList;
	private bool enableChangeCallback;
    public event Action<OperationObject> OnChangeUnitList;

	partial void InitOrganization(in List<int> unitList)
	{
		unitOrganizationList = new OperationUnitList(this,OnChangeOrganizationList);
		enableChangeCallback = true;

		int length = unitList.Count;
		bool isChange = false;
		for (int i = 0 ; i < length ; i++)
		{
			int unitID = unitList[i];
			if (!StrategyManager.Collector.TryFind<UnitObject>(unitID, out var unitObj)) continue;

			if (AddUnitObject(unitObj, false))
			{
				isChange = true;
			}
		}
		if (isChange)
		{
			OnChangeOrganizationList();
		}
	}

	private void OnChangeOrganizationList()
	{
		if (!enableChangeCallback) return;

		OnChangeUnitList?.Invoke(this);
	}

	partial void DeInitOrganization()
	{
		RelaseAndDestroyAllUnit();
		UnitOrganizationList.Dispose();
		unitOrganizationList = null;
	}

	public bool HasUnitType(in UnitKey unitKey)
	{
		return UnitOrganizationList.HasUnitType(unitKey);
	}
	public bool AddUnitObject(UnitObject unitObject, bool callback = true)
	{
		if (unitObject == null) return false;
		if (factionID != unitObject.InstanceData.factionID) return false;

		enableChangeCallback = callback;
		bool onChange = UnitOrganizationList.Add(unitObject);
		enableChangeCallback = true;
		return onChange;
	}
	public bool RemoveUnitObject(UnitObject unitObject, bool callback = true)
	{
		if (unitObject == null) return false;

		enableChangeCallback = callback;
		bool onChange = UnitOrganizationList.Remove(unitObject);
		enableChangeCallback = true;
		return onChange;
	}
	/// <summary>
	/// </summary>
	/// <param name="withDestroy">true 인 경우 == OperationObject와 함꼐 소속된 Unit 도 같이 삭제되는 경우</param>
	public void RelaseAllUnit(bool withDestroy = false)
	{
		if(UnitOrganizationList == null) return;
		UnitOrganizationList.Clear(withDestroy);
	}
	public void RelaseAndDestroyAllUnit()
	{
		RelaseAllUnit(true);
	}
}

public partial class OperationObject // OperationUnitList
{
	public class OperationUnitList : IDisposable, ISet<UnitObject>
	{
		private readonly HashSet<UnitObject> unitList = new ();
		private readonly HashSet<int> idList = new HashSet<int>();
		private readonly HashSet<Transform> transforms = new HashSet<Transform>();
		private readonly Dictionary<UnitKey, int> organization = new Dictionary<UnitKey, int>();

		private readonly OperationObject thisOperation;

		private readonly Action changeCallback;

		public IEnumerable<UnitObject> UnitList => unitList;
		public IEnumerable<int> GetIDList => idList;
		public IEnumerable<Transform> GetTransforms => transforms;
		public IEnumerable<KeyValuePair<UnitKey, int>> Organization => Organization;

		public int Count => unitList.Count;
		public OperationUnitList(OperationObject operationObject, Action changeCallback) {
			thisOperation = operationObject;
			this.changeCallback = changeCallback;
		}
		public OperationUnitList(OperationObject operationObject, Action changeCallback, IEnumerable<UnitObject> collection)
		{
			thisOperation = operationObject;
			foreach (var item in collection)
			{
				Add(item);
			}
			this.changeCallback = changeCallback;
		}
		public void Dispose()
		{
			Clear();
		}
		public bool Add(UnitObject item)
		{
			if (unitList.Add(item))
			{
				var unitKey = item.InstanceData.unitKey;
				idList.Add(item.UnitID);
				transforms.Add(item.gameObject.transform);
				if (organization.ContainsKey(unitKey))
				{
					organization[unitKey]++;
				}
				else
				{
					organization.Add(unitKey, 1);
				}

				if (item is IOperationBelonger belonger)
				{
					belonger.SetOperationBelong(thisOperation);
				}

				changeCallback?.Invoke();
				return true;
			}
			return false;
		}
		public void Clear()
		{
			Clear(false);
		}
		public void Clear(bool destroyUnitObject)
		{
			if(destroyUnitObject)
			{
                foreach (var unit in unitList)
                {
					if (unit is IOperationBelonger belonger)
					{
						belonger.RelaseOperationBelong();
					}
					unit.DestroyWithOperation();
				}
            }
			else
			{
				foreach (var unit in unitList)
				{
					if (unit is IOperationBelonger belonger)
					{
						belonger.RelaseOperationBelong();
					}
				}
			}
			idList.Clear();
			transforms.Clear();

			changeCallback?.Invoke();
		}
	
		public bool Remove(UnitObject item)
		{
			if (unitList.Remove(item))
			{
				var unitKey = item.InstanceData.unitKey;
				idList.Remove(item.UnitID);
				transforms.Remove(item.gameObject.transform);
				if (organization.TryGetValue(unitKey, out int count) && count > 0)
				{
					organization[unitKey]--;
				}

				if (item is IOperationBelonger belonger)
				{
					belonger.RelaseOperationBelong();
				}

				changeCallback?.Invoke();
				return true;
			}
			return false;
		}
		public bool Contains(UnitObject item) => unitList.Contains(item);

		public bool HasUnitType(UnitKey unitKey)=> organization.ContainsKey(unitKey);

		#region ISet
		public bool IsReadOnly => false;
		public void ExceptWith(IEnumerable<UnitObject> other) { unitList.ExceptWith(other); }
		public void IntersectWith(IEnumerable<UnitObject> other) { unitList.IntersectWith(other); }
		public bool IsProperSubsetOf(IEnumerable<UnitObject> other) { return unitList.IsProperSubsetOf(other); }
		public bool IsProperSupersetOf(IEnumerable<UnitObject> other) { return unitList.IsProperSupersetOf(other); }
		public bool IsSubsetOf(IEnumerable<UnitObject> other) { return unitList.IsSubsetOf(other); }
		public bool IsSupersetOf(IEnumerable<UnitObject> other) { return unitList.IsSupersetOf(other); }
		public bool Overlaps(IEnumerable<UnitObject> other) { return unitList.Overlaps(other); }
		public bool SetEquals(IEnumerable<UnitObject> other) { return unitList.SetEquals(other); }
		public void SymmetricExceptWith(IEnumerable<UnitObject> other) { unitList.SymmetricExceptWith(other); }
		public void UnionWith(IEnumerable<UnitObject> other) { unitList.UnionWith(other); }
		void ICollection<UnitObject>.Add(UnitObject item) { unitList.Add(item); }
		void ICollection<UnitObject>.CopyTo(UnitObject[] array, int arrayIndex) { unitList.CopyTo(array, arrayIndex); }
		public IEnumerator<UnitObject> GetEnumerator() { return unitList.GetEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return ((IEnumerable)unitList).GetEnumerator(); }
		#endregion
	}
}
