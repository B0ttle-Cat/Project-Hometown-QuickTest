public interface ISelectable
{
	public enum SelectableType
	{
		None = 0,
		Unit = 1,
		Sector = 2,
	}
	public SelectableType Type
	{
		get
		{
			return this switch
			{
				UnitObject unit => SelectableType.Unit,
				SectorObject sector => SelectableType.Sector,
				_ => SelectableType.None
			};
		}
	}

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
