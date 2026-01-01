using GameUI;

public class UnitCardGroupPanel : CardGroupPanelComponent, IShowHideAsync
{
	void IShowHide.EndedHide()
	{
		Faction playerFaction = FactionAPI.ID2Faction(StrategyManager.PlayerFactionID);
		if (playerFaction != null)
		{
			playerFaction.RemoveChangeUnit(OnChangeValue);
		}
		AllHideAndClear();
	}
	void IShowHide.StartShow()
	{
		Faction playerFaction = FactionAPI.ID2Faction(StrategyManager.PlayerFactionID);
		if (playerFaction != null)
		{
			playerFaction.AddChangeUnit(OnChangeValue);
			InitObjects(playerFaction.UnitList.ForPanel);
		}
		AllShow();
	}
	private void OnChangeValue(IStrategyElement element, bool added)
	{
		if (element is not UnitObject item) return;

		if (added)
		{
			this.AddObject(item);
		}
		else
		{
			this.RemoveObject(item);
		}
	}
}
