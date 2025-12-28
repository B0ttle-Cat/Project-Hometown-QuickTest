public partial class UnitObject : IStrategyMonoElement, IStrategyStartGame
{
	int unitElementID;
	public IStrategyElement ThisElement => this;
	public bool IsInCollector { get; set; }
	int IStrategyElement.ID { get => unitElementID; set => unitElementID = value; }

	public void InStrategyCollector()
	{
		instanceData.SetElementID(in unitElementID);
		if(FactionID >=0) Faction.AddUnit(this);
	}

	public void OutStrategyCollector()
	{
		if (FactionID >= 0) Faction.RemoveUnit(this);
	}

	void IStrategyStartGame.OnStartGame()
	{
		if (FactionID >= 0) Faction.AddUnit(this);
	}
	void IStrategyStartGame.OnStopGame()
	{
		if (FactionID >= 0) Faction.RemoveUnit(this);
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