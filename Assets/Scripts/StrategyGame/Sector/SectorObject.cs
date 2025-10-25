using UnityEngine;

using static StrategyGamePlayData;

using SectorData = StrategyGamePlayData.SectorData;

public partial class SectorObject : MonoBehaviour 
{
	public void Init()
	{
		sectorColor = GetComponentInChildren<SectorColor>();
	}

	private SectorColor sectorColor;
}

public partial class SectorObject // Profile
{
	private SectorData.Profile profileData;
	private SectorData.Capture captureData;
	private SectorData.MainStats mainStatsData;
	private SectorData.Facilities facilitiesData;
	private SectorData.Support supportData;

	public StatsGroup facilitiesStatsGroup;
	public StatsGroup supportStatsGroup;

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
	public StatsGroup FacilitiesBuffGroup => facilitiesStatsGroup;
	public StatsGroup SupportBuffGroup => supportStatsGroup;


	public void Init(StrategyStartSetterData.SectorData data)
	{
		if (profileData == null) profileData = new SectorData.Profile(data.profileData);
		else profileData.SetData(data.profileData);

		if (captureData == null) captureData = new SectorData.Capture(new()
		{
			captureFactionID = -1,
			captureProgress = 1,
			captureTime = data.captureTime,
		});

		if (mainStatsData == null) mainStatsData = new SectorData.MainStats(data.mainStatsData);
		else mainStatsData.SetData(data.mainStatsData);

		if (facilitiesData == null) facilitiesData = new SectorData.Facilities(data.facilitiesStatsData);
		else facilitiesData.SetData(data.facilitiesStatsData);

		if (supportData == null) supportData = new SectorData.Support(data.supportStatsData);
		else supportData.SetData(data.supportStatsData);
	}

	public (int value, int max) GetDurability()
	{
		return (0,0);
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

	internal void OnStartFacilitiesConstruct(int slotIndex, string facilitiesKey)
	{
	}
	internal void OnFinishFacilitiesConstruct(int slotIndex, string facilitiesKey)
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
}