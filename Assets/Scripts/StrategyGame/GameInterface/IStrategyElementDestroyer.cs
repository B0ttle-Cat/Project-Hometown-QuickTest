public interface IStrategyElementDestroyer : IStrategyElement
{
	public IStrategyElementDestroyer ThisDestroyer { get; }
	public bool IsDestroy { get; set; }
	public void InitLife();
	public void OnDestroy();
	public void OnReservationDestroy()
	{
		if (IsDestroy) return;
		IsDestroy = true;
		StrategyManager.Collector.Add<IStrategyElementDestroyer>(this);
	}
}
