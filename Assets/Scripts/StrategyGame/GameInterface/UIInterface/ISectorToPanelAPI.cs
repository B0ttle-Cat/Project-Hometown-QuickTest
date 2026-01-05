using GameUI;

public interface ISectorToPanelAPI : ITargetToCardAPI, ITargetToLabelAPI, IStrategyElement
{
	public string GetFactionName();
	bool IsEnableResourcesSupply { get; set; }
	public (int[] values, int total, int max) GetShieldDetailValue();
	public (int[] values, int total, int max) GetPersonnelDetailValue();
	public (int[] values, int total, int max) GetMaterialDetailValue();
	public (int[] values, int total, int max) GetElectricDetailValue();
	public (int total, int max) GetShieldSimpleValue();
	public (int total, int max) GetPersonnelSimpleValue();
	public (int total, int max) GetMaterialSimpleValue();
	public (int total, int max) GetElectricSimpleValue();
}
