using System;
using System.Linq;

using Sirenix.OdinInspector;

using StrategyManagerModule;

using UnityEngine;

using static StrategyGamePlayData;

[Serializable]
public record SectorStatsData
{
	public string SectorName;

	public bool IsEnableResourcesSupply;

	[FoldoutGroup("리소스", VisibleIf = "@IsEnableResourcesSupply")]
#if UNITY_EDITOR
	[HorizontalGroup("리소스/인력", width: 40), HideLabel, DisplayAsString, EnableGUI, ShowInInspector]
	private const string PersonnelString = "인력: ";
	[HorizontalGroup("리소스/재료", width: 40), HideLabel, DisplayAsString, EnableGUI, ShowInInspector]
	private const string MaterialString = "재료: ";
	[HorizontalGroup("리소스/전력", width: 40), HideLabel, DisplayAsString, EnableGUI, ShowInInspector]
	private const string ElectricString = "전력: ";
#endif
	[HorizontalGroup("리소스/인력"), HideLabel, SuffixLabel("최대 수용량", Overlay = true)]
	public int CapacityPersonnel;
	[HorizontalGroup("리소스/인력"), HideLabel, SuffixLabel("분당 회복량", Overlay = true)]
	public int RecoveryPersonnel;
	[HorizontalGroup("리소스/재료"), HideLabel, SuffixLabel("최대 수용량", Overlay = true)]
	public int CapacityMaterial;
	[HorizontalGroup("리소스/재료"), HideLabel, SuffixLabel("분당 회복량", Overlay = true)]
	public int RecoveryMaterial;
	[HorizontalGroup("리소스/전력"), HideLabel, SuffixLabel("최대 수용량", Overlay = true)]
	public int CapacityElectric;
	[HorizontalGroup("리소스/전력"), HideLabel, SuffixLabel("분당 회복량", Overlay = true)]
	public int RecoveryElectric;


	[FoldoutGroup("리소스")]
	[LabelText("회복 및 분배 주기(초)")]
	public float CycleTime;

	[FoldoutGroup("리소스")]
	[LabelText("기본 분배 거리")]
	public int DistributionDepth;

	[FoldoutGroup("Faction보너스리소스", GroupName = "이 구역을 점령시 증가하는 세력의 최대 수용량")]
	[LabelText("인력 수용량")]
	public int MaxPersonnelCapacityBonusOfFaction;

	[FoldoutGroup("Faction보너스리소스")]
	[LabelText("재료 수용량")]
	public int MaxMaterialCapacityBonusOfFaction;

	[FoldoutGroup("Faction보너스리소스")]
	[LabelText("전력 수용량")]
	public int MaxElectricCapacityBonusOfFaction;

	[LabelText("구역 확보에 필요한 시간")]
	public float CaptureTimeRequired;
	[LabelText("구역 환경 키")]
	public EnvironmentalKey EnvironmentalKey;
	[LabelText("구역 영구 상태이상")]
	public StatusEffectsFlag PermanentStatus;

	public SectorStatsData(StrategyStartSetterData.SectorData data)
	{
		this.SectorName = data.SectorName;
		this.IsEnableResourcesSupply = data.EnableResourcesSupply;
		this.CapacityPersonnel = data.CapacityPersonnel;
		this.RecoveryPersonnel = data.RecoveryPersonnel;
		this.CapacityMaterial = data.CapacityMaterial;
		this.RecoveryMaterial = data.RecoveryMaterial;
		this.CapacityElectric = data.CapacityElectric;
		this.RecoveryElectric = data.RecoveryElectric;
		this.CycleTime = data.CycleTime;
		this.DistributionDepth = data.DistributionDepth;
		this.MaxPersonnelCapacityBonusOfFaction = data.MaxPersonnelCapacityBonusOfFaction;
		this.MaxMaterialCapacityBonusOfFaction = data.MaxMaterialCapacityBonusOfFaction;
		this.MaxElectricCapacityBonusOfFaction = data.MaxElectricCapacityBonusOfFaction;
		this.CaptureTimeRequired = data.CaptureTimeRequired;
		this.EnvironmentalKey = data.EnvironmentalKey;
		this.PermanentStatus = data.PermanentStatus;
	}
}

[Serializable]
public record SectorRuntimeData
{
	[BoxGroup("현지 리소스")]
	[HorizontalGroup("현지 리소스/H")]
	[HideLabel, SuffixLabel("인력량", Overlay = true)]
	public int LocalPersonnel;
	[HorizontalGroup("현지 리소스/H")]
	[HideLabel, SuffixLabel("재료량", Overlay = true)]
	public int LocalMaterial;
	[HorizontalGroup("현지 리소스/H")]
	[HideLabel, SuffixLabel("전력량", Overlay = true)]
	public int LocalElectric;
	[BoxGroup("현지 리소스")]
	[LabelText("추가 분배 거리")]
	public int DistributionDepth;

	[LabelText("상태이상")]
	public StatusEffectsFlag Status;

	[SerializeField]
	[FoldoutGroup("점령 정보"), InlineProperty, HideLabel]
	private Capture capture;
	[SerializeField]
	[FoldoutGroup("시설물 정보"), ListDrawerSettings(ShowFoldout = false)]
	private FacilityInfo[] facilitiesInfo;
	[SerializeField]
	[FoldoutGroup("지원 점수 정보"), InlineProperty, HideLabel]
	private Support support;

	public SectorRuntimeData()
	{
		capture = new();
		facilitiesInfo = new FacilityInfo[0];
		support = new();
	}

	[Serializable]
	public record Capture
	{
		public int CaptureFactionID;
		public float CaptureProgress;

		public Capture()
		{
			CaptureFactionID = -1;
			CaptureProgress = 0f;
		}
	}
	public int CaptureFactionID { get { return capture.CaptureFactionID; } set { capture.CaptureFactionID = value; } }
	public float CaptureProgress { get { return capture.CaptureProgress; } set { capture.CaptureProgress = value; } }
	[Serializable]
	public record FacilityInfo
	{
		[HorizontalGroup, HideLabel]
		public FacilityKey FacilityKey;
		[HorizontalGroup, HideLabel, Range(0f,1f)]
		public float ConstructionProgress;
		public FacilityInfo()
		{
			FacilityKey = FacilityKey.None;
			ConstructionProgress = 0;
		}
	}
	public FacilityInfo[] FacilitiesInfo { get { return facilitiesInfo; } set { facilitiesInfo = value; } }
	public int FacilitiesCount => facilitiesInfo == null ? 0 : facilitiesInfo.Length;
	[Serializable]
	public record Support
	{
		public int remainingPoint;

		public int offensivePoint;
		public int defensivePoint;
		public int supplyPoint;
		public int facilityPoint;

		public Support()
		{
			remainingPoint = 0;
			offensivePoint = 0;
			defensivePoint = 0;
			supplyPoint = 0;
			facilityPoint = 0;
		}
	}
	public int RemainingPoint { get { return support.remainingPoint; } set { support.remainingPoint = value; } }
	public int OffensivePoint { get { return support.offensivePoint; } set { support.offensivePoint = value; } }
	public int DefensivePoint { get { return support.defensivePoint; } set { support.defensivePoint = value; } }
	public int SupplyPoint { get { return support.supplyPoint; } set { support.supplyPoint = value; } }
	public int FacilityPoint { get { return support.facilityPoint; } set { support.facilityPoint = value; } }

	public SectorRuntimeData(StrategyStartSetterData.SectorData data)
	{
		this.LocalPersonnel = data.LocalPersonnel;
		this.LocalMaterial = data.LocalMaterial;
		this.LocalElectric = data.LocalElectric;
		this.Status = data.DynamicStatus;
		this.capture = new Capture();
		this.facilitiesInfo = data.facilitiesInfo == null
			? new FacilityInfo[0]
			: data.facilitiesInfo.Select(id => new FacilityInfo
			{
				ConstructionProgress = id.ConstructionProgress,
				FacilityKey = id.FacilityKey
			}).ToArray();
		this.support = new Support
		{
			remainingPoint = data.remainingPoint,
			offensivePoint = data.offensivePoint,
			defensivePoint = data.defensivePoint,
			supplyPoint = data.supplyPoint,
			facilityPoint = data.facilityPoint
		};
	}
	public void InitCaptureData(StrategyStartSetterData.CaptureData data)
	{
		CaptureFactionID = data.captureFactionID;
		CaptureProgress = data.captureProgress;
	}
}