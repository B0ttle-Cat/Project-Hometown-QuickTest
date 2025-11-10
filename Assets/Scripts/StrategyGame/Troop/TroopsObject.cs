using System.Collections.Generic;
using System.Linq;

using Sirenix.OdinInspector;

using UnityEngine;

using static StrategyGamePlayData;

public partial class TroopsObject
{
	[SerializeField]
	private int troopsID;
	[SerializeField]
	private int factionID;
	[ShowInInspector]
	private Dictionary<UnitKey, UnitListInTroops> unitOrganization;
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
			foreach (var item in unitOrganization)
			{
				var list = item.Value;
				if (list == null || list.UnitIDList == null) continue;
				var idList = list.UnitIDList;
				foreach (int id in idList)
				{
					if(StrategyManager.Collector.TryFindUnit(id, out var unitObject))
					{
						yield return unitObject;
					}
				}
			}
		}
	}
	public class UnitListInTroops
	{
		private TroopsObject troops;
		private List<int> unitIDs;
		public List<int> UnitIDList => unitIDs;

		public UnitListInTroops(TroopsObject troops)
		{
			this.troops = troops;
			unitIDs = new List<int>();
		}
		public bool Add(UnitObject unitObject)
		{
			if (unitObject == null) return false;

			int unitID = unitObject.ProfileData.unitID;
			if (unitID < 0) return false;

			if (unitIDs.Contains(unitID)) return false;

			unitIDs.Add(unitID);
			unitObject.SetTroopBelong(troops);
			return true;
		}
		public bool Remove(UnitObject unitObject)
		{
			if (unitObject == null) return false;

			int unitID = unitObject.ProfileData.unitID;
			if (unitID < 0) return false;

			if (unitIDs.Remove(unitID))
			{
				unitObject.RelaseTroopBelong();
				return true;
			}
			return false;
		}
	}

	public int TroopsID { get => troopsID; private set => troopsID = value; }

	public TroopsObject(int factionID, List<int> unitList)
	{
		this.factionID = factionID;
		unitOrganization = new Dictionary<UnitKey, UnitListInTroops>();

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
			unitList = new UnitListInTroops(this);
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
public partial class TroopsObject // Stats
{
	public int GetMoveSpeed()
	{
		float average = (float)GetAllUnitObject.Select(i => i.GetStateValue(StatsType.유닛_이동속도)).Average();
		return Mathf.RoundToInt(average);
	}
}
public partial class TroopsObject : IStrategyElement
{
	public IStrategyElement ThisElement => this;
	bool IStrategyElement.IsInCollector { get; set; }
	int IStrategyElement.ID { get => TroopsID; set => TroopsID = value; }
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
public partial class TroopsObject : INodeMovement
{
	private Vector3 position;
	private Vector3 velocity;
	private float smoothTime;
	public INodeMovement ThisMovement => this;
	[FoldoutGroup("INodeMovement"), ShowInInspector, ReadOnly]
	Vector3 INodeMovement.CurrentPosition => position;
	[FoldoutGroup("INodeMovement"), ShowInInspector, ReadOnly]
	Vector3 INodeMovement.CurrentVelocity => velocity;
	[FoldoutGroup("INodeMovement"), ShowInInspector, ReadOnly]
	float INodeMovement.SmoothTime => smoothTime;
	[FoldoutGroup("INodeMovement"), ShowInInspector, ReadOnly]
	float INodeMovement.MaxSpeed => GetMoveSpeed();
	[FoldoutGroup("INodeMovement"), ShowInInspector, ReadOnly]
	int INodeMovement.RecentVisitedNode => 0;
	LinkedList<INodeMovement.MovementPlan> INodeMovement.MovementPlanList { get; set; }

	void INodeMovement.OnMoveStart()
	{
		velocity = Vector3.zero;
		smoothTime = 0.5f;
	}
	void INodeMovement.OnExitFirstNode()
	{
		smoothTime = 0f;
	}
	void INodeMovement.OnEnterLastNode()
	{
		smoothTime = 0.5f;
	}
	void INodeMovement.OnMoveEnded()
	{
		velocity = Vector3.zero;
		smoothTime = 0.5f;
	}
	void INodeMovement.OnSetPositionAndVelocity(in Vector3 position, in Vector3 velocity)
	{
		this.position = position;
		this.velocity = velocity;
	}
}