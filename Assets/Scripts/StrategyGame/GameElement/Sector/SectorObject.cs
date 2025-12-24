using System;

using Sirenix.OdinInspector;

using StrategyManagerModule;

using UnityEngine;

using static StrategyGamePlayData;

//using static StrategyGamePlayData.SectorData;
//using static StrategyGamePlayData.SectorData.Support;


[RequireComponent(typeof(CameraVisibilityGroupInStrategy))]
public partial class SectorObject : MonoBehaviour
{
#if UNITY_EDITOR
	[ShowInInspector, ToggleGroup("EditData", GroupName = "EditData")]
	bool EditData { get; set; } = false;
#endif
	[SerializeField, ToggleGroup("EditData")]
	private SectorStatsData sectorStatsData;
	[SerializeField, ToggleGroup("EditData")]
	private SectorRuntimeData sectorRuntimeData;

	public SectorStatsData StatsData => sectorStatsData;
	public SectorRuntimeData RuntimeData => sectorRuntimeData;

	// 카메라에서 보이는지 판단하는 기능
	private CameraVisibilityGroup visibilityGroup;

	public void Awake()
	{
		visibilityGroup = GetComponent<CameraVisibilityGroupInStrategy>();
	}
	public void Init(in StrategyStartSetterData.SectorData data)
	{
		sectorStatsData = new SectorStatsData(data);
		sectorRuntimeData = new SectorRuntimeData(data);
	}
	public void Init(in StrategyStartSetterData.CaptureData data)
	{
		sectorRuntimeData.InitCaptureData(data);
	}
}
public partial class SectorObject // Getter
{
	public string SectorName => StatsData.SectorName;
	public Faction CaptureFaction => StrategyManager.Collector.Find<Faction>(CaptureFactionID);
	public int CaptureFactionID => RuntimeData.CaptureFactionID;
	public float CaptureProgress => RuntimeData.CaptureProgress;
}
public partial class SectorObject : IStrategyElement, IStrategyStartGame
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

public partial class SectorObject : IStatsValueControl , ISupplyStats
{
	public IStatsValueControl StatsValue => this;

	public Action<ISupplyStats> OnSupplyChange { get; set; }


	int IStatsValueControl.GetStatsValue(StatsType type)
	{
		int baseValue = type switch
		{
			StatsType.자원_인력_최대   => StatsData.CapacityPersonnel,
			StatsType.자원_인력_회복   => StatsData.RecoveryPersonnel,
			StatsType.자원_인력_현재   => RuntimeData.LocalPersonnel,

			StatsType.자원_재료_최대   => StatsData.CapacityMaterial,
			StatsType.자원_재료_회복   => StatsData.RecoveryMaterial,
			StatsType.자원_재료_현재   => RuntimeData.LocalMaterial,

			StatsType.자원_전력_최대   => StatsData.CapacityElectric,
			StatsType.자원_전력_회복   => StatsData.RecoveryElectric,
			StatsType.자원_전력_현재   => RuntimeData.LocalElectric,

			StatsType.시설_내구도_최대 => 0,
			StatsType.시설_내구도_회복 => 0,
			StatsType.시설_내구도_현재 => 0,

			_ => 0
		};

		return baseValue;
	}

	float IStatsValueControl.GetStatsValuePrecent(StatsType type)
	{
		float baseValue = type switch
		{
			StatsType.자원_인력_최대   => StatsData.CapacityPersonnel * 0.01f,
			StatsType.자원_인력_회복   => StatsData.RecoveryPersonnel * 0.01f,
			StatsType.자원_인력_현재   => RuntimeData.LocalPersonnel * 0.01f,

			StatsType.자원_재료_최대   => StatsData.CapacityMaterial * 0.01f,
			StatsType.자원_재료_회복   => StatsData.RecoveryMaterial * 0.01f,
			StatsType.자원_재료_현재   => RuntimeData.LocalMaterial * 0.01f,

			StatsType.자원_전력_최대   => StatsData.CapacityElectric * 0.01f,
			StatsType.자원_전력_회복   => StatsData.RecoveryElectric * 0.01f,
			StatsType.자원_전력_현재   => RuntimeData.LocalElectric * 0.01f,

			StatsType.시설_내구도_최대 => 0,
			StatsType.시설_내구도_회복 => 0,
			StatsType.시설_내구도_현재 => 0,

			_ => 0
		};
		return baseValue;
	}

	void IStatsValueControl.SetStatsValue(StatsType type, int value)
	{
		switch (type)
		{
			case StatsType.자원_인력_최대: StatsData.CapacityPersonnel = value; break;
			case StatsType.자원_인력_회복: StatsData.RecoveryPersonnel = value; break;
			case StatsType.자원_인력_현재: RuntimeData.LocalPersonnel = value; break;

			case StatsType.자원_재료_최대: StatsData.CapacityMaterial = value; break;
			case StatsType.자원_재료_회복: StatsData.RecoveryMaterial = value; break;
			case StatsType.자원_재료_현재: RuntimeData.LocalMaterial = value; break;

			case StatsType.자원_전력_최대: StatsData.CapacityElectric = value; break;
			case StatsType.자원_전력_회복: StatsData.RecoveryElectric = value; break;
			case StatsType.자원_전력_현재: RuntimeData.LocalElectric = value; break;

			case StatsType.시설_내구도_최대: break;
			case StatsType.시설_내구도_회복: break;
			case StatsType.시설_내구도_현재: break;
			default: break;
		}
	}

	void IStatsValueControl.SetStatsValuePrecent(StatsType type, float valuePercent)
	{
		switch (type)
		{
			case StatsType.자원_인력_최대: StatsData.CapacityPersonnel = Mathf.FloorToInt(valuePercent * 100); break;
			case StatsType.자원_인력_회복: StatsData.RecoveryPersonnel = Mathf.FloorToInt(valuePercent * 100); break;
			case StatsType.자원_인력_현재: RuntimeData.LocalPersonnel = Mathf.FloorToInt(valuePercent * 100); break;

			case StatsType.자원_재료_최대: StatsData.CapacityMaterial = Mathf.FloorToInt(valuePercent * 100); break;
			case StatsType.자원_재료_회복: StatsData.RecoveryMaterial = Mathf.FloorToInt(valuePercent * 100); break;
			case StatsType.자원_재료_현재: RuntimeData.LocalMaterial = Mathf.FloorToInt(valuePercent * 100); break;

			case StatsType.자원_전력_최대: StatsData.CapacityElectric = Mathf.FloorToInt(valuePercent * 100); break;
			case StatsType.자원_전력_회복: StatsData.RecoveryElectric = Mathf.FloorToInt(valuePercent * 100); break;
			case StatsType.자원_전력_현재: RuntimeData.LocalElectric = Mathf.FloorToInt(valuePercent * 100); break;

			case StatsType.시설_내구도_최대: break;
			case StatsType.시설_내구도_회복: break;
			case StatsType.시설_내구도_현재: break;
			default: break;
		}
	}
}