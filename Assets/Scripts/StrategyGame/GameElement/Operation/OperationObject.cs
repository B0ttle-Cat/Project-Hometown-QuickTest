using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using static StrategyGamePlayData;
public partial class OperationObject : MonoBehaviour  // Main
{
	[SerializeField]
	private int operationID;
	[SerializeField]
	private string teamName;
	[SerializeField]
	private int factionID;
	public OperationObject This => this;
	public int OperationID => operationID;
	public string TeamName => teamName;
	public int FactionID => factionID;
	internal void Awake()
	{
		this.operationID = -1;
		this.factionID = -1;
		this.teamName = "";
	}
	private void OnDestroy()
	{
		operationID = -1;
		teamName = "";
		factionID = -1;
	}
	internal void Init(int factionID, string teamName)
	{
		this.factionID = factionID;
		this.teamName = teamName;
	}

	public void Init(in List<int> unitList, in float baseRadius)
	{
		InitOrganization(in unitList);
		InitMovement();
		InitFSM();
		InitNearby(in baseRadius);
	}
	partial void InitOrganization(in List<int> unitList);
	partial void InitMovement();
	partial void InitFSM();
	partial void InitNearby(in float baseRadius);
	
	public void DeInit()
	{
		DeInitOrganization();
		DeselectSelf();
		DeinitFSM();
		DeInitNearby();
	}
	partial void DeInitOrganization();
	partial void DeselectSelf();
	partial void DeinitFSM();
	partial void DeInitNearby();
}
public partial class OperationObject // Stats
{
	int computeFrame = -1;
	public void ComputeOperationValue()
	{
		int thisFrame = Time.frameCount;
		if (computeFrame == thisFrame) return;
		computeFrame = thisFrame;
		moveSpeed = ComputeMoveSpeed();

		SearcherRange = ComputeViewRange();
	}

	private int ComputeMoveSpeed()
	{
		double average = GetAllUnitObj.Count == 0 ? 0 : GetAllUnitObj.Select(i => i.GetStateValue(StatsType.유닛_이동속도)).Average();
		return Mathf.RoundToInt((float)average);
	}
	private float ComputeViewRange()
	{
		int max = GetAllUnitObj.Count == 0 ? 0 : GetAllUnitObj.Select(i => i.GetStateValue(StatsType.유닛_시야범위)).Max();
		return (float)max * 0.01f;
	}
}
public partial class OperationObject : IStrategyElement
{
	public IStrategyElement ThisElement => this;
	bool IStrategyElement.IsInCollector { get; set; }
	int IStrategyElement.ID { get => operationID; set => operationID = value; }
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
public partial class OperationObject : ISelectable
{
	partial void DeselectSelf()
	{
		(this as ISelectable).SelfDeselect();
	}

	void ISelectable.OnSelect()
	{

	}
	void ISelectable.OnDeselect()
	{

	}
	void ISelectable.OnFirstSelect()
	{
	}
	void ISelectable.OnLastDeselect()
	{
	}
	void ISelectable.OnSingleSelect()
	{
		if (StrategyManager.ViewAndControl.CurrentMode == ViewAndControlModeType.OperationsMode)
		{
			StrategyManager.GameUI.ControlPanelUI.OpenUI();
			var setTarget = StrategyManager.GameUI.ControlPanelUI.ShowOperationPlannerPanel();
			if (setTarget == null) return;
			setTarget.AddTarget(this);
		}
	}
	void ISelectable.OnSingleDeselect()
	{
		if (StrategyManager.ViewAndControl.CurrentMode == ViewAndControlModeType.OperationsMode)
		{
			StrategyManager.GameUI.ControlPanelUI.HideOperationPlannerPanel();
		}
	}
}