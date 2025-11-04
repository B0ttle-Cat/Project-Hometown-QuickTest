using UnityEngine;

using static StrategyGamePlayData;

using SectorData = StrategyGamePlayData.SectorData;

public partial class SectorObject : MonoBehaviour
{
	private SectorData.Profile profileData;
	private SectorData.Capture captureData;
	private SectorData.MainStats mainStatsData;
	private SectorData.Facilities facilitiesData;
	private SectorData.Support supportData;

	public StatsGroup facilitiesStatsGroup;
	public StatsGroup supportStatsGroup;
	public StatsGroup effectStatsGroup;

	public CameraVisibilityGroup visibilityGroup;

	public string SectorName => profileData.GetData().sectorName;

	public SectorData.Profile Profile => profileData;
	public SectorData.Capture Capture => captureData;
	public SectorData.MainStats Stats => mainStatsData;
	public SectorData.Facilities Facilities => facilitiesData;
	public SectorData.Support Support => supportData;

	public SectorData.Profile.Data ProfileData => profileData.GetData();
	public SectorData.Capture.Data CaptureData => captureData.GetData();
	public SectorData.MainStats.Data StatsData => mainStatsData.GetData();
	public SectorData.Facilities.Data FacilitiesData => facilitiesData.GetData();
	public SectorData.Support.Data SupportData => supportData.GetData();

	public StatsList MainStatsList => StatsData.GetStatsList();
	public StatsGroup FacilitiesBuffGroup => facilitiesStatsGroup ??= new StatsGroup();
	public StatsGroup SupportBuffGroup => supportStatsGroup ??= new StatsGroup();
	public StatsGroup EffectStatsGroup => effectStatsGroup ??= new StatsGroup();

	public void Awake()
	{
		visibilityGroup = GetComponent<CameraVisibilityGroup>();
	}

	public void Init(StrategyStartSetterData.SectorData data)
	{
		if (profileData == null) profileData = new SectorData.Profile(data.profileData.Copy());
		else profileData.SetData(data.profileData.Copy());

		if (captureData == null) captureData = new SectorData.Capture(new()
		{
			captureFactionID = -1,
			captureProgress = 1,
			captureTime = data.captureTime,
		});

		if (mainStatsData == null) mainStatsData = new SectorData.MainStats(data.mainStatsData.Copy());
		else mainStatsData.SetData(data.mainStatsData.Copy());

		if (facilitiesData == null) facilitiesData = new SectorData.Facilities(data.facilitiesStatsData.Copy());
		else facilitiesData.SetData(data.facilitiesStatsData.Copy());

		if (supportData == null) supportData = new SectorData.Support(data.supportStatsData.Copy());
		else supportData.SetData(data.supportStatsData.Copy());
	}

	public (int value, int max) GetDurability()
	{
		return (0, 0);
	}
	public (int value, int max) GetGarrison()
	{
		return (0, 0);
	}
	public (int value, int max) GetMaterial()
	{
		return (0, 0);
	}
	public (int value, int max) GetElectric()
	{
		return (0, 0);
	}

}
public partial class SectorObject // SectorControl
{
    internal void OnShowDetail()
    {
		var gamePlayData = StrategyManager.GamePlayData;
		StrategyManager.GameUI.DetailsPanelUI.OpenUI();
		StrategyManager.GameUI.DetailsPanelUI.OnShowSectorDetail(this);
	}
	internal void OnStartFacilitiesConstruct(int slotIndex, string facilitiesKey)
	{
	}
	internal void OnFinishFacilitiesConstruct(int slotIndex, string facilitiesKey)
	{
	}

	internal void OnDeployCombatants()
	{
	}

	internal void OnConstructFacilities()
	{
	}

	internal void OnMoveTroops()
	{
	}

	internal void OnUseFacilitiesSkill()
	{
	}
}
public partial class SectorObject // CaptureData
{
	//public float CaptureTime => captureData.GetData().captureTime;
	public void Init(StrategyStartSetterData.CaptureData data)
	{
		SectorData.Capture.Data initData = new ()
		{
			captureFactionID = StrategyManager.Collector.TryFindFaction(data.captureFaction, out var find) ? find.FactionID : -1,
			captureProgress = data.captureProgress,
			captureTime = captureData == null ? 0 : captureData.GetData().captureTime,
		};

		if (captureData == null) captureData = new SectorData.Capture(initData);
		else captureData.SetData(initData);
	}
	public Faction CaptureFaction => StrategyManager.Collector.FindFaction(CaptureFactionID);
	public int CaptureFactionID => captureData.GetData().captureFactionID;
	public float CaptureProgress => captureData.GetData().captureProgress;
	public void SetCaptureData(int factionID, float progress)
	{
		var data= captureData.GetData();
		data.captureFactionID = factionID;
		data.captureProgress = progress;
		captureData.SetData(data);
	}
}
public partial class SectorObject : IStrategyElement
{
	public bool IsInCollector { get; set; }

	public void InStrategyCollector()
	{
	}

	public void OutStrategyCollector()
	{
	}

	void IStartGame.OnStartGame()
	{
	}

	void IStartGame.OnStopGame()
	{
	}
}

public partial class SectorObject : ISelectMouse
{
	public Vector3 clickCenter => visibilityGroup == null ? transform.position : visibilityGroup.VisibleWorldBounds.center;
	bool ISelectMouse.IsSelectMouse { get; set; }
	bool ISelectMouse.IsPointEnter { get; set; }
	Vector3 ISelectMouse.ClickCenter => clickCenter;


	void ISelectMouse.OnPointEnter()
	{
	}
	void ISelectMouse.OnPointExit()
	{

	}
	bool ISelectMouse.OnSelect()
	{
		return true;
	}
	bool ISelectMouse.OnDeselect()
	{
		return true;
	}
	void ISelectMouse.OnSingleSelect()
	{
		var sectorTargeting = StrategyManager.GameUI.MapPanelUI.SectorSelectTargeting;
		if (sectorTargeting == null) return;
		sectorTargeting.AddTarget(this);
	}

	void ISelectMouse.OnSingleDeselect()
	{
		var sectorTargeting = StrategyManager.GameUI.MapPanelUI.SectorSelectTargeting;
		if (sectorTargeting == null) return;
		sectorTargeting.RemoveTarget(this);
	}

	void ISelectMouse.OnFirstSelect()
	{
	}

	void ISelectMouse.OnLastDeselect()
	{
	}

	private void OnWillRenderObject()
	{

	}
}
