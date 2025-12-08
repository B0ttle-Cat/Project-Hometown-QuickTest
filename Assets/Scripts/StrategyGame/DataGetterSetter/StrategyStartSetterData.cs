using System;
using System.Collections.Generic;
using System.Linq;

using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

using UnityEngine;

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
			public ProjectileData[] projectileDatas;
			[TitleGroup("MainData")]
			public OperationData[] operationDatas;
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

				return factionDatas.Select(x => x.factionName);
			}
#endif
		}
		[Serializable]
		public struct FactionData
		{
			[FoldoutGroup("@factionName")]
			public string factionName;

			[FoldoutGroup("@factionName")]
			public Color factionColor;
			[FoldoutGroup("@factionName")]
			public Sprite factionIcon;

			[FoldoutGroup("@factionName/StatsData")] public int maxOperationPoint;
			[FoldoutGroup("@factionName/StatsData")] public int requireOperationPoint;
			[FoldoutGroup("@factionName/StatsData")] public int currentOperationPoint;
			[Space]
			[FoldoutGroup("@factionName/StatsData")] public int maxMaterialPoint;
			[FoldoutGroup("@factionName/StatsData")] public int currentMaterialPoint;
			[Space]
			[FoldoutGroup("@factionName/StatsData")] public int maxElectricPoint;
			[FoldoutGroup("@factionName/StatsData")] public int currentElectricPoint;
			[Space]
			[FoldoutGroup("@factionName/StatsData")] public int captureSpeed;

			[FoldoutGroup("@factionName")] public GameObject defaultUnitPrefab;

			[FoldoutGroup("@factionName"),SerializeField]
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
#if UNITY_EDITOR
			private string GroupName => profileData.sectorName;
			[FoldoutGroup("@GroupName")]
			[ShowInInspector, InlineButton("PushData"), InlineButton("PullData"), PropertyOrder(-99)]
			[LabelWidth(50)]
			private SectorObject Target { get; set; }
			private void PullData()
			{
				if (Target == null) return;
				profileData = Target.ProfileData.Copy();
				mainStatsData = Target.StatsData.Copy();
				facilitiesStatsData = Target.FacilitiesData.Copy();
				supportStatsData = Target.SupportData.Copy();
				captureTime = Target.CaptureData.captureTime;

				profileData.sectorName = Target.gameObject.name;
			}
			private void PushData()
			{
				if (Target == null) return;
				Target.Profile.SetData(profileData.Copy(), true);
				Target.Stats.SetData(mainStatsData.Copy(), true);
				Target.Facilities.SetData(facilitiesStatsData.Copy(), true);
				Target.Support.SetData(supportStatsData.Copy(), true);

				var captureData =  Target.CaptureData;
				captureData.captureTime = captureTime;
				Target.Capture.SetData(captureData, true);
			}

			[ButtonGroup("@GroupName/Button"), PropertyOrder(-98)]
			private void ResetDetulsStats()
			{
				profileData.currentStats = StatsList.SectorCurrentStatsList;
				mainStatsData.stats = StatsList.SectorStatsList;
			}
#endif
			[FoldoutGroup("@GroupName")]
			[InlineProperty, HideLabel, TitleGroup("@GroupName/Profile")]
			public StrategyGamePlayData.SectorData.Profile.Data profileData;
			[InlineProperty, HideLabel, FoldoutGroup("@GroupName/MainStats")]
			public StrategyGamePlayData.SectorData.MainStats.Data mainStatsData;
			[FoldoutGroup("@GroupName/MainStats")]
			public float captureTime;
			[InlineProperty, HideLabel, FoldoutGroup("@GroupName/Facilities")]
			public StrategyGamePlayData.SectorData.Facilities.Data facilitiesStatsData;
			[InlineProperty, HideLabel, FoldoutGroup("@GroupName/Support")]
			public StrategyGamePlayData.SectorData.Support.Data supportStatsData;
		}
		[Serializable]
		public struct UnitData
		{
			[FoldoutGroup("@unitKey")]
			public UnitKey unitKey;
			[FoldoutGroup("@unitKey")]
			[ShowIf("ShowProfileObject"), SerializeField]
			private UnitProfileObject unitProfile;
			[FoldoutGroup("@unitKey")]
			[ValueDropdown("@GetFactionNames($property)")]
			[InlineButton("Clear_factionName","Clear")]
			public int factionID;
			[FoldoutGroup("@unitKey")]
			[ValueDropdown("@GetOperationNames($property)")]
			[InlineButton("Clear_belongedOperation","Clear")]
			public int belongedOperation;
			[FoldoutGroup("@unitKey")]
			[ValueDropdown("@GetSectorNames($property)")]
			[LabelText("SectorName")]
			[InlineButton("Clear_visiteSectorName","Clear")]
			public int visiteSectorID;

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

				var items = bases.Select(x => x.factionName).ToList();
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

				var items = bases.Select(x => x.profileData.sectorName).ToList();
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
					list.Add("No Parent Info", -1);
					return list;
				}
				var bases = root.data.operationDatas;
				if (bases == null || bases.Length == 0)
				{
					list.Add("No Info", -1);
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
			private bool ShowProfileObject => unitKey == UnitKey.None;
#endif

			public readonly string DisplayName()
			{
				return unitProfile != null ? unitProfile.displayName : StrategyManager.Key2Unit.GetAsset(unitKey).DisplayName;
			}
			public readonly UnitProfileObject GetUnitProfile
			{
				get
				{
					return (unitKey != UnitKey.None || unitProfile == null)
						? 
						StrategyManager.Key2Unit.GetAsset(unitKey).UnitProfileObject 
						: unitProfile;
				}
			}
		}
		[Serializable]
		public struct ProjectileData
		{
			public ProjectileKey projectilKey;
			public int count;
			public Info[] infos;
			public readonly Info this[int index] => infos[index];
			[Serializable]
			public struct Info
			{
				public int orderInSetterIndex;
				public int targetInSetterIndex;
				public Vector3 startPosition;
				public Vector3 targetPosition;

				public Vector3 position;
				public Quaternion rotation;
				public Vector3 velocity;

				public float lifeTime;
				public int piercingPoint;
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

				var items = bases.Select(x => x.factionName).ToList();
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

				var items = bases.Select(x => x.profileData.sectorName).ToList();
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

				var items = bases.Select(x => x.profileData.sectorName).ToList();
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

				var items = bases.Select(x => x.factionName).ToList();
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

				var items = bases.Select(x => x.profileData.sectorName).ToList();
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

				var items = bases.Select(x => x.factionName).ToList();
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