using System.Collections.Generic;

using UnityEngine;

public class ControlBaseOccupation : MonoBehaviour
{
	private ControlBase controlBase;
	private ControlBaseTrigger controlBaseCollider;

	private bool isKeepFaction;

	private void Awake()
	{
	}
	public void SetOccupation(int factionID)
	{
		isKeepFaction = factionID >= 0;
	}
	internal void UpdateOccupation(ControlBase _controlBase)
	{
		controlBase = _controlBase;

		if (controlBaseCollider == null)
			controlBaseCollider = GetComponentInChildren<ControlBaseTrigger>();
		if (controlBaseCollider == null) return;

		Dictionary<int,int> occupationPoint = new Dictionary<int, int>();

		occupationPoint ??= new Dictionary<int, int>();
		int totalPoint = 0;
		ComputeFactionPointList();
		void ComputeFactionPointList()
		{
			var colliders = controlBaseCollider.TriggingList;
			if (colliders == null || colliders.Count == 0)
			{
				return;
			}

			HashSet<UnitObject> unitObject = new HashSet<UnitObject>();
			foreach (Collider collider in colliders)
			{
				UnitObject unit = collider.GetComponentInParent<UnitObject>();
				if (unit != null)
				{
					unitObject.Add(unit);
				}
			}
			int unitCount = unitObject.Count;
			if (unitCount == 0)
			{
				return;
			}

			foreach (UnitObject unit in unitObject)
			{
				OccupationTag[] pointElements = unit.GetComponentsInChildren<OccupationTag>();

				foreach (var pointElement in pointElements)
				{
					if (pointElement == null) continue;

					int factionID = pointElement.factionID;
					int pointValue = pointElement.pointValue;
					if (pointValue <= 0) continue;
					if (!occupationPoint.ContainsKey(factionID))
						occupationPoint.Add(factionID, 0);
					totalPoint += pointValue;
					occupationPoint[factionID] += pointValue;
				}
			}
		}

		Faction currentFaction = controlBase.OccupyingFaction;
		int currentFactionID = currentFaction == null ? -1 : currentFaction.FactionID;

		if (totalPoint == 0)
		{
			if (isKeepFaction)
			{
				KeepOccupation(currentFactionID);
			}
			else
			{
				Neutralization();
			}
			return;
		}

		int bigPointFactionID = -1;
		foreach (var item in occupationPoint)
		{
			int faction = item.Key;
			int point = item.Value;

			if (point > totalPoint - point)
			{
				bigPointFactionID = faction;
			}
		}

		if(isKeepFaction)
		{
			if (bigPointFactionID < 0)
			{
				Neutralization();
				return;
			}
			if (bigPointFactionID == currentFactionID)
			{
				KeepOccupation(currentFactionID);
				return;
			}
			Neutralization();
			return;
		}
		else
		{
			if(bigPointFactionID < 0)
			{
				Neutralization();
				return;
			}
			if (bigPointFactionID == currentFactionID)
			{
				KeepOccupation(currentFactionID);
				return;
			}
			Neutralization(bigPointFactionID);
			return;
		}
	}


	public void KeepOccupation(int factionID)
	{
		if (controlBase.OccupationProgress >= 1f)
		{
			return;
		}
		var faction = StrategyGameManager.Collector.FindFaction(factionID);

		float occupationTime = controlBase.OccupationTime;
		float occupationSpeed =  faction == null ? 1 : faction.OccupationSpeed;

		occupationTime = Mathf.Max(occupationTime, 1f);
		occupationSpeed = Mathf.Max(occupationSpeed, 0.1f);

		float totalSpeed = occupationSpeed / occupationTime;
		float delta = totalSpeed * Time.deltaTime;

		controlBase.OccupationProgress += delta;
		if (controlBase.OccupyingFaction != null && controlBase.OccupationProgress >= 1f)
		{
			isKeepFaction = true;
		}
	}

	public void Neutralization(int attackFactionID = -1)
	{
		var faction = StrategyGameManager.Collector.FindFaction(attackFactionID);

		float occupationTime = controlBase.OccupationTime;
		float occupationSpeed =  faction == null ? 1 : faction.OccupationSpeed;

		occupationTime = Mathf.Max(occupationTime, 1f);
		occupationSpeed = Mathf.Max(occupationSpeed, 0.1f);

		float totalSpeed = occupationSpeed / occupationTime;
		float delta = totalSpeed * Time.deltaTime;

		controlBase.OccupationProgress -= delta;
		if (controlBase.OccupationProgress <= 0f)
		{
			controlBase.OccupyingFaction = faction;
			controlBase.OccupationProgress = 0f;
			isKeepFaction = false;
		}
	}

}
