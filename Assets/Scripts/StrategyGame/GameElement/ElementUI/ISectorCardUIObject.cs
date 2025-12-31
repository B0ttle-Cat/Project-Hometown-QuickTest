using GameUI;

public interface ISectorCardUIObject : ICardUIObject, IStrategyElement
{
	bool IsEnableResourcesSupply { get; set; }
	public (float[] values, float total, float max) GetShieldDetailValue();
	public (float[] values, float total, float max) GetPersonnelDetailValue();
	public (float[] values, float total, float max) GetMaterialDetailValue();
	public (float[] values, float total, float max) GetElectricDetailValue();
	public (float total, float max) GetShieldSimpleValue();
	public (float total, float max) GetPersonnelSimpleValue();
	public (float total, float max) GetMaterialSimpleValue();
	public (float total, float max) GetElectricSimpleValue();

	public string GetShieldValueText(bool simpleText = false)
	{
		if (simpleText)
		{
			(float total, float max) = GetShieldSimpleValue();
			string text = $"보호막: {total}/{max}";
			return text;
		}
		else
		{
			(float[] values, float total, float max) = GetShieldDetailValue();
			string text = $"보호막: {total}/{max}";
			int length = values.Length;
			for (int i = 0 ; i < length ; i++)
			{
				float value = values[i];
				text += $"\t{value:+#;-#;0}";
			}

			return text;
		}
	}
	public string GetPersonnelValueText(bool simpleText = false)
	{
		if (simpleText)
		{
			(float total, float max) = GetPersonnelSimpleValue();
			string text = $"인력: {total}/{max}";
			return text;
		}
		else
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
	}
	public string GetMaterialValueText(bool simpleText = false)
	{
		if (simpleText)
		{
			(float total, float max) = GetMaterialSimpleValue();
			string text = $"재료: {total}/{max}";
			return text;
		}
		else
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
	}
	public string GetElectricValueText(bool simpleText = false)
	{
		if (simpleText)
		{
			(float total, float max) = GetElectricSimpleValue();
			string text = $"전력: {total}/{max}";
			return text;
		}
		else
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
	}
}
