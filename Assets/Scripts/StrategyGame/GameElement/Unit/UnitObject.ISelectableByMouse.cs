using UnityEngine;

public partial class UnitObject : IMapSelectable
{
	public ISelectable ThisSelectable => this;
	partial void InitSelect()
	{
		if (TryGetComponent<SelectVisualizer>(out thisSelectVisualizer))
		{
			thisSelectVisualizer.OnInit(this);
		}
	}
	partial void DeinitDelect()
	{
		if(thisSelectVisualizer.IsNotNullRef())
		{
			thisSelectVisualizer.Deinit();
			thisSelectVisualizer = null;
		}
	}

	partial void DeselectSelf()
	{
		(this as ISelectable).SelfDeselect();
	}

	Vector3 IMapSelectable.SelectCenter => transform.position;
	void ISelectable.OnSelect()
	{
		ThisSelectVisualizer.OnSelect();
	}
	void ISelectable.OnDeselect()
	{
		ThisSelectVisualizer.OnDeselect();
	}
	void ISelectable.OnPointing()
	{
		ThisSelectVisualizer.OnPointing();
	}
	void IMapSelectable.OnPointEnter()
	{
		ThisSelectVisualizer.OnPointEnter();
	}
	void IMapSelectable.OnPointExit()
	{
		ThisSelectVisualizer.OnPointExit();
	}
}

public partial class UnitObject : ISelectVisualizer
{
	private SelectVisualizer thisSelectVisualizer;
	public ISelectVisualizer ThisSelectVisualizer => thisSelectVisualizer.IsNullRef() ? null : thisSelectVisualizer;
	void ISelectVisualizer.OnSelect() => ThisSelectVisualizer?.OnSelect();
	void ISelectVisualizer.OnDeselect() => ThisSelectVisualizer?.OnDeselect();
	void ISelectVisualizer.OnPointing() => ThisSelectVisualizer?.OnPointing();
	void ISelectVisualizer.OnPointEnter() => ThisSelectVisualizer?.OnPointEnter();
	void ISelectVisualizer.OnPointExit() => ThisSelectVisualizer?.OnPointExit();
}