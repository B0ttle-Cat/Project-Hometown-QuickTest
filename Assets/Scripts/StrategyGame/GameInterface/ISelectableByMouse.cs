using UnityEngine;
public interface ISelectable
{
	bool CanSelect() => true;
	bool HasPass(out ISelectable pass)
	{
		pass = null;
		return false;
	}
	void SelfSelect() => StrategyManager.Selecter.OnSystemSelectObject(this);
	void SelfDeselect() => StrategyManager.Selecter.OnSystemDeselectObject(this);
	void SelfPointing() => StrategyManager.Selecter.OnSystemPointingTarget(this);
	void OnSelect();
	void OnDeselect();
	void OnSingleSelect();
	void OnSingleDeselect();
	void OnFirstSelect();
	void OnLastDeselect();
}
public interface ISelectableByMouse : ISelectable
{
	Vector3 ClickCenter { get; }
	bool IsPointEnter { get; set; }
	bool IsSelectMouse { get; set; }
	void OnPointEnter() { }
	void OnPointExit() { }
}
