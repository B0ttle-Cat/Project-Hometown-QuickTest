public partial class StrategyControlPanelUI : FloatingPanelUI, IGamePanelUI, IStartGame
{
	public bool IsOpen { get; set; }
    private void Awake()
    {
		CloseUI();
	}
    public void OpenUI()
	{
		
	}
	public void CloseUI()
	{
	
	}
    void IStartGame.OnStartGame()
    {
		StrategyManager.Selecter.AddListener_OnFirstAndLast(OpenUI, CloseUI);
		if(StrategyManager.Selecter.GetCurrentSelectList.Count > 0)
		{
			OpenUI();
		}
    }
    void IStartGame.OnStopGame()
	{
		CloseUI();
		StrategyManager.Selecter.RemoveListener_OnFirsAndLast(OpenUI, CloseUI);
	}
	public void AddFloatingUI()
	{

	}
}

public partial class StrategyControlPanelUI
{

}
