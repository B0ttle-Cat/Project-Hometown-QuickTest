public partial class UnitObject : IStrategyMonoElement, IStrategyStartGame
{
	int unitElementID;
	public IStrategyElement ThisElement => this;
	public bool IsInCollector { get; set; }
	int IStrategyElement.ID { get => unitElementID; set => unitElementID = value; }

	public void InStrategyCollector()
	{
		instanceData.SetElementID(in unitElementID);
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

	private void OnDestroy()
	{
		if (!ThisDestroyer.IsDestroy)
		{
			ThisDestroyer.OnDestroy();
		}
	}

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
		if(HasOperation)
		{
			Operation.ThisOrganization.RemoveUnitObject(this);
		}

		ThisDestroyer.OnReservationDestroy();
	}
	public void DestroyWithOperation()
	{
		ThisDestroyer.OnReservationDestroy();
	}
}