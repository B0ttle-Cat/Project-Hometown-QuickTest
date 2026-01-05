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
	Color ITargetToLabelAPI.GetLabelAccentColor()
	{
		if (CaptureFactionID < 0) return Color.white;
		return CaptureFaction.FactionColor;
	}
	Color ITargetToLabelAPI.GetLabelTextColor()
	{
		return Color.black;
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

	public (int[] values, int total, int max) GetShieldDetailValue()
	{
		int max = ThisStatsValue.GetStatsValue(StatsType.시설_내구도_최대);
		int total = ThisStatsValue.GetStatsValue(StatsType.시설_내구도_현재);
		int[] values = new int[1];
		values[0] = ThisStatsValue.GetStatsValue(StatsType.시설_내구도_현재);
		return (values, total, max);
	}
	public (int[] values, int total, int max) GetPersonnelDetailValue()
	{
		int max = ThisStatsValue.GetStatsValue(StatsType.자원_인력_최대);
		int total = ThisStatsValue.GetStatsValue(StatsType.자원_인력_현재);
		int[] values = new int[1];
		values[0] = ThisStatsValue.GetStatsValue(StatsType.자원_인력_현재);
		return (values, total, max);
	}
	public (int[] values, int total, int max) GetMaterialDetailValue()
	{
		int max = ThisStatsValue.GetStatsValue(StatsType.자원_재료_최대);
		int total = ThisStatsValue.GetStatsValue(StatsType.자원_재료_현재);
		int[] values = new int[1];
		values[0] = ThisStatsValue.GetStatsValue(StatsType.자원_재료_현재);
		return (values, total, max);
	}
	public (int[] values, int total, int max) GetElectricDetailValue()
	{
		int max = ThisStatsValue.GetStatsValue(StatsType.자원_전력_최대);
		int total = ThisStatsValue.GetStatsValue(StatsType.자원_전력_현재);

		int[] values = new int[1];
		values[0] = ThisStatsValue.GetStatsValue(StatsType.자원_전력_현재);
		return (values, total, max);
	}
	public (int total, int max) GetShieldSimpleValue()
	{
		int max = ThisStatsValue.GetStatsValue(StatsType.시설_내구도_최대);
		int total = ThisStatsValue.GetStatsValue(StatsType.시설_내구도_현재);
		return (total, max);
	}
	public (int total, int max) GetPersonnelSimpleValue()
	{
		int max = ThisStatsValue.GetStatsValue(StatsType.자원_인력_최대);
		int total = ThisStatsValue.GetStatsValue(StatsType.자원_인력_현재);
		return (total, max);
	}
	public (int total, int max) GetMaterialSimpleValue()
	{
		int max = ThisStatsValue.GetStatsValue(StatsType.자원_재료_최대);
		int total = ThisStatsValue.GetStatsValue(StatsType.자원_재료_현재);
		return (total, max);
	}
	public (int total, int max) GetElectricSimpleValue()
	{
		int max = ThisStatsValue.GetStatsValue(StatsType.자원_전력_최대);
		int total = ThisStatsValue.GetStatsValue(StatsType.자원_전력_현재);
		return (total, max);
	}
	public string GetShieldValueText(bool simpleText = false)
	{
		if (simpleText)
		{
			(int total, int max) = GetShieldSimpleValue();
			string text = $"보호막: {total}/{max}";
			return text;
		}
		else
		{
			(int[] values, int total, int max) = GetShieldDetailValue();
			string text = $"보호막: {total}/{max}";
			int length = values.Length;
			for (int i = 0 ; i < length ; i++)
			{
				int value = values[i];
				text += $"\t{value:+#;-#;0}";
			}

			return text;
		}
	}
	public string GetPersonnelValueText(bool simpleText = false)
	{
		if (simpleText)
		{
			(int total, int max) = GetPersonnelSimpleValue();
			string text = $"인력: {total}/{max}";
			return text;
		}
		else
		{
			(int[] values, int total, int max) = GetPersonnelDetailValue();
			string text = $"인력: {total}/{max}";
			int length = values.Length;
			for (int i = 0 ; i < length ; i++)
			{
				int value = values[i];
				text += $"\t{value:+#;-#;0}";
			}

			return text;
		}
	}
	public string GetMaterialValueText(bool simpleText = false)
	{
		if (simpleText)
		{
			(int total, int max) = GetMaterialSimpleValue();
			string text = $"재료: {total}/{max}";
			return text;
		}
		else
		{
			(int[] values, int total, int max) = GetMaterialDetailValue();
			string text = $"재료: {total}/{max}";
			int length = values.Length;
			for (int i = 0 ; i < length ; i++)
			{
				int value = values[i];
				text += $"\t{value:+#;-#;0}";
			}

			return text;
		}
	}
	public string GetElectricValueText(bool simpleText = false)
	{
		if (simpleText)
		{
			(int total, int max) = GetElectricSimpleValue();
			string text = $"전력: {total}/{max}";
			return text;
		}
		else
		{
			(int[] values, int total, int max) = GetElectricDetailValue();
			string text = $"전력: {total}/{max}";
			int length = values.Length;
			for (int i = 0 ; i < length ; i++)
			{
				int value = values[i];
				text += $"\t{value:+#;-#;0}";
			}

			return text;
		}
	}


}