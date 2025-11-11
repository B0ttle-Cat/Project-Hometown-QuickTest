using System;
using System.Collections.Generic;
using System.Linq;

using Sirenix.OdinInspector;

using UnityEngine;

using static StrategyGamePlayData;

public partial class OperationObject
{
	[SerializeField]
	private int operationID;
	[SerializeField]
	private string teamName;
	[SerializeField]
	private int factionID;
	[ShowInInspector]
	private Dictionary<UnitKey, UnitListInOperation> unitOrganization;
	private IEnumerable<int> GetAllUnitID
	{
		get
		{
			if (unitOrganization == null) yield break;
			foreach (var item in unitOrganization)
			{
				var list = item.Value;
				if (list == null || list.UnitIDList == null) continue;
				var idList = list.UnitIDList;
				foreach (int id in idList)
				{
					yield return id;
				}
			}
		}
	}
	private IEnumerable<UnitObject> GetAllUnitObject
	{
		get
		{
			if (unitOrganization == null) yield break;
			foreach (var item in GetAllUnitID)
			{
				if (StrategyManager.Collector.TryFindUnit(item, out var unitObject))
				{
					yield return unitObject;
				}

			}
		}
	}
	[Serializable]
	public class UnitListInOperation
	{
		private OperationObject operation;
		[SerializeField]
		private List<int> unitIDs;
		public List<int> UnitIDList => unitIDs;

		public UnitListInOperation(OperationObject operation)
		{
			this.operation = operation;
			unitIDs = new List<int>();
		}
		public bool Add(UnitObject unitObject)
		{
			if (unitObject == null) return false;

			int unitID = unitObject.ProfileData.unitID;
			if (unitID < 0) return false;

			if (unitIDs.Contains(unitID)) return false;

			unitIDs.Add(unitID);
			unitObject.SetOperationBelong(operation);
			return true;
		}
		public bool Remove(UnitObject unitObject)
		{
			if (unitObject == null) return false;

			int unitID = unitObject.ProfileData.unitID;
			if (unitID < 0) return false;

			if (unitIDs.Remove(unitID))
			{
				unitObject.RelaseOperationBelong();
				return true;
			}
			return false;
		}
	}

	public int OperationID { get => operationID; private set => operationID = value; }
	public string TeamName
	{
		get
		{
			if (string.IsNullOrWhiteSpace(teamName))
			{
				if (operationID < 0) return "임시 편성 부대";
				return $"제{operationID:oo}부대";
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
		unitOrganization = new Dictionary<UnitKey, UnitListInOperation>();

		int length = unitList.Count;
		for (int i = 0 ; i < length ; i++)
		{
			int unitID = unitList[i];
			if (!StrategyManager.Collector.TryFindUnit(unitID, out var unitObj)) continue;

			AddUnitObject(unitObj);
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

		if (unitOrganization.TryGetValue(unitKey, out var unitList))
		{
			unitList.Add(unitObject);
		}
		else
		{
			unitList = new UnitListInOperation(this);
			unitList.Add(unitObject);
			unitOrganization.Add(unitKey, unitList);
		}
	}
	public void RemoveUnitObject(UnitObject unitObject)
	{
		if (unitObject == null) return;

		UnitKey unitKey = unitObject.ProfileData.unitKey;

		if (unitOrganization.TryGetValue(unitKey, out var unitList))
		{
			unitList.Remove(unitObject);
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
		foreach (var item in GetAllUnitObject)
		{
			point += item.ThisMovement.CurrentPosition;
			++count;
		}
		if (count > 1) point /= count;
		return point;
	}
	private int GetMoveSpeed()
	{
		float average = (float)GetAllUnitObject.Select(i => i.GetStateValue(StatsType.유닛_이동속도)).Average();
		return Mathf.RoundToInt(average);
	}
}
public partial class OperationObject // VisibleCheck
{
	public bool IsVisibleAnybody => (visibleUnitList == null ? 0 : visibleUnitList.Count) > 0;
	private HashSet<UnitObject> visibleUnitList;
	public void ChangeVisibleUnit(UnitObject unitObject)
	{
		visibleUnitList ??= new HashSet<UnitObject>();
		if (visibleUnitList.Add(unitObject))
		{
			if (visibleUnitList.Count == 1)
			{
				OnVisibleAnybody?.Invoke(this);
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
				OnInvisibleEverybody?.Invoke(this);
			}
		}
	}

	public event Action<OperationObject> OnVisibleAnybody;
	public event Action<OperationObject> OnInvisibleEverybody;
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