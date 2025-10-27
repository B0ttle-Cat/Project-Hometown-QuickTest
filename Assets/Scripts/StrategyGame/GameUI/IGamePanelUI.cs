public interface IGamePanelUI
{
	public bool IsOpen { get; set; }
	public void OpenUI();
	public void CloseUI();
}
