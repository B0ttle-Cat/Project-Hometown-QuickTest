using Sirenix.OdinInspector;

using StrategyManagerModule;

using UnityEngine;

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