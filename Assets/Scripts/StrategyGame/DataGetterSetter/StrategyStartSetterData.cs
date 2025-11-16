using System;
using System.Collections.Generic;
using System.Linq;

using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

using UnityEngine;

using static StrategyGamePlayData;
using static StrategyMissionTree;

[CreateAssetMenu(fileName = "StrategyStartSetterData", menuName = "Scriptable Objects/StrategyGame/StrategyStartSetterData")]
public class StrategyStartSetterData : DataGetterSetter<StrategyStartSetterData.Data>
{
	[Serializable]
	public struct Data
	{
		[ValueDropdown("GetFactionName")]
		public string playerFactionName;

		public double unscaleGamePlayTime;
		public double gamePlayTime;

		[Space]
		[FoldoutGroup("Overview")]
		public Overview overview;
		[FoldoutGroup("Mission")]
		public Mission mission;

		[Space]
		public FactionData[] factionDatas;
		public SectorData[] sectorDatas;
		public UnitData[] unitDatas;
		public OperationData[] operationDatas;
		[Space]
		[TableList]
		public CaptureData[] captureDatas;
		[Space]
		public SectorLinkData[] sectorLinkDatas;
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

		[FoldoutGroup("@factionName/Stats")] public int maxOperationPoint;
		[FoldoutGroup("@factionName/Stats")] public int requireOperationPoint;
		[FoldoutGroup("@factionName/Stats")] public int currentOperationPoint;
		[Space]
		[FoldoutGroup("@factionName/Stats")] public int maxMaterialPoint;
		[FoldoutGroup("@factionName/Stats")] public int currentMaterialPoint;
		[Space]
		[FoldoutGroup("@factionName/Stats")] public int maxElectricPoint;
		[FoldoutGroup("@factionName/Stats")] public int currentElectricPoint;
		[Space]
		[FoldoutGroup("@factionName/Stats")] public int captureSpeed;

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
			spawnOperationData = Target.SpawnOperationData.Copy();
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
			Target.SpawnOperation.SetData(spawnOperationData.Copy(), true);

			var captureData =  Target.CaptureData;
			captureData.captureTime = captureTime;
			Target.Capture.SetData(captureData, true);
		}

		[ButtonGroup("@GroupName/Button"), PropertyOrder(-98)]
		private void ResetDetulsStats()
		{
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
		[InlineProperty, HideLabel, FoldoutGroup("@GroupName/Spawn")]
		public StrategyGamePlayData.SectorData.SpawnOperation.Data spawnOperationData;
	}
	[Serializable]
	public struct UnitData
	{
		[FoldoutGroup("@unitKey")]
		public UnitKey unitKey;
		[FoldoutGroup("@unitKey")]
		[ValueDropdown("@GetFactionNames($property)")]
		[InlineButton("Clear_factionName","Clear")]
		public string factionName;
		[FoldoutGroup("@unitKey")]
		[ValueDropdown("@GetOperationNames($property)")]
		[InlineButton("Clear_belongedOperation","Clear")]
		public string belongedOperation;
		[FoldoutGroup("@unitKey")]
		[ValueDropdown("@GetSectorNames($property)")]
		[LabelText("SectorName")]
		[InlineButton("Clear_visiteSectorName","Clear")]
		public string visiteSectorName;
		[FoldoutGroup("@unitKey")]
		public UnitProfileObject unitProfile;

		[ToggleGroup("showEdit")]
		public Vector3 position;
		[ToggleGroup("showEdit")]
		public Vector3 rotation;

#if UNITY_EDITOR
		[ToggleGroup("showEdit","Transform")]
		public bool showEdit;
		private void Clear_factionName()
		{
			factionName = "";
		}
		private void Clear_visiteSectorName()
		{
			visiteSectorName = "";
		}
		private void Clear_belongedOperation()
		{
			belongedOperation = "";
		}
		private static IEnumerable<string> GetFactionNames(InspectorProperty property)
		{
			// 루트까지 올라감
			var root = property.Tree.WeakTargets.FirstOrDefault() as StrategyStartSetterData;
			if (root == null)
				return new[] { "(No Parent Data)" };

			var bases = root.data.factionDatas;
			if (bases == null || bases.Length == 0)
				return new[] { "(No Data)" };

			return bases.Select(x => x.factionName).Prepend("");
		}
		private static IEnumerable<string> GetSectorNames(InspectorProperty property)
		{
			// 루트까지 올라감
			var root = property.Tree.WeakTargets.FirstOrDefault() as StrategyStartSetterData;
			if (root == null)
				return new[] { "(No Parent Data)" };

			var bases = root.data.sectorDatas;
			if (bases == null || bases.Length == 0)
				return new[] { "(No Data)" };

			return bases.Select(x => x.profileData.sectorName).Prepend("");
		}
		private static ValueDropdownList<string> GetOperationNames(InspectorProperty property)
		{
			string factionName="";
			ValueDropdownList<string> list = new ValueDropdownList<string>();

			if (property.Parent != null && property.ParentValueProperty.ValueEntry != null)
			{
				var parent = property.ParentValueProperty.ValueEntry.WeakSmartValue;
				if (parent != null && parent is UnitData unitData)
				{
					if (string.IsNullOrWhiteSpace(unitData.factionName))
					{
						list.Add("FactionName Is Empty", "");
						return list;
					}
					else
					{
						factionName = unitData.factionName;
					}
				}
			}

			// 루트까지 올라감
			var root = property.Tree.WeakTargets.FirstOrDefault() as StrategyStartSetterData;
			if (root == null)
			{
				list.Add("No Parent Data", "");
				return list;
			}
			var bases = root.data.operationDatas;
			if (bases == null || bases.Length == 0)
			{
				list.Add("No Data", "");
				return list;
			}

			bases.Where(x => x.factionName.Equals(factionName)).Select(x => x.teamName).Prepend("");
			int length = bases.Length;
            for (int i = 0 ; i < length ; i++)
            {
				if (!bases[i].factionName.Equals(factionName)) continue;
				if (string.IsNullOrWhiteSpace(bases[i].teamName)) continue;
				list.Add(bases[i].teamName);
			}
			return list;
		}
#endif

		public readonly string DisplayName()
		{
			return unitProfile != null ? unitProfile.displayName : StrategyManager.Key2UnitInfo.GetAsset(unitKey).DisplayName;
		}
	}
	[Serializable]
	public struct OperationData
	{
		[FoldoutGroup("@teamName")]
		public string teamName;
		[FoldoutGroup("@teamName")]
		[ValueDropdown("@GetFactionNames($property)")]
		[InlineButton("Clear_factionName","Clear")]
		public string factionName;

		[FoldoutGroup("@teamName")]
		[ValueDropdown("@GetSectorNames($property)")]
		[LabelText("SectorName")]
		[InlineButton("Clear_visiteSectorName","Clear")]
		public string visiteSectorName;
#if UNITY_EDITOR
		private void Clear_factionName()
		{
			factionName = "";
		}
		private void Clear_visiteSectorName()
		{
			visiteSectorName = "";
		}
		private static IEnumerable<string> GetFactionNames(InspectorProperty property)
		{
			// 루트까지 올라감
			var root = property.Tree.WeakTargets.FirstOrDefault() as StrategyStartSetterData;
			if (root == null)
				return new[] { "(No Parent Data)" };

			var bases = root.data.factionDatas;
			if (bases == null || bases.Length == 0)
				return new[] { "(No Data)" };

			return bases.Select(x => x.factionName).Prepend("");
		}
		private static IEnumerable<string> GetSectorNames(InspectorProperty property)
		{
			// 루트까지 올라감
			var root = property.Tree.WeakTargets.FirstOrDefault() as StrategyStartSetterData;
			if (root == null)
				return new[] { "(No Parent Data)" };

			var bases = root.data.sectorDatas;
			if (bases == null || bases.Length == 0)
				return new[] { "(No Data)" };

			return bases.Select(x => x.profileData.sectorName).Prepend("");
		}
#endif

	}
	[Serializable]
	public struct CaptureData
	{
		[ValueDropdown("@GetSectorNames($property)")]
		public string captureSector;
		[ValueDropdown("@GetCaptureFactionNames($property)")]
		public string captureFaction;
		[Range(0f,1f)]
		public float captureProgress;
		public struct CaptureProgress
		{
			public float pogress;
			public bool isFixed;
		}
#if UNITY_EDITOR
		private static IEnumerable<string> GetSectorNames(InspectorProperty property)
		{
			// 루트까지 올라감
			var root = property.Tree.WeakTargets.FirstOrDefault() as StrategyStartSetterData;
			if (root == null)
				return new[] { "(No Parent Data)" };

			var bases = root.data.sectorDatas;
			if (bases == null || bases.Length == 0)
				return new[] { "(No Data)" };

			return bases.Select(x => x.profileData.sectorName).Prepend("");
		}
		// Odin의 PropertyContext를 통해 상위 오브젝트 접근
		private static IEnumerable<string> GetCaptureFactionNames(InspectorProperty property)
		{
			// 루트까지 올라감
			var root = property.Tree.WeakTargets.FirstOrDefault() as StrategyStartSetterData;
			if (root == null)
				return new[] { "(No Parent Data)" };

			var bases = root.data.factionDatas;
			if (bases == null || bases.Length == 0)
				return new[] { "(No Data)" };

			return bases.Select(x => x.factionName).Prepend("");
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
		private static IEnumerable<string> GetSectorNames(InspectorProperty property)
		{
			// 루트까지 올라감
			var root = property.Tree.WeakTargets.FirstOrDefault() as StrategyStartSetterData;
			if (root == null)
				return new[] { "(No Parent Data)" };

			var bases = root.data.sectorDatas;
			if (bases == null || bases.Length == 0)
				return new[] { "(No Data)" };
			var test = new string[1] { ""};

			return bases.Select(x => x.profileData.sectorName).Prepend("");
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

#if UNITY_EDITOR
	[ShowInInspector, FoldoutGroup("GizmoOption", order: -99)]
	public bool onShowGizmo { get; set; } = false;
	[ShowInInspector, FoldoutGroup("GizmoOption"), ShowIf("onShowGizmo")]
	public bool onShowSectorLink { get; set; } = true;
	[ShowInInspector, FoldoutGroup("GizmoOption"), ShowIf("onShowGizmo")]
	public bool onShowUnitPreview { get; set; } = true;
#endif
	[Space, SerializeField, InlineProperty, HideLabel]
	private Data data;
	protected override Data _data { get => data; set => data = value; }
}
