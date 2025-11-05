using UnityEngine;

using static StrategyGamePlayData;

using SectorData = StrategyGamePlayData.SectorData;

[RequireComponent(typeof(CameraVisibilityGroupInStrategy))]
public partial class SectorObject : MonoBehaviour
{
	private SectorData.Profile profileData;
	private SectorData.Capture captureData;
	private SectorData.MainStats mainStatsData;
	private SectorData.Facilities facilitiesData;
	private SectorData.Support supportData;

	// 시설물에 대한 스텟
	private StatsGroup facilitiesStatsGroup;
	// 지원 정책에 대한 스텟
	private StatsGroup supportStatsGroup;
	// 상태 이상에 대한 스텟
	private StatsGroup statusEffectStatsGroup;
	// 카메라에서 보이는지 판단하는 기능
	private CameraVisibilityGroup visibilityGroup;

	public void Awake()
	{
		visibilityGroup = GetComponent<CameraVisibilityGroupInStrategy>();
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
}
public partial class SectorObject // Getter
{
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
	public StatsGroup StatusEffectStatsGroup => statusEffectStatsGroup ??= new StatsGroup();

	public string SectorName => profileData.GetData().sectorName;
	public Faction CaptureFaction => StrategyManager.Collector.FindFaction(CaptureFactionID);
	public int CaptureFactionID => captureData.GetData().captureFactionID;
	public float CaptureProgress => captureData.GetData().captureProgress;

	public (int value, int max) GetDurability()
	{
		return (0, 0);
	}
	public (int value, int max) GetTroops()
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
