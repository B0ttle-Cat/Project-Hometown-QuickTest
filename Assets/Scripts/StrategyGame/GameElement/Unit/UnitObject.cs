using GameUI;

using Sirenix.OdinInspector;

using StrategyManagerModule;

using UnityEngine;

using static StrategyGamePlayData;

public partial class UnitObject : MonoBehaviour
{
#if UNITY_EDITOR
	[ShowInInspector, ToggleGroup("EditUnitData", GroupName = "UnitData")]
	bool EditUnitData { get; set; } = false;
#endif
	[SerializeField, ToggleGroup("EditUnitData")]
	private UnitStatsData statsData;
	[SerializeField, ToggleGroup("EditUnitData")]
	private UnitRuntimeData runtimeData;
	[SerializeField, ToggleGroup("EditUnitData")]
	private UnitInstanceData instanceData;

	public UnitRuntimeData RuntimeData => runtimeData;
	public UnitStatsData StatsData => statsData;
	public UnitInstanceData InstanceData => instanceData;

	private CaptureTag captureTag;
	private UnitDebugRender debugRender;
	public CaptureTag UnitCaptureTag { get => captureTag; set => captureTag = value; }
	public string UnitName => InstanceData.displayName;
	public int UnitID => InstanceData.unitID;
	public int FactionID => InstanceData.factionID;
	public Faction Faction
	{
		get => StrategyManager.IsNotReadyScene ? null : StrategyManager.Collector.Find<Faction>(FactionID);
	}
	public void Init(UnitProfileObject data, int factionID = -1)
	{
		statsData = new UnitStatsData(data);
		runtimeData = new UnitRuntimeData(statsData);
		instanceData = new UnitInstanceData(data, factionID);
	}
	public void Init(in StrategyStartSetterData.UnitData data) // UnitData
	{
		instanceData.Init(in data);

		int durability = Mathf.Min(data.durability, StatsData.MaxDurability);
		if (durability < 1) durability = 1;
		RuntimeData.CurrentDurability = durability;
	}
	public void InitOther()
	{
		InitLife();
		InitDebugRender();
		InitCaptureTag();
		InitMovement();
		InitOperationObject();
		InitVisibility();
		InitFSM();
		InitNearby();
		InitCombat();
		InitAttack();
	}
	partial void InitCaptureTag();
	private void InitDebugRender()
	{
		if (debugRender == null || Faction == null) return;
		debugRender.SetColor(Faction.FactionColor);
	}
	partial void InitMovement();
	partial void InitVisibility();
	partial void InitOperationObject();
	partial void InitFSM();
	partial void InitNearby();
	partial void InitCombat();
	partial void InitAttack();

	public void Deinit()
	{
		DeselectSelf();
		DeinitCaptureTag();
		DeinitFSM();
		DeinitNearby();
		DeinitCombat();
		DeinitMovement();
		DeinitAttack();
	}
	partial void DeinitCaptureTag();
	partial void DeselectSelf();
	partial void DeinitFSM();
	partial void DeinitNearby();
	partial void DeinitCombat();
	partial void DeinitMovement();
	partial void DeinitAttack();
}

public partial class UnitObject // UnitCaptureTag
{
	partial void InitCaptureTag()
	{
		if (StatsData.CaptureScore > 0)
		{
			if (UnitCaptureTag == null) UnitCaptureTag = GetComponentInChildren<CaptureTag>();
			if (UnitCaptureTag == null) UnitCaptureTag = gameObject.AddComponent<CaptureTag>();

			UnitCaptureTag.Init(FactionID, StatsData.CaptureScore);
		}
		else
		{
			if (UnitCaptureTag != null)
			{
				Destroy(UnitCaptureTag);
				UnitCaptureTag = null;
			}
		}
	}
	partial void DeinitCaptureTag()
	{
		if (UnitCaptureTag != null)
		{
			UnitCaptureTag.Deinit();
			Destroy(UnitCaptureTag);
			UnitCaptureTag = null;
		}
	}
}


public partial class UnitObject : IUnitForPanel
{
	#region ITargetForLabelPanel
	string ITargetForLabelPanel.GetLabelName()
	{
		return InstanceData.displayName;
	}
	Sprite ITargetForLabelPanel.GetLabelIcon()
	{
		return InstanceData.unitIconSprite;
	}
	#endregion

	#region ITargetForCardPanel
	Sprite ITargetForCardPanel.GetCardImage()
	{
		return InstanceData.unitFullBodySprite;
	}
	string ITargetForCardPanel.GetCardName()
	{
		return InstanceData.displayName;
	}
	#endregion

	#region IUnitForPanel
	string IUnitForPanel.GetFactionName()
	{
		return Faction.IsNullRef() ? "중립" : Faction.FactionName;
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
	#endregion
}