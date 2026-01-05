using GameUI;

using UnityEngine;

using static StrategyGamePlayData;

public partial class SectorObject : ISectorToPanelAPI
{
	Sprite ITargetToCardAPI.GetCardImage()
	{
		return null;
	}
	string ITargetToCardAPI.GetCardName()
	{
		return StatsData.SectorName;
	}

	string ITargetToLabelAPI.GetLabelName()
	{	
		return StatsData.SectorName;
	}

	Sprite ITargetToLabelAPI.GetLabelIcon()
	{
		return StatsData.SectorIcon;
	}

	Vector3 ITargetToLabelAPI.LabelWorldPosition()
	{
		return transform.position;
	}


	string ISectorToPanelAPI.GetFactionName()
	{
		Faction faction = FactionAPI.ID2Faction(RuntimeData.CaptureFactionID);
		if (faction == null) return "점령 없음";
		return faction.FactionName;
	}

	public (float[] values, float total, float max) GetShieldDetailValue()
	{
		float max = ThisStatsValue.GetStatsValue(StatsType.시설_내구도_최대);
		float total = ThisStatsValue.GetStatsValue(StatsType.시설_내구도_현재);
		float[] values = new float[1];
		values[0] = ThisStatsValue.GetStatsValue(StatsType.시설_내구도_현재);
		return (values, total, max);
	}
	public (float[] values, float total, float max) GetPersonnelDetailValue()
	{
		float max = ThisStatsValue.GetStatsValue(StatsType.자원_인력_최대);
		float total = ThisStatsValue.GetStatsValue(StatsType.자원_인력_현재);
		float[] values = new float[1];
		values[0] = ThisStatsValue.GetStatsValue(StatsType.자원_인력_현재);
		return (values, total, max);
	}
	public (float[] values, float total, float max) GetMaterialDetailValue()
	{
		float max = ThisStatsValue.GetStatsValue(StatsType.자원_재료_최대);
		float total = ThisStatsValue.GetStatsValue(StatsType.자원_재료_현재);
		float[] values = new float[1];
		values[0] = ThisStatsValue.GetStatsValue(StatsType.자원_재료_현재);
		return (values, total, max);
	}
	public (float[] values, float total, float max) GetElectricDetailValue()
	{
		float max = ThisStatsValue.GetStatsValue(StatsType.자원_전력_최대);
		float total = ThisStatsValue.GetStatsValue(StatsType.자원_전력_현재);

		float[] values = new float[1];
		values[0] = ThisStatsValue.GetStatsValue(StatsType.자원_전력_현재);
		return (values, total, max);
	}
	public (float total, float max) GetShieldSimpleValue()
	{
		float max = ThisStatsValue.GetStatsValue(StatsType.시설_내구도_최대);
		float total = ThisStatsValue.GetStatsValue(StatsType.시설_내구도_현재);
		return (total, max);
	}
	public (float total, float max) GetPersonnelSimpleValue()
	{
		float max = ThisStatsValue.GetStatsValue(StatsType.자원_인력_최대);
		float total = ThisStatsValue.GetStatsValue(StatsType.자원_인력_현재);
		return (total, max);
	}
	public (float total, float max) GetMaterialSimpleValue()
	{
		float max = ThisStatsValue.GetStatsValue(StatsType.자원_재료_최대);
		float total = ThisStatsValue.GetStatsValue(StatsType.자원_재료_현재);
		return (total, max);
	}
	public (float total, float max) GetElectricSimpleValue()
	{
		float max = ThisStatsValue.GetStatsValue(StatsType.자원_전력_최대);
		float total = ThisStatsValue.GetStatsValue(StatsType.자원_전력_현재);
		return (total, max);
	}
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