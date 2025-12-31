using System;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;

using static StrategyGamePlayData;

[Serializable]
public class FactionStatsData
{
	public string FactionName;
	public int FactionID;
	public Color FactionColor;
	public Sprite FactionIcon;

	[BoxGroup("세력 리소스")]
	public bool EnableResourcesSupply;
	[BoxGroup("세력 리소스/인력")]
	[HorizontalGroup("세력 리소스/인력/H"), HideLabel,SuffixLabel("최대 수용량", Overlay = true)]
	public int CapacityPersonnel;
	[HorizontalGroup("세력 리소스/인력/H"), HideLabel,SuffixLabel("분당 회복량", Overlay = true)]
	public int RecoveryPersonnel;

	[BoxGroup("세력 리소스/재료")]
	[HorizontalGroup("세력 리소스/재료/H"), HideLabel,SuffixLabel("최대 수용량", Overlay = true)]
	public int CapacityMaterial;
	[HorizontalGroup("세력 리소스/재료/H"), HideLabel,SuffixLabel("분당 회복량", Overlay = true)]
	public int RecoveryMaterial;

	[BoxGroup("세력 리소스/전력")]
	[HorizontalGroup("세력 리소스/전력/H"), HideLabel,SuffixLabel("최대 수용량", Overlay = true)]
	public int CapacityElectric;
	[HorizontalGroup("세력 리소스/전력/H"), HideLabel,SuffixLabel("분당 회복량", Overlay = true)]
	public int RecoveryElectric;

	[FoldoutGroup("세력 리소스")]
	[LabelText("회복 및 분배 주기(초)")]
	public float CycleTime;

	[BoxGroup("사용가능유닛"),SerializeField]
	public List<UnitKey> AvailableUnitKeyList;
}
[Serializable]
public class FactionRuntimeData
{
	[BoxGroup("세력리소스", GroupName ="세력이 보유한 현재 리소스")]
	[HorizontalGroup("세력리소스/H"), HideLabel,SuffixLabel("인력", Overlay = true)]
	public int CurrentPersonnel;

	[HorizontalGroup("세력리소스/H"), HideLabel,SuffixLabel("재료", Overlay = true)]
	public int CurrentMaterial;

	[HorizontalGroup("세력리소스/H"), HideLabel,SuffixLabel("전력", Overlay = true)]
	public int CurrentElectric;

	[FoldoutGroup("세력유지비", GroupName ="세력 유지비(최대 수량 및 회복량이 감소)")]
	[BoxGroup("세력유지비/병력")]
	[HorizontalGroup("세력유지비/병력/H"), HideLabel,SuffixLabel("병력에 배정된 인원", Overlay = true)]
	public int AssignedMilitaryPersonnel;

	[BoxGroup("세력유지비/시설")]
	[HorizontalGroup("세력유지비/시설/H"), HideLabel,SuffixLabel("시설에 배정된 인원", Overlay = true)]
	public int AssignedFacilitiesPersonnel;

	[BoxGroup("세력유지비/시설")]
	[HorizontalGroup("세력유지비/시설/H"), HideLabel,SuffixLabel("총 시설 유지비(재료)", Overlay = true)]
	public int MaintenanceCostFacilitiesMaterial;

	[BoxGroup("세력유지비/시설")]
	[HorizontalGroup("세력유지비/시설/H"), HideLabel,SuffixLabel("총 시설 유지비(전력)", Overlay = true)]
	public int MaintenanceCostFacilitiesElectric;

	[BoxGroup("사용가능유닛"),SerializeField]
	public List<UnitKey> DynamicAvailableUnitKeyList;

	public StatsList DynamicKeyStatsList;
}