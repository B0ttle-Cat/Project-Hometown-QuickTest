using GameUI;

using UnityEngine;

public partial class SectorObject : ISectorCardUIObject
{
	Sprite ICardUIObject.GetTitleImage()
	{
		return null;
	}
	string ICardUIObject.GetTitleName()
	{
		return StatsData.SectorName;
	}
	string ICardUIObject.GetDescription()
	{
		return StatsData.SectorName;
	}
	string ICardUIObject.GetFactionName()
	{
		Faction faction = FactionAPI.ID2Faction(RuntimeData.CaptureFactionID);
		if (faction == null) return "점령 없음";
		return faction.FactionName;
	}
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
		string text = $"{total}/{max}";
		return text;
	}
	public string GetMaterialSimpleText()
	{
		(float total, float max) = GetMaterialSimpleValue();
		string text = $"{total}/{max}";
		return text;
	}
	public string GetElectricSimpleText()
	{
		(float total, float max) = GetElectricSimpleValue();
		string text = $"{total}/{max}";
		return text;
	}
}