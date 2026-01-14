using UnityEngine;

public partial class UnitObject : IMapSelectable
{
	partial void DeselectSelf()
	{
		(this as ISelectable).SelfDeselect();
	}

	Vector3 IMapSelectable.SelectCenter => transform.position;
	void IMapSelectable.OnPointEnter()
	{
	}
	void IMapSelectable.OnPointExit()
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
