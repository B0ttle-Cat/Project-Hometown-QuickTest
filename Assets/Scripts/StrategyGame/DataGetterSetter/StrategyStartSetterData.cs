using System;
using System.Collections.Generic;
using System.Linq;

using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

using UnityEngine;

using static SectorRuntimeData;
using static StrategyGamePlayData;
using static StrategyManagerModule.StrategyMissionTree;

namespace StrategyManagerModule
{
	[CreateAssetMenu(fileName = "StrategyStartSetterData", menuName = "Scriptable Objects/StrategyGame/StrategyStartSetterData")]
	public class StrategyStartSetterData : DataGetterSetter<StrategyStartSetterData.Data>
	{
#if UNITY_EDITOR
		[ShowInInspector, ToggleGroup("onShowGizmo", "GizmoOption", Order = -99)]
		public bool onShowGizmo { get; set; } = false;
		[ShowInInspector, ToggleGroup("onShowGizmo")]
		public bool onShowSectorLink { get; set; } = true;
		[ShowInInspector, ToggleGroup("onShowGizmo")]
		public bool onShowUnitPreview { get; set; } = true;
#endif
		[Serializable]
		public struct Data
		{
			[ValueDropdown("GetFactionName")]
			public string playerFactionName;

			public double unscaleGamePlayTime;
			public double gamePlayTime;

			public Overview overview;
			public Mission mission;

			[TitleGroup("MainData")]
			public FactionData[] factionDatas;
			[TitleGroup("MainData")]
			public SectorData[] sectorDatas;
			[TitleGroup("MainData")]
			public UnitData[] unitDatas;
			[TitleGroup("MainData")]
			public OperationData[] operationDatas;
			[TitleGroup("PoolingData")]
			public ProjectileData[] projectileDatas;

			[TitleGroup("OtherData"), TableList]
			public CaptureData[] captureDatas;
			[TitleGroup("OtherData")]
			public SectorLinkData[] sectorLinkDatas;
			[TitleGroup("OtherData")]
			public FactionRelation[] factionRelations;
#if UNITY_EDITOR
			private IEnumerable<string> GetFactionName()
			{
				if (factionDatas == null || factionDatas.Length == 0)
					return new[] { "(No SectorData)" };

				return factionDatas.Select(x => x.FactionName);
			}
#endif
		}
		[Serializable]
		public struct FactionData
		{
			[FoldoutGroup("@FactionName")]
			public string FactionName;
			[FoldoutGroup("@FactionName")]
			public Color FactionColor;
			[FoldoutGroup("@FactionName")]
			public Sprite FactionIcon;

			[BoxGroup("@FactionName/세력 리소스")]
			public bool EnableResourcesSupply;

			[BoxGroup("@FactionName/세력 리소스/인력", VisibleIf = "@EnableResourcesSupply")]
			[HorizontalGroup("@FactionName/세력 리소스/인력/H"), HideLabel, SuffixLabel("최대 수용량", Overlay = true)]
			public int CapacityPersonnel;
			[HorizontalGroup("@FactionName/세력 리소스/인력/H"), HideLabel, SuffixLabel("분당 회복량", Overlay = true)]
			public int RecoveryPersonnel;
			[HorizontalGroup("@FactionName/세력 리소스/인력/H"), HideLabel, SuffixLabel("현재 보유량", Overlay = true)]
			public int CurrentPersonnel;

			[BoxGroup("@FactionName/세력 리소스/재료", VisibleIf = "@EnableResourcesSupply")]
			[HorizontalGroup("@FactionName/세력 리소스/재료/H"), HideLabel, SuffixLabel("최대 수용량", Overlay = true)]
			public int CapacityMaterial;
			[HorizontalGroup("@FactionName/세력 리소스/재료/H"), HideLabel, SuffixLabel("분당 회복량", Overlay = true)]
			public int RecoveryMaterial;
			[HorizontalGroup("@FactionName/세력 리소스/재료/H"), HideLabel, SuffixLabel("현재 보유량", Overlay = true)]
			public int CurrentMaterial;

			[BoxGroup("@FactionName/세력 리소스/전력", VisibleIf = "@EnableResourcesSupply")]
			[HorizontalGroup("@FactionName/세력 리소스/전력/H"), HideLabel, SuffixLabel("최대 수용량", Overlay = true)]
			public int CapacityElectric;
			[HorizontalGroup("@FactionName/세력 리소스/전력/H"), HideLabel, SuffixLabel("분당 회복량", Overlay = true)]
			public int RecoveryElectric;
			[HorizontalGroup("@FactionName/세력 리소스/전력/H"), HideLabel, SuffixLabel("현재 보유량", Overlay = true)]
			public int CurrentElectric;


			[FoldoutGroup("@FactionName"),SerializeField]
			private List<UnitKeySelecter> availableUnitKeyList;
			[Serializable]
			private struct UnitKeySelecter
			{
				[SerializeField, HorizontalGroup(20), ToggleLeft, HideLabel]
				private bool Range;
				[SerializeField, HorizontalGroup, HideLabel]
				private  UnitKey unitKey;
				[ShowIf("Range"), SerializeField, HorizontalGroup, LabelText(" ~ "), LabelWidth(20)]
				private  UnitKey endUnitKey;
				public List<UnitKey> GetUnitKeyList()
				{
					if (!Range)
					{
						return new List<UnitKey>() { unitKey };
					}
					// 모든 UnitKey를 선언 순서대로 가져옴
					var allKeys = Enum.GetValues(typeof(UnitKey)).Cast<UnitKey>().ToList();

					if (!Range)
						return new List<UnitKey> { unitKey };

					int startIndex = allKeys.IndexOf(unitKey);
					int endIndex = allKeys.IndexOf(endUnitKey);

					// 잘못된 입력 처리
					if (startIndex == -1 || endIndex == -1)
						return new List<UnitKey> { unitKey };

					// 순서가 반대일 수도 있으니 정렬 보정
					if (startIndex > endIndex)
						(startIndex, endIndex) = (endIndex, startIndex);

					// 범위 추출 (포함 범위)
					return allKeys.GetRange(startIndex, endIndex - startIndex + 1);
				}
			}
			public readonly List<UnitKey> AvailableUnitKeyList()
			{
				if (availableUnitKeyList == null) return new List<UnitKey>();
				return availableUnitKeyList.SelectMany(k => k.GetUnitKeyList()).Distinct().ToList();
			}
		}
		[Serializable]
		public struct SectorData
		{
			[FoldoutGroup("@SectorName"), SerializeField]
			[ValueDropdown("SectorObjectListInScene", AppendNextDrawer = true)]
			public string SectorName;

			[BoxGroup("@SectorName/Local리소스/인력")]
			[HorizontalGroup("@SectorName/Local리소스/인력/H"), HideLabel,SuffixLabel("최대 수용량", Overlay = true)]
			public int CapacityPersonnel;
			[HorizontalGroup("@SectorName/Local리소스/인력/H"), HideLabel,SuffixLabel("분당 회복량", Overlay = true)]
			public int RecoveryPersonnel;
			[HorizontalGroup("@SectorName/Local리소스/인력/H"), HideLabel,SuffixLabel("현지 보유량", Overlay = true)]
			public int LocalPersonnel;

			[BoxGroup("@SectorName/Local리소스/재료")]
			[HorizontalGroup("@SectorName/Local리소스/재료/H"), HideLabel,SuffixLabel("최대 수용량", Overlay = true)]
			public int CapacityMaterial;
			[HorizontalGroup("@SectorName/Local리소스/재료/H"), HideLabel,SuffixLabel("분당 회복량", Overlay = true)]
			public int RecoveryMaterial;
			[HorizontalGroup("@SectorName/Local리소스/재료/H"), HideLabel,SuffixLabel("현지 보유량", Overlay = true)]
			public int LocalMaterial;

			[BoxGroup("@SectorName/Local리소스/전력")]
			[HorizontalGroup("@SectorName/Local리소스/전력/H"), HideLabel,SuffixLabel("최대 수용량", Overlay = true)]
			public int CapacityElectric;
			[HorizontalGroup("@SectorName/Local리소스/전력/H"), HideLabel,SuffixLabel("분당 회복량", Overlay = true)]
			public int RecoveryElectric;
			[HorizontalGroup("@SectorName/Local리소스/전력/H"), HideLabel,SuffixLabel("현지 보유량", Overlay = true)]
			public int LocalElectric;

			[BoxGroup("@SectorName/Local리소스/분배 거리")]
			[HorizontalGroup("@SectorName/Local리소스/분배 거리/H")]
			[LabelText("기본 거리")]
			public int DistributionDepth;
			[HorizontalGroup("@SectorName/Local리소스/분배 거리/H")]
			[LabelText("추가 거리")]
			public int DistributionAddDepth;
			[FoldoutGroup("@SectorName/Local리소스")]
			[LabelText("회복 및 분배 주기(초)")]
			public float CycleTime;

			[FoldoutGroup("@SectorName/Faction보너스리소스", GroupName = "이 구역을 점령시 증가하는 세력의 최대 수용량")]
			[HorizontalGroup("@SectorName/Faction보너스리소스/H"), HideLabel,SuffixLabel("인력 수용량", Overlay = true)]
			public int MaxPersonnelCapacityBonusOfFaction;

			[FoldoutGroup("@SectorName/Faction보너스리소스")]
			[HorizontalGroup("@SectorName/Faction보너스리소스/H"), HideLabel,SuffixLabel("재료 수용량", Overlay = true)]
			public int MaxMaterialCapacityBonusOfFaction;

			[FoldoutGroup("@SectorName/Faction보너스리소스")]
			[HorizontalGroup("@SectorName/Faction보너스리소스/H"), HideLabel,SuffixLabel("전력 수용량", Overlay = true)]
			public int MaxElectricCapacityBonusOfFaction;

			[FoldoutGroup("@SectorName/FacilityInfo")]
			[ListDrawerSettings(ShowFoldout = false)]
			public FacilityInfo[] facilitiesInfo;

			[FoldoutGroup("@SectorName/Support Point"),LabelText("잔여 지원 점수")]
			public int remainingPoint;
			[FoldoutGroup("@SectorName/Support Point"),LabelText("공격 지원 점수")]
			public int offensivePoint;
			[FoldoutGroup("@SectorName/Support Point"),LabelText("방어 지원 점수")]
			public int defensivePoint;
			[FoldoutGroup("@SectorName/Support Point"),LabelText("보급 지원 점수")]
			public int supplyPoint;
			[FoldoutGroup("@SectorName/Support Point"),LabelText("시설 지원 점수")]
			public int facilityPoint;

			[FoldoutGroup("@SectorName")]
			[LabelText("구역 확보에 필요한 시간")]
			public float CaptureTimeRequired;
			[FoldoutGroup("@SectorName")]
			[LabelText("구역 환경 키")]
			public EnvironmentalKey EnvironmentalKey;
			[FoldoutGroup("@SectorName")]
			[LabelText("영구 적용 효과")]
			[FoldoutGroup("@SectorName")]
			public StatusEffectsFlag PermanentStatus;
			[FoldoutGroup("@SectorName")]
			[LabelText("동적 적용 효과")]
			public StatusEffectsFlag DynamicStatus;
#if UNITY_EDITOR
			private ValueDropdownList<string> SectorObjectListInScene()
			{
				ValueDropdownList<string> list = new ValueDropdownList<string>();
				SectorObject[] objects = GameObject.FindObjectsByType<SectorObject>(FindObjectsSortMode.None);
				foreach (var obj in objects) { list.Add(obj.gameObject.name, obj.gameObject.name); }
				if (list.Count == 0) { list.Add("Empty", ""); }
				return list;
			}

			[FoldoutGroup("@SectorName")]
			[Button("리소스 값 초기화"), PropertyOrder(-1)]
			private void SetDefault()
			{
				CapacityPersonnel = 20;
				CapacityMaterial = 200;
				CapacityElectric = 200;

				RecoveryPersonnel = 2;
				RecoveryMaterial = 100;
				RecoveryElectric = 100;

				LocalPersonnel = 2;
				LocalMaterial = 100;
				LocalElectric = 100;

				MaxPersonnelCapacityBonusOfFaction = 10;
				MaxMaterialCapacityBonusOfFaction = 50;
				MaxElectricCapacityBonusOfFaction = 50;

				DistributionDepth = 0;
				CycleTime = 10;
			}
#endif
		}
		[Serializable]
		public struct UnitData
		{
			[FoldoutGroup("@unitKey")]
			public UnitKey unitKey;
			[FoldoutGroup("@unitKey")]
			[ValueDropdown("@GetFactionNames($property)")]
			[InlineButton("Clear_factionName","Clear")]
			[OnValueChanged("OnValueChanged_factionID")]
			public int factionID;
			[FoldoutGroup("@unitKey")]
			[ValueDropdown("@GetOperationNames($property)")]
			[InlineButton("Clear_belongedOperation","Clear")]
			[ShowIf("@ShowOperationSelect($property)")]
			public int belongedOperation;
			[FoldoutGroup("@unitKey")]
			[ValueDropdown("@GetSectorNames($property)")]
			[LabelText("SectorName")]
			[InlineButton("Clear_visiteSectorName","Clear")]
			public int visiteSectorID;

			[FoldoutGroup("@unitKey")]
			[LabelText("현재 내구도")]
			[InlineButton("MaxDurability","Max")]
			public int durability;
			[ToggleGroup("showEdit")]
			public Vector3 position;
			[ToggleGroup("showEdit")]
			public Vector3 rotation;

#if UNITY_EDITOR
			[ToggleGroup("showEdit","EditTransform")]
			public bool showEdit;
			private void Clear_factionName()
			{
				factionID = -1;
			}
			private void Clear_visiteSectorName()
			{
				visiteSectorID = -1;
			}
			private void Clear_belongedOperation()
			{
				belongedOperation = -1;
			}
			private void MaxDurability()
			{
				durability = int.MaxValue;
			}
			private static ValueDropdownList<int> GetFactionNames(InspectorProperty property)
			{
				ValueDropdownList<int> list = new ValueDropdownList<int>();

				// 루트까지 올라감
				var root = property.Tree.WeakTargets.FirstOrDefault() as StrategyStartSetterData;
				if (root == null)
				{
					list.Add("(No Parent Info)", -1);
					return list;
				}

				var bases = root.data.factionDatas;
				if (bases == null || bases.Length == 0)
				{
					list.Add("(No Info)", -1);
					return list;
				}

				var items = bases.Select(x => x.FactionName).ToList();
				list.Add("", -1);
				for (int i = 0 ; i < items.Count ; i++)
				{
					list.Add(items[i], i);
				}

				return list;
			}
			private static ValueDropdownList<int> GetSectorNames(InspectorProperty property)
			{
				ValueDropdownList<int> list = new ValueDropdownList<int>();

				// 루트까지 올라감
				var root = property.Tree.WeakTargets.FirstOrDefault() as StrategyStartSetterData;
				if (root == null)
				{
					list.Add("(No Parent Info)", -1);
					return list;
				}

				var bases = root.data.sectorDatas;
				if (bases == null || bases.Length == 0)
				{
					list.Add("(No Info)", -1);
					return list;
				}

				var items = bases.Select(x => x.SectorName).ToList();
				list.Add("", -1);
				for (int i = 0 ; i < items.Count ; i++)
				{
					list.Add(items[i], i);
				}

				return list;
			}
			private static ValueDropdownList<int> GetOperationNames(InspectorProperty property)
			{
				int factionID = -1;
				ValueDropdownList<int> list = new ValueDropdownList<int>();

				if (property.Parent != null && property.ParentValueProperty.ValueEntry != null)
				{
					var parent = property.ParentValueProperty.ValueEntry.WeakSmartValue;
					if (parent != null && parent is UnitData unitData)
					{
						if (unitData.factionID == -1)
						{
							list.Add("FactionName Is Empty", -1);
							return list;
						}
						else
						{
							factionID = unitData.factionID;
						}
					}
				}

				// 루트까지 올라감
				var root = property.Tree.WeakTargets.FirstOrDefault() as StrategyStartSetterData;
				if (root == null)
				{
					list.Add("(No Parent Info)", -1);
					return list;
				}
				var bases = root.data.operationDatas;
				if (bases == null || bases.Length == 0)
				{
					list.Add("(No Info)", -1);
					return list;
				}

				bases.Where(x => x.factionID.Equals(factionID)).Select(x => x.teamName);
				int length = bases.Length;
				list.Add("", -1);
				for (int i = 0 ; i < length ; i++)
				{
					if (!bases[i].factionID.Equals(factionID)) continue;
					if (string.IsNullOrWhiteSpace(bases[i].teamName)) continue;
					list.Add(bases[i].teamName, i);
				}
				return list;
			}
			private bool ShowOperationSelect(InspectorProperty property)
			{
				int factionID = -1;

				if (property.Parent != null && property.ParentValueProperty.ValueEntry != null)
				{
					var parent = property.ParentValueProperty.ValueEntry.WeakSmartValue;
					if (parent != null && parent is UnitData unitData)
					{
						if (unitData.factionID == -1)
						{
							return false;
						}
						else
						{
							factionID = unitData.factionID;
						}
					}
				}

				// 루트까지 올라감
				var root = property.Tree.WeakTargets.FirstOrDefault() as StrategyStartSetterData;
				if (root == null)
				{
					return false;
				}
				var bases = root.data.operationDatas;
				if (bases == null || bases.Length == 0)
				{
					return false;
				}

				bases.Where(x => x.factionID.Equals(factionID)).Select(x => x.teamName);
				int length = bases.Length;
				for (int i = 0 ; i < length ; i++)
				{
					if (!bases[i].factionID.Equals(factionID)) continue;
					if (string.IsNullOrWhiteSpace(bases[i].teamName)) continue;
					return true;
				}
				return false;
			}
			private bool ShowProfileObject => unitKey == UnitKey.None;
			private void OnValueChanged_factionID()
			{
				belongedOperation = -1;
			}
#endif

			public readonly string DisplayName()
			{
				return StrategyManager.Key2Unit.GetAsset(unitKey).DisplayName;
			}
		}
		[Serializable]
		public struct ProjectileData
		{
			public ProjectileKey projectilKey;
			public int count => infos == null ? 0 : infos.Length;
			public Info[] infos;
			public readonly Info this[int index] => infos[index];
			[Serializable]
			public struct Info
			{
				[HorizontalGroup("Idx")]
				[ValueDropdown("@GetUnitNames($property)"), LabelText("Order")]
				public int orderInSetterIndex;
				[HorizontalGroup("Idx")]
				[ValueDropdown("@GetUnitNames($property)"), LabelText("Target")]
				public int targetInSetterIndex;
				[HorizontalGroup("Pos"), LabelText("Start")]
				public Vector3 startPosition;
				[HorizontalGroup("Pos"), LabelText("Target")]
				public Vector3 targetPosition;
				[HorizontalGroup("Pos"), LabelText("Target-Ended")]
				public Vector3 endedPosition;


				public Vector3 position;
				public Quaternion rotation;
				public Vector3 velocity;

				public float lifeTime;
				public int piercingCount;
#if UNITY_EDITOR
				private static ValueDropdownList<int> GetUnitNames(InspectorProperty property)
				{
					ValueDropdownList<int> list = new ValueDropdownList<int>();

					// 루트까지 올라감
					var root = property.Tree.WeakTargets.FirstOrDefault() as StrategyStartSetterData;
					if (root == null)
					{
						list.Add("(No Parent Info)", -1);
						return list;
					}

					var bases = root.data.unitDatas;
					if (bases == null || bases.Length == 0)
					{
						list.Add("(No Info)", -1);
						return list;
					}

					var items = bases.Select(x => x.unitKey.ToString()).ToList();
					list.Add("", -1);
					for (int i = 0 ; i < items.Count ; i++)
					{
						list.Add($"{i:00}:{items[i]}", i);
					}

					return list;
				}
#endif
			}
		}

		[Serializable]
		public struct OperationData
		{
			[FoldoutGroup("@teamName")]
			public string teamName;
			[FoldoutGroup("@teamName")]
			[ValueDropdown("@GetFactionNames($property)")]
			[InlineButton("Clear_faction","Clear")]
			public int factionID;

			[FoldoutGroup("@teamName")]
			[ValueDropdown("@GetSectorNames($property)")]
			[LabelText("SectorName")]
			[InlineButton("Clear_visiteSectorName","Clear")]
			public int visiteSectorID;
#if UNITY_EDITOR
			private void Clear_faction()
			{
				factionID = -1;
			}
			private void Clear_visiteSectorName()
			{
				visiteSectorID = -1;
			}
			private static ValueDropdownList<int> GetFactionNames(InspectorProperty property)
			{
				ValueDropdownList<int> list = new ValueDropdownList<int>();

				// 루트까지 올라감
				var root = property.Tree.WeakTargets.FirstOrDefault() as StrategyStartSetterData;
				if (root == null)
				{
					list.Add("(No Parent Info)", -1);
					return list;
				}

				var bases = root.data.factionDatas;
				if (bases == null || bases.Length == 0)
				{
					list.Add("(No Info)", -1);
					return list;
				}

				var items = bases.Select(x => x.FactionName).ToList();
				list.Add("", -1);
				for (int i = 0 ; i < items.Count ; i++)
				{
					list.Add(items[i], i);
				}

				return list;
			}
			private static ValueDropdownList<int> GetSectorNames(InspectorProperty property)
			{
				ValueDropdownList<int> list = new ValueDropdownList<int>();

				// 루트까지 올라감
				var root = property.Tree.WeakTargets.FirstOrDefault() as StrategyStartSetterData;
				if (root == null)
				{
					list.Add("(No Parent Info)", -1);
					return list;
				}

				var bases = root.data.sectorDatas;
				if (bases == null || bases.Length == 0)
				{
					list.Add("(No Info)", -1);
					return list;
				}

				var items = bases.Select(x => x.SectorName).ToList();
				list.Add("", -1);
				for (int i = 0 ; i < items.Count ; i++)
				{
					list.Add(items[i], i);
				}

				return list;
			}
#endif

		}
		[Serializable]
		public struct CaptureData
		{
			[ValueDropdown("@GetSectorNames($property)")]
			public int captureSectorID;
			[ValueDropdown("@GetFactionNames($property)")]
			public int captureFactionID;
			[Range(0f,1f)]
			public float captureProgress;
			public struct CaptureProgress
			{
				public float pogress;
				public bool isFixed;
			}
#if UNITY_EDITOR
			private static ValueDropdownList<int> GetSectorNames(InspectorProperty property)
			{
				ValueDropdownList<int> list = new ValueDropdownList<int>();

				// 루트까지 올라감
				var root = property.Tree.WeakTargets.FirstOrDefault() as StrategyStartSetterData;
				if (root == null)
				{
					list.Add("(No Parent Info)", -1);
					return list;
				}

				var bases = root.data.sectorDatas;
				if (bases == null || bases.Length == 0)
				{
					list.Add("(No Info)", -1);
					return list;
				}

				var items = bases.Select(x => x.SectorName).ToList();
				list.Add("", -1);
				for (int i = 0 ; i < items.Count ; i++)
				{
					list.Add(items[i], i);
				}

				return list;
			}
			private static ValueDropdownList<int> GetFactionNames(InspectorProperty property)
			{
				ValueDropdownList<int> list = new ValueDropdownList<int>();

				// 루트까지 올라감
				var root = property.Tree.WeakTargets.FirstOrDefault() as StrategyStartSetterData;
				if (root == null)
				{
					list.Add("(No Parent Info)", -1);
					return list;
				}

				var bases = root.data.factionDatas;
				if (bases == null || bases.Length == 0)
				{
					list.Add("(No Info)", -1);
					return list;
				}

				var items = bases.Select(x => x.FactionName).ToList();
				list.Add("", -1);
				for (int i = 0 ; i < items.Count ; i++)
				{
					list.Add(items[i], i);
				}

				return list;
			}
#endif
		}
		[Serializable]
		public struct SectorLinkData
		{
			[HorizontalGroup, ValueDropdown("@GetSectorNames($property)"), HideLabel, SuffixLabel("Sector A  ",overlay: true)]
			public string sectorA;
			[HorizontalGroup(width:80), HideLabel]
			public NetworkLink.ConnectDirType connectDir;
			[HorizontalGroup, ValueDropdown("@GetSectorNames($property)"), HideLabel, SuffixLabel("Sector B  ",overlay: true)]
			public string sectorB;
			[TableList]
			public WaypointUtility.Waypoint[] waypoint;

#if UNITY_EDITOR
			[ShowInInspector]
			public bool onShowEditPoint { get; set; }
			private static ValueDropdownList<int> GetSectorNames(InspectorProperty property)
			{
				ValueDropdownList<int> list = new ValueDropdownList<int>();

				// 루트까지 올라감
				var root = property.Tree.WeakTargets.FirstOrDefault() as StrategyStartSetterData;
				if (root == null)
				{
					list.Add("(No Parent Info)", -1);
					return list;
				}

				var bases = root.data.sectorDatas;
				if (bases == null || bases.Length == 0)
				{
					list.Add("(No Info)", -1);
					return list;
				}

				var items = bases.Select(x => x.SectorName).ToList();
				list.Add("", -1);
				for (int i = 0 ; i < items.Count ; i++)
				{
					list.Add(items[i], i);
				}

				return list;
			}
#endif
			public SectorLinkData ReverseDir
			{
				get
				{
					return new SectorLinkData()
					{
						sectorA = sectorB,
						sectorB = sectorA,
						connectDir = connectDir == NetworkLink.ConnectDirType.Forward ? NetworkLink.ConnectDirType.Backward :
									 connectDir == NetworkLink.ConnectDirType.Backward ? NetworkLink.ConnectDirType.Forward :
									 connectDir,
						waypoint = waypoint?.Select(wp => new WaypointUtility.Waypoint()
						{
							point = wp.point,
							width = wp.width
						}).Reverse().ToArray()
					};
				}
			}
		}

		[Serializable]
		public struct Overview : IDataCopy<Overview>
		{
			public string title;
			[TextArea(2,10)]
			public string description;

			public Overview Copy()
			{
				return new Overview()
				{
					title = title,
					description = description
				};
			}
		}
		[Serializable]
		public struct Mission : IDataCopy<Mission>
		{
			public string id;
			public string title;
			[TextArea(1,10)]
			public string description;

			[TextArea(2,10)]
			[Tooltip(MissionParser.testParserData)]
			public string victoryScript;
			[TextArea(2,10)]
			[Tooltip(MissionParser.testParserData)]
			public string defeatScript;

			public MissionBlock[] enableSubMissions;

			public Mission Copy()
			{
				return new Mission()
				{
					id = id,
					title = title,
					description = description,
					victoryScript = victoryScript,
					defeatScript = defeatScript,
					enableSubMissions = enableSubMissions?.Select(s => s.Copy()).ToArray()
				};
			}
		}
		[Serializable]
		public struct MissionBlock : IDataCopy<MissionBlock>
		{
			public string id;
			[TextArea(2,10)]
			[Tooltip(MissionParser.testParserData)]
			public string missionScript;

			public MissionBlock Copy()
			{
				return new MissionBlock()
				{
					id = id,
					missionScript = missionScript
				};
			}
		}

		[Serializable]
		public struct FactionRelation
		{
			[HorizontalGroup, ValueDropdown("@GetFactionNames($property)"), HideLabel, SuffixLabel("Faction A  ",overlay: true)]
			public string factionA;
			[HorizontalGroup(width:80), ValueDropdown("GetRelationType"), HideLabel]
			public int relationType;
			[HorizontalGroup, ValueDropdown("@GetFactionNames($property)"), HideLabel, SuffixLabel("Faction B  ",overlay: true)]
			public string factionB;


#if UNITY_EDITOR
			private static ValueDropdownList<int> GetFactionNames(InspectorProperty property)
			{
				ValueDropdownList<int> list = new ValueDropdownList<int>();

				// 루트까지 올라감
				var root = property.Tree.WeakTargets.FirstOrDefault() as StrategyStartSetterData;
				if (root == null)
				{
					list.Add("(No Parent Info)", -1);
					return list;
				}

				var bases = root.data.factionDatas;
				if (bases == null || bases.Length == 0)
				{
					list.Add("(No Info)", -1);
					return list;
				}

				var items = bases.Select(x => x.FactionName).ToList();
				list.Add("", -1);
				for (int i = 0 ; i < items.Count ; i++)
				{
					list.Add(items[i], i);
				}

				return list;
			}
			private static ValueDropdownList<int> GetRelationType()
			{
				ValueDropdownList<int> list = new ValueDropdownList<int>();

				list.Add("중립", 0);
				list.Add("우호", 1);
				list.Add("적대", 2);

				return list;
			}
#endif
		}
	}

}