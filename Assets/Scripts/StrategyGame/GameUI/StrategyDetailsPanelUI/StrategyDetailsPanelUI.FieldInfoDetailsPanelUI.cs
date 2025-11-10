using System;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using TMPro;

using UnityEngine;

using static StrategyMissionTree;

public partial class StrategyDetailsPanelUI // FieldInfoDetailsPanelUI
{
	[FoldoutGroup("전장 정보 UI"), SerializeField, HideInPlayMode, InlineProperty, HideLabel]
	private FieldInfoDetailsPanelUI.UIPrefabStruct fieldInfoPrefabs;
	[FoldoutGroup("전장 정보 UI"), SerializeField, InlineProperty, HideLabel]
	private FieldInfoDetailsPanelUI fieldInfoDetailsPanelUI;
	public void OnShowFieldInfoDetails(FieldInfoDetailsPanelUI.DetailsType detailsType = FieldInfoDetailsPanelUI.DetailsType.None)
	{
		fieldInfoDetailsPanelUI = new FieldInfoDetailsPanelUI(this, fieldInfoPrefabs);
		fieldInfoDetailsPanelUI.OnShowFieldInfoDetails(detailsType);
	}
	public void OnHideFieldInfoDetails()
	{
		if (fieldInfoDetailsPanelUI == null) return;
		fieldInfoDetailsPanelUI.OnHideFieldInfoDetails();
		fieldInfoDetailsPanelUI.Dispose();
		fieldInfoDetailsPanelUI = null;
	}

	[Serializable, InlineProperty, HideLabel]
	public class FieldInfoDetailsPanelUI : IDisposable
	{
		[Serializable]
		public struct UIPrefabStruct
		{
			public RectTransform overview;
			public RectTransform statistics;
			public RectTransform mission;
			public RectTransform storyboard;
		}
		public enum DetailsType
		{
			None,
			FieldInfo_Overview,
			FieldInfo_Statistics,
			FieldInfo_MainMission,
			FieldInfo_MainStory,
		}

		private  StrategyDetailsPanelUI ThisPanelUI;
		private UIPrefabStruct uiPrefabStruct;
		public FieldInfoDetailsPanelUI(StrategyDetailsPanelUI strategyDetailsPanelUI, UIPrefabStruct uiPrefabStruct)
		{
			ThisPanelUI = strategyDetailsPanelUI;
			this.uiPrefabStruct = uiPrefabStruct;
		}
		public void Dispose()
		{
			ThisPanelUI = null;
		}

		[SerializeField]
		private DetailsType lastDetailsType;
        [ShowInInspector, HideInEditorMode]
		private FieldInfo_Overview fieldInfoDetails_Overview { get; set; }
		[ShowInInspector, HideInEditorMode]
		private FieldInfo_Statistics fieldInfoDetails_Statistics { get; set; }
		[ShowInInspector, HideInEditorMode]
		private FieldInfo_Mission fieldInfoDetails_MainMission { get; set; }
		[ShowInInspector, HideInEditorMode]
		private FieldInfo_Storyboard fieldInfoDetails_Storyboard { get; set; }
		public void OnShowFieldInfoDetails(DetailsType detailsType = DetailsType.None)
		{
			//StrategyManager.GamePlayData

			fieldInfoDetails_Overview?.Dispose();
			fieldInfoDetails_Statistics?.Dispose();
			fieldInfoDetails_MainMission?.Dispose();
			fieldInfoDetails_Storyboard?.Dispose();

			fieldInfoDetails_Overview = new FieldInfo_Overview(ThisPanelUI, uiPrefabStruct.overview);
			fieldInfoDetails_Statistics = new FieldInfo_Statistics(ThisPanelUI, uiPrefabStruct.statistics);
			fieldInfoDetails_MainMission = new FieldInfo_Mission(ThisPanelUI, uiPrefabStruct.mission);
			fieldInfoDetails_Storyboard = new FieldInfo_Storyboard(ThisPanelUI, uiPrefabStruct.storyboard);

			ThisPanelUI.tabControl.ClearTab();
			ThisPanelUI.tabControl.AddTab(("", null),
				("개요", ShowContnet_Overview),
				("게임 통계", ShowContnet_Statistics),
				("핵심 임무", ShowContnet_Mission),
				("이야기", ShowContnet_Storyboard)
				);

			if (detailsType == DetailsType.None) detailsType = lastDetailsType;
			Action onShow = detailsType switch
			{
				DetailsType.FieldInfo_Overview  => ShowContnet_Overview,
				DetailsType.FieldInfo_Statistics => ShowContnet_Statistics,
				DetailsType.FieldInfo_MainMission => ShowContnet_Mission,
				DetailsType.FieldInfo_MainStory => ShowContnet_Storyboard,
				_ => ShowContnet_Overview,
			};
			onShow();
			void ShowContnet_Overview()
			{
				lastDetailsType = DetailsType.FieldInfo_Overview;

				fieldInfoDetails_Statistics.Hide();
				fieldInfoDetails_MainMission.Hide();
				fieldInfoDetails_Storyboard.Hide();

				fieldInfoDetails_Overview.Show();
			}
			void ShowContnet_Statistics()
			{
				lastDetailsType = DetailsType.FieldInfo_Statistics;

				fieldInfoDetails_Overview.Hide();
				fieldInfoDetails_MainMission.Hide();
				fieldInfoDetails_Storyboard.Hide();

				fieldInfoDetails_Statistics.Show();
			}
			void ShowContnet_Mission()
			{
				lastDetailsType = DetailsType.FieldInfo_MainMission;

				fieldInfoDetails_Overview.Hide();
				fieldInfoDetails_Statistics.Hide();
				fieldInfoDetails_Storyboard.Hide();

				fieldInfoDetails_MainMission.Show();
			}
			void ShowContnet_Storyboard()
			{
				lastDetailsType = DetailsType.FieldInfo_MainStory;

				fieldInfoDetails_Overview.Hide();
				fieldInfoDetails_Statistics.Hide();
				fieldInfoDetails_MainMission.Hide();

				fieldInfoDetails_Storyboard.Show();
			}
		}
		public void OnHideFieldInfoDetails()
		{
			fieldInfoDetails_Overview?.Dispose();
			fieldInfoDetails_Statistics?.Dispose();
			fieldInfoDetails_MainMission?.Dispose();
			fieldInfoDetails_Storyboard?.Dispose();

			fieldInfoDetails_Overview = null;
			fieldInfoDetails_Statistics = null;
			fieldInfoDetails_MainMission = null;
			fieldInfoDetails_Storyboard = null;

		}
		public abstract class FieldInfoContentPanel : DetailsContentPanel
		{
			protected FieldInfoContentPanel(StrategyDetailsPanelUI thisPanel, RectTransform contentPrefab)
				: base(thisPanel, contentPrefab)
			{
			}
		}
		[Serializable]
		public class FieldInfo_Overview : FieldInfoContentPanel
		{
			public FieldInfo_Overview(StrategyDetailsPanelUI thisPanel, RectTransform contentPrefab)
				: base(thisPanel, contentPrefab)
			{
			}

			protected override void OnDispose()
			{
			}
			protected override void OnShow()
			{
				ref readonly var data = ref StrategyManager.PreparedData.ReadonlyData();
				var overviewData = data.overview;

				if (KeyPair.TryFindPair<TMP_Text>("Title", out var title))
				{
					title.text = overviewData.title;
				}

				if (KeyPair.TryFindPair<TMP_Text>("Content", out var overview))
				{
					overview.text = overviewData.description;
				}
			}
			protected override void OnHide()
			{
			}

		}
		[Serializable]
		public class FieldInfo_Statistics : FieldInfoContentPanel
		{
			[SerializeField,ReadOnly]
			private GameObject contentParent;
			[SerializeField,ReadOnly]
			private GameObject groupPrefab;
			[SerializeField,ReadOnly]
			private GameObject itemPrefab;
			[SerializeField,ReadOnly]
			private List<StatisticsGroup> statisticsGroups = new List<StatisticsGroup>();
			public abstract class StatisticsNode : IDisposable
			{
				public abstract void Dispose();
			}
			[Serializable]
			public class StatisticsGroup : StatisticsNode, IDisposable
			{
				[SerializeField,ReadOnly]
				private GameObject group;
				[SerializeField,ReadOnly]
				private TMP_Text groupText;
				[SerializeField,ReadOnly]
				private List<StatisticsItem> nodes = new List<StatisticsItem>();
				public StatisticsGroup(GameObject prefab, Transform parent, string groupName)
				{
					group = GameObject.Instantiate(prefab, parent);
					group.SetActive(true);
					group.name = $"Group_{groupName}";
					groupText = group.GetComponentInChildren<TMP_Text>();
					nodes = new List<StatisticsItem>();

					if (groupText != null)
					{
						groupText.text = groupName;
					}
				}
				public override void Dispose()
				{
					if (nodes != null)
					{
						int length = nodes.Count;
						for (int i = 0 ; i < length ; i++)
						{
							var node = nodes[i];
							if (node == null) continue;
							node.Dispose();
						}
						nodes = null;
					}

					GameObject.Destroy(group);
					group = null;
					groupText = null;
				}
				public StatisticsGroup AddNode(StatisticsItem item)
				{
					if (item == null) return this;
					(nodes ??= new List<StatisticsItem>()).Add(item);
					return this;
				}
				public StatisticsGroup AddItem(IEnumerable<StatisticsItem> items)
				{
					if (items == null) return this;
					foreach (var item in items)
					{
						AddNode(item);
					}
					return this;
				}
			}
			[Serializable]
			public class StatisticsItem : StatisticsNode, IDisposable
			{
				private GameObject item;
				private TMP_Text itemText;
				private string catagory;
				private string itemKey;

				private string displayName;
				public StatisticsItem(GameObject prefab, Transform parent, string catagory, string itemKey)
				{
					item = GameObject.Instantiate(prefab, parent);
					item.SetActive(true);
					item.name = $"Item_{itemKey}";
					itemText = item.GetComponentInChildren<TMP_Text>();

					this.catagory = catagory;
					this.itemKey = itemKey;
					displayName = StrategyManager.Key2Name[itemKey];

					if (itemText == null) return;
					StrategyManager.Statistics.AddListener_ToString(catagory, itemKey, OnChangeStatsValueString, true);
				}
				void OnChangeStatsValueString(string value)
				{
					if (itemText == null) return;

					itemText.text =
						$"<indent=20%><line-height=0>{displayName}" +
						$"\n<align=right>{value}";
				}
				public override void Dispose()
				{
					StrategyManager.Statistics.RemoveListener_ToString(catagory, itemKey, OnChangeStatsValueString);
					if (item != null)
					{
						GameObject.Destroy(item);
						item = null;
					}
					itemText = null;
					displayName = null;
					catagory = null;
					itemKey = null;
				}
			}

			public FieldInfo_Statistics(StrategyDetailsPanelUI thisPanel, RectTransform contentPrefab)
				: base(thisPanel, contentPrefab)
			{
				if (!KeyPair.TryFindPair("Content", out contentParent)) return;
				if (!KeyPair.TryFindPair("Group", out groupPrefab)) return;
				if (!KeyPair.TryFindPair("KeyValue", out itemPrefab)) return;
			}
			protected override void OnDispose()
			{
				contentParent = null;
				groupPrefab = null;
				itemPrefab = null;
				ClearStatisticsList();
			}
			protected override void OnShow()
			{
				ClearStatisticsList();
				statisticsGroups = new List<StatisticsGroup>();

				NewGroup("파괴한 적의 유닛")
					.AddItem(NewItems(StatsCatagory(StatsKey.InGamePlay.DestroyCount_Unit, "Enemy")));

				NewGroup("파괴된 나의 유닛")
					.AddItem(NewItems(StatsCatagory(StatsKey.InGamePlay.DestroyCount_Unit, "Player")));
			}
			protected override void OnHide()
			{
				ClearStatisticsList();
			}
			private void ClearStatisticsList()
			{
				if (statisticsGroups != null)
				{
					int length = statisticsGroups.Count;
					for (int i = 0 ; i < length ; i++)
					{
						var group = statisticsGroups[i];
						if (group == null) continue;
						group.Dispose();
					}
					statisticsGroups = null;
				}
			}

			StatisticsGroup NewGroup(string text)
			{
				StatisticsGroup newGroup = new StatisticsGroup(groupPrefab, contentParent.transform, text);
				statisticsGroups.Add(newGroup);
				return newGroup;
			}
			StatisticsItem NewItem(string catagory, string itemKey)
			{
				StatisticsItem nweItem = new StatisticsItem(itemPrefab, contentParent.transform, catagory, itemKey);
				return nweItem;
			}
			List<(string, string)> StatsCatagory(params string[] args)
			{
				string catagory = StatsKey.JoinPath(args);
				return StrategyManager.Statistics.SelectKeyList(key =>
				{
					return key.catagory.Equals(catagory);
				});
			}
			IEnumerable<StatisticsItem> NewItems(IEnumerable<(string catagory, string key)> items)
			{
				foreach ((string catagory, string key) item in items) yield return NewItem(item.catagory, item.key);
			}
		}
		[Serializable]
		public class FieldInfo_Mission : FieldInfoContentPanel
		{
			[SerializeField,ReadOnly]
			private MissionTreeView victoryMissionView;
			[SerializeField,ReadOnly]
			private MissionTreeView defeatMissionView;

			[Serializable]
			public class MissionTreeView : IDisposable
			{
				[SerializeField,ReadOnly]
				private MissionTree mission;
				[SerializeField,ReadOnly]
				private List<TMP_Text> nodes;

				public MissionTreeView(MissionTree mission, GameObject viewObject, TMP_Text itemPrefab)
				{
					if (viewObject == null || itemPrefab == null || mission == null) return;

					this.mission = mission;
					this.nodes = new List<TMP_Text>();

					mission.Foreach(item => NodeUI(item, viewObject.transform, itemPrefab), false);
				}
				void NodeUI(Node item, Transform itemParent, TMP_Text itemPrefab)
				{
					int indent = item.indent;
					var result = item.IsCmplete();
					string text = item.Description;
					bool enable = item.enable;

					var itemText = GameObject.Instantiate(itemPrefab, itemParent);
					itemText.gameObject.SetActive(true);
					itemText.text = ItemText(indent, result, text);
					nodes.Add(itemText);
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
				public void Dispose()
				{
					mission = null;
					if (nodes != null)
					{
						int length = nodes.Count;
						for (int i = 0 ; i < length ; i++)
						{
							var node = nodes[i];
							if (node == null) continue;
							GameObject.Destroy(node.gameObject);
						}
						nodes = null;
					}
				}
			}

			public FieldInfo_Mission(StrategyDetailsPanelUI thisPanel, RectTransform contentPrefab)
				: base(thisPanel, contentPrefab)
			{
				victoryMissionView = null;
				defeatMissionView = null;
			}
			protected override void OnDispose()
			{
				victoryMissionView?.Dispose();
				victoryMissionView = null;

				defeatMissionView?.Dispose();
				defeatMissionView = null;
			}
			protected override void OnShow()
			{
				KeyPair.FindPairChain<TMP_Text>("KeyValue", out var itemPrefab);
				if (itemPrefab == null) return;

				victoryMissionView?.Dispose();
				defeatMissionView?.Dispose();

				if (KeyPair.TryFindPair("Victory", out GameObject victory))
				{
					victoryMissionView = new MissionTreeView(StrategyManager.Mission.VictoryMission, victory, itemPrefab);
				}
				if (KeyPair.TryFindPair("Defeat", out GameObject defeat))
				{
					defeatMissionView = new MissionTreeView(StrategyManager.Mission.DefeatMission, defeat, itemPrefab);
				}
			}
			protected override void OnHide()
			{
				victoryMissionView?.Dispose();
				victoryMissionView = null;

				defeatMissionView?.Dispose();
				defeatMissionView = null;
			}
		}
		[Serializable]
		public class FieldInfo_Storyboard : FieldInfoContentPanel
		{
			public FieldInfo_Storyboard(StrategyDetailsPanelUI thisPanel, RectTransform contentPrefab)
				: base(thisPanel, contentPrefab)
			{
			}
			protected override void OnDispose()
			{
			}
			protected override void OnShow()
			{
			}
			protected override void OnHide()
			{
			}
		}
	}
}