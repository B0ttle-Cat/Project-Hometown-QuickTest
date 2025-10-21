using UnityEngine;

using static StrategyGamePlayData;

using ControlBaseData = StrategyGamePlayData.ControlBaseData;

public partial class ControlBase : MonoBehaviour
{
	public void Init()
	{
		controlBaseCapture = GetComponentInChildren<ControlBaseCapture>();
		controlBaseColor = GetComponentInChildren<ControlBaseColor>();
	}

	private ControlBaseCapture controlBaseCapture;
	private ControlBaseColor controlBaseColor;

	// Update is called once per frame
	public void UpdateControlBase()
	{
		UpdateCapture();
		UpdateColor();

		void UpdateCapture()
		{
			if (controlBaseCapture == null) return;
			controlBaseCapture.UpdateCapture(this);
		}
		void UpdateColor()
		{
			if (controlBaseColor == null) return;
			controlBaseColor.UpdateColor(CaptureFaction, CaptureProgress);
		}
	}
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

	public int GetMaxDurability()
	{
		return 0;
	}

	public int GetDurability()
	{
		return 0;
	}

	public int GetMaxGarrison()
	{
		return 0;
	}

	public int GetGarrison()
	{
		return 0;
	}
	public int GetMaxMaterial()
	{
		return 0;
	}

	public int GetMaterial()
	{
		return 0;
	}
	public int GetMaxElectric()
	{
		return 0;
	}

	public int GetElectric()
	{
		return 0;
	}
}
public partial class ControlBase // CaptureData
{
	public float CaptureTime => captureData.GetData().captureTime;

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

		if (controlBaseCapture == null) controlBaseCapture = GetComponent<ControlBaseCapture>();
		if (controlBaseCapture != null) controlBaseCapture.SetCapture(CaptureFactionID);
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