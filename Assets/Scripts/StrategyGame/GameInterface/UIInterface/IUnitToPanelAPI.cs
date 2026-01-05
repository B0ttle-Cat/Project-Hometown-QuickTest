using GameUI;

public interface IUnitToPanelAPI : ITargetToCardAPI , ITargetToLabelAPI, IStrategyElement
{
	public string GetFactionName();
	public (int[] values, int total, int max) GetShieldDetailValue();
	public (int total, int max) GetShieldSimpleValue();
}
