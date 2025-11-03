using System;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using TMPro;

using UnityEngine;

using static StrategyMissionTree;

public partial class StrategyDetailsPanelUI // FieldInfoViewController UI
{
	[Serializable]
	public struct FieldInfoUIPrefab
	{
		public RectTransform overview;
		public RectTransform statistics;
		public RectTransform mission;
		public RectTransform storyboard;
	}
	[FoldoutGroup("전장 정보 UI"), InlineProperty,PropertyOrder(9), HideLabel]
	public FieldInfoUIPrefab fieldInfoUIPrefab;
	public class FieldInfoViewController : StrategyContentController
	{
		public FieldInfoViewController(StrategyDetailsPanelUI component) : base(component)
		{
		}
		FieldInfoUIPrefab UIPrefab => ThisComponent.fieldInfoUIPrefab;

		public override void OnShow()
		{
			ThisComponent.ContentTitleText("전장 정보");
			AddTabAndContnet_Overview(initContent == StrategyDetailsPanelType.FieldInfo_Overview);
			AddTabAndContnet_Statistics(initContent == StrategyDetailsPanelType.FieldInfo_Statistics);
			AddTabAndContnet_Mission(initContent == StrategyDetailsPanelType.FieldInfo_MainMission);
		}
		public override void OnHide()
		{

		}
		// 개요 화면
		private void AddTabAndContnet_Overview(bool isOn = false)
		{
			ThisComponent.AddTabAndContnet<FieldInfo_Overview>("개요", UIPrefab.overview, this, isOn);
		}
		// 통개 화면
		private void AddTabAndContnet_Statistics(bool isOn = false)
		{
			ThisComponent.AddTabAndContnet<FieldInfo_Statistics>("통계", UIPrefab.statistics, this, isOn);
		}
		// 미션 정보 화면
		private void AddTabAndContnet_Mission(bool isOn = false)
		{
			ThisComponent.AddTabAndContnet<FieldInfo_Mission>("임무", UIPrefab.mission, this, isOn);
		}
		// 미션중에 진행된 스토리 기록 화면
		private void AddTabAndContnet_Storyboard(string storyName, string storyboardText, bool isOn = false)
		{
			ThisComponent.AddTabAndContnet<FieldInfo_Storyboard>($"{storyName}", UIPrefab.storyboard, this, isOn);
		}
	}

	public class FieldInfo_Overview : StrategyViewController
	{
		public override void OnShow(RectTransform viewRect)
		{
			var data = StrategyGamePlayData.PreparedData.GetData();
			var overviewData = data.overview;

			if (viewRect.gameObject.TryFindPair<TMP_Text>("Title", out var title))
			{
				title.text = overviewData.title;
			}

			if (viewRect.gameObject.TryFindPair<TMP_Text>("Content", out var overview))
			{
				overview.text = overviewData.description;
			}
		}
		public override void OnHide()
		{

		}
		public override void OnDispose()
		{
		}
	}
	public class FieldInfo_Statistics : StrategyViewController
	{
		Action onDisconnectListener;
		public override void OnShow(RectTransform viewRect)
		{
			if (!viewRect.gameObject.TryFindPair("Content", out var contentParent)) return;
			if (!viewRect.gameObject.TryFindPair("Group", out var groupPrefab)) return;
			if (!viewRect.gameObject.TryFindPair("KeyValue", out var itemPrefab)) return;

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
			void NewItems(IEnumerable<(string catagory, string key)> items)
			{
				foreach ((string catagory, string key) item in items) NewItem(item.catagory, item.key);
			}
			void NewItem(string catagory, string itemKey)
			{
				var newObject = GameObject.Instantiate(itemPrefab, contentParent.transform);
				newObject.SetActive(true);
				TMP_Text textUI = newObject.GetComponentInChildren<TMP_Text>();
				if (textUI == null)
				{
					Destroy(newObject);
					return;
				}
				string displayName = StrategyManager.Key2Name.GetAsset(itemKey);

				StrategyManager.Statistics.AddListener_ToString(catagory, itemKey, OnChangeStatsValueString, true);
				onDisconnectListener += () => StrategyManager.Statistics.RemoveListener_ToString(catagory, itemKey, OnChangeStatsValueString);

				void OnChangeStatsValueString(string value)
				{
					textUI.text =
						$"<indent=20%><line-height=0>{displayName}" +
						$"\n<align=right>{value}";
				}
			}
		}
		public override void OnHide()
		{
			if (onDisconnectListener != null)
			{
				onDisconnectListener();
				onDisconnectListener = null;
			}
		}
		public override void OnDispose()
		{
			if (onDisconnectListener != null)
			{
				onDisconnectListener();
				onDisconnectListener = null;
			}
		}
	}
	public class FieldInfo_Mission : StrategyViewController
	{
		public override void OnShow(RectTransform viewRect)
		{
			var findPairChain = viewRect.gameObject.GetPairChain();
			if (findPairChain == null) return;

			findPairChain.FindPairChain<TMP_Text>("KeyValue", out var itemPrefab);
			if (itemPrefab == null) return;

			GameObject itemParent = null;
			
			findPairChain.FindPairChain("Victory", out GameObject victory);
			if (victory != null)
			{
				itemParent = victory;
				MissionTree mission = StrategyManager.Mission.VictoryMission;
				string missionName = mission.name;
				string description = mission.description;
				mission.Foreach(NodeUI, false);
			}

			findPairChain.FindPairChain("Defeat", out GameObject defeat);
			if (defeat != null)
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
		public override void OnHide()
		{

		}
		public override void OnDispose()
		{
		}
	}
	public class FieldInfo_Storyboard : StrategyViewController
	{
		public override void OnShow(RectTransform viewRect)
		{

		}
		public override void OnHide()
		{

		}
		public override void OnDispose()
		{
		}
	}
}
