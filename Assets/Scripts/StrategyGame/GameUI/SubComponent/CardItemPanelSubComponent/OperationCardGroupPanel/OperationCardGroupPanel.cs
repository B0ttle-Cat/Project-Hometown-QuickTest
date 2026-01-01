using GameUI;

public class OperationCardGroupPanel : CardGroupPanelComponent<OperationObject>, IShowHideAsync
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
			playerFaction.AddChangeOperation(OnChangeValue, false);
			InitCardList(playerFaction.OperationList.CardUIList);
		}
		AllShow();
	}

	private void OnChangeValue(IStrategyElement element, bool added)
	{
		if (element is not OperationObject item) return;

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