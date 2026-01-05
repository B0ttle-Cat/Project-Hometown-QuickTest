using GameUI;

public interface IOperationToPanelAPI : ITargetToCardAPI, ITargetToLabelAPI, IStrategyElement
{
	public string GetFactionName();
	public (float[] values, float total, float max) GetPersonnelDetailValue();
	public (float[] values, float total, float max) GetMaterialDetailValue();
	public (float[] values, float total, float max) GetElectricDetailValue();
	public (float total, float max) GetPersonnelSimpleValue();
	public (float total, float max) GetMaterialSimpleValue();
	public (float total, float max) GetElectricSimpleValue();
}
