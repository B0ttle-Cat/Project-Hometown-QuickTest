public partial class UnitObject : IStrategyElement, IStrategyStartGame
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

public partial class UnitObject : IStrategyElementDestroyer
{
	public IStrategyElementDestroyer ThisDestroyer => this;
    public bool IsDestroy { get; set; }

    public void InitLife()
	{
		IsDestroy = false;
	}

	void IStrategyElementDestroyer.OnDestroy()
    {
		IsDestroy = true;
		StrategyElementFactory.Destroy(this);
	}

	public void DamageDeath()
	{
		ThisDestroyer.ReservationDestroy();
	}
	public void DestroyWithOperation()
	{
		ThisDestroyer.ReservationDestroy();
	}
}