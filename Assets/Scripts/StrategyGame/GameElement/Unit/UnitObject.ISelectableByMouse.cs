using UnityEngine;

public partial class UnitObject : IMouseSelectable
{
	partial void DeselectSelf()
	{
		(this as ISelectable).SelfDeselect();
	}

	Vector3 IMouseSelectable.SelectCenter => transform.position;
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
}
