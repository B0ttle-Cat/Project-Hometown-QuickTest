using UnityEngine;

public class StrategyMainPanelUI : MonoBehaviour, IGamePanelUI, IStrategyStartGame
{
	public void OpenUI()
	{
	}
	public void CloseUI()
	{
	}

    void IStrategyStartGame.OnStartGame()
	{
		OpenUI();
	}

    void IStrategyStartGame.OnStopGame()
	{
		CloseUI();
	}
}
