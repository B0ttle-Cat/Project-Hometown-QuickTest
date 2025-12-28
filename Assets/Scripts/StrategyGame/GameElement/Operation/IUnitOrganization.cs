using System;

using static OperationObject;
using static StrategyGamePlayData;

public interface IUnitOrganization
{
	IUnitOrganization ThisOrganization { get; }
	public OperationUnitList UnitOrganizationList {  get; }
	public event Action<OperationObject> OnChangeUnitList;
	public bool HasUnitType(in UnitKey unitKey);
	public bool AddUnitObject(UnitObject unitObject, bool callback = true);
	public bool RemoveUnitObject(UnitObject unitObject, bool callback = true);
	public void RelaseAllUnit(bool withDestroy = false);
	public void RelaseAndDestroyAllUnit() => RelaseAllUnit(true);
}