using System;

using Sirenix.OdinInspector;

using static StrategyGamePlayData;

[Serializable]
public record SectorStatsData
{
	public string SectorName;

	[LabelText("최대 내구도")]
	public int MaxDurability;

	[BoxGroup("Personnel 인력")]
	[HorizontalGroup("Personnel/H"), HideLabel,SuffixLabel("최대 한도", Overlay = true)]
	public int MaxPersonnel;
	[HorizontalGroup("Personnel/H"), HideLabel,SuffixLabel("획득 점수", Overlay = true)]
	public int PointPersonnel;

	[BoxGroup("Material 재료")]
	[HorizontalGroup("Material/H"), HideLabel,SuffixLabel("최대 한도", Overlay = true)]
	public int MaxMaterial;
	[HorizontalGroup("Material/H"), HideLabel,SuffixLabel("획득 점수", Overlay = true)]
	public int PointMaterial;

	[BoxGroup("Electric 전력")]
	[HorizontalGroup("Electric/H"), HideLabel,SuffixLabel("최대 한도", Overlay = true)]
	public int MaxElectric;
	[HorizontalGroup("Electric/H"), HideLabel,SuffixLabel("획득 점수", Overlay = true)]
	public int PointElectric;

	[LabelText("구역 환경 키")]
	public EnvironmentalKey EnvironmentalKey;
	[LabelText("영구 적용 효과")]
	public StatusEffectsFlag PermanentStatus;

	public float CaptureTimeRequired;

	public int FacilitieSlotCount;
}

[Serializable]
public record SectorRuntimeData
{
	[LabelText("현재 내구도")]
	public int CurrentDurability;

	[LabelText("현재 인력")]
	public int CurrentPersonnel;
	[LabelText("현재 재료")]
	public int CurrentMaterial;
	[LabelText("현재 전력")]
	public int CurrentElectric;
	[LabelText("상태이상")]
	public StatusEffectsFlag Status;

	private Capture capture;
	private Facilities facilities;
	private Support support;

	public Capture GetCapture;
	public Facilities GetFacilities;
	public Support GetSupport;


	public SectorRuntimeData()
	{
		capture = new();
		facilities = new Facilities();
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
	public int CaptureFactionID { get { return capture.CaptureFactionID;} set { capture.CaptureFactionID = value; } }
	public float CaptureProgress { get { return capture.CaptureProgress; } set { capture.CaptureProgress = value; } }
	[Serializable]
	public record Facilities
	{
		public FacilityKey[] facilityKey;
		public float[] progress;

		public (FacilityKey Key,float Progress) this[int index]
		{
			get
			{
				return (facilityKey[index], progress[index]);
			}
			set 
			{
				(facilityKey[index], progress[index]) = value;
			}
		}
		public Facilities()
		{
			facilityKey = new FacilityKey[0];
			progress = new float [0];
		}
	}
	public (FacilityKey Key, float Progress) FacilityKey(int index) => facilities[index];
	public void SetFacility(int index, FacilityKey key, float progress) => facilities[index] = (key,progress);
	public void SetFacilityKey(int index, FacilityKey key) => facilities[index] = (key, facilities[index].Progress);
	public void SetFacilityProgress(int index, float progress) => facilities[index] = (facilities[index].Key, progress);

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
}