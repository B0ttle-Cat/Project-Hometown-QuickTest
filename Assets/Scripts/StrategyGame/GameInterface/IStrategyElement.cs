public interface IStrategyElement : IStrategyStartGame
{
	public IStrategyElement ThisElement { get; }
	public bool IsInCollector { get; set; }
	public int ID { get; set; }
	void _InStrategyCollector()
	{
		if (IsInCollector) return;
		IsInCollector = true;
		InStrategyCollector();
	}
	void _OutStrategyCollector()
	{
		if (!IsInCollector) return;
		IsInCollector = false;
		OutStrategyCollector();
	}
	void InStrategyCollector();
	void OutStrategyCollector();
}
