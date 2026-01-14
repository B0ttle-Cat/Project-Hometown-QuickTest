public interface ISelectable
{
	bool CanSelect() => true;
	bool HasPassthrough(out ISelectable pass)
	{
		pass = null;
		return false;
	}
	void SelfSelect() => StrategyManager.Selecter.OnSystemSelectObject(this);
	void SelfDeselect() => StrategyManager.Selecter.OnSystemDeselectObject(this);
	void SelfPointing() => StrategyManager.Selecter.OnSystemPointingTarget(this);
	void OnSelect();
	void OnDeselect();
	void OnPointing();
}
