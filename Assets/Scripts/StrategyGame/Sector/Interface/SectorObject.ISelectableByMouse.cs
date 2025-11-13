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
	void ISelectable.OnSelect()
	{

	}
	void ISelectable.OnDeselect()
	{
	}
	void ISelectable.OnSingleSelect()
	{
		Controller.OnShowUI_SelectUI();
	}

	void ISelectable.OnSingleDeselect()
	{
		Controller.OnHideUI_SelectUI();
	}

	void ISelectable.OnFirstSelect()
	{
	}

	void ISelectable.OnLastDeselect()
	{
	}
}
