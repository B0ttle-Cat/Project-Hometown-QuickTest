using System;
using System.Collections.Generic;
public partial class OperationObject : IVisibilityEvent<OperationObject>
{
	public IVisibilityEvent<OperationObject> ThisVisibility => this;
	bool IVisibilityEvent<OperationObject>.IsVisible => (visibleUnitList == null ? 0 : visibleUnitList.Count) > 0;
	private Action<OperationObject> onChangeVisible;
	private Action<OperationObject> onChangeInvisible;
	event Action<OperationObject> IVisibilityEvent<OperationObject>.OnChangeVisible
	{
		add => onChangeVisible += value;
		remove => onChangeVisible -= value;
	}
	event Action<OperationObject> IVisibilityEvent<OperationObject>.OnChangeInvisible
	{
		add => onChangeInvisible += value;
		remove => onChangeInvisible -= value;
	}

	private HashSet<UnitObject> visibleUnitList;
	public void ChangeVisibleUnit(UnitObject unitObject)
	{
		visibleUnitList ??= new HashSet<UnitObject>();
		if (visibleUnitList.Add(unitObject))
		{
			if (visibleUnitList.Count == 1)
			{
				onChangeVisible?.Invoke(this);
			}
		}
	}
	public void ChangeInvisibleUnit(UnitObject unitObject)
	{
		if (visibleUnitList == null) return;

		if (visibleUnitList.Remove(unitObject))
		{
			if (visibleUnitList.Count == 0)
			{
				onChangeInvisible?.Invoke(this);
			}
		}
	}
}
