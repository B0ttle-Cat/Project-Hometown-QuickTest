using GameUI;

public interface IUnitCardUIObject : ICardUIObject , IStrategyElement
{
	public (float[] values, float total, float max) GetShieldDetailValue();
	public (float[] values, float total, float max) GetMaterialDetailValue();
	public (float[] values, float total, float max) GetElectricDetailValue();
	public (float total, float max) GetShieldSimpleValue();
	public (float total, float max) GetMaterialSimpleValue();
	public (float total, float max) GetElectricSimpleValue();
}
