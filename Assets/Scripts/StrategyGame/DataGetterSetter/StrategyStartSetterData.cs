using System;
using System.Collections.Generic;
using System.Linq;

using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

using UnityEngine;

using static StrategyMissionTree;

[CreateAssetMenu(fileName = "StrategyStartSetterData", menuName = "Scriptable Objects/StrategyGame/StrategyStartSetterData")]
public class StrategyStartSetterData : DataGetterSetter<StrategyStartSetterData.Data>
{
	[Serializable]
	public struct Data
	{
		[ValueDropdown("GetFactionName")]
		public string playerFactionName;
		public Overview overview;
		public Mission mission;

		[Space]
		public FactionData[] factionDatas;
		public ControlBaseData[] controlBaseDatas;
		public UnitData[] unitDatas;
		[Space]
		[TableList]
		public CaptureData[] captureDatas;

#if UNITY_EDITOR
		private IEnumerable<string> GetFactionName()
		{
			if (factionDatas == null || factionDatas.Length == 0)
				return new[] { "(No ControlBaseData)" };

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
	public struct ControlBaseData
	{
#if UNITY_EDITOR
		private string GroupName => profileData.controlBaseName;
		[FoldoutGroup("@GroupName")]
		[ShowInInspector, InlineButton("PushData"), InlineButton("PullData"), PropertyOrder(-1)]
		[LabelWidth(50)]
		private ControlBase Target { get; set; }
		private void PullData()
		{
			if (Target == null) return;
			profileData = Target.ProfileData;
			mainStatsData = Target.StatsData;
			facilitiesStatsData = Target.FacilitiesData;
			supportStatsData = Target.SupportData;
			captureTime = Target.CaptureData.captureTime;

			profileData.controlBaseName = Target.gameObject.name;
		}
		private void PushData()
		{
			if (Target == null) return;
			Target.Profile.SetData(profileData, true);
			Target.Stats.SetData(mainStatsData, true);
			Target.Facilities.SetData(facilitiesStatsData, true);
			Target.Support.SetData(supportStatsData, true);

			var captureData =  Target.CaptureData;
			captureData.captureTime = captureTime;
			Target.Capture.SetData(captureData, true);
		}
#endif
		[FoldoutGroup("@GroupName")]
		[InlineProperty, HideLabel, TitleGroup("@GroupName/Profile")]
		public StrategyGamePlayData.ControlBaseData.Profile.Data profileData;
		[InlineProperty, HideLabel, FoldoutGroup("@GroupName/MainStats")]
		public StrategyGamePlayData.ControlBaseData.MainStats.Data mainStatsData;
		[FoldoutGroup("@GroupName/MainStats")]
		public float captureTime;
		[InlineProperty, HideLabel, FoldoutGroup("@GroupName/Facilities")]
		public StrategyGamePlayData.ControlBaseData.Facilities.Data facilitiesStatsData;
		[InlineProperty, HideLabel, FoldoutGroup("@GroupName/Support")]
		public StrategyGamePlayData.ControlBaseData.Support.Data supportStatsData;
	}
	[Serializable]
	public struct UnitData
	{
		[FoldoutGroup("@unitName")]
		public string unitName;
		public int unitID { get; set; }
		[FoldoutGroup("@unitName")]
		[ValueDropdown("@GetFactionName($property)")]
		public string factionName;
		[FoldoutGroup("@unitName")]
		public UnitProfileObject unitProfile;

		[FoldoutGroup("@unitName")]
		public Vector3 position;
		[FoldoutGroup("@unitName")]
		public Vector3 rotation;

#if UNITY_EDITOR
		private static IEnumerable<string> GetFactionName(InspectorProperty property)
		{
			// 루트까지 올라감
			var root = property.Tree.WeakTargets.FirstOrDefault() as StrategyStartSetterData;
			if (root == null)
				return new[] { "(No Root Data)" };

			var bases = root.data.factionDatas;
			if (bases == null || bases.Length == 0)
				return new[] { "(No ControlBaseData)" };

			return bases.Select(x => x.factionName);
		}
#endif
	}
	[Serializable]
	public struct CaptureData
	{
		[ValueDropdown("@GetControlBaseNames($property)")]
		public string captureControlBase;
		[ValueDropdown("@GetCaptureFactionName($property)")]
		public string captureFaction;
		[Range(0f,1f)]
		public float captureProgress;
		public struct CaptureProgress
		{
			public float pogress;
			public bool isFixed;
		}
#if UNITY_EDITOR
		private static IEnumerable<string> GetControlBaseNames(InspectorProperty property)
		{
			// 루트까지 올라감
			var root = property.Tree.WeakTargets.FirstOrDefault() as StrategyStartSetterData;
			if (root == null)
				return new[] { "(No Root Data)" };

			var bases = root.data.controlBaseDatas;
			if (bases == null || bases.Length == 0)
				return new[] { "(No ControlBaseData)" };

			return bases.Select(x => x.profileData.controlBaseName);
		}
		// Odin의 PropertyContext를 통해 상위 오브젝트 접근
		private static IEnumerable<string> GetCaptureFactionName(InspectorProperty property)
		{
			// 루트까지 올라감
			var root = property.Tree.WeakTargets.FirstOrDefault() as StrategyStartSetterData;
			if (root == null)
				return new[] { "(No Root Data)" };

			var bases = root.data.factionDatas;
			if (bases == null || bases.Length == 0)
				return new[] { "(No ControlBaseData)" };

			return bases.Select(x => x.factionName);
		}
#endif

	}
	[Serializable]
	public struct Overview
	{
		public string title;
		[TextArea(2,10)]
		public string description;
	}
	[Serializable]
	public struct Mission
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

		public SubMission[] enableSubMissions;
	}
	[Serializable]
	public struct SubMission
	{
		public string id;
		[TextArea(2,10)]
		[Tooltip(MissionParser.testParserData)]
		public string missionScript;
	}
	[SerializeField, InlineProperty, HideLabel]
	private Data data;
	protected override Data _data { get => data; set => data = value; }
}
