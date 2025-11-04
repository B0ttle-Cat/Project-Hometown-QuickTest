using UnityEngine;

public class StrategyMainPanelUI : MonoBehaviour, IGamePanelUI, IStartGame
{
	public void OpenUI()
	{
	}
	public void CloseUI()
	{
	}

    void IStartGame.OnStartGame()
	{
		OpenUI();
	}

    void IStartGame.OnStopGame()
	{
		CloseUI();
	}
}
