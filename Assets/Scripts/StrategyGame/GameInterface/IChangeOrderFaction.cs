using System;

public interface IChangeOrderFaction : IStrategyElement
{
	event Action<IStrategyElement, int> OnChangeFaction;
}
