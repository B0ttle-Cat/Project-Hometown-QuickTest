using UnityEngine;

public partial class SectorObject : ISelectMouse
{
	public Vector3 clickCenter => visibilityGroup == null ? transform.position : visibilityGroup.VisibleWorldBounds.center;
	bool ISelectMouse.IsSelectMouse { get; set; }
	bool ISelectMouse.IsPointEnter { get; set; }
	Vector3 ISelectMouse.ClickCenter => clickCenter;


	void ISelectMouse.OnPointEnter()
	{
	}
	void ISelectMouse.OnPointExit()
	{

	}
	bool ISelectMouse.OnSelect()
	{
		return true;
	}
	bool ISelectMouse.OnDeselect()
	{
		return true;
	}
	void ISelectMouse.OnSingleSelect()
	{
		var sectorTargeting = StrategyManager.GameUI.MapPanelUI.SectorSelectTargeting;
		if (sectorTargeting == null) return;
		sectorTargeting.AddTarget(this);
	}

	void ISelectMouse.OnSingleDeselect()
	{
		var sectorTargeting = StrategyManager.GameUI.MapPanelUI.SectorSelectTargeting;
		if (sectorTargeting == null) return;
		sectorTargeting.RemoveTarget(this);
	}

	void ISelectMouse.OnFirstSelect()
	{
	}

	void ISelectMouse.OnLastDeselect()
	{
	}
}
