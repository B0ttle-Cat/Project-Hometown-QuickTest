using GameUI;

public interface ISectorToPanelAPI : ITargetToCardAPI, ITargetToLabelAPI, IStrategyElement
{
	public string GetFactionName();
	bool IsEnableResourcesSupply { get; set; }
	public (float[] values, float total, float max) GetShieldDetailValue();
	public (float[] values, float total, float max) GetPersonnelDetailValue();
	public (float[] values, float total, float max) GetMaterialDetailValue();
	public (float[] values, float total, float max) GetElectricDetailValue();
	public (float total, float max) GetShieldSimpleValue();
	public (float total, float max) GetPersonnelSimpleValue();
	public (float total, float max) GetMaterialSimpleValue();
	public (float total, float max) GetElectricSimpleValue();
}
