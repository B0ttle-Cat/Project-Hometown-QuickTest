using GameUI;

using UnityEngine;

[RequireComponent(typeof(KeyPairTarget))]
[RequireComponent(typeof(RectTransform))]
public class StrategyMainPanelUI : GameUIController, IPanelItem
{
    private RectTransform rectTransform;
	private KeyPairTarget keyPairTarget;

    public IPanelItem ThisPanel => this;
    RectTransform IPanelItem.ThisRect => rectTransform;

    protected override void Awake()
    {
        base.Awake();
		rectTransform = GetComponent<RectTransform>();
		keyPairTarget.GetComponent<KeyPairTarget>();
	}

    protected override void Hide()
    {
        
    }

    protected override void Show()
    {
        var keyPair = gameObject.GetKeyPairChain();

		if (keyPair.TryFindPair<StrategyResourcesView>("MainFillRect", out var view))
		{
			Faction playerFaction = StrategyManager.Collector.Find<Faction>(StrategyManager.PlayerFactionID);
			view.SetTarget(playerFaction);
		}
	}
}
