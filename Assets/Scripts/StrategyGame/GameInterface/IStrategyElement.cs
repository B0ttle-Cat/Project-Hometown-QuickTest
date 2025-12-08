public interface IStrategyElement : IStrategyStartGame
{
	public IStrategyElement ThisElement { get; }
	public int ID { get; set; }
	void InStrategyCollector();
	void OutStrategyCollector();
}
