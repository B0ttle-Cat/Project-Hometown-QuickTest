using System;

using UnityEngine;

public partial class UnitObject : IVisibilityEvent<UnitObject>
{
	private IVisibilityEvent<Component> ChildVisibility => childVisibility;
	private CameraVisibilityGroup childVisibility;
	public IVisibilityEvent<UnitObject> ThisVisibility => this;
	bool IVisibilityEvent<UnitObject>.IsVisible => ChildVisibility == null ? false : ChildVisibility.IsVisible;
	private Action<UnitObject> onChangeVisible;
	private Action<UnitObject> onChangeInvisible;
	event Action<UnitObject> IVisibilityEvent<UnitObject>.OnChangeVisible
	{
		add {onChangeVisible += value;}
		remove{onChangeVisible += value;}
	}

	event Action<UnitObject> IVisibilityEvent<UnitObject>.OnChangeInvisible
	{
		add {onChangeInvisible += value;}
		remove{onChangeInvisible += value;}
	}

	partial void InitVisibility()
	{
		if (childVisibility != null) return;
		childVisibility = GetComponentInChildren<CameraVisibilityGroup>();
		if (childVisibility == null) return;
		childVisibility.OnChangeVisible += CameraVisibilityGroup_OnChangeVisible;
		childVisibility.OnChangeInvisible += CameraVisibilityGroup_OnChangeInvisible;
		if (ChildVisibility.IsVisible)
		{
			CameraVisibilityGroup_OnChangeVisible(this);
		}
		else
		{
			CameraVisibilityGroup_OnChangeInvisible(this);
		}
	}
	private void CameraVisibilityGroup_OnChangeVisible(Component obj)
	{
		if (obj == null) return;
		if (obj is not UnitObject unit || unit != this) return;
		if (onChangeVisible == null) return;
		onChangeVisible.Invoke(unit);
	}
	private void CameraVisibilityGroup_OnChangeInvisible(Component obj)
	{
		if (obj == null) return;
		if (obj is not UnitObject unit || unit != this) return;
		if (onChangeInvisible == null) return;
		onChangeInvisible.Invoke(unit);
	}
}
