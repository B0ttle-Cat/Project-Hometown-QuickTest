using UnityEngine;

public partial class UnitObject : ISelectableByMouse
{
	public Vector3 clickCenter => transform.position;
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
	}

	void ISelectableByMouse.OnSingleDeselect()
	{
	}

	void ISelectableByMouse.OnFirstSelect()
	{
	}

	void ISelectableByMouse.OnLastDeselect()
	{
	}
}
