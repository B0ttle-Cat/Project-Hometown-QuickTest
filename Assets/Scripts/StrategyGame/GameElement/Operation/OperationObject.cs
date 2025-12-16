using System.Collections.Generic;
using System.Linq;

using UnityEngine;
public partial class OperationObject : MonoBehaviour  // Main
{
	[SerializeField]
	private int operationID;
	[SerializeField]
	private string teamName;
	[SerializeField]
	private int factionID;
	private float operationRange = 5;
	public OperationObject This => this;
	public int OperationID => operationID;
	public string TeamName => teamName;
	public int FactionID => factionID;
	public float OperationRadius => operationRange;
	internal void Awake()
	{
		this.operationID = -1;
		this.factionID = -1;
		this.teamName = "";
		operationRange = 5;
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
		InitNearby(in baseRadius);
		InitFSM();
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
public partial class OperationObject // StatsData_old
{
	int computeFrame = -1;
	public void ComputeOperationValue()
	{
		int thisFrame = Time.frameCount;
		if (computeFrame == thisFrame) return;
		computeFrame = thisFrame;
		moveSpeed = ComputeMoveSpeed();
		searchViewRange = ComputeViewRange();
		searchCenterPosition = ComputeCenter();
	}

	private float ComputeMoveSpeed()
	{
		return GetAllUnitObj.Count == 0 ? 0 : GetAllUnitObj.Select(i => i.StatsData.MovementSpeed).Average();
	}
	private float ComputeViewRange()
	{
		return GetAllUnitObj.Count == 0 ? 0 : GetAllUnitObj.Select(i => i.StatsData.VisionRange).Max();
	}
	private Vector3 ComputeCenter()
	{
		int length = GetAllUnitObj.Count;
		if(length < 1) return transform.position;

		Bounds bounds = new Bounds(GetAllUnitObj[0].ThisMovement.CurrentPosition,Vector3.zero);
        for (int i = 1 ; i < length ; i++)
        {
			bounds.Encapsulate(GetAllUnitObj[i].ThisMovement.CurrentPosition);
		}

		return bounds.center;
	}
}
public partial class OperationObject : IStrategyElement, IStrategyElementDestroyer
{
	public IStrategyElement ThisElement => this;
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

	public IStrategyElementDestroyer ThisDestroyer => this;
	bool IStrategyElementDestroyer.IsDestroy { get; set; }

	public void InitLife()
	{
		ThisDestroyer.IsDestroy = false;
	}

	private void OnDestroy()
	{
		if (!ThisDestroyer.IsDestroy)
		{
			ThisDestroyer.OnDestroy();
		}
	}

	void IStrategyElementDestroyer.OnDestroy()
	{
		ThisDestroyer.IsDestroy = true;
		StrategyElementFactory.Destroy(this);
	}
	private void ControllerDestory()
	{
		ThisDestroyer.OnReservationDestroy();
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