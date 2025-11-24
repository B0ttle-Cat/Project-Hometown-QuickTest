public partial class UnitObject : IStrategyElement
{
	int unitElementID;
	public IStrategyElement ThisElement => this;
	public bool IsInCollector { get; set; }
	int IStrategyElement.ID { get => unitElementID; set => unitElementID = value; }

	public void InStrategyCollector()
	{

	}

	public void OutStrategyCollector()
	{
	}

	void IStrategyStartGame.OnStartGame()
	{
	}
	void IStrategyStartGame.OnStopGame()
	{
	}
}
