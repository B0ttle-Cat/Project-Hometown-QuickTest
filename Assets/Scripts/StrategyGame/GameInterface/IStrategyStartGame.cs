public interface IStrategyStartGame
{
	public static int Default = 0;
	int StartEventOrder() => Default;
	int StopEventOrder() => Default;
	void OnStartGame();
	void OnStopGame();
}
