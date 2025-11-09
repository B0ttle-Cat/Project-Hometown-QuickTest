public partial class UnitObject : IStrategyElement
{
	public IStrategyElement ThisElement => this;
	public bool IsInCollector { get; set; }
	int IStrategyElement.ID { get => UnitID; set => Profile.SetUnitID(value); }

	public void InStrategyCollector()
	{
		string name = $"{ProfileData.displayName}_{UnitID:00}";
		gameObject.name = name;
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
