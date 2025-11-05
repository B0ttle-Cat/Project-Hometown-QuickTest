using System;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using TMPro;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using static StrategyGamePlayData;

public partial class StrategyDetailsPanelUI // SectorDetailsPanelUI
{
	[FoldoutGroup("거점 정보 UI"), SerializeField, HideInPlayMode, InlineProperty, HideLabel]
	private SectorDetailsPanelUI.UIPrefabStruct sectorPrefabs;
	[FoldoutGroup("거점 정보 UI"), SerializeField, InlineProperty, HideLabel]
	private SectorDetailsPanelUI sectorDetailsPanelUI;

	public void OnShowSectorDetail(SectorObject sectorObject, SectorDetailsPanelUI.DetailsType detailsType = SectorDetailsPanelUI.DetailsType.None)
	{
		sectorDetailsPanelUI = new SectorDetailsPanelUI(this, sectorPrefabs);
		sectorDetailsPanelUI.OnShowSectorDetail(sectorObject, detailsType);
	}
	public void OnHideSectorDetail()
	{
		if (sectorDetailsPanelUI == null) return;
		sectorDetailsPanelUI.OnHideSectorDetail();
		sectorDetailsPanelUI.Dispose();
		sectorDetailsPanelUI = null;
	}

	[Serializable, InlineProperty, HideLabel]
	public class SectorDetailsPanelUI : IDisposable
	{
		[Serializable]
		public struct UIPrefabStruct
		{
			public RectTransform mainInfo;
			public RectTransform support;
			public RectTransform facilities;
			public RectTransform troops;
		}
		public enum DetailsType
		{
			None,
			MainInfo,
			Support,
			Facilities,
			Troops,
		}

		private  StrategyDetailsPanelUI ThisPanelUI;
		private UIPrefabStruct uiPrefabStruct;
		public SectorDetailsPanelUI(StrategyDetailsPanelUI strategyDetailsPanelUI, UIPrefabStruct uiPrefabStruct)
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
		private SectorDetails_Info sectorDetails_MainInfo { get; set; }
		[ShowInInspector, HideInEditorMode]
		private SectorDetails_Support sectorDetails_Support { get; set; }
		[ShowInInspector, HideInEditorMode]
		private SectorDetails_Facilities sectorDetails_Facilities { get; set; }
		[ShowInInspector, HideInEditorMode]
		private SectorDetails_Troops sectorDetails_Troops { get; set; }
		public void OnShowSectorDetail(SectorObject sectorObject, DetailsType detailsType = DetailsType.None)
		{
			sectorDetails_MainInfo?.Dispose();
			sectorDetails_Support?.Dispose();
			sectorDetails_Facilities?.Dispose();
			sectorDetails_Troops?.Dispose();

			sectorDetails_MainInfo = new SectorDetails_Info(sectorObject, ThisPanelUI, uiPrefabStruct.mainInfo);
			sectorDetails_Support = new SectorDetails_Support(sectorObject, ThisPanelUI, uiPrefabStruct.support);
			sectorDetails_Facilities = new SectorDetails_Facilities(sectorObject, ThisPanelUI, uiPrefabStruct.facilities);
			sectorDetails_Troops = new SectorDetails_Troops(sectorObject, ThisPanelUI, uiPrefabStruct.troops);

			ThisPanelUI.tabControl.ClearTab();
			ThisPanelUI.tabControl.AddTab(("", null),
				("거점 정보", ShowTabAndContnet_Info),
				("지원 정책", ShowTabAndContnet_Support),
				("설치 시설", ShowTabAndContnet_Facilities),
				("주둔 병력", ShowTabAndContnet_Troops)
				);

			if (detailsType == DetailsType.None) detailsType = lastDetailsType;
			Action onShow = detailsType switch
			{
				DetailsType.MainInfo => ShowTabAndContnet_Info,
				DetailsType.Support => ShowTabAndContnet_Support,
				DetailsType.Facilities => ShowTabAndContnet_Facilities,
				DetailsType.Troops => ShowTabAndContnet_Troops,
				_ => ShowTabAndContnet_Info,
			};
			onShow();
			void ShowTabAndContnet_Info()
			{
				lastDetailsType = DetailsType.MainInfo;

				sectorDetails_Support.Hide();
				sectorDetails_Facilities.Hide();
				sectorDetails_Troops.Hide();

				sectorDetails_MainInfo.Show();
			}
			void ShowTabAndContnet_Support()
			{
				lastDetailsType = DetailsType.Support;

				sectorDetails_MainInfo.Hide();
				sectorDetails_Facilities.Hide();
				sectorDetails_Troops.Hide();

				sectorDetails_Support.Show();
			}
			void ShowTabAndContnet_Facilities()
			{
				lastDetailsType = DetailsType.Support;

				sectorDetails_MainInfo.Hide();
				sectorDetails_Support.Hide();
				sectorDetails_Troops.Hide();

				sectorDetails_Facilities.Show();
			}
			void ShowTabAndContnet_Troops()
			{
				lastDetailsType = DetailsType.Troops;

				sectorDetails_MainInfo.Hide();
				sectorDetails_Support.Hide();
				sectorDetails_Facilities.Hide();

				sectorDetails_Troops.Show();
			}
		}
		public void OnHideSectorDetail()
		{
			sectorDetails_MainInfo?.Dispose();
			sectorDetails_Support?.Dispose();
			sectorDetails_Facilities?.Dispose();
			sectorDetails_Troops?.Dispose();

			sectorDetails_MainInfo = null;
			sectorDetails_Support = null;
			sectorDetails_Facilities = null;
			sectorDetails_Troops = null;
		}
		public abstract class SectorContentPanel : DetailsContentPanel
		{
			protected SectorObject selectSector;
			protected SectorContentPanel(SectorObject selectSector, StrategyDetailsPanelUI thisPanel, RectTransform contentPrefab) : base(thisPanel, contentPrefab)
			{
				this.selectSector = selectSector;
			}
			public override void Show()
			{
				if (selectSector == null) return;
				base.Show();
			}
			public override void Hide()
			{
				if (selectSector == null) return;
				base.Hide();
			}
			public override void Dispose()
			{
				base.Dispose();
				selectSector = null;
			}

			protected virtual void SetFillRectUI(string fillRectName, (int value, int max) item, string noneText)
			{
				int value = item.value;
				int max = item.max;
				PairChain.FindPairChain<FillRectUI>(fillRectName, out var fillRectUI);
				if (fillRectUI == null) return;

				if (max > 0)
				{
					float rate = (float)value / (float)max;
					if (!fillRectUI.gameObject.activeSelf)
						fillRectUI.gameObject.SetActive(true);
					fillRectUI.SetValueText(rate, $"{value,10} / {max,-10}");
				}
				else
				{
					if (fillRectUI.gameObject.activeSelf)
						fillRectUI.gameObject.SetActive(false);
				}
			}
			protected virtual void SetFillRectUIAndLabel(string fillRectName, (int value, int max) item, string noneText)
			{
				int value = item.value;
				int max = item.max;
				PairChain.FindPairChain<FillRectUIAndLabel>(fillRectName, out var fillRectUI);
				if (fillRectUI == null) return;

				if (max > 0)
				{
					float rate = (float)value / (float)max;
					if (!fillRectUI.gameObject.activeSelf)
						fillRectUI.gameObject.SetActive(true);
					fillRectUI.SetValueText(rate, $"{value,10} / {max,-10}");
				}
				else
				{
					if (fillRectUI.gameObject.activeSelf)
						fillRectUI.gameObject.SetActive(false);
				}
			}
			protected virtual void DeleteStatsItemList(Dictionary<StatsType, LabelTextUI> list)
			{
				if (list != null)
				{
					int oldLength = list.Count;
					foreach (var item in list)
					{
						if (item.Value != null)
						{
							GameObject.DestroyImmediate(item.Value.gameObject);
						}
					}
					list.Clear();
				}
			}
			protected virtual LabelTextUI NewLabelTextUI(string pairKey, string label, string text, Transform parent)
			{
				PairChain.FindPairChainAndCopy<LabelTextUI>(pairKey, parent, out var statsItem);
				if (statsItem == null) return null;
				statsItem.gameObject.SetActive(true);
				statsItem.SetText(label, text);
				return statsItem;
			}
			protected virtual void SetupSlider(GameObject parentPanel, int value, UnityAction<float> onChangeValue)
			{
				if (parentPanel == null) return;
				Slider slider = parentPanel.GetComponentInChildren<Slider>();
				if (slider == null) return;

				slider.onValueChanged.RemoveAllListeners();
				slider.value = value;
				slider.onValueChanged.AddListener(onChangeValue);
			}
		}

		[Serializable]
		public class SectorDetails_Info : SectorContentPanel
		{
			[ShowInInspector,ReadOnly]
			private Dictionary<StatsType, LabelTextUI> detailStatsItemList;
			[ShowInInspector,ReadOnly]
			private Dictionary<StatsType, LabelTextUI> facilitiesStateItemList;
			[ShowInInspector,ReadOnly]
			private Dictionary<StatsType, LabelTextUI> supportStatsItemList;

			public SectorDetails_Info(SectorObject selectSector, StrategyDetailsPanelUI thisPanel, RectTransform contentPrefab)
				: base(selectSector, thisPanel, contentPrefab)
			{
				detailStatsItemList ??= new();
				facilitiesStateItemList ??= new();
				supportStatsItemList ??= new();
			}
			protected override void OnDispose()
			{
				if (selectSector != null)
				{
					selectSector.Profile.RemoveListener(OnChangeProfileData);
					selectSector.Capture.RemoveListener(OnChangeCaptureData);
					selectSector.Stats.RemoveListener(OnChangeStatsData);
					selectSector.Facilities.RemoveListener(OnChangeFacilitiesData);
					selectSector.Support.RemoveListener(OnChangeSupportData);
				}

				DeleteStatsItemList(detailStatsItemList);
				DeleteStatsItemList(facilitiesStateItemList);
				DeleteStatsItemList(supportStatsItemList);
				detailStatsItemList = null;
				facilitiesStateItemList = null;
				supportStatsItemList = null;
			}
			protected override void OnShow()
			{
				selectSector.Profile.AddLateListener(OnChangeProfileData);
				selectSector.Capture.AddLateListener(OnChangeCaptureData);
				selectSector.Stats.AddLateListener(OnChangeStatsData);
				selectSector.Facilities.AddLateListener(OnChangeFacilitiesData);
				selectSector.Support.AddLateListener(OnChangeSupportData);

				OnChangeProfileData(selectSector.ProfileData);
				OnChangeCaptureData(selectSector.CaptureData);
				OnChangeStatsData(selectSector.StatsData);
				OnChangeFacilitiesData(selectSector.FacilitiesData);
				OnChangeSupportData(selectSector.SupportData);
			}

			protected override void OnHide()
			{
				selectSector.Profile.RemoveListener(OnChangeProfileData);
				selectSector.Capture.RemoveListener(OnChangeCaptureData);
				selectSector.Stats.RemoveListener(OnChangeStatsData);
				selectSector.Facilities.RemoveListener(OnChangeFacilitiesData);
				selectSector.Support.RemoveListener(OnChangeSupportData);
			}

			void OnChangeProfileData(SectorData.Profile.Data data)
			{
				PairChain
					.FindPairChain<Image>("MainImage", out var mainImage)
					.FindPairChain<TMP_Text>("NameText", out var nameText)
					.FindPairChain<TMP_Text>("EffectText", out var effectText)
					;

				if (mainImage != null)
				{
					mainImage.sprite = StrategyManager.Key2Sprite.GetAsset(data.environmentalKey);
				}
				if (nameText != null)
				{
					nameText.text = data.sectorName;
				}
				if (effectText != null)
				{
					effectText.text = data.EffectString();
				}
			}
			void OnChangeCaptureData(SectorData.Capture.Data data)
			{
				PairChain
				  .FindPairChain<TMP_Text>("CaptureText", out var captureText)
				  ;

				if (captureText != null)
				{
					if (StrategyManager.Collector.TryFindFaction(data.captureFactionID, out var faction))
					{
						captureText.text = faction.FactionName;
					}
					else
					{
						captureText.text = "중립 지역";
					}
				}
			}
			void OnChangeStatsData(SectorData.MainStats.Data data)
			{
				UpdateFillRectUI();
				UpdateStatePanel("Stats KeyValue", "Base Stats", selectSector.MainStatsList?.GetValueList());
			}
			void OnChangeFacilitiesData(SectorData.Facilities.Data data)
			{
				UpdateFillRectUI();
				UpdateStatePanel("Stats KeyValue", "Facilities Stats", selectSector.FacilitiesBuffGroup.GetValueList());
			}
			void OnChangeSupportData(SectorData.Support.Data data)
			{
				UpdateFillRectUI();
				UpdateStatePanel("Stats KeyValue", "Support Stats", selectSector.SupportBuffGroup.GetValueList());
			}
			void UpdateFillRectUI()
			{
				SetFillRectUI("Fill Durability", selectSector.GetDurability(), "방어벽 없음");
				SetFillRectUI("Fill Troops", selectSector.GetTroops(), "병력 보충 불가");
				SetFillRectUI("Fill Material", selectSector.GetMaterial(), "물자 보충 불가");
				SetFillRectUI("Fill Electric", selectSector.GetElectric(), "전력 보충 불가");
			}
			void UpdateStatePanel(string stateItemName, string statePanelName, List<StatsValue> list)
			{
				if (list == null) return;

				PairChain.FindPairChain(stateItemName, out var statsItem);
				if (statsItem == null) return;

				PairChain.FindPairChain(statePanelName, out var baseStats);
				if (baseStats == null) return;

				int length = list == null ? 0 : list.Count;
				for (int i = 0 ; i < length ; i++)
				{
					var item = list[i];
					var key = item.StatsType;
					var value = item.Value;
					UpdateStatsItem(key, value, detailStatsItemList, baseStats.transform);
				}
			}
			void UpdateStatsItem(StatsType key, int value, Dictionary<StatsType, LabelTextUI> list, Transform parent)
			{
				if (list.TryGetValue(key, out var uiObject))
				{
					//string label = StrategyManager.Key2Name.GetAsset(type.ToString());
					string text =  $"{(value>=0?"+":"-")}{value.ToString()}{SuffixStatsType(key)}";
					list[key].SetText(text);

					if (value == 0)
					{
						list[key].gameObject.SetActive(false);
					}
					else
					{
						list[key].gameObject.SetActive(true);
					}
				}
				else
				{
					string pairKey = "Stats Item";
					string label = StrategyManager.Key2Name.GetAsset(key.ToString());
					string text =  $"{(value>=0?"+":"-")}{value.ToString()}{SuffixStatsType(key)}";
					var newItem = NewLabelTextUI(pairKey, label, text, parent);
					list.Add(key, newItem);

					if (value == 0)
					{
						newItem.gameObject.SetActive(false);
					}
					else
					{
						newItem.gameObject.SetActive(true);
					}
				}
			}
		}
		[Serializable]
		public class SectorDetails_Support : SectorContentPanel
		{
			[ShowInInspector,ReadOnly]private Dictionary<StatsType, LabelTextUI> offensiveItemList;
			[ShowInInspector,ReadOnly]private Dictionary<StatsType, LabelTextUI> defensiveItemList;
			[ShowInInspector,ReadOnly]private Dictionary<StatsType, LabelTextUI> supplyItemList;
			[ShowInInspector,ReadOnly]private Dictionary<StatsType, LabelTextUI> facilitiesItemList;

			public SectorDetails_Support(SectorObject sectorObject, StrategyDetailsPanelUI thisPanel, RectTransform contentPrefab)
				: base(sectorObject, thisPanel, contentPrefab)
			{
				offensiveItemList ??= new();
				defensiveItemList ??= new();
				offensiveItemList ??= new();
				facilitiesItemList ??= new();
			}

			protected override void OnDispose()
			{
				DeleteStatsItemList(offensiveItemList);
				DeleteStatsItemList(defensiveItemList);
				DeleteStatsItemList(supplyItemList);
				DeleteStatsItemList(facilitiesItemList);

				offensiveItemList = null;
				defensiveItemList = null;
				supplyItemList = null;
				facilitiesItemList = null;
			}
			protected override void OnShow()
			{
				selectSector.Support.AddLateListener(OnChangeSupportData);
				OnChangeSupportData(selectSector.SupportData);
			}

			protected override void OnHide()
			{
				selectSector.Support.RemoveListener(OnChangeSupportData);
			}
			void OnChangeSupportData(SectorData.Support.Data data)
			{
				PairChain.FindPairChain<TMP_Text>("PointText", out var PointText);
				if (PointText != null)
				{
					PointText.text = $"잉여 점수: {data.supportPoint}";
				}

				PairChain.FindPairChain("Offensive", out var Offensive);
				PairChain.FindPairChain("Defensive", out var Defensive);
				PairChain.FindPairChain("Supply", out var Supply);
				PairChain.FindPairChain("Facilities", out var Facilities);

				SetupSlider(Offensive, data.offensivePoint, selectSector.Controller.OnChangeSupport_Offensive);
				SetupSlider(Defensive, data.defensivePoint, selectSector.Controller.OnChangeSupport_Defensive);
				SetupSlider(Supply, data.supplyPoint, selectSector.Controller.OnChangeSupport_Supply);
				SetupSlider(Facilities, data.facilitiesPoint, selectSector.Controller.OnChangeSupport_Facilities);

				UpdateSupportState("Offensive", Offensive, offensiveItemList);
				UpdateSupportState("Defensive", Defensive, defensiveItemList);
				UpdateSupportState("Supply", Supply, supplyItemList);
				UpdateSupportState("Facilities", Facilities, facilitiesItemList);
			}
			void UpdateSupportState(string key, GameObject supportPanel, Dictionary<StatsType, LabelTextUI> itemList)
			{
				if (!selectSector.SupportBuffGroup.TryGetList(key, out var statsList)) return;

				var list = statsList.GetValueList();
				int length = list == null ? 0 : list.Count;
				for (int i = 0 ; i < length ; i++)
				{
					var item = list[i];
					var type = item.StatsType;
					var value = item.Value;
					UpdateStatsItem(type, value, itemList, supportPanel.transform);
				}
			}
			void UpdateStatsItem(StatsType key, int value, Dictionary<StatsType, LabelTextUI> list, Transform parent)
			{
				if (list.TryGetValue(key, out var uiObject))
				{
					//string label = StrategyManager.Key2Name.GetAsset(type.ToString());
					string text =  $"{(value>=0?"+":"-")}{value.ToString()}{SuffixStatsType(key)}";
					list[key].SetText(text);

					if (value == 0)
					{
						list[key].gameObject.SetActive(false);
					}
					else
					{
						list[key].gameObject.SetActive(true);
					}
				}
				else
				{
					string pairKey = "Stats Item";
					string label = StrategyManager.Key2Name.GetAsset(key.ToString());
					string text =  $"{(value>=0?"+":"-")}{value.ToString()}{SuffixStatsType(key)}";
					var newItem = NewLabelTextUI(pairKey,label, text, parent);
					list.Add(key, newItem);

					if (value == 0)
					{
						newItem.gameObject.SetActive(false);
					}
					else
					{
						newItem.gameObject.SetActive(true);
					}
				}
			}
		}
		[Serializable]
		public class SectorDetails_Facilities : SectorContentPanel
		{
			private IKeyPairChain facilitiesControlUI;
			private IKeyPairChain facilitiesInstallableUI;

			private TMP_Text facilitiesInfoText;

			[ShowInInspector,ReadOnly]private List<IKeyPairChain> facilitiesSlots;
			[ShowInInspector,ReadOnly]private Dictionary<StatsType, LabelTextUI> facilitiesItemList;
			public SectorDetails_Facilities(SectorObject sectorObject, StrategyDetailsPanelUI thisPanel, RectTransform contentPrefab)
				: base(sectorObject, thisPanel, contentPrefab)
			{
				PairChain.FindPairChain<TMP_Text>("InfoText", out facilitiesInfoText);
				facilitiesControlUI = PairChain.FindSubPairChain("Facilities Control");
				facilitiesInstallableUI = PairChain.FindSubPairChain("Facilities Installable");

				facilitiesSlots ??= new();
				facilitiesItemList ??= new();
			}

			protected override void OnDispose()
			{
				facilitiesInfoText = null;
				facilitiesControlUI = null;
				facilitiesInstallableUI = null;
				DeleteFacilitiesSlots(facilitiesSlots);
				DeleteStatsItemList(facilitiesItemList);
				facilitiesItemList = null;

				void DeleteFacilitiesSlots(List<IKeyPairChain> list)
				{
					if (list != null)
					{
						int oldLength = list.Count;
						foreach (var item in list)
						{
							if (item != null && item.This != null)
							{
								GameObject.DestroyImmediate(item.This.gameObject);
							}
						}
						list.Clear();
					}
				}
			}
			protected override void OnShow()
			{
				selectSector.Facilities.AddLateListener(OnChangeFacilitiesData);
				OnChangeFacilitiesData(selectSector.FacilitiesData);
			}

			protected override void OnHide()
			{
				selectSector.Facilities.RemoveListener(OnChangeFacilitiesData);
			}
			void OnChangeFacilitiesData(SectorData.Facilities.Data data)
			{
				SectorData.Facilities.Slot[] slotData = data.slotData;
				CreateSlot(slotData == null ? 0 : slotData.Length);

				int slotLength = slotData.Length;
				for (int i = 0 ; i < slotLength ; i++)
				{
					UpdateSlotInfo(i, slotData[i]);
				}
				PairChain.FindPairChain("Total Statistics", out var statistics);
				UpdateFacilitiesState(statistics, facilitiesItemList);


			}
			void CreateSlot(int slotSize)
			{
				if (facilitiesSlots.Count == 0)
				{
					PairChain.FindPairChain("Facilities List", out var slotParent);

					for (int i = 0 ; i < slotSize ; i++)
					{
						PairChain.FindPairChainAndCopy("Facilities Slot", slotParent.transform, out var newSlot);
						if (newSlot == null) break;
						if (newSlot.TryGetComponent<KeyPairTarget>(out var slotObject))
						{
							facilitiesSlots.Add(slotObject);
						}
					}
				}
			}
			void UpdateSlotInfo(int index, SectorData.Facilities.Slot data)
			{
				var slotChain = facilitiesSlots[index];
				string key = data.facilitiesKey;
				var constructing = data.constructing;

				slotChain.FindPairChain<Button>("Button", out var Button);
				slotChain.FindPairChain<Image>("Image", out var Image);
				slotChain.FindPairChain<Image>("Progress", out var ProgressImage);
				slotChain.FindPairChain<TMP_Text>("_label", out var Label);

				if (Button != null)
				{
					Button.onClick.RemoveAllListeners();
					Button.onClick.AddListener(() => ShowSlotControlUI(index, data.facilitiesKey));
				}
				if (Image != null)
				{
					Image.sprite = StrategyManager.Key2Sprite.GetAsset(key);
				}
				if (ProgressImage != null)
				{
					if (string.IsNullOrWhiteSpace(constructing.facilitiesKey) || constructing.facilitiesKey == key)
					{
						ProgressImage.enabled = false;
					}
					else
					{
						ProgressImage.enabled = true;
						float installingTime = constructing.constructTime;
						float timeRemaining = constructing.duration;
						if (installingTime < 1) installingTime = 1;
						ProgressImage.fillAmount = 1f - (timeRemaining / installingTime);
					}
				}
				if (Label != null)
				{
					Label.text = StrategyManager.Key2Name.GetAsset(key);
				}
			}
			void ShowSlotControlUI(int index, string facilitiesKey)
			{
				if (facilitiesControlUI == null) return;
				facilitiesControlUI.This.gameObject.SetActive(true);

				var slotChain = facilitiesSlots[index];

				facilitiesControlUI.FindPairChain<Button>("Back", out var Back);
				facilitiesControlUI.FindPairChain<Button>("Change", out var Change);
				facilitiesControlUI.FindPairChain<Button>("Upgrade", out var Upgrade);
				facilitiesControlUI.FindPairChain<Button>("Downgrade", out var Downgrade);

				if (Back != null)
				{
					Back.onClick.RemoveAllListeners();
					Back.onClick.AddListener(HideSlotControlUI);
				}
				if (Change != null)
				{
					Change.onClick.RemoveAllListeners();
					Change.onClick.AddListener(() =>
					{
						HideSlotControlUI();
						ShowSlotInstallableUI(index, facilitiesKey);
					});
				}
				if (Upgrade != null)
				{
					Upgrade.onClick.RemoveAllListeners();
					Upgrade.onClick.AddListener(() =>
					{
						HideSlotControlUI();
						ShowSlotInstallableUI(index, facilitiesKey);
					});
				}
				if (Downgrade != null)
				{
					Downgrade.onClick.RemoveAllListeners();
					Downgrade.onClick.AddListener(() =>
					{
						HideSlotControlUI();
						ShowSlotInstallableUI(index, facilitiesKey);
					});
				}
			}
			void HideSlotControlUI()
			{
				if (facilitiesControlUI == null) return;
				facilitiesControlUI.This.gameObject.SetActive(false);
			}
			void HideSlotInstallableUI()
			{
				if (facilitiesInstallableUI == null) return;
				facilitiesInstallableUI.This.gameObject.SetActive(false);
			}
			void ShowSlotInstallableUI(int index, string facilitiesKey)
			{

			}

			void UpdateFacilitiesState(GameObject FacilitiesPanel, Dictionary<StatsType, LabelTextUI> itemList)
			{
				var list = selectSector.FacilitiesBuffGroup.GetValueList();

				int length = list == null ? 0 : list.Count;
				for (int i = 0 ; i < length ; i++)
				{
					var item = list[i];
					var type = item.StatsType;
					var value = item.Value;
					UpdateStatsItem(type, value, itemList, FacilitiesPanel.transform);
				}
			}
			void UpdateStatsItem(StatsType key, int value, Dictionary<StatsType, LabelTextUI> list, Transform parent)
			{
				if (list.TryGetValue(key, out var uiObject))
				{
					//string label = StrategyManager.Key2Name.GetAsset(type.ToString());
					string text =  $"{(value>=0?"+":"-")}{value.ToString()}{SuffixStatsType(key)}";
					list[key].SetText(text);

					if (value == 0)
					{
						list[key].gameObject.SetActive(false);
					}
					else
					{
						list[key].gameObject.SetActive(true);
					}
				}
				else
				{
					string pairKey = "Stats Item";
					string label = StrategyManager.Key2Name.GetAsset(key.ToString());
					string text =  $"{(value>=0?"+":"-")}{value.ToString()}{SuffixStatsType(key)}";
					var newItem = NewLabelTextUI(pairKey, label, text, parent);
					list.Add(key, newItem);

					if (value == 0)
					{
						newItem.gameObject.SetActive(false);
					}
					else
					{
						newItem.gameObject.SetActive(true);
					}
				}
			}
		}
		[Serializable]
		public class SectorDetails_Troops : SectorContentPanel
		{
			public SectorDetails_Troops(SectorObject sectorObject, StrategyDetailsPanelUI thisPanel, RectTransform contentPrefab)
				: base(sectorObject, thisPanel, contentPrefab)
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