using System.Collections.Generic;

using UnityEngine;

[RequireComponent(typeof(KeyPairTarget))]
public partial class StrategyPopupPanelUI : MonoBehaviour, IGamePanelUI, IStrategyStartGame
{
	void IGamePanelUI.OpenUI()
	{
		gameObject.SetActive(true);
	}
	void IGamePanelUI.CloseUI()
	{
		gameObject.SetActive(false);
	}

	void IStrategyStartGame.OnStartGame()
	{
		if (this is IGamePanelUI panelUI)
			panelUI.OpenUI();
	}

	void IStrategyStartGame.OnStopGame()
	{
		if (this is IGamePanelUI panelUI)
			panelUI.CloseUI();
	}
}

public partial class StrategyPopupPanelUI
{
	private KeyPairTarget keyPairTarget;
	public IKeyPairChain KeyPair
	{
		get
		{
			if (keyPairTarget == null && !TryGetComponent<KeyPairTarget>(out keyPairTarget))
			{
				keyPairTarget = gameObject.AddComponent<KeyPairTarget>();
			}
			return keyPairTarget;
		}
	}

	private Dictionary<GameObject, object> uiOrderPair;
	Dictionary<GameObject, object> OrderPair => uiOrderPair ??= new Dictionary<GameObject, object>();
	public void ShowTopMessage(object order, string message)
	{
		if(TopMessage(out var ui))
		{
			OrderPair[ui.gameObject] = order;
			ui.Text = message;
			ui.OnShow();
		}
	}
	public void HideTopMessage(object order)
	{
		if (TopMessage(out var ui))
		{
			if(OrderPair[ui.gameObject] == order)
			{
				ui.OnHide();
				OrderPair.Remove(ui.gameObject);
			}
		}
	}
	private bool TopMessage(out MessageBox messageBox)
	{
		KeyPair.FindPairChain<MessageBox>("TopMessage", out messageBox);
		return messageBox != null;
	}

}