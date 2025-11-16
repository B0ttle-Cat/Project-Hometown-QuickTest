using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using static StrategyGamePlayData;

public partial class OperationObject : MonoBehaviour, IDisposable  // Main
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
	internal void Init(int factionID)
	{
		this.operationID = -1;
		this.teamName = "";
		this.factionID = factionID;
	}
	public void Init(in List<int> unitList)
	{
		InitOrganization(unitList);
		InitMovement();
	}
	public void DeInit()
	{
		DeInitOrganization();
	}
	public void Dispose()
	{
		operationID = -1;
		teamName = "";
		factionID = -1;
	}
	partial void InitOrganization(in List<int> unitList);
	partial void InitMovement();
	partial void DeInitOrganization();


}

public partial class OperationObject // Stats
{
	int computeFrame = -1;



	public void ComputeOperationValue()
	{
		int thisFrame = Time.frameCount;
		if (computeFrame == thisFrame) return;
		computeFrame = thisFrame;
		//moveSpeed = GetMoveSpeed();
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
	event Action<OperationObject> IVisibilityEvent<OperationObject>.OnChangeVisible
	{
		add => onChangeVisible += value;
		remove => onChangeVisible -= value;
	}
	event Action<OperationObject> IVisibilityEvent<OperationObject>.OnChangeInvisible
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
public partial class OperationObject : ISelectable
{
	void ISelectable.OnSelect()
	{
		if(StrategyManager.ViewAndControl.CurrentMode == ViewAndControlModeType.OperationsMode)
		{
			StrategyManager.GameUI.ControlPanelUI.OpenUI();
			var setTarget = StrategyManager.GameUI.ControlPanelUI.ShowOperationPlannerPanel();
			setTarget.AddTarget(this);
		}
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
	}
	void ISelectable.OnSingleDeselect()
	{
	}
}