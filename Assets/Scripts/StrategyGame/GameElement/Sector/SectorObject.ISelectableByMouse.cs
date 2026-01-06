using UnityEngine;

public partial class SectorObject : IMouseSelectable
{
	public Vector3 clickCenter => visibilityGroup == null ? transform.position : visibilityGroup.VisibleWorldBounds.center;
	Vector3 IMouseSelectable.SelectCenter => clickCenter;


    void IMouseSelectable.OnPointEnter()
	{
	}
	void IMouseSelectable.OnPointExit()
	{

	}
	void ISelectable.OnSelect()
	{

	}
	void ISelectable.OnDeselect()
	{
	}
	void ISelectable.OnPointing()
	{
	}
	//void ISelectable.OnSingleSelect()
	//{
	//	Controller.OnShowUI_SelectUI();
	//}

	//void ISelectable.OnSingleDeselect()
	//{
	//	Controller.OnHideUI_SelectUI();
	//}

	//void ISelectable.OnFirstSelect()
	//{
	//}

	//void ISelectable.OnLastDeselect()
	//{
	//}
}
