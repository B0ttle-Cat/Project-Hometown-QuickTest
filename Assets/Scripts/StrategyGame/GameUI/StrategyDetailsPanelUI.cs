using System;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

using static RectUIBuilder;
using static RectUIBuilder.UILayoutBuilder;
using static StrategyGamePlayData;

public partial class StrategyDetailsPanelUI : DetailsPanelUI // // FieldInfo UI
{
	public enum StrategyDetailsPanelType
	{
		None,
		FieldInfo,
		ControlBase,
		BattleUnit,
		BattleSkill,
	}
	public void OpenUI(StrategyDetailsPanelType openContent)
	{
		ClearAllContent();
		switch (openContent)
		{
			case StrategyDetailsPanelType.None:
			break;
			case StrategyDetailsPanelType.FieldInfo:
			OnShow_FieldInfoUI();
			break;
			case StrategyDetailsPanelType.ControlBase:
			OnShow_ControlBaseUI();
			break;
			case StrategyDetailsPanelType.BattleUnit:
			OnShow_BattleUnitUI();
			break;
			case StrategyDetailsPanelType.BattleSkill:
			OnShow_BattleSkillUI();
			break;
			default:
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

public partial class StrategyDetailsPanelUI : DetailsPanelUI // // FieldInfo UI
{
	private const string FieldInfoTabName = "전장 정보";
	private const string MissionObjectiveTabName = "목표";
	public void OnShow_FieldInfoUI()
	{
		OnShow_OverviewAndStatistics();
		OnShow_MissionObjective();

		OnShow_Storyboard("Test", "Test StoryboardText");
	}
	public void OnHide_FieldInfoUI()
	{

	}
	// 개요 및 통개화면
	private void OnShow_OverviewAndStatistics()
	{
		string contentName = $"{FieldInfoTabName}_content";
		AddContnet(FieldInfoTabName, new RectUIBuilder(contentName, contentPanel, Build));

		void Build(GameObject uiObj, UILayoutBuilder layout)
		{
			layout
				.Root().Child("Overview")
					.RectUISize(Diraction.Top, 0f, 300f)
					.VerticalLayout(Overview)
				.Root().Child("Statistics")
					.RectUISize(300f, 0f, 0f, 0f)
					.VerticalLayout(Statistics);

			void Overview(UILayoutBuilder building)
			{
				building
					.Root().Child("TITLE")
						.LayoutElementHeight(minHeight: 300)
						.Component<TMP_Text>((text) =>
						{
							text.text = "TITLE";
						})
					.Root().Child("Line")
						.Component<Image>((i) => i.color = Color.black)
						.LayoutElementHeight(minHeight: 4)
					.Root().Child("Overview")
						.LayoutElementHeight(flexibleHeight: 100000)
						.Scroll_VerticalLayout(ScrollView);

				void ScrollView(ScrollUILayoutBuilder scrollBuilder)
				{
					var contentLayout = scrollBuilder.ContentLayout;
					contentLayout
						.Root().Component<TMP_Text>(text =>
						{
							text.text = "Overview";
						}).ContentSizeFitter(false);
				}
			}
			void Statistics(UILayoutBuilder building)
			{
				building.RectUISize(300f, 0f, 0f, 0f)
					.Scroll_VerticalLayout(ScrollView);
				void ScrollView(ScrollUILayoutBuilder scrollBuilder)
				{
					var contentLayout = scrollBuilder.ContentLayout;
					contentLayout
						.Root().Component<TMP_Text>(text =>
						{
							text.text = "Statistics List";
						}).ContentSizeFitter(false);
				}
			}
		}
	}
	// 미션 정보 화면
	private void OnShow_MissionObjective()
	{
		string contentName = $"{MissionObjectiveTabName}_content";

		AddContnet(FieldInfoTabName, new RectUIBuilder(contentName, contentPanel, Build));

		void Build(GameObject uiObj, UILayoutBuilder layout)
		{
			layout.Child("StrategicObjectives")
				.Child("VictoryConditions").Parent()
				.Child("DefeatConditions").Root();

			layout.Child("SecondaryObjective").Root();
		}
	}
	// 미션중에 진행된 스토리 기록 화면
	private void OnShow_Storyboard(string storyName, string storyboardText)
	{
		string contentName = $"{storyName}_content";
		AddContnet(storyName, new RectUIBuilder(contentName, contentPanel, Build));

		void Build(GameObject uiObj, UILayoutBuilder layout)
		{
			TMP_Text text = uiObj.AddComponent<TMP_Text>();
			text.text = storyboardText;
		}
	}
}

public partial class StrategyDetailsPanelUI : DetailsPanelUI // ControlBase UI
{
	private ControlBaseData controlBaseData;
	private ControlBaseBuildingData controlBaseBuildingData;
	private Action<ControlBaseData.Data, ControlBaseBuildingData.Data> repaintTarget_ControlBase;
	public void OnShow_ControlBaseUI()
	{
		var tempData = StrategyGamePlayData.TempData;

		if (!tempData.TryGetValue(nameof(ControlBaseData), out controlBaseData)) return;
		if (!tempData.TryGetValue(nameof(ControlBaseBuildingData), out controlBaseBuildingData)) return;

		repaintTarget_ControlBase = null;

		OnShowControlBaseInfo();

		controlBaseData.AddListener(RepaintUI);
		controlBaseBuildingData.AddListener(RepaintUI);

		RepaintUI(controlBaseData.GetData());
		RepaintUI(controlBaseBuildingData.GetData());
	}
	public void OnHide_ControlBaseUI()
	{
		if (controlBaseData != null)
		{
			controlBaseData.RemoveListener(RepaintUI);
			controlBaseData = null;
		}
		repaintTarget_ControlBase = null;
	}
	private void RepaintUI(ControlBaseData.Data data)
	{
		repaintTarget_ControlBase?.Invoke(data, controlBaseBuildingData.GetData());
	}
	private void RepaintUI(ControlBaseBuildingData.Data data)
	{
		repaintTarget_ControlBase?.Invoke(controlBaseData.GetData(), data);
	}

	// 선택한 거점의 기본 정보 보기
	private void OnShowControlBaseInfo()
	{
		string tabName = "거점 정보";
		string contentName = $"{tabName}_content";
		AddContnet(tabName, new RectUIBuilder(contentName, contentPanel, Build));
		void Build(GameObject uiObj, UILayoutBuilder layout)
		{
			layout.Root().Child("TopView")
				.LayoutElementHeight(minHeight: 300)
				.HorizontalLayout(TopView_HorizontalLayout);


			void TopView_HorizontalLayout(UILayoutBuilder builder)
			{
				builder.Child("Icon Rect")
					.Component<RectMask2D>()
					.Child("Icon Image")
					.AspectRatioFitter(AspectRatioFitter.AspectMode.EnvelopeParent, 1f)
					.Component<Image>(Draw_IconImage)
				.Root().Child("Building Rect")
					.Component<RectMask2D>()
					.Child("Building Image")
					.AspectRatioFitter(AspectRatioFitter.AspectMode.EnvelopeParent, 1f)
					.Component<Image>(Draw_BuildingImage)
				.Root().Child("ControlBase Status Rect")
					.VerticalLayout(StatusLayout_VerticalLayout);
				void Draw_IconImage(Image i)
				{
					i.color = Color.white;
					i.sprite = null;
					i.type = Image.Type.Simple;
					repaintTarget_ControlBase += (cpData, buildingData) =>
					{
						i.sprite = cpData.iconImage;
					};
				}
				void Draw_BuildingImage(Image i)
				{
					i.color = Color.white;
					i.sprite = null;
					i.type = Image.Type.Simple;
					repaintTarget_ControlBase += (cpData, buildingData) =>
					{
						i.sprite = buildingData.buildingImage;
					};
				}
				void StatusLayout_VerticalLayout(UILayoutBuilder builder, VerticalLayoutGroup layout)
				{
					builder
						.Root().Child("Manpower Text")
							.Component<TMP_Text>(ManpowerText)
						.Root().Child("Manpower Bar")
							.Component<Image>(ManpowerBar)
						
						.Root().Child("SuppliePoint Text")
							.Component<TMP_Text>(SuppliePointText)
						.Root().Child("SuppliePoint Bar")
							.Component<Image>(SuppliePointBar)

						.Root().Child("ElectricPoint Text")
							.Component<TMP_Text>(ElectricPointText)
						.Root().Child("ElectricPoint Bar")
							.Component<Image>(ElectricPointBar)
						;
						   // TODO) 아이콘 집어 넣어 야 함.
					void ManpowerText(TMP_Text text)
					{
						repaintTarget_ControlBase += (cpData, buildingData) =>
						{
							float value = cpData.reservesManpower;
							float maxValue = cpData.maxManpower + buildingData.maxManpower;

							text.text = $"{value} / {maxValue}";
						};
					}
					void SuppliePointText(TMP_Text text)
					{
						repaintTarget_ControlBase += (cpData, buildingData) =>
						{
							float value = cpData.reservesSupplie;
							float maxValue = cpData.maxSuppliePoint + buildingData.maxSuppliePoint;

							text.text = $"{value} / {maxValue}";
						};
					}
					void ElectricPointText(TMP_Text text)
					{
						repaintTarget_ControlBase += (cpData, buildingData) =>
						{
							float value = cpData.reservesElectric;
							float maxValue = cpData.maxElectricPoint + buildingData.maxElectricPoint;

							text.text = $"{value} / {maxValue}";
						};
					}
					void ManpowerBar(Image bar)
					{
						bar.type = Image.Type.Filled; 
						bar.fillMethod = Image.FillMethod.Horizontal;
						bar.fillOrigin = 0;

						repaintTarget_ControlBase += (cpData, buildingData) =>
						{
							float value = cpData.reservesManpower;
							float maxValue = cpData.maxManpower + buildingData.maxManpower;

							float fillArmount = maxValue <=0 ? 0 : value / maxValue;
							bar.fillAmount = fillArmount;
						};
					}
					void SuppliePointBar(Image bar)
					{
						bar.type = Image.Type.Filled;
						bar.fillMethod = Image.FillMethod.Horizontal;
						bar.fillOrigin = 0;

						repaintTarget_ControlBase += (cpData, buildingData) =>
						{
							float value = cpData.reservesSupplie;
							float maxValue = cpData.maxSuppliePoint + buildingData.maxSuppliePoint;

							float fillArmount = maxValue <=0 ? 0 : value / maxValue;
							bar.fillAmount = fillArmount;
						};
					}
					void ElectricPointBar(Image bar)
					{
						bar.type = Image.Type.Filled;
						bar.fillMethod = Image.FillMethod.Horizontal;
						bar.fillOrigin = 0;

						repaintTarget_ControlBase += (cpData, buildingData) =>
						{
							float value = cpData.reservesElectric;
							float maxValue = cpData.maxElectricPoint+ buildingData.maxElectricPoint;

							float fillArmount = maxValue <=0 ? 0 : value / maxValue;
							bar.fillAmount = fillArmount;
						};
					}
				}
			}
		}
	}
}

public partial class StrategyDetailsPanelUI : DetailsPanelUI // BattleUnit UI
{
	public void OnShow_BattleUnitUI()
	{

	}
	public void OnHide_BattleUnitUI()
	{

	}
}

public partial class StrategyDetailsPanelUI : DetailsPanelUI // BattleSkill UI
{
	public void OnShow_BattleSkillUI()
	{

	}
	public void OnHide_BattleSkillUI()
	{

	}
}



public static class ExpandUIBuilder
{
	public static UILayoutBuilder HorizontalStatusBar(this UILayoutBuilder builder,
		float fillArmount, bool leftToRight)
	{
		Image image = null;
		image.type = Image.Type.Filled;
		image.fillMethod = Image.FillMethod.Horizontal;
		image.fillOrigin = leftToRight ? 0 : 1;
		image.fillAmount = fillArmount;
		return builder;
	}
}