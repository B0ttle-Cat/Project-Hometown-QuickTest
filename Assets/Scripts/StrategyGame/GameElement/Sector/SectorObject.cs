using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;

using static StrategyGamePlayData;
using static StrategyGamePlayData.SectorData;
using static StrategyGamePlayData.SectorData.Support;

using SectorData = StrategyGamePlayData.SectorData;

[RequireComponent(typeof(CameraVisibilityGroupInStrategy))]
public partial class SectorObject : MonoBehaviour
{
	[SerializeField, BoxGroup("Main")]
	private SectorData.Profile profile;
	[SerializeField, BoxGroup("Main")]
	private SectorData.MainStats mainStats;
	[SerializeField, BoxGroup("Main")]
	private SectorData.Facilities facilities;
	[SerializeField, BoxGroup("Main")]
	private SectorData.Support support;
	[SerializeField, BoxGroup("Main")]
	private SectorData.Capture capture;
	[SerializeField, BoxGroup("Main")]
	private StatsGroup sectorStatsGroup;
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

		if (mainStats == null) mainStats = new SectorData.MainStats(data.mainStatsData.Copy());
		else mainStats.SetData(data.mainStatsData.Copy());

		if (facilities == null) facilities = new SectorData.Facilities(data.facilitiesStatsData.Copy());
		else facilities.SetData(data.facilitiesStatsData.Copy());

		if (support == null) support = new SectorData.Support(data.supportStatsData.Copy());
		else support.SetData(data.supportStatsData.Copy());

		if (capture == null) capture = new SectorData.Capture(new()
		{
			captureFactionID = -1,
			captureProgress = 1,
			captureTime = data.captureTime,
		});
		InitStateGroup();
	}
	private void InitStateGroup()
	{
		sectorStatsGroup = new StatsGroup();
		ref readonly MainStats.Data mainData = ref mainStats.ReadonlyData();
		ref readonly Facilities.Data facilitiesData =  ref facilities.ReadonlyData();
		ref readonly Support.Data supportData = ref support.ReadonlyData();

		// 기본 스텟 설정
		SetStatsList_MainStats(mainData.GetStatsList());
		// 구조물 스텟 설정
		var slots = facilitiesData.slotData;
		int length = slots.Length;
        for (int i = 0 ; i < length ; i++)
        {
			var slot = slots[i];
			var facilitiesKey = slot.facilitiesKey;
			if (string.IsNullOrWhiteSpace(facilitiesKey)) continue;
			SetStatsList_Facilities(i, FindStatsList_Facilities(facilitiesKey));
		}

		// 지원 스텟 설정
		SetStatsList_Support(SupportType.Offensive, FindStatsList_Support(SupportType.Offensive, supportData.offensivePoint));
		SetStatsList_Support(SupportType.Defensive, FindStatsList_Support(SupportType.Defensive, supportData.defensivePoint));
		SetStatsList_Support(SupportType.Supply, FindStatsList_Support(SupportType.Supply, supportData.supplyPoint));
		SetStatsList_Support(SupportType.Facilities, FindStatsList_Support(SupportType.Facilities, supportData.facilitiesPoint));

		StatsList FindStatsList_Facilities(string facilitiesKey)
		{
			// TODO:: facilitiesKey 를 사용해 Table 에서 값 가져와기
			return null;
		}
		StatsList FindStatsList_Support(SupportType type, int point)
		{
			string key = $"{type}_{point}";
			// TODO:: key 를 사용해 Table 에서 값 가져와기
			return null;
		}
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
	public ref readonly SectorData.Profile.Data ProfileData => ref profile.ReadonlyData();
	public ref readonly SectorData.Capture.Data CaptureData => ref capture.ReadonlyData();
	public ref readonly SectorData.MainStats.Data StatsData => ref mainStats.ReadonlyData();
	public ref readonly SectorData.Facilities.Data FacilitiesData => ref facilities.ReadonlyData();
	public ref readonly SectorData.Support.Data SupportData => ref support.ReadonlyData();

	public StatsList CurrStatsList => ProfileData.GetStatsList();
	public StatsGroup SectorStatsGroup => sectorStatsGroup ??= new StatsGroup();
	public const string StatsGroupName_MainStats = "MainStats";
	public const string StatsGroupName_Facilities = "Facilities_";
	public const string StatsGroupName_Support = "Support_";
	public const string StatsGroupName_StatusEffect = "StatusEffect_";
	public bool TryGetStatsListInGroup(string groupName, out StatsList statsList)
	{
		return SectorStatsGroup.TryGetList(groupName, out statsList);
	}
	public void SetStatsListInGroup(string groupName, StatsList statsList)
	{
		SectorStatsGroup.SetList(groupName, statsList);
	}
	public List<string> GetStatsKeyListInGroup(string startsWith = "", string endsWith = "")
	{
		return SectorStatsGroup.GetkeyList(startsWith, endsWith);
	}
	public bool TryGetStatsList_MainStats( out StatsList statsList)
	{
		return TryGetStatsListInGroup(StatsGroupName_MainStats, out statsList);
	}
	public bool TryGetStatsList_Facilities(int slotIndex, out StatsList statsList)
	{
		return TryGetStatsListInGroup($"{StatsGroupName_Facilities}{slotIndex}", out statsList);
	}
	public bool TryGetStatsList_Support(SupportType supportType, out StatsList statsList)
	{
		return TryGetStatsListInGroup($"{StatsGroupName_Support}{supportType}", out statsList);
	}
	public bool TryGetStatsList_StatusEffec(string effectKey,out StatsList statsList)
	{
		return TryGetStatsListInGroup($"{StatsGroupName_StatusEffect}{effectKey}", out statsList);
	}
	private void SetStatsList_MainStats(StatsList statsList)
	{
		SetStatsListInGroup(StatsGroupName_MainStats, statsList);
	}
	private void SetStatsList_Facilities(int slotIndex, StatsList statsList)
	{
		SetStatsListInGroup($"{StatsGroupName_Facilities}{slotIndex}", statsList);
	}
	private void SetStatsList_Support(SupportType supportType, StatsList statsList)
	{
		SetStatsListInGroup($"{StatsGroupName_Support}{supportType}", statsList);
	}
	private void SetStatsList_StatusEffec(string effectKey, StatsList statsList)
	{
		SetStatsListInGroup($"{StatsGroupName_StatusEffect}{effectKey}", statsList);
	}
	public string SectorName => ProfileData.sectorName;
	public Faction CaptureFaction => StrategyManager.Collector.FindFaction(CaptureFactionID);
	public int CaptureFactionID => CaptureData.captureFactionID;
	public float CaptureProgress => CaptureData.captureProgress;

	public (int value, int max) GetDurability()
	{
		const StatsType CurrType = StatsType.거점_내구도_현재;
		const StatsType MaxType = StatsType.거점_내구도_최대;
		//const StatsType supplyType = StatsType.거점_인력_회복;

		var currMain = CurrStatsList.GetValue(statsType: CurrType);
		var maxMain = SectorStatsGroup.GetValue(MaxType);
		//var supplyMain = CurrStatsList.GetValue(supplyType);

		return (currMain.Value, maxMain.Value);
	}
	public (int value, int max) GetManpower()
	{
		const StatsType CurrType = StatsType.거점_인력_현재;
		const StatsType MaxType = StatsType.거점_인력_최대;
		//const StatsType supplyType = StatsType.거점_인력_회복;

		var currMain = CurrStatsList.GetValue(statsType: CurrType);
		var maxMain = SectorStatsGroup.GetValue(MaxType);
		//var supplyMain = CurrStatsList.GetValue(supplyType);

		return (currMain.Value, maxMain.Value);
	}
	public (int value, int max, int supply) GetMaterial()
	{
		const StatsType CurrType = StatsType.거점_재료_현재;
		const StatsType MaxType = StatsType.거점_재료_최대;
		const StatsType SupplyType = StatsType.거점_재료_회복;

		var currMain = CurrStatsList.GetValue(statsType: CurrType);
		var maxMain = SectorStatsGroup.GetValue(MaxType);
		var supplyMain = SectorStatsGroup.GetValue(SupplyType);

		return (currMain.Value, maxMain.Value, supplyMain.Value);
	}
	public (int value, int max, int supply) GetElectric()
	{
		const StatsType CurrType = StatsType.거점_전력_현재;
		const StatsType MaxType = StatsType.거점_전력_최대;
		const StatsType SupplyType = StatsType.거점_전력_회복;

		var currMain = CurrStatsList.GetValue(statsType: CurrType);
		var maxMain = SectorStatsGroup.GetValue(MaxType);
		var supplyMain = SectorStatsGroup.GetValue(SupplyType);

		return (currMain.Value, maxMain.Value, supplyMain.Value);
	}
	public void SetCaptureData(int factionID, float progress)
	{
		ref var data = ref capture.RefData();
		data.captureFactionID = factionID;
		data.captureProgress = progress;
		capture.Invoke();
	}
	public void SetManpower(int value)
	{
		CurrStatsList.SetValue(StatsType.거점_인력_현재, value);
	}
	public void SetMaterial(int value)
	{
		CurrStatsList.SetValue(StatsType.거점_재료_현재, value);
	}
	public void SetElectric(int value)
	{
		CurrStatsList.SetValue(StatsType.거점_전력_현재, value);
	}
}
public partial class SectorObject : IStrategyElement
{
	public IStrategyElement ThisElement => this;
	public bool IsInCollector { get; set; }
    int IStrategyElement.ID { get; set; }
	public int SectorID => ThisElement.ID;

	public void InStrategyCollector()
	{
	}

	public void OutStrategyCollector()
	{
	}

	void IStrategyStartGame.OnStartGame()
	{
	}

	void IStrategyStartGame.OnStopGame()
	{
	}
}

public partial class SectorObject // ThisRendering
{
}