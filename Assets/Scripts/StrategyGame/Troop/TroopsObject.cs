using System;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;

using static StrategyGamePlayData;

public partial class TroopsObject : MonoBehaviour
{
	[SerializeField]
	private int troopsID;
	[SerializeField]
	private int factionID;
	[ShowInInspector]
	private  Dictionary<UnitKey, OrganizationPlan> planList;

	private UnitKey influencerKey;
	public UnitObject InfluencerUnit {get
		{
			if(!planList.TryGetValue(influencerKey, out var plan))
			{
				return null;
			}
			return plan.UnitObject;
		}
	}

	[Serializable]
	public struct OrganizationPlan
	{
		private readonly UnitKey unitKey;   // 어떤 타입의 유닛이 편제 되었는지?
		private readonly int unitID;        // 편제된 유닛의 ID
		private int count;                  // 편제된 유닛의 현재 수
		private int limit;                  // 편제된 유닛 회복 제한
		public OrganizationPlan(UnitKey unitKey, int unitID, int count, int limit)
		{
			this.unitKey = unitKey;
			this.unitID = unitID;
			this.count = count;
			this.limit = limit;
		}
		public void SetCount(int count)
		{
			if (count < 0) count = 0;
			this.count = count;
		}
		public void SetLimit(int limit)
		{
			if (limit < 0) limit = 0;
			this.limit = limit;
		}
		public void SetCountAndLimit(int count, int limit)
		{
			if (count < 0) count = 0;
			if (limit < 0) limit = 0;
			this.count = count;
			this.limit = limit;
		}

		public readonly UnitKey UnitKey => unitKey;
		public readonly int UnitID => unitID;
		public readonly int Count => count;
		public readonly int Limit => limit;
		public readonly UnitObject UnitObject => StrategyManager.Collector.FindUnit(unitID);
	}

	public void Init(in ISectorController.SpawnTroopsInfo troopsInfo)
	{
		factionID = troopsInfo.factionID;
		planList = new Dictionary<UnitKey, OrganizationPlan>();
		var organizations  = troopsInfo.organizations;
		int length = organizations.Length;
		influencerKey = UnitKey.None;
		for (int i = 0 ; i < length ; i++)
		{
			(UnitKey key, int count, int limit) = organizations[i];
			if (key == UnitKey.None || count <= 0) continue;
			if (CreatePlanAndUnitObject(in factionID, in key, in count, in limit))
			{
				if (influencerKey == UnitKey.None) influencerKey = key;
			}
		}
	}
	private bool CreatePlanAndUnitObject(in int factionID, in UnitKey key, in int count = 0, in int limit = 0)
	{
		if (planList.TryGetValue(key, out var unit))
		{
			unit.SetCountAndLimit(unit.Count + count, unit.Limit + limit);
		}
		else
		{
			var unitObject = StrategyElementUtility.Instantiate(key, factionID, this.transform);
			if (unitObject == null) return false;
			unitObject.SetTroopBelong(this);
			unit = new OrganizationPlan(key, unitObject.UnitID, count, limit);
		}
		planList[key] = unit;
		return true;
	}
	public OrganizationPlan GetPlan(in UnitKey unitKey)
	{
		planList.TryGetValue(unitKey, out var plan);
		return planList[unitKey];
	}
	public bool HasUnitKey(in UnitKey unitKey)
	{
		return planList.ContainsKey(unitKey);
	}
	public void SetCountAndLimit(in UnitKey unitKey, in int count, in int limit)
	{
		CreatePlanAndUnitObject(in factionID, in unitKey);
		var _plan = GetPlan(unitKey);
		_plan.SetCountAndLimit(count, limit);
		planList[unitKey] = _plan;
	}
}

public partial class TroopsObject : IStrategyElement
{
	public IStrategyElement ThisElement => this;
	bool IStrategyElement.IsInCollector { get; set; }
	public int ID { get => troopsID; set => troopsID = value; }
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
public partial class TroopsObject : ISelectableByMouse
{
	Vector3 ISelectableByMouse.ClickCenter { get; }
	bool ISelectableByMouse.IsPointEnter { get; set; }
	bool ISelectableByMouse.IsSelectMouse { get; set; }

	bool ISelectableByMouse.OnDeselect()
	{
		return true;
	}
	void ISelectableByMouse.OnFirstSelect()
	{
	}
	void ISelectableByMouse.OnLastDeselect()
	{
	}
	bool ISelectableByMouse.OnSelect()
	{
		return true;
	}
	void ISelectableByMouse.OnSingleDeselect()
	{
	}
	void ISelectableByMouse.OnSingleSelect()
	{
	}
}