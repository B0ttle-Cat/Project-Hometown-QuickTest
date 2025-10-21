using System;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using TMPro;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using static StrategyGamePlayData;

public partial class StrategyDetailsPanelUI // ControlBaseData UI
{
	[Serializable]
	public struct ControlBaseUIPrefab
	{
		public RectTransform info;
		public RectTransform support;
		public RectTransform facilities;
		public RectTransform garrison;
	}
	[FoldoutGroup("거점 정보 UI"), InlineProperty, HideLabel]
	public ControlBaseUIPrefab controlBaseUIPrefab;

	public class ControlBaseUIStruct : StrategyContentController
	{
		public ControlBaseUIStruct(StrategyDetailsPanelUI component) : base(component) { }
		ControlBaseUIPrefab UIPrefab => ThisComponent.controlBaseUIPrefab;

		public override void OnShow()
		{
			ThisComponent.ContentTitleText("거점");
			AddTabAndContnet_Info(initContent == StrategyDetailsPanelType.ControlBase_Info);
			AddTabAndContnet_Support(initContent == StrategyDetailsPanelType.ControlBase_Support);
			AddTabAndContnet_Facilities(initContent == StrategyDetailsPanelType.ControlBase_Facilities);
			AddTabAndContnet_Garrison(initContent == StrategyDetailsPanelType.ControlBase_Garrison);
		}
		public override void OnHide()
		{

		}

		// 선택한 거점의 기본 정보 보기
		private void AddTabAndContnet_Info(bool isOn = false)
		{
			ThisComponent.AddTabAndContnet<ControlBase_Info>("거점 정보", UIPrefab.info, this, isOn);
		}
		private void AddTabAndContnet_Support(bool isOn = false)
		{
			ThisComponent.AddTabAndContnet<ControlBase_Support>("지원 정책", UIPrefab.support, this, isOn);
		}
		private void AddTabAndContnet_Facilities(bool isOn = false)
		{
			ThisComponent.AddTabAndContnet<ControlBase_Facilities>("설치 시설", UIPrefab.facilities, this, isOn);
		}
		private void AddTabAndContnet_Garrison(bool isOn = false)
		{
			ThisComponent.AddTabAndContnet<ControlBase_Garrison>("방어 병력", UIPrefab.garrison, this, isOn);
		}
	}

	public class ControlBase_Info : StrategyViewController
	{
		private IKeyPairChain pairChain;
		private ControlBase selectControlBase;

		private Action<string> onSelectControlBase;

		private Action<ControlBaseData.Profile.Data>       onChangeProfileData;
		private Action<ControlBaseData.Capture.Data>       onChangeCaptureData;
		private Action<ControlBaseData.MainStats.Data>     onChangeStatsData;
		private Action<ControlBaseData.Facilities.Data>    onChangeFacilitiesData;
		private Action<ControlBaseData.Support.Data>       onChangeSupportData;

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
			onSelectControlBase = SelectControlBase;
			StrategyManager.GamePlayData.selectControlBase.AddLateListener(onSelectControlBase);

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
			SelectControlBase(StrategyManager.GamePlayData.selectControlBase.Value);
			void SelectControlBase(string controlBase)
			{
				if (selectControlBase != null)
				{
					selectControlBase.Profile.RemoveListener(onChangeProfileData);
					selectControlBase.Capture.RemoveListener(onChangeCaptureData);
					selectControlBase.Stats.RemoveListener(onChangeStatsData);
					selectControlBase.Facilities.RemoveListener(onChangeFacilitiesData);
					selectControlBase.Support.RemoveListener(onChangeSupportData);
					selectControlBase = null;
				}
				DeleteStatsItemList(detailStatsItemList);
				DeleteStatsItemList(facilitiesStateItemList);
				DeleteStatsItemList(supportStatsItemList);

				if (!StrategyManager.Collector.TryFindControlBase(controlBase, out selectControlBase))
				{
					DeInitEvent();
					return;
				}

				selectControlBase.Profile.AddLateListener(onChangeProfileData);
				selectControlBase.Capture.AddLateListener(onChangeCaptureData);
				selectControlBase.Stats.AddLateListener(onChangeStatsData);
				selectControlBase.Facilities.AddLateListener(onChangeFacilitiesData);
				selectControlBase.Support.AddLateListener(onChangeSupportData);

				onChangeProfileData.Invoke(selectControlBase.ProfileData);
				onChangeCaptureData.Invoke(selectControlBase.CaptureData);
				onChangeStatsData.Invoke(selectControlBase.StatsData);
				onChangeFacilitiesData.Invoke(selectControlBase.FacilitiesData);
				onChangeSupportData.Invoke(selectControlBase.SupportData);
			}

			void OnChangeProfileData(ControlBaseData.Profile.Data data)
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
					nameText.text = data.controlBaseName;
				}
				if (effectText != null)
				{
					effectText.text = data.EffectString();
				}
			}
			void OnChangeCaptureData(ControlBaseData.Capture.Data data)
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
			void OnChangeStatsData(ControlBaseData.MainStats.Data data)
			{
				UpdateFillRectUI();
				UpdateStatePanel("Stats KeyValue", "Base Stats", selectControlBase.MainStatsList?.GetValueList());
			}
			void OnChangeFacilitiesData(ControlBaseData.Facilities.Data data)
			{
				UpdateFillRectUI();
				UpdateStatePanel("Stats KeyValue", "Facilities Stats", selectControlBase.FacilitiesBuffGroup?.MergedStatsValueList());
			}
			void OnChangeSupportData(ControlBaseData.Support.Data data)
			{
				UpdateFillRectUI();
				UpdateStatePanel("Stats KeyValue", "Support Stats", selectControlBase.supportStatsGroup?.MergedStatsValueList());
			}

			/*
			void OnChangeDetailStats(StatsValue item)
			{
				StrategyGamePlayData.StatsType key = item.StatsType;
				UpdateFillRectUI_Key(key);

				pairChain.FindPairChain("Base Stats", out var baseStats);
				if (baseStats == null) return;
				int value = item.Value;
				var symbol = item.Symbol;
				UpdateStatsItem(key, value, symbol, detailStatsItemList, baseStats.transform);
			}
			void OnChangeFacilitiesStats(StatsValue item)
			{
				StrategyGamePlayData.StatsType key = item.StatsType;
				UpdateFillRectUI_Key(key);

				pairChain.FindPairChain("Facilities Stats", out var baseStats);
				if (baseStats == null) return;
				int value = item.Value;
				var symbol = item.Symbol;
				UpdateStatsItem(key, value, symbol, detailStatsItemList, baseStats.transform);
			}
			void OnChangeSupportStats(StatsValue item)
			{
				StrategyGamePlayData.StatsType key = item.StatsType;
				UpdateFillRectUI_Key(key);

				pairChain.FindPairChain("Support Stats", out var baseStats);
				if (baseStats == null) return;
				int value = item.Value;
				var symbol = item.Symbol;
				UpdateStatsItem(key, value, symbol, detailStatsItemList, baseStats.transform);
			}

			void OnChangeDetailBuff(StatsValue item)
			{
				StrategyGamePlayData.StatsType key = item.StatsType;
				UpdateFillRectUI_Key(key);

				pairChain.FindPairChain("Base Stats", out var baseStats);
				if (baseStats == null) return;
				var value = item.Value;
				var symbol = item.Symbol;
				UpdateStatsItem(key, value, symbol, detailStatsItemList, baseStats.transform);
			}
			void OnChangeFacilitiesBuff(StatsValue item)
			{
				StrategyGamePlayData.StatsType key = item.StatsType;
				UpdateFillRectUI_Key(key);

				pairChain.FindPairChain("Facilities Stats", out var baseStats);
				if (baseStats == null) return;
				var value = item.Value;
				var symbol = item.Symbol;
				UpdateStatsItem(key, value, symbol, detailStatsItemList, baseStats.transform);
			}
			void OnChangeSupportBuff(StatsValue item)
			{
				StrategyGamePlayData.StatsType key = item.StatsType;
				UpdateFillRectUI_Key(key);

				pairChain.FindPairChain("Support Stats", out var baseStats);
				if (baseStats == null) return;
				var value = item.Value;
				var symbol = item.Symbol;
				UpdateStatsItem(key, value, symbol, detailStatsItemList, baseStats.transform);
			}
			*/
			void UpdateFillRectUI()
			{
				SetFillRectUI("Fill Durability", selectControlBase.GetDurability(), selectControlBase.GetMaxDurability(), "방어벽 없음");
				SetFillRectUI("Fill Garrison", selectControlBase.GetGarrison(), selectControlBase.GetMaxGarrison(), "병력 보충 불가");
				SetFillRectUI("Fill Material", selectControlBase.GetMaterial(), selectControlBase.GetMaxMaterial(), "물자 보충 불가");
				SetFillRectUI("Fill Electric", selectControlBase.GetElectric(), selectControlBase.GetMaxElectric(), "전력 보충 불가");
			}
			void UpdateFillRectUI_Key(StatsType key)
			{
				if (key == StrategyGamePlayData.StatsType.거점_최대내구도 || key == StrategyGamePlayData.StatsType.거점_현재내구도)
				{
					SetFillRectUI("Fill Durability",
						selectControlBase.GetDurability(),
						selectControlBase.GetMaxDurability(),
						"방어벽 없음");
				}
				else if (key == StrategyGamePlayData.StatsType.거점_인력_최대보유량 || key == StrategyGamePlayData.StatsType.거점_인력_현재보유량)
				{
					SetFillRectUI("Fill Garrison",
						selectControlBase.GetGarrison(),
						selectControlBase.GetMaxGarrison(),
						"병력 보충 불가");
				}
				else if (key == StrategyGamePlayData.StatsType.거점_물자_최대보유량 || key == StrategyGamePlayData.StatsType.거점_물자_현재보유량)
				{
					SetFillRectUI("Fill Material",
						selectControlBase.GetMaterial(),
						selectControlBase.GetMaxMaterial(),
						"물자 보충 불가");
				}
				else if (key == StrategyGamePlayData.StatsType.거점_전력_최대보유량 || key == StrategyGamePlayData.StatsType.거점_전력_현재보유량)
				{
					SetFillRectUI("Fill Electric",
						selectControlBase.GetElectric(),
						selectControlBase.GetMaxElectric(),
						"전력 보충 불가");
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
					var symbol = item.Symbol;
					UpdateStatsItem(key, value, symbol, detailStatsItemList, baseStats.transform);
				}
			}

			void UpdateStatsItem(StatsType key, int value, SymbolType symbol, Dictionary<StatsType, LabelTextUI> list, Transform parent)
			{
				if (list.TryGetValue(key, out var uiObject))
				{
					//string label = StrategyManager.Key2Name.GetAsset(key.ToString());
					string text =  $"{(value>=0?"+":"-")}{value.ToString()}{SymbolToString(symbol)}";
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
					string text =  $"{(value>=0?"+":"-")}{value.ToString()}{SymbolToString(symbol)}";
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

				string SymbolToString(SymbolType symbolType) => symbol switch
				{
					SymbolType.Number => "",
					SymbolType.Percent => "%",
					_ => ""
				};
			}
			LabelTextUI NewStatsItem(string label, string text, Transform parent)
			{
				pairChain.FindPairChainAndCopy<LabelTextUI>("Stats Item", parent, out var statsItem);
				if (statsItem == null) return null;
				statsItem.gameObject.SetActive(true);
				statsItem.SetText(label, text);
				return statsItem;
			}
			void SetFillRectUI(string fillRectName, int value, int maxValue, string noneText)
			{
				pairChain.FindPairChain<FillRectUI>(fillRectName, out var fillRect);
				if (fillRect == null) return;

				if (maxValue > 0)
				{
					float rate = (float)value / (float)maxValue;
					fillRect.SetValueText(rate, $"{value,10} / {maxValue,-10}");
				}
				else
				{
					fillRect.SetValueText(0, noneText);
				}
			}
		}
		public override void OnHide()
		{
			DeInitEvent();
			pairChain = null;
			selectControlBase = null;
		}
		public override void OnDispose()
		{
			DeInitEvent();
			pairChain = null;
			selectControlBase = null;
		}

		private void DeInitEvent()
		{
			if (onSelectControlBase != null)
			{
				StrategyManager.GamePlayData.selectControlBase.RemoveListener(onSelectControlBase);
			}

			if (selectControlBase != null)
			{
				selectControlBase.Profile.RemoveListener(onChangeProfileData);
				selectControlBase.Capture.RemoveListener(onChangeCaptureData);
				selectControlBase.Stats.RemoveListener(onChangeStatsData);
				selectControlBase.Facilities.RemoveListener(onChangeFacilitiesData);
				selectControlBase.Support.RemoveListener(onChangeSupportData);

				//selectControlBase.MainStatsList.RemoveListener(onChangeDetailStats);
				//selectControlBase.FacilitiesStatsList.RemoveListener(onChangeFacilitiesStats);
				//selectControlBase.SupportStatsList.RemoveListener(onChangeSupportStats);
				//
				//selectControlBase.MainBuffList.RemoveListener(onChangeDetailBuff);
				//selectControlBase.FacilitiesBuffGroup.RemoveListener(onChangeFacilitiesBuff);
				//selectControlBase.SupportBuffGroup.RemoveListener(onChangeSupportBuff);

			}
			DeleteStatsItemList(detailStatsItemList);
			DeleteStatsItemList(facilitiesStateItemList);
			DeleteStatsItemList(supportStatsItemList);

			onSelectControlBase = null;

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
	public class ControlBase_Support : StrategyViewController
	{
		private IKeyPairChain pairChain;
		private ControlBase selectControlBase;

		private Action<string> onSelectControlBase;

		private Action<ControlBaseData.Support.Data>       onChangeSupportData;

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
			onSelectControlBase = SelectControlBase;
			StrategyManager.GamePlayData.selectControlBase.AddLateListener(onSelectControlBase);

			// 항목 변경 이벤트
			onChangeSupportData = OnChangeSupportData;

			// 아이템 리스트
			offensiveItemList ??= new();
			defensiveItemList ??= new();
			offensiveItemList ??= new();
			facilitiesItemList ??= new();

			// 실행
			SelectControlBase(StrategyManager.GamePlayData.selectControlBase.Value);
			void SelectControlBase(string controlBase)
			{
				if (selectControlBase != null)
				{
					selectControlBase.Support.RemoveListener(onChangeSupportData);
					selectControlBase = null;
				}

				DeleteStatsItemList(offensiveItemList);
				DeleteStatsItemList(defensiveItemList);
				DeleteStatsItemList(supplyItemList);
				DeleteStatsItemList(facilitiesItemList);

				if (!StrategyManager.Collector.TryFindControlBase(controlBase, out selectControlBase))
				{
					DeInitEvent();
					return;
				}
				selectControlBase.Support.AddLateListener(onChangeSupportData);

				onChangeSupportData.Invoke(selectControlBase.SupportData);
			}

			void OnChangeSupportData(ControlBaseData.Support.Data data)
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
				selectControlBase.SupportBuffGroup.TryGetList(key, out var statsList);

				var list = statsList.GetValueList();
				int length = list == null ? 0 : list.Count;
				for (int i = 0 ; i < length ; i++)
				{
					var item = list[i];
					var type = item.StatsType;
					var value = item.Value;
					var symbol = item.Symbol;
					UpdateStatsItem(type, value, symbol, itemList, supportPanel.transform);
				}
			}
			void UpdateStatsItem(StatsType key, int value, SymbolType symbol, Dictionary<StatsType, LabelTextUI> list, Transform parent)
			{
				if (list.TryGetValue(key, out var uiObject))
				{
					//string label = StrategyManager.Key2Name.GetAsset(key.ToString());
					string text =  $"{(value>=0?"+":"-")}{value.ToString()}{SymbolToString(symbol)}";
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
					string text =  $"{(value>=0?"+":"-")}{value.ToString()}{SymbolToString(symbol)}";
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

				string SymbolToString(SymbolType symbolType) => symbol switch
				{
					SymbolType.Number => "",
					SymbolType.Percent => "%",
					_ => ""
				};
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
			selectControlBase = null;
		}
		private void DeInitEvent()
		{
			if (onSelectControlBase != null)
			{
				StrategyManager.GamePlayData.selectControlBase.RemoveListener(onSelectControlBase);
				onSelectControlBase = null;
			}

			if (selectControlBase != null)
			{
				selectControlBase.Support.RemoveListener(onChangeSupportData);
				selectControlBase = null;
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
	public class ControlBase_Facilities : StrategyViewController
	{
		private IKeyPairChain pairChain;
		private ControlBase selectControlBase;
		private IKeyPairChain facilitiesControlUI;
		private IKeyPairChain facilitiesInstallableUI;

		private TMP_Text facilitiesInfoText;

		private Action<string> onSelectControlBase;

		private Action<ControlBaseData.Facilities.Data>       onChangeFacilitiesData;

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
			onSelectControlBase = SelectControlBase;
			StrategyManager.GamePlayData.selectControlBase.AddLateListener(onSelectControlBase);

			// 항목 변경 이벤트
			onChangeFacilitiesData = OnChangeFacilitiesData;

			// 아이템 리스트
			facilitiesSlots ??= new();
			facilitiesItemList ??= new();

			// 실행
			SelectControlBase(StrategyManager.GamePlayData.selectControlBase.Value);
			void SelectControlBase(string controlBase)
			{
				if (selectControlBase != null)
				{
					selectControlBase.Facilities.RemoveListener(onChangeFacilitiesData);
					selectControlBase = null;
				}

				DeleteFacilitiesSlots(facilitiesSlots);
				DeleteStatsItemList(facilitiesItemList);

				if (!StrategyManager.Collector.TryFindControlBase(controlBase, out selectControlBase))
				{
					DeInitEvent();
					return;
				}
				selectControlBase.Facilities.AddLateListener(onChangeFacilitiesData);

				onChangeFacilitiesData.Invoke(selectControlBase.FacilitiesData);
			}

			void OnChangeFacilitiesData(ControlBaseData.Facilities.Data data)
			{
				ControlBaseData.Facilities.Slot[] slotData = data.slotData;
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
			void UpdateSlotInfo(int index, ControlBaseData.Facilities.Slot data)
			{
				var slotChain = facilitiesSlots[index];
				string key = data.facilitiesKey;
				var installing = data.installing;
			
				slotChain.FindPairChain<Button>("Button", out var Button);
				slotChain.FindPairChain<Image>("Image", out var Image);
				slotChain.FindPairChain<Image>("Installing", out var InstallingImage);
				slotChain.FindPairChain<TMP_Text>("Label", out var Label);

				if (Button != null)
				{
					Button.onClick.RemoveAllListeners();
					Button.onClick.AddListener(() => ShowSlotControlUI(index, data.facilitiesKey));
				}
				if (Image != null)
				{
					Image.sprite = StrategyManager.Key2Sprite.GetAsset(key);
				}
				if(InstallingImage != null)
				{
					if(string.IsNullOrWhiteSpace(installing.facilitiesKey) || installing.facilitiesKey == key)
					{
						InstallingImage.enabled = false;
					}
					else
					{
						InstallingImage.enabled = true;
						float installingTime = installing.installingTime;
						float timeRemaining = installing.timeRemaining;
						if(installingTime < 1) installingTime = 1;
						InstallingImage.fillAmount = 1f - (timeRemaining / installingTime);
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
				var list = selectControlBase.FacilitiesBuffGroup.MergedStatsValueList();

				int length = list == null ? 0 : list.Count;
				for (int i = 0 ; i < length ; i++)
				{
					var item = list[i];
					var type = item.StatsType;
					var value = item.Value;
					var symbol = item.Symbol;
					UpdateStatsItem(type, value, symbol, itemList, FacilitiesPanel.transform);
				}
			}
			void UpdateStatsItem(StatsType key, int value, SymbolType symbol, Dictionary<StatsType, LabelTextUI> list, Transform parent)
			{
				if (list.TryGetValue(key, out var uiObject))
				{
					//string label = StrategyManager.Key2Name.GetAsset(key.ToString());
					string text =  $"{(value>=0?"+":"-")}{value.ToString()}{SymbolToString(symbol)}";
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
					string text =  $"{(value>=0?"+":"-")}{value.ToString()}{SymbolToString(symbol)}";
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

				string SymbolToString(SymbolType symbolType) => symbol switch
				{
					SymbolType.Number => "",
					SymbolType.Percent => "%",
					_ => ""
				};
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
			selectControlBase = null;
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
			if (onSelectControlBase != null)
			{
				StrategyManager.GamePlayData.selectControlBase.RemoveListener(onSelectControlBase);
				onSelectControlBase = null;
			}

			if (selectControlBase != null)
			{
				selectControlBase.Facilities.RemoveListener(onChangeFacilitiesData);
				selectControlBase = null;
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


		public void OnStartFacilitiesInstall(int slotIndex, string facilitiesKey)
		{

		}

		public void OnShowInfoText(string text)
		{
			if (facilitiesInfoText == null) return;
			facilitiesInfoText.text = text;
		}
	}
	public class ControlBase_Garrison : StrategyViewController
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
