using GameUI;

public class OperationCardGroupPanel : CardGroupPanelComponent<OperationCardItemPanel>, IShowHideAsync
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
			InitObjects(playerFaction.OperationList.ForPanel);
		}
		AllShow();
	}
	private void OnChangeValue(IStrategyElement element, bool added)
	{
		if (element is not OperationObject item) return;

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