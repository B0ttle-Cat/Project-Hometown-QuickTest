using UnityEngine;

using static StrategyGamePlayData;

using ControlBaseData = StrategyGamePlayData.ControlBaseData;

public partial class ControlBase : MonoBehaviour 
{
	public void Init()
	{
		controlBaseColor = GetComponentInChildren<ControlBaseColor>();
	}

	private ControlBaseColor controlBaseColor;
}

public partial class ControlBase // Profile
{
	private ControlBaseData.Profile profileData;
	private ControlBaseData.Capture captureData;
	private ControlBaseData.MainStats mainStatsData;
	private ControlBaseData.Facilities facilitiesData;
	private ControlBaseData.Support supportData;


	public StatsGroup facilitiesStatsGroup;
	public StatsGroup supportStatsGroup;

	public string ControlBaseName => profileData.GetData().controlBaseName;


	public ControlBaseData.Profile Profile => profileData;
	public ControlBaseData.Capture Capture => captureData;
	public ControlBaseData.MainStats Stats => mainStatsData;
	public ControlBaseData.Facilities Facilities => facilitiesData;
	public ControlBaseData.Support Support => supportData;

	public ControlBaseData.Profile.Data ProfileData => profileData.GetData();
	public ControlBaseData.Capture.Data CaptureData => captureData.GetData();
	public ControlBaseData.MainStats.Data StatsData => mainStatsData.GetData();
	public ControlBaseData.Facilities.Data FacilitiesData => facilitiesData.GetData();
	public ControlBaseData.Support.Data SupportData => supportData.GetData();

	public StatsList MainStatsList => StatsData.GetStatsList();
	public StatsList MainBuffList => StatsData.GetBuffList();  
	public StatsGroup FacilitiesBuffGroup => facilitiesStatsGroup;
	public StatsGroup SupportBuffGroup => supportStatsGroup;


	public void Init(StrategyStartSetterData.ControlBaseData data)
	{
		if (profileData == null) profileData = new ControlBaseData.Profile(data.profileData);
		else profileData.SetData(data.profileData);

		if (captureData == null) captureData = new ControlBaseData.Capture(new()
		{
			captureFactionID = -1,
			captureProgress = 1,
			captureTime = data.captureTime,
		});

		if (mainStatsData == null) mainStatsData = new ControlBaseData.MainStats(data.mainStatsData);
		else mainStatsData.SetData(data.mainStatsData);

		if (facilitiesData == null) facilitiesData = new ControlBaseData.Facilities(data.facilitiesStatsData);
		else facilitiesData.SetData(data.facilitiesStatsData);

		if (supportData == null) supportData = new ControlBaseData.Support(data.supportStatsData);
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

	internal void OnStartFacilitiesInstall(int slotIndex, string facilitiesKey)
	{
	}
}
public partial class ControlBase // CaptureData
{
	//public float CaptureTime => captureData.GetData().captureTime;

	public void Init(StrategyStartSetterData.CaptureData data)
	{
		ControlBaseData.Capture.Data initData = new ()
		{
			 captureFactionID = StrategyManager.Collector.TryFindFaction(data.captureFaction, out var find) ? find.FactionID : -1,
			 captureProgress = data.captureProgress,
			 captureTime = captureData == null ? 0 : captureData.GetData().captureTime,
		};

		if (captureData == null) captureData = new ControlBaseData.Capture(initData);
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

public partial class ControlBase : IStrategyElement
{
	public bool IsInCollector { get; set; }

	public void InStrategyCollector()
	{
	}

	public void OutStrategyCollector()
	{
	}
}