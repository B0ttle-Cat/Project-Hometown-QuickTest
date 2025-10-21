//using System.Collections.Generic;

//using UnityEngine;

//public class Update_ControlBaseCapture : StrategyUpdate.IUpdate
//{
//	private Dictionary<ControlBase, ControlBaseTrigger> trigger;
//	private Dictionary<ControlBase, bool> isKeep;
//	private ControlBase controlBase;
//	private ControlBaseTrigger controlBaseCollider;

//	private bool isKeepFaction;

//	private void Awake()
//	{
//	}
//	public void SetCapture(int factionID)
//	{
//		isKeepFaction = factionID >= 0;
//	}
//	internal void UpdateCapture(ControlBase _controlBase)
//	{
//		controlBase = _controlBase;

//		if (controlBaseCollider == null)
//			controlBaseCollider = _controlBase.GetComponentInChildren<ControlBaseTrigger>();
//		if (controlBaseCollider == null) return;

//		Dictionary<int,int> capturePoint = new Dictionary<int, int>();

//		capturePoint ??= new Dictionary<int, int>();
//		int totalPoint = 0;
//		ComputeFactionPointList();
//		void ComputeFactionPointList()
//		{
//			var colliders = controlBaseCollider.TriggingList;
//			if (colliders == null || colliders.Count == 0)
//			{
//				return;
//			}

//			HashSet<UnitObject> unitObject = new HashSet<UnitObject>();
//			foreach (Collider collider in colliders)
//			{
//				UnitObject unit = collider.GetComponentInParent<UnitObject>();
//				if (unit != null)
//				{
//					unitObject.Add(unit);
//				}
//			}
//			int unitCount = unitObject.Count;
//			if (unitCount == 0)
//			{
//				return;
//			}

//			foreach (UnitObject unit in unitObject)
//			{
//				CaptureTag[] pointElements = unit.GetComponentsInChildren<CaptureTag>();

//				foreach (var pointElement in pointElements)
//				{
//					if (pointElement == null) continue;

//					int factionID = pointElement.factionID;
//					int pointValue = pointElement.pointValue;
//					if (pointValue <= 0) continue;
//					if (!capturePoint.ContainsKey(factionID))
//						capturePoint.Add(factionID, 0);
//					totalPoint += pointValue;
//					capturePoint[factionID] += pointValue;
//				}
//			}
//		}

//		Faction currentFaction = controlBase.CaptureFaction;
//		int currentFactionID = currentFaction == null ? -1 : currentFaction.FactionID;

//		if (totalPoint == 0)
//		{
//			if (isKeepFaction)
//			{
//				KeepCapture(currentFactionID);
//			}
//			else
//			{
//				Neutralization();
//			}
//			return;
//		}

//		int bigPointFactionID = -1;
//		foreach (var item in capturePoint)
//		{
//			int faction = item.Key;
//			int point = item.Value;

//			if (point > totalPoint - point)
//			{
//				bigPointFactionID = faction;
//			}
//		}

//		if(isKeepFaction)
//		{
//			if (bigPointFactionID < 0)
//			{
//				Neutralization();
//				return;
//			}
//			if (bigPointFactionID == currentFactionID)
//			{
//				KeepCapture(currentFactionID);
//				return;
//			}
//			Neutralization();
//			return;
//		}
//		else
//		{
//			if(bigPointFactionID < 0)
//			{
//				Neutralization();
//				return;
//			}
//			if (bigPointFactionID == currentFactionID)
//			{
//				KeepCapture(currentFactionID);
//				return;
//			}
//			Neutralization(bigPointFactionID);
//			return;
//		}
//	}
//	public void KeepCapture(int factionID)
//	{
//		if (controlBase.CaptureProgress >= 1f)
//		{
//			return;
//		}
//		var faction = StrategyManager.Collector.FindFaction(factionID);

//		float captureTime = controlBase.CaptureTime;
//		float captureSpeed =  faction == null ? 1 : faction.CaptureSpeed;

//		captureTime = Mathf.Max(captureTime, 1f);
//		captureSpeed = Mathf.Max(captureSpeed, 0.1f);

//		float totalSpeed = captureSpeed / captureTime;
//		float delta = totalSpeed * Time.deltaTime;

//		float captureProgress = controlBase.CaptureProgress + delta;
//		controlBase.SetCaptureData(faction.FactionID, captureProgress);

//		if (controlBase.CaptureFaction != null && controlBase.CaptureProgress >= 1f)
//		{
//			isKeepFaction = true;
//		}
//	}
//	public void Neutralization(int attackFactionID = -1)
//	{
//		var faction = StrategyManager.Collector.FindFaction(attackFactionID);

//		float captureTime = controlBase.CaptureTime;
//		float captureSpeed =  faction == null ? 1 : faction.CaptureSpeed;

//		captureTime = Mathf.Max(captureTime, 1f);
//		captureSpeed = Mathf.Max(captureSpeed, 0.1f);

//		float totalSpeed = captureSpeed / captureTime;
//		float delta = totalSpeed * Time.deltaTime;

//		float captureProgress = controlBase.CaptureProgress;
//		captureProgress -= delta;
//		if (captureProgress <= 0f)
//		{

//			controlBase.SetCaptureData(faction.FactionID, 0f);
//			isKeepFaction = false;
//		}
//	}

//    void StrategyUpdate.IUpdate.Update(in float deltaTime)
//    {
//        throw new System.NotImplementedException();
//    }
//}
