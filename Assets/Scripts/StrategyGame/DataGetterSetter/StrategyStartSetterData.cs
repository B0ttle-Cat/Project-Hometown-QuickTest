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

		public Overview overview;
		public Mission mission;

		[Space]
		public FactionData[] factionDatas;
		public SectorData[] sectorDatas;
		public UnitData[] unitDatas;
		[Space]
		[TableList]
		public CaptureData[] captureDatas;

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
		public int factionID { get; set; }

		[FoldoutGroup("@factionName")]
		public Color factionColor;
		[FoldoutGroup("@factionName")]
		public Sprite factionIcon;

		[FoldoutGroup("@factionName")] public int maxPersonnel;
		[FoldoutGroup("@factionName")] public int maxMaterialPoint;
		[FoldoutGroup("@factionName")] public int maxElectricPoint;

		[FoldoutGroup("@factionName")] public GameObject defaultUnitPrefab;
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
		[FoldoutGroup("@unitName")]
		public string unitName;
		public int unitID { get; set; }
		[FoldoutGroup("@unitName")]
		[ValueDropdown("@GetFactionNames($property)")]
		[InlineButton("Clear_factionName","Clear")]
		public string factionName;
		[FoldoutGroup("@unitName")]
		public UnitProfileObject unitProfile;

		[FoldoutGroup("@unitName")]
		public Vector3 position;
		[FoldoutGroup("@unitName")]
		public Vector3 rotation;

		[FoldoutGroup("@unitName")]
		[ValueDropdown("@GetSectorNames($property)")]
		[LabelText("SectorName")]
		[InlineButton("Clear_connectSectorName","Clear")]
		public string connectSectorName;

		[FoldoutGroup("@unitName")]
		public Vector2Int[] skillData;
#if UNITY_EDITOR
		private void Clear_factionName()
		{
			factionName = "";
		}
		private void Clear_connectSectorName()
		{
			connectSectorName = "";
		}
		private static IEnumerable<string> GetFactionNames(InspectorProperty property)
		{
			// 루트까지 올라감
			var root = property.Tree.WeakTargets.FirstOrDefault() as StrategyStartSetterData;
			if (root == null)
				return new[] { "(No Root Data)" };

			var bases = root.data.factionDatas;
			if (bases == null || bases.Length == 0)
				return new[] { "(No Data)" };

			return bases.Select(x => x.factionName);
		}
		private static IEnumerable<string> GetSectorNames(InspectorProperty property)
		{
			// 루트까지 올라감
			var root = property.Tree.WeakTargets.FirstOrDefault() as StrategyStartSetterData;
			if (root == null)
				return new[] { "(No Root Data)" };

			var bases = root.data.sectorDatas;
			if (bases == null || bases.Length == 0)
				return new[] { "(No Data)" };

			return bases.Select(x => x.profileData.sectorName);
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
				return new[] { "(No Root Data)" };

			var bases = root.data.sectorDatas;
			if (bases == null || bases.Length == 0)
				return new[] { "(No Data)" };

			return bases.Select(x => x.profileData.sectorName);
		}
		// Odin의 PropertyContext를 통해 상위 오브젝트 접근
		private static IEnumerable<string> GetCaptureFactionNames(InspectorProperty property)
		{
			// 루트까지 올라감
			var root = property.Tree.WeakTargets.FirstOrDefault() as StrategyStartSetterData;
			if (root == null)
				return new[] { "(No Root Data)" };

			var bases = root.data.factionDatas;
			if (bases == null || bases.Length == 0)
				return new[] { "(No Data)" };

			return bases.Select(x => x.factionName);
		}
#endif

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
				enableSubMissions = enableSubMissions?.Select(s=>s.Copy()).ToArray()
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
	[SerializeField, InlineProperty, HideLabel]
	private Data data;
	protected override Data _data { get => data; set => data = value; }
}
