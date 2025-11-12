using System;
using System.Collections.Generic;
using System.Linq;

using Sirenix.OdinInspector;

using UnityEngine;

using static StrategyGamePlayData;

public partial class OperationObject // Main
{
	[SerializeField]
	private int operationID;
	[SerializeField]
	private string teamName;
	[SerializeField]
	private int factionID;

	public OperationObject This => this;
	public int OperationID { get => operationID; private set => operationID = value; }
	public string TeamName
	{
		get
		{
			if (string.IsNullOrWhiteSpace(teamName))
			{
				if (operationID < 0) return "임시 편성 부대";
				return $"제{operationID:00}부대";
			}
			return teamName;
		}
		set
		{
			teamName = value;
		}
	}
	public OperationObject(int factionID, List<int> unitList)
	{
		operationID = -1;
		teamName = "";
		this.factionID = factionID;
		unitOrganization = new Dictionary<UnitKey, OrganizationInfo>();

		int length = unitList.Count;
		for (int i = 0 ; i < length ; i++)
		{
			int unitID = unitList[i];
			if (!StrategyManager.Collector.TryFindUnit(unitID, out var unitObj)) continue;

			AddUnitObject(unitObj);
		}
	}

}

public partial class OperationObject // Organization
{
	[ShowInInspector]
	private Dictionary<UnitKey, OrganizationInfo> unitOrganization;
	public event Action<OperationObject> OnChangeUnitList;
	private IEnumerable<UnitObject> allUnit;
	private int[] allUnitID;
	private UnitObject[] allUnitObj;

	public IEnumerable<UnitObject> GetAllUnit => allUnit;
	public int[] GetAllUnitID => allUnitID;
	public UnitObject[] GetAllUnitObj => allUnitObj;
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

			int unitID = unitObject.ProfileData.unitID;
			if (unitID < 0) return false;

			if (unitIDs.Contains(unitID)) return false;

			return unitIDs.Add(unitID);
		}
		public bool Remove(UnitObject unitObject)
		{
			if (unitObject == null) return false;

			int unitID = unitObject.ProfileData.unitID;
			if (unitID < 0) return false;

			return unitIDs.Remove(unitID);
		}
	}

	public bool HasUnitType(in UnitKey unitKey)
	{
		return unitOrganization.ContainsKey(unitKey);
	}
	public void AddUnitObject(UnitObject unitObject)
	{
		if (unitObject == null) return;
		if (factionID != unitObject.ProfileData.factionID) return;

		UnitKey unitKey = unitObject.ProfileData.unitKey;

		bool onChange = false;
		if (unitOrganization.TryGetValue(unitKey, out var unitList))
		{
			if (unitList.Add(unitObject))
			{
				unitObject.SetOperationBelong(this);
				onChange = true;
			}
		}
		else
		{
			unitList = new OrganizationInfo();
			if (unitList.Add(unitObject))
			{
				unitOrganization.Add(unitKey, unitList);
				unitObject.SetOperationBelong(this);
				onChange = true;
			}
		}

		if (onChange)
		{
			ChangeUnitListUpdate();

		}
	}
	public void RemoveUnitObject(UnitObject unitObject)
	{
		if (unitObject == null) return;

		UnitKey unitKey = unitObject.ProfileData.unitKey;

		bool onChange = false;
		if (unitOrganization.TryGetValue(unitKey, out var unitList))
		{
			if (unitList.Remove(unitObject))
			{
				onChange = true;
				unitObject.RelaseOperationBelong();
			}
		}

		if (onChange)
		{
			ChangeUnitListUpdate();
		}
	}
	private void ChangeUnitListUpdate()
	{
		if (unitOrganization == null) return;
		HashSet<UnitObject> unitList = new HashSet<UnitObject>();

		foreach (var item in unitOrganization)
		{
			var value = item.Value;
			if (value == null || value.UnitIDList == null) continue;
			var list = value.UnitIDList;
			foreach (int id in list)
			{
				if (StrategyManager.Collector.TryFindUnit(id, out var unitObject))
				{
					unitList.Add(unitObject);
				}
			}
		}

		allUnit = unitList;
		allUnitObj = allUnit.ToArray();
		allUnitID = allUnit.Select(i => i.UnitID).ToArray();


		if (OnChangeUnitList != null)
		{
			OnChangeUnitList.Invoke(this);
		}
	}
}

public partial class OperationObject // Stats
{
	int computeFrame = -1;

	private Vector3 position;
	private int moveSpeed;
	public void ComputeOperationValue()
	{
		int thisFrame = Time.frameCount;
		if (computeFrame == thisFrame) return;
		computeFrame = thisFrame;
		position = GetPosition();
		moveSpeed = GetMoveSpeed();
	}
	private Vector3 GetPosition()
	{
		int count = 0;
		Vector3 point = Vector3.zero;
		foreach (var item in GetAllUnitObj)
		{
			point += item.ThisMovement.CurrentPosition;
			++count;
		}
		if (count > 1) point /= count;
		return point;
	}
	private int GetMoveSpeed()
	{
		float average = (float)GetAllUnitObj.Select(i => i.GetStateValue(StatsType.유닛_이동속도)).Average();
		return Mathf.RoundToInt(average);
	}
}
public partial class OperationObject : IVisibilityEvent<OperationObject>
{
	public IVisibilityEvent<OperationObject> ThisVisibility => this;
	bool IVisibilityEvent<OperationObject>.IsVisible => (visibleUnitList == null ? 0 : visibleUnitList.Count) > 0;
	private Action<OperationObject> onChangeVisible;
	private Action<OperationObject> onChangeInvisible;
	event Action<OperationObject> IVisibilityEvent<OperationObject>.OnChangeInvisible
	{
		add => onChangeVisible += value;
		remove => onChangeVisible -= value;
	}

	event Action<OperationObject> IVisibilityEvent<OperationObject>.OnChangeVisible
	{
		add => onChangeInvisible += value;
		remove => onChangeInvisible -= value;
	}
	private HashSet<UnitObject> visibleUnitList;
	public void ChangeVisibleUnit(UnitObject unitObject)
	{
		visibleUnitList ??= new HashSet<UnitObject>();
		if (visibleUnitList.Add(unitObject))
		{
			if (visibleUnitList.Count == 1)
			{
				onChangeVisible?.Invoke(this);
			}
		}
	}
	public void ChangeInvisibleUnit(UnitObject unitObject)
	{
		if (visibleUnitList == null) return;

		if (visibleUnitList.Remove(unitObject))
		{
			if (visibleUnitList.Count == 0)
			{
				onChangeInvisible?.Invoke(this);
			}
		}
	}

}
public partial class OperationObject : IStrategyElement
{
	public IStrategyElement ThisElement => this;
	bool IStrategyElement.IsInCollector { get; set; }
	int IStrategyElement.ID { get => OperationID; set => OperationID = value; }
	void IStrategyElement.InStrategyCollector()
	{
	}
	void IStrategyElement.OutStrategyCollector()
	{
	}
	void IStrategyStartGame.OnStartGame()
	{
	}
	void IStrategyStartGame.OnStopGame()
	{
	}
}