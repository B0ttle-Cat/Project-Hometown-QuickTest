using GameUI;

public class StrategyMapPanelUI : GameUIController
{
	protected override void Hide()
	{
		DeinitOperationLabelGroupPanel();

		int count = ThisUIFinder.TryFinds<IShowHide>(out var finds);
		for (int i = 0 ; i < count ; i++)
		{
			finds[i].OnHide();
		}
	}
	protected override void Show()
	{
		InitOperationLabelGroupPanel();

		int count = ThisUIFinder.TryFinds<IShowHide>(out var finds);
		for (int i = 0 ; i < count ; i++)
		{
			finds[i].OnShow();
		}
	}



	private void InitOperationLabelGroupPanel()
	{
		if (!ThisUIFinder.TryFind<OperationLabelGroupPanel>(out var find)) return;

		find.SetTargetFaction(FactionAPI.ID2Faction(StrategyManager.PlayerFactionID));
	}
	private void DeinitOperationLabelGroupPanel()
	{
		if (!ThisUIFinder.TryFind<OperationLabelGroupPanel>(out var find)) return;

		find.SetTargetFaction(null);
	}
}
