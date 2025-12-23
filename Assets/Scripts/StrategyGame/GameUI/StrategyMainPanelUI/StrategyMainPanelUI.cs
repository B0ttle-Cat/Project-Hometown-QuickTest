using GameUI;

using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class StrategyMainPanelUI : GameUIController, IPanelItem	,IStrategyStartGame
{
	private RectTransform rectTransform;

	public IPanelItem ThisPanel => this;
	RectTransform IPanelItem.ThisRect => rectTransform;

	protected override void Awake()
	{
		base.Awake();
		rectTransform = GetComponent<RectTransform>();
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

    void IStrategyStartGame.OnStartGame()
    {
		ThisShowHide.OnShow();
	}

    void IStrategyStartGame.OnStopGame()
    {

		ThisShowHide.OnHide();
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
