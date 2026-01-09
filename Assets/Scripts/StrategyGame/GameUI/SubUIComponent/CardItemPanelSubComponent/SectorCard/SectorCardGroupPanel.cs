using GameUI;


public class SectorCardGroupPanel : CardGroupPanelComponent<SectorCardItemPanel>, IShowHideAsync
{
	void IShowHide.EndedHide()
	{
		Faction playerFaction = FactionAPI.ID2Faction(StrategyManager.PlayerFactionID);
		if (playerFaction != null)
		{
			playerFaction.RemoveChangeCaptured(OnChangeValue);
		}
		AllHideAndClear();
	}
	void IShowHide.StartShow()
	{
		Faction playerFaction = FactionAPI.ID2Faction(StrategyManager.PlayerFactionID);
		if (playerFaction != null)
		{
			playerFaction.AddChangeCaptured(OnChangeValue, false);
			InitObjects(playerFaction.CapturedList.ForPanel);
		}
		AllShow();
	}
	private void OnChangeValue(IStrategyElement element, bool added)
	{
		if (element is not SectorObject item) return;

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
