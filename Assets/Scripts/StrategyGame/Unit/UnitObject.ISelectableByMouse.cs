using UnityEngine;

public partial class UnitObject : ISelectableByMouse
{
	partial void DeselectSelf()
	{
		(this as ISelectable).SelfDeselect();
	}
	bool ISelectableByMouse.IsSelectMouse { get; set; }
	bool ISelectableByMouse.IsPointEnter { get; set; }
	Vector3 ISelectableByMouse.ClickCenter => transform.position;
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
	}

	void ISelectable.OnSingleDeselect()
	{
	}

	void ISelectable.OnFirstSelect()
	{
	}

	void ISelectable.OnLastDeselect()
	{
	}
}
