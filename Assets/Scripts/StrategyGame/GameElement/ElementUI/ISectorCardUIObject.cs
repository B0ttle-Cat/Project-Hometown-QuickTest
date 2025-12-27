using GameUI;

using UnityEngine;

public interface ISectorCardUIObject : ICardUIObject
{
	public Sprite GetTitleImage();
	public string GetTitleName();
	public string GetDescription();
	public string GetCaptureFactionName();

	bool IsEnableResourcesSupply { get; set; }
	public (float[] values, float total, float max) GetPersonnelDetailValue();
    public (float[] values, float total, float max) GetMaterialDetailValue();
    public (float[] values, float total, float max) GetElectricDetailValue();
	public (float total, float max) GetPersonnelSimpleValue();
	public (float total, float max) GetMaterialSimpleValue();
	public (float total, float max) GetElectricSimpleValue();
	public string GetPersonnelDetailText()
	{
		(float[] values, float total, float max) = GetPersonnelDetailValue();
		string text = $"인력: {total}/{max}";
		int length = values.Length;
		for (int i = 0 ; i < length ; i++)
		{
			float value = values[i];
			text += $"\t{value:+#;-#;0}";
		}

		return text;
	}
	public string GetMaterialDetailText()
	{
		(float[] values, float total, float max) = GetMaterialDetailValue();
		string text = $"재료: {total}/{max}";
		int length = values.Length;
		for (int i = 0 ; i < length ; i++)
		{
			float value = values[i];
			text += $"\t{value:+#;-#;0}";
		}

		return text;
	}
	public string GetElectricDetailText()
	{
		(float[] values, float total, float max) = GetElectricDetailValue();
		string text = $"전력: {total}/{max}";
		int length = values.Length;
		for (int i = 0 ; i < length ; i++)
		{
			float value = values[i];
			text += $"\t{value:+#;-#;0}";
		}

		return text;
	}
	public string GetPersonnelSimpleText()
	{
		(float total, float max) = GetPersonnelSimpleValue();
		string text = $"인력: {total}/{max}";
		return text;
	}
	public string GetMaterialSimpleText()
	{
		(float total, float max) = GetMaterialSimpleValue();
		string text = $"재료: {total}/{max}";
		return text;
	}
	public string GetElectricSimpleText()
	{
		(float total, float max) = GetElectricSimpleValue();
		string text = $"전력: {total}/{max}";
		return text;
	}

}
