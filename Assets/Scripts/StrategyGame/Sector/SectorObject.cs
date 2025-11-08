using UnityEngine;

using static StrategyGamePlayData;

using SectorData = StrategyGamePlayData.SectorData;

[RequireComponent(typeof(CameraVisibilityGroupInStrategy))]
public partial class SectorObject : MonoBehaviour
{
	private SectorData.Profile profile;
	private SectorData.Capture capture;
	private SectorData.MainStats mainStats;
	private SectorData.Facilities facilities;
	private SectorData.Support support;
	private SectorData.SpawnTroop spawnTroop;

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
	public void Init(in StrategyStartSetterData.SectorData data)
	{
		if (profile == null) profile = new SectorData.Profile(data.profileData.Copy());
		else profile.SetData(data.profileData.Copy());

		if (capture == null) capture = new SectorData.Capture(new()
		{
			captureFactionID = -1,
			captureProgress = 1,
			captureTime = data.captureTime,
		});

		if (mainStats == null) mainStats = new SectorData.MainStats(data.mainStatsData.Copy());
		else mainStats.SetData(data.mainStatsData.Copy());

		if (facilities == null) facilities = new SectorData.Facilities(data.facilitiesStatsData.Copy());
		else facilities.SetData(data.facilitiesStatsData.Copy());

		if (support == null) support = new SectorData.Support(data.supportStatsData.Copy());
		else support.SetData(data.supportStatsData.Copy());
	}
	public void Init(in StrategyStartSetterData.CaptureData data)
	{
		SectorData.Capture.Data initData = new ()
		{
			captureFactionID = StrategyManager.Collector.TryFindFaction(data.captureFaction, out var find) ? find.FactionID : -1,
			captureProgress = data.captureProgress,

			captureTime = capture == null ? 0 :  CaptureData.captureTime,
		};

		if (capture == null) capture = new SectorData.Capture(initData);
		else capture.SetData(initData);
	}
}
public partial class SectorObject // Getter
{
	public SectorData.Profile Profile => profile;
	public SectorData.Capture Capture => capture;
	public SectorData.MainStats Stats => mainStats;
	public SectorData.Facilities Facilities => facilities;
	public SectorData.Support Support => support;
	public SectorData.SpawnTroop SpawnTroop => spawnTroop;
	public ref readonly SectorData.Profile.Data ProfileData => ref profile.ReadonlyData();
	public ref readonly SectorData.Capture.Data CaptureData => ref capture.ReadonlyData();
	public ref readonly SectorData.MainStats.Data StatsData => ref mainStats.ReadonlyData();
	public ref readonly SectorData.Facilities.Data FacilitiesData => ref facilities.ReadonlyData();
	public ref readonly SectorData.Support.Data SupportData => ref support.ReadonlyData();
	public ref readonly SectorData.SpawnTroop.Data SpawnTroopData => ref spawnTroop.ReadonlyData();

	public StatsList MainStatsList => StatsData.GetStatsList();
	public StatsGroup FacilitiesBuffGroup => facilitiesStatsGroup ??= new StatsGroup();
	public StatsGroup SupportBuffGroup => supportStatsGroup ??= new StatsGroup();
	public StatsGroup StatusEffectStatsGroup => statusEffectStatsGroup ??= new StatsGroup();

	public string SectorName => ProfileData.sectorName;
	public Faction CaptureFaction => StrategyManager.Collector.FindFaction(CaptureFactionID);
	public int CaptureFactionID => CaptureData.captureFactionID;
	public float CaptureProgress => CaptureData.captureProgress;

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
		ref var data = ref capture.RefData();
		data.captureFactionID = factionID;
		data.captureProgress = progress;
		capture.Invoke();
	}
}
public partial class SectorObject : IStrategyElement
{
	public IStrategyElement ThisElement => this;
	public bool IsInCollector { get; set; }
    int IStrategyElement.ID { get; set; }

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