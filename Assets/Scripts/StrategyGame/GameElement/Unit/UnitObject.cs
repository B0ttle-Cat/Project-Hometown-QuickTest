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


public partial class UnitObject : IUnitToPanelAPI
{
	#region ITargetToLabelAPI
	string ITargetToLabelAPI.GetLabelName()
	{
		return InstanceData.displayName;
	}
	Sprite ITargetToLabelAPI.GetLabelIcon()
	{
		return InstanceData.unitIconSprite;
	}
	Vector3 ITargetToLabelAPI.LabelWorldPosition()
	{
		return ThisMovement.CurrentPosition;
	}
	Color ITargetToLabelAPI.GetLabelAccentColor()
	{
		return Faction.FactionColor;
	}
	Color ITargetToLabelAPI.GetLabelTextColor()
	{
		return Color.black;
	}

	#endregion

	#region ITargetForCardAPI
	Sprite ITargetToCardAPI.GetCardImage()
	{
		return InstanceData.unitFullBodySprite;
	}
	string ITargetToCardAPI.GetCardName()
	{
		return InstanceData.displayName;
	}
	#endregion

	#region IUnitForPanel
	string IUnitToPanelAPI.GetFactionName()
	{
		return Faction.IsNullRef() ? "중립" : Faction.FactionName;
	}
	public (int[] values, int total, int max) GetShieldDetailValue()
	{
		int max = ThisStatsValue.GetStatsValue(StatsType.시설_내구도_최대);
		int total = ThisStatsValue.GetStatsValue(StatsType.시설_내구도_현재);
		int[] values = new int[1];
		values[0] = ThisStatsValue.GetStatsValue(StatsType.시설_내구도_현재);
		return (values, total, max);
	}
	public (int total, int max) GetShieldSimpleValue()
	{
		int max = ThisStatsValue.GetStatsValue(StatsType.시설_내구도_최대);
		int total = ThisStatsValue.GetStatsValue(StatsType.시설_내구도_현재);
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
	#endregion
}