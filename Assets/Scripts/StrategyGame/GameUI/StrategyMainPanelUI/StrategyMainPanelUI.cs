using GameUI;

using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class StrategyMainPanelUI : GameUIController	,IStrategyStartGame
{
	protected override void Awake()
	{
		base.Awake();
	}
	void IStrategyStartGame.OnStartGame()
	{
		OnShow();
	}
	void IStrategyStartGame.OnStopGame()
	{
		OnHide();
	}

	protected override void Hide()
	{
		DeinitStrategyResourcesView();
 
		int count = ThisUIFinder.TryFinds<IShowHide>(out var finds);
		for (int i = 0 ; i < count ; i++)
		{
			finds[i].OnHide();
		}
	}
	protected override void Show()
	{
		InitStrategyResourcesView();

		int count = ThisUIFinder.TryFinds<IShowHide>(out var finds);
        for (int i = 0 ; i < count; i++)
        {
			finds[i].OnShow();
		}
	}




	private void InitStrategyResourcesView()
	{
		if(ThisUIFinder.TryFind<StrategyResourcesView>(out var resourcesView))
		{
			resourcesView.SetTarget(FactionAPI.ID2Faction(StrategyManager.PlayerFactionID));
		}
	}
	private void DeinitStrategyResourcesView()
	{
		if (ThisUIFinder.TryFind<StrategyResourcesView>(out var resourcesView))
		{
			resourcesView.SetTarget(FactionAPI.ID2Faction(StrategyManager.PlayerFactionID));
		}
	}
}
