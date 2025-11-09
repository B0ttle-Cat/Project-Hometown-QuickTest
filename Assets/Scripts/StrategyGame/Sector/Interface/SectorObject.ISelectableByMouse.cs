using UnityEngine;

public partial class SectorObject : ISelectableByMouse
{
	public Vector3 clickCenter => visibilityGroup == null ? transform.position : visibilityGroup.VisibleWorldBounds.center;
	bool ISelectableByMouse.IsSelectMouse { get; set; }
	bool ISelectableByMouse.IsPointEnter { get; set; }
	Vector3 ISelectableByMouse.ClickCenter => clickCenter;


    void ISelectableByMouse.OnPointEnter()
	{
	}
	void ISelectableByMouse.OnPointExit()
	{

	}
	bool ISelectableByMouse.OnSelect()
	{
		return true;
	}
	bool ISelectableByMouse.OnDeselect()
	{
		return true;
	}
	void ISelectableByMouse.OnSingleSelect()
	{
		Controller.OnShowUI_SelectUI();
	}

	void ISelectableByMouse.OnSingleDeselect()
	{
		Controller.OnHideUI_SelectUI();
	}

	void ISelectableByMouse.OnFirstSelect()
	{
	}

	void ISelectableByMouse.OnLastDeselect()
	{
	}
}
