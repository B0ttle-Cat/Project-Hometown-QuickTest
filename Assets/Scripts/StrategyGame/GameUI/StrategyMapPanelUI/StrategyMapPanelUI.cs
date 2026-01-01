using GameUI;

public class StrategyMapPanelUI : GameUIController
{
	protected override void Hide()
	{
		int count = ThisUIFinder.TryFinds<IShowHide>(out var finds);
		for (int i = 0 ; i < count ; i++)
		{
			finds[i].OnHide();
		}
	}
	protected override void Show()
	{
		int count = ThisUIFinder.TryFinds<IShowHide>(out var finds);
		for (int i = 0 ; i < count ; i++)
		{
			finds[i].OnShow();
		}
	}

}
