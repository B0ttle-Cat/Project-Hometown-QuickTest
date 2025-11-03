using System;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using TMPro;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using static StrategyGamePlayData;

public partial class StrategyDetailsPanelUI // SectorData UI
{
	[Serializable]
	public struct SectorUIPrefab
	{
		public RectTransform info;
		public RectTransform support;
		public RectTransform facilities;
		public RectTransform garrison;
	}
	[FoldoutGroup("거점 정보 UI"), InlineProperty,PropertyOrder(9), HideLabel]
	public SectorUIPrefab sectorUIPrefab;

	public class SectorUIStruct : StrategyContentController
	{
		public SectorUIStruct(StrategyDetailsPanelUI component) : base(component) { }
		SectorUIPrefab UIPrefab => ThisComponent.sectorUIPrefab;

		public override void OnShow()
		{
			ThisComponent.ContentTitleText("거점");
			AddTabAndContnet_Info(initContent == StrategyDetailsPanelType.Sector_Info);
			AddTabAndContnet_Support(initContent == StrategyDetailsPanelType.Sector_Support);
			AddTabAndContnet_Facilities(initContent == StrategyDetailsPanelType.Sector_Facilities);
			AddTabAndContnet_Garrison(initContent == StrategyDetailsPanelType.Sector_Garrison);
		}
		public override void OnHide()
		{

		}

		// 선택한 거점의 기본 정보 보기
		private void AddTabAndContnet_Info(bool isOn = false)
		{
			ThisComponent.AddTabAndContnet<Sector_Info>("거점 정보", UIPrefab.info, this, isOn);
		}
		private void AddTabAndContnet_Support(bool isOn = false)
		{
			ThisComponent.AddTabAndContnet<Sector_Support>("지원 정책", UIPrefab.support, this, isOn);
		}
		private void AddTabAndContnet_Facilities(bool isOn = false)
		{
			ThisComponent.AddTabAndContnet<Sector_Facilities>("설치 시설", UIPrefab.facilities, this, isOn);
		}
		private void AddTabAndContnet_Garrison(bool isOn = false)
		{
			ThisComponent.AddTabAndContnet<Sector_Garrison>("방어 병력", UIPrefab.garrison, this, isOn);
		}
	}

	public class Sector_Info : StrategyViewController
	{
		private IKeyPairChain pairChain;
		private SectorObject selectSector;

		private Action<string> onSelectSector;

		private Action<SectorData.Profile.Data>       onChangeProfileData;
		private Action<SectorData.Capture.Data>       onChangeCaptureData;
		private Action<SectorData.MainStats.Data>     onChangeStatsData;
		private Action<SectorData.Facilities.Data>    onChangeFacilitiesData;
		private Action<SectorData.Support.Data>       onChangeSupportData;

		private Dictionary<StatsType, LabelTextUI> detailStatsItemList;
		private Dictionary<StatsType, LabelTextUI> facilitiesStateItemList;
		private Dictionary<StatsType, LabelTextUI> supportStatsItemList;
		public override void OnShow(RectTransform viewRect)
		{
			if (!viewRect.gameObject.TryFindPairChain(out pairChain))
			{
				DeInitEvent();
				return;
			}

			// 거점 변경 변경
			onSelectSector = SelectSector;
			StrategyManager.GamePlayData.selectSector.AddLateListener(onSelectSector);

			// 항목 변경 이벤트
			onChangeProfileData = OnChangeProfileData;
			onChangeCaptureData = OnChangeCaptureData;
			onChangeStatsData = OnChangeStatsData;
			onChangeFacilitiesData = OnChangeFacilitiesData;
			onChangeSupportData = OnChangeSupportData;

			// 아이템 리스트
			detailStatsItemList ??= new();
			facilitiesStateItemList ??= new();
			supportStatsItemList ??= new();

			// 실행
			SelectSector(StrategyManager.GamePlayData.selectSector.Value);
			void SelectSector(string sector)
			{
				if (selectSector != null)
				{
					selectSector.Profile.RemoveListener(onChangeProfileData);
					selectSector.Capture.RemoveListener(onChangeCaptureData);
					selectSector.Stats.RemoveListener(onChangeStatsData);
					selectSector.Facilities.RemoveListener(onChangeFacilitiesData);
					selectSector.Support.RemoveListener(onChangeSupportData);
					selectSector = null;
				}
				DeleteStatsItemList(detailStatsItemList);
				DeleteStatsItemList(facilitiesStateItemList);
				DeleteStatsItemList(supportStatsItemList);

				if (!StrategyManager.Collector.TryFindSector(sector, out selectSector))
				{
					DeInitEvent();
					return;
				}

				selectSector.Profile.AddLateListener(onChangeProfileData);
				selectSector.Capture.AddLateListener(onChangeCaptureData);
				selectSector.Stats.AddLateListener(onChangeStatsData);
				selectSector.Facilities.AddLateListener(onChangeFacilitiesData);
				selectSector.Support.AddLateListener(onChangeSupportData);

				onChangeProfileData.Invoke(selectSector.ProfileData);
				onChangeCaptureData.Invoke(selectSector.CaptureData);
				onChangeStatsData.Invoke(selectSector.StatsData);
				onChangeFacilitiesData.Invoke(selectSector.FacilitiesData);
				onChangeSupportData.Invoke(selectSector.SupportData);
			}

			void OnChangeProfileData(SectorData.Profile.Data data)
			{
				pairChain
					.FindPairChain<Image>("MainImage", out var mainImage)
					.FindPairChain<Text>("NameText", out var nameText)
					.FindPairChain<Text>("EffectText", out var effectText)
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
				pairChain
				  .FindPairChain<Text>("CaptureText", out var captureText)
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
				UpdateStatePanel("Stats KeyValue", "Support Stats", selectSector.supportStatsGroup.GetValueList());
			}

			void UpdateFillRectUI()
			{
				SetFillRectUI("Fill Durability", selectSector.GetDurability(), "방어벽 없음");
				SetFillRectUI("Fill Garrison", selectSector.GetGarrison(), "병력 보충 불가");
				SetFillRectUI("Fill Material", selectSector.GetMaterial(), "물자 보충 불가");
				SetFillRectUI("Fill Electric", selectSector.GetElectric(), "전력 보충 불가");
			}
			void UpdateFillRectUI_Key(StatsType key)
			{
				if (key == StrategyGamePlayData.StatsType.거점_최대내구도 || key == StrategyGamePlayData.StatsType.거점_현재내구도)
				{
					SetFillRectUI("Fill Durability",selectSector.GetDurability(),"방어벽 없음");
				}
				else if (key == StrategyGamePlayData.StatsType.거점_인력_최대보유량 || key == StrategyGamePlayData.StatsType.거점_인력_현재보유량)
				{
					SetFillRectUI("Fill Garrison",selectSector.GetGarrison(),"병력 보충 불가");
				}
				else if (key == StrategyGamePlayData.StatsType.거점_물자_최대보유량 || key == StrategyGamePlayData.StatsType.거점_물자_현재보유량)
				{
					SetFillRectUI("Fill Material",selectSector.GetMaterial(),"물자 보충 불가");
				}
				else if (key == StrategyGamePlayData.StatsType.거점_전력_최대보유량 || key == StrategyGamePlayData.StatsType.거점_전력_현재보유량)
				{
					SetFillRectUI("Fill Electric",selectSector.GetElectric(),"전력 보충 불가");
				}
			}
			void UpdateStatePanel(string stateItemName, string statePanelName, List<StatsValue> list)
			{
				if (list == null) return;

				pairChain.FindPairChain(stateItemName, out var statsItem);
				if (statsItem == null) return;

				pairChain.FindPairChain(statePanelName, out var baseStats);
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
					string label = StrategyManager.Key2Name.GetAsset(key.ToString());
					string text =  $"{(value>=0?"+":"-")}{value.ToString()}{SuffixStatsType(key)}";
					var newItem = NewStatsItem(label, text, parent);
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
			LabelTextUI NewStatsItem(string label, string text, Transform parent)
			{
				pairChain.FindPairChainAndCopy<LabelTextUI>("Stats Item", parent, out var statsItem);
				if (statsItem == null) return null;
				statsItem.gameObject.SetActive(true);
				statsItem.SetText(label, text);
				return statsItem;
			}
			void SetFillRectUI(string fillRectName, (int value, int max) item, string noneText)
			{
				int value = item.value;
				int max = item.max;
				pairChain.FindPairChain<FillRectUIAndLabel>(fillRectName, out var fillRectAndLabel);
				if (fillRectAndLabel == null) return;

				if (max > 0)
				{
					float rate = (float)value / (float)max;
					if(!fillRectAndLabel.gameObject.activeSelf)
						fillRectAndLabel.gameObject.SetActive(true);
					fillRectAndLabel.SetValueText(rate, $"{value,10} / {max,-10}");
				}
				else
				{
					if (fillRectAndLabel.gameObject.activeSelf)
						fillRectAndLabel.gameObject.SetActive(false);
				}
			}
		}
		public override void OnHide()
		{
			DeInitEvent();
			pairChain = null;
			selectSector = null;
		}
		public override void OnDispose()
		{
			DeInitEvent();
			pairChain = null;
			selectSector = null;
		}

		private void DeInitEvent()
		{
			if (onSelectSector != null)
			{
				StrategyManager.GamePlayData.selectSector.RemoveListener(onSelectSector);
			}

			if (selectSector != null)
			{
				selectSector.Profile.RemoveListener(onChangeProfileData);
				selectSector.Capture.RemoveListener(onChangeCaptureData);
				selectSector.Stats.RemoveListener(onChangeStatsData);
				selectSector.Facilities.RemoveListener(onChangeFacilitiesData);
				selectSector.Support.RemoveListener(onChangeSupportData);

				//sector.MainStatsList.RemoveListener(onChangeDetailStats);
				//sector.FacilitiesStatsList.RemoveListener(onChangeFacilitiesStats);
				//sector.SupportStatsList.RemoveListener(onChangeSupportStats);
				//
				//sector.MainBuffList.RemoveListener(onChangeDetailBuff);
				//sector.FacilitiesBuffGroup.RemoveListener(onChangeFacilitiesBuff);
				//sector.SupportBuffGroup.RemoveListener(onChangeSupportBuff);

			}
			DeleteStatsItemList(detailStatsItemList);
			DeleteStatsItemList(facilitiesStateItemList);
			DeleteStatsItemList(supportStatsItemList);

			onSelectSector = null;

			onChangeProfileData = null;
			onChangeCaptureData = null;
			onChangeStatsData = null;
			onChangeFacilitiesData = null;
			onChangeSupportData = null;

			//onChangeDetailStats = null;
			//onChangeFacilitiesStats = null;
			//onChangeSupportStats = null;
			//
			//onChangeDetailBuff = null;
			//onChangeFacilitiesBuff = null;
			//onChangeSupportBuff = null;

			detailStatsItemList = null;
			facilitiesStateItemList = null;
			supportStatsItemList = null;
		}

		void DeleteStatsItemList(Dictionary<StatsType, LabelTextUI> list)
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
	}
	public class Sector_Support : StrategyViewController
	{
		private IKeyPairChain pairChain;
		private SectorObject selectSector;

		private Action<string> onSelectSector;

		private Action<SectorData.Support.Data>       onChangeSupportData;

		private Dictionary<StatsType, LabelTextUI> offensiveItemList;
		private Dictionary<StatsType, LabelTextUI> defensiveItemList;
		private Dictionary<StatsType, LabelTextUI> supplyItemList;
		private Dictionary<StatsType, LabelTextUI> facilitiesItemList;

		public override void OnShow(RectTransform viewRect)
		{
			if (!viewRect.gameObject.TryFindPairChain(out pairChain))
			{
				DeInitEvent();
				return;
			}

			// 거점 변경 변경
			onSelectSector = SelectSector;
			StrategyManager.GamePlayData.selectSector.AddLateListener(onSelectSector);

			// 항목 변경 이벤트
			onChangeSupportData = OnChangeSupportData;

			// 아이템 리스트
			offensiveItemList ??= new();
			defensiveItemList ??= new();
			offensiveItemList ??= new();
			facilitiesItemList ??= new();

			// 실행
			SelectSector(StrategyManager.GamePlayData.selectSector.Value);
			void SelectSector(string sector)
			{
				if (selectSector != null)
				{
					selectSector.Support.RemoveListener(onChangeSupportData);
					selectSector = null;
				}

				DeleteStatsItemList(offensiveItemList);
				DeleteStatsItemList(defensiveItemList);
				DeleteStatsItemList(supplyItemList);
				DeleteStatsItemList(facilitiesItemList);

				if (!StrategyManager.Collector.TryFindSector(sector, out selectSector))
				{
					DeInitEvent();
					return;
				}
				selectSector.Support.AddLateListener(onChangeSupportData);

				onChangeSupportData.Invoke(selectSector.SupportData);
			}

			void OnChangeSupportData(SectorData.Support.Data data)
			{
				pairChain.FindPairChain<TMP_Text>("PointText", out var PointText);
				if (PointText != null)
				{
					PointText.text = $"잉여 점수: {data.supportPoint}";
				}

				pairChain.FindPairChain("Offensive", out var Offensive);
				pairChain.FindPairChain("Defensive", out var Defensive);
				pairChain.FindPairChain("Supply", out var Supply);
				pairChain.FindPairChain("Facilities", out var Facilities);

				UpdateSlider(Offensive, data.offensivePoint, null);
				UpdateSlider(Defensive, data.defensivePoint, null);
				UpdateSlider(Supply, data.supplyPoint, null);
				UpdateSlider(Facilities, data.facilitiesPoint, null);

				UpdateSupportState("Offensive", Offensive, offensiveItemList);
				UpdateSupportState("Defensive", Defensive, defensiveItemList);
				UpdateSupportState("Supply", Supply, supplyItemList);
				UpdateSupportState("Facilities", Facilities, facilitiesItemList);
			}
			void UpdateSupportState(string key, GameObject supportPanel, Dictionary<StatsType, LabelTextUI> itemList)
			{
				selectSector.SupportBuffGroup.TryGetList(key, out var statsList);

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
					string label = StrategyManager.Key2Name.GetAsset(key.ToString());
					string text =  $"{(value>=0?"+":"-")}{value.ToString()}{SuffixStatsType(key)}";
					var newItem = NewStatsItem(label, text, parent);
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
			LabelTextUI NewStatsItem(string label, string text, Transform parent)
			{
				pairChain.FindPairChainAndCopy<LabelTextUI>("Stats Item", parent, out var statsItem);
				if (statsItem == null) return null;
				statsItem.gameObject.SetActive(true);
				statsItem.SetText(label, text);
				return statsItem;
			}
			void UpdateSlider(GameObject parentPanel, int value, UnityAction<float> onChangeValue)
			{
				if (parentPanel == null) return;
				Slider slider = parentPanel.GetComponentInChildren<Slider>();
				if (slider == null) return;

				slider.onValueChanged.RemoveAllListeners();
				slider.value = value;
				slider.onValueChanged.AddListener(onChangeValue);
			}
		}
		public override void OnHide()
		{
			DeInitEvent();
		}
		public override void OnDispose()
		{
			DeInitEvent();
			pairChain = null;
			selectSector = null;
		}
		private void DeInitEvent()
		{
			if (onSelectSector != null)
			{
				StrategyManager.GamePlayData.selectSector.RemoveListener(onSelectSector);
				onSelectSector = null;
			}

			if (selectSector != null)
			{
				selectSector.Support.RemoveListener(onChangeSupportData);
				selectSector = null;
			}

			DeleteStatsItemList(offensiveItemList);
			DeleteStatsItemList(defensiveItemList);
			DeleteStatsItemList(supplyItemList);
			DeleteStatsItemList(facilitiesItemList);

			offensiveItemList = null;
			defensiveItemList = null;
			offensiveItemList = null;
			facilitiesItemList = null;
		}
		void DeleteStatsItemList(Dictionary<StatsType, LabelTextUI> list)
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
	}
	public class Sector_Facilities : StrategyViewController
	{
		private IKeyPairChain pairChain;
		private SectorObject selectSector;
		private IKeyPairChain facilitiesControlUI;
		private IKeyPairChain facilitiesInstallableUI;

		private TMP_Text facilitiesInfoText;

		private Action<string> onSelectSector;

		private Action<SectorData.Facilities.Data>       onChangeFacilitiesData;

		private List<IKeyPairChain> facilitiesSlots;
		private Dictionary<StatsType, LabelTextUI> facilitiesItemList;

		public override void OnShow(RectTransform viewRect)
		{
			if (!viewRect.gameObject.TryFindPairChain(out pairChain))
			{
				DeInitEvent();
				return;
			}
			facilitiesControlUI = pairChain.FindSubPairChain("Facilities Control");
			facilitiesInstallableUI = pairChain.FindSubPairChain("Facilities Installable");
			pairChain.FindPairChain<TMP_Text>("InfoText", out facilitiesInfoText);

			// 거점 변경 변경
			onSelectSector = SelectSector;
			StrategyManager.GamePlayData.selectSector.AddLateListener(onSelectSector);

			// 항목 변경 이벤트
			onChangeFacilitiesData = OnChangeFacilitiesData;

			// 아이템 리스트
			facilitiesSlots ??= new();
			facilitiesItemList ??= new();

			// 실행
			SelectSector(StrategyManager.GamePlayData.selectSector.Value);
			void SelectSector(string sector)
			{
				if (selectSector != null)
				{
					selectSector.Facilities.RemoveListener(onChangeFacilitiesData);
					selectSector = null;
				}

				DeleteFacilitiesSlots(facilitiesSlots);
				DeleteStatsItemList(facilitiesItemList);

				if (!StrategyManager.Collector.TryFindSector(sector, out selectSector))
				{
					DeInitEvent();
					return;
				}
				selectSector.Facilities.AddLateListener(onChangeFacilitiesData);

				onChangeFacilitiesData.Invoke(selectSector.FacilitiesData);
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
				pairChain.FindPairChain("Total Statistics", out var statistics);
				UpdateFacilitiesState(statistics, facilitiesItemList);


			}
			void CreateSlot(int slotSize)
			{
				if (facilitiesSlots.Count == 0)
				{
					pairChain.FindPairChain("Facilities List", out var slotParent);

					for (int i = 0 ; i < slotSize ; i++)
					{
						pairChain.FindPairChainAndCopy("Facilities Slot", slotParent.transform, out var newSlot);
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
				if(ProgressImage != null)
				{
					if(string.IsNullOrWhiteSpace(constructing.facilitiesKey) || constructing.facilitiesKey == key)
					{
						ProgressImage.enabled = false;
					}
					else
					{
						ProgressImage.enabled = true;
						float installingTime = constructing.constructTime;
						float timeRemaining = constructing.timeRemaining;
						if(installingTime < 1) installingTime = 1;
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
					string label = StrategyManager.Key2Name.GetAsset(key.ToString());
					string text =  $"{(value>=0?"+":"-")}{value.ToString()}{SuffixStatsType(key)}";
					var newItem = NewStatsItem(label, text, parent);
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
			LabelTextUI NewStatsItem(string label, string text, Transform parent)
			{
				pairChain.FindPairChainAndCopy<LabelTextUI>("Stats Item", parent, out var statsItem);
				if (statsItem == null) return null;
				statsItem.gameObject.SetActive(true);
				statsItem.SetText(label, text);
				return statsItem;
			}
		}
		public override void OnHide()
		{
			HideSlotControlUI();
			HideSlotInstallableUI();
			DeInitEvent();
		}
		public override void OnDispose()
		{
			HideSlotControlUI();
			HideSlotInstallableUI();
			DeInitEvent();
			pairChain = null;
			selectSector = null;
			facilitiesControlUI = null;
			facilitiesInstallableUI = null;
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
		private void DeInitEvent()
		{
			if (onSelectSector != null)
			{
				StrategyManager.GamePlayData.selectSector.RemoveListener(onSelectSector);
				onSelectSector = null;
			}

			if (selectSector != null)
			{
				selectSector.Facilities.RemoveListener(onChangeFacilitiesData);
				selectSector = null;
			}

			DeleteStatsItemList(facilitiesItemList);

			facilitiesItemList = null;
		}
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
		void DeleteStatsItemList(Dictionary<StatsType, LabelTextUI> list)
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


		private void OnStartFacilitiesInstall(int slotIndex, string facilitiesKey)
		{
			selectSector.OnStartFacilitiesConstruct(slotIndex, facilitiesKey);
		}

		private void OnShowInfoText(string text)
		{
			if (facilitiesInfoText == null) return;
			facilitiesInfoText.text = text;
		}
	}
	public class Sector_Garrison : StrategyViewController
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
