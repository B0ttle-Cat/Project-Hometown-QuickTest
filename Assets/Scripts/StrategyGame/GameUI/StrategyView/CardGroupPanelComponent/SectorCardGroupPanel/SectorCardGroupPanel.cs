using GameUI;


public class SectorCardGroupPanel : CardGroupPanelComponent<SectorObject>, IShowHideAsync
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
			InitCardList(playerFaction.CapturedList.CardUIList);
		}
		AllShow();
	}

	private void OnChangeValue(IStrategyElement element, bool added)
	{
		if (element is not SectorObject item) return;

		if (added)
		{
			this.AddPoolData(item);

		}
		else
		{
			this.RemovePoolData(item);
		}
	}
}
