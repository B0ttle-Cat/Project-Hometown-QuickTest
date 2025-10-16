using System;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using TMPro;

using UnityEngine;

using static StrategyMissionTree;

public partial class StrategyDetailsPanelUI : DetailsPanelUI // // FieldInfo UI
{
#if UNITY_EDITOR
	[ShowInInspector, InlineButton(nameof(_Select),"Open UI"),  HideLabel]
	private StrategyDetailsPanelType _select;
	private void _Select() => OpenUI(_select);
#endif
	public enum StrategyDetailsPanelType
	{
		None,

		[HideInInspector]
		FieldInfo = 100,
		FieldInfo_Overview,
		FieldInfo_MainMission,
		FieldInfo_Statistics,
		FieldInfo_Storyboard,

		[HideInInspector]
		ControlBase = 200,

		[HideInInspector]
		BattleUnit = 300,

		[HideInInspector]
		BattleSkill= 400,
	}
	public void OpenUI(StrategyDetailsPanelType openContent)
	{
		ClearAllContent();
		Init();
		switch (openContent)
		{
			case StrategyDetailsPanelType.None:
			break;
			case > StrategyDetailsPanelType.FieldInfo and < StrategyDetailsPanelType.ControlBase:
			OnShow_FieldInfoUI(openContent);
			break;
			case > StrategyDetailsPanelType.ControlBase and < StrategyDetailsPanelType.BattleUnit:
			OnShow_ControlBaseUI(openContent);
			break;
			case > StrategyDetailsPanelType.BattleUnit and < StrategyDetailsPanelType.BattleSkill:
			OnShow_BattleUnitUI(openContent);
			break;
			case > StrategyDetailsPanelType.BattleSkill:
			OnShow_BattleSkillUI(openContent);
			break;
		}
	}
	public void HideUI()
	{
		OnHide_FieldInfoUI();
		OnHide_ControlBaseUI();
		OnHide_BattleUnitUI();
		OnHide_BattleSkillUI();

		ClearAllContent();
	}

}

public partial class StrategyDetailsPanelUI : DetailsPanelUI // FieldInfo UI
{
	[Serializable]
	public struct FieldInfoUISample
	{
		public RectTransform overview;
		public RectTransform statistics;
		public RectTransform mission;
		public RectTransform storyboard;
	}
	[SerializeField, FoldoutGroup("전장 정보 UI"), InlineProperty, HideLabel]
	private FieldInfoUISample fieldInfoUISample;


	public void OnShow_FieldInfoUI(StrategyDetailsPanelType openContent)
	{
		ContentTitleText("전장 정보");
		OnShow_Overview(openContent == StrategyDetailsPanelType.FieldInfo_Overview);
		OnShow_Statistics(openContent == StrategyDetailsPanelType.FieldInfo_Statistics);
		OnShow_MissionObjective(openContent == StrategyDetailsPanelType.FieldInfo_Overview);

		// OnShow_Storyboard("","",openContent == StrategyDetailsPanelType.FieldInfo_Overview);
	}
	public void OnHide_FieldInfoUI()
	{

	}
	// 개요 화면
	private void OnShow_Overview(bool isOn = false)
	{
		AddContnet("개요", fieldInfoUISample.overview, OnStartShow, isOn);
		void OnStartShow(RectTransform layout)
		{
			var data = StrategyGamePlayData.PreparedData.GetData();
			var overviewData = data.overview;

			if (layout.gameObject.TryFindPair<TMP_Text>("Title", out var title))
			{
				title.text = overviewData.title;
			}

			if (layout.gameObject.TryFindPair<TMP_Text>("Content", out var overview))
			{
				overview.text = overviewData.description;
			}
		}
	}
	// 통개 화면
	private void OnShow_Statistics(bool isOn = false)
	{
		AddContnet("통계", null, OnStartShow, isOn);
		void OnStartShow(RectTransform layout)
		{
			if (!layout.gameObject.TryFindPair("Content", out var contentParent)) return;
			if (!layout.gameObject.TryFindPair("Group", out var groupPrefab)) return;
			if (!layout.gameObject.TryFindPair("Item", out var itemPrefab)) return;

			NewGroup("파괴한 적 유닛");
			NewItems(StatsCatagory(StatsKey.InGamePlay.DestroyCount_Unit, "Enemy"));

			NewGroup("파괴된 플레이어 유닛");
			NewItems(StatsCatagory(StatsKey.InGamePlay.DestroyCount_Unit, "Player"));

			void NewGroup(string text)
			{
				var newObject = GameObject.Instantiate(groupPrefab, contentParent.transform);
				newObject.SetActive(true);
				TMP_Text textUI = newObject.GetComponentInChildren<TMP_Text>();
				if (textUI == null)
				{
					Destroy(newObject);
					return;
				}
				textUI.text = text;
			}
			List<(string, string)> StatsCatagory(params string[] args)
			{
				string catagory = StatsKey.JoinPath(args);
				return StrategyManager.Statistics.SelectKeyList(key =>
				{
					return key.catagory.Equals(catagory);
				});
			}
			void NewItems(IEnumerable<(string catagory, string itemID)> keys)
			{
				foreach ((string catagory, string itemID) key in keys) NewItem(key);
			}
			void NewItem((string catagory, string itemID) key)
			{
				var newObject = GameObject.Instantiate(itemPrefab, contentParent.transform);
				newObject.SetActive(true);
				TMP_Text textUI = newObject.GetComponentInChildren<TMP_Text>();
				if (textUI == null)
				{
					Destroy(newObject);
					return;
				}

				string text ="<indent=20%><line-height=0>통계 요소 명\n<align=right>통계 값";
				StrategyManager.Statistics.AddListener_ToString(key.catagory, key.itemID, toString =>
				{
					textUI.text = 
						$"<indent=20%><line-height=0>{StrategyManager.ID2Name.DisplayName(key.itemID)}" +
						$"\n<align=right>{toString}";
				}, true);
			}
		}
	}
	// 미션 정보 화면
	private void OnShow_MissionObjective(bool isOn = false)
	{
		AddContnet("임무", null, OnStartShow, isOn);
		void OnStartShow(RectTransform layout)
		{
			if (!layout.gameObject.TryFindPair("Item", out TMP_Text itemPrefab))
			{
				return;
			}
			GameObject itemParent = null;
			if (layout.gameObject.TryFindPair("Victory", out GameObject victory))
			{
				itemParent = victory;
				MissionTree mission = StrategyManager.Mission.VictoryMission;
				string missionName = mission.name;
				string description = mission.description;
				mission.Foreach(NodeUI, false);
			}
			if (layout.gameObject.TryFindPair("Defeat", out GameObject defeat))
			{
				itemParent = defeat;
				MissionTree mission = StrategyManager.Mission.VictoryMission;
				string missionName = mission.name;
				string description = mission.description;
				mission.Foreach(NodeUI, false);
			}
			void NodeUI(Node item)
			{
				int indent = item.indent;
				var result = item.IsCmplete();
				string text = item.Description;
				bool enable = item.enable;

				var itemText = GameObject.Instantiate(itemPrefab, itemParent.transform);
				itemPrefab.gameObject.SetActive(true);
				itemPrefab.text = ItemText(indent, result, text);
			}
			string ItemText(int indent, StrategyGamePlayData.MissionTreeData.ResultTyoe result, string text)
			{
				bool isDarkBackground = true;
				int checkBoxIndex = result switch
				{
					StrategyGamePlayData.MissionTreeData.ResultTyoe.Wait => isDarkBackground ? 0 : 1,
					StrategyGamePlayData.MissionTreeData.ResultTyoe.Succeed => isDarkBackground ? 2 : 3,
					StrategyGamePlayData.MissionTreeData.ResultTyoe.Failed => isDarkBackground ? 4 : 5,
					_ => isDarkBackground ? 0 : 1
				};

				return $"<indent={indent}em><sprite=\"icon_checkbox\" index={checkBoxIndex}><indent={indent + 1}.2em>{text}";
			}
		}
	}
	// 미션중에 진행된 스토리 기록 화면
	private void OnShow_Storyboard(string storyName, string storyboardText, bool isOn = false)
	{
		AddContnet($"{storyName}", null, OnStartShow, isOn);
		void OnStartShow(RectTransform layout)
		{

		}
	}
}

public partial class StrategyDetailsPanelUI : DetailsPanelUI // ControlBase UI
{
	public void OnShow_ControlBaseUI(StrategyDetailsPanelType openContent)
	{

	}
	public void OnHide_ControlBaseUI()
	{

	}
	// 선택한 거점의 기본 정보 보기
	private void OnShowControlBaseInfo()
	{
		AddContnet("거점 정보", null, OnStartShow);
		void OnStartShow(RectTransform layout)
		{

		}
	}
}

public partial class StrategyDetailsPanelUI : DetailsPanelUI // BattleUnit UI
{
	public void OnShow_BattleUnitUI(StrategyDetailsPanelType openContent)
	{

	}
	public void OnHide_BattleUnitUI()
	{

	}
}

public partial class StrategyDetailsPanelUI : DetailsPanelUI // BattleSkill UI
{
	public void OnShow_BattleSkillUI(StrategyDetailsPanelType openContent)
	{

	}
	public void OnHide_BattleSkillUI()
	{

	}
}