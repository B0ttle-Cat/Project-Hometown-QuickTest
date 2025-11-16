using System;
using System.Collections.Generic;
using System.Linq;

using Sirenix.OdinInspector;

using UnityEngine;
using UnityEngine.UI;

using static KeyPairUnitInfo;
using static StrategyGamePlayData;
public partial class StrategyControlPanelUI // SpawnTroops
{
	[SerializeField, FoldoutGroup("SpawnTroops")]
	private GameObject spawnOperationPrefab;
	[SerializeField, FoldoutGroup("SpawnTroops")]
	private Transform spawnOperationRoot;
	[SerializeField, FoldoutGroup("SpawnTroops"), InlineProperty, HideLabel]
	private SpawnTroopsPanel spawnOperationPanel;
	public IPanelTarget ShowSpawnTroops()
	{
		spawnOperationPanel = new SpawnTroopsPanel(spawnOperationPrefab, spawnOperationRoot, this);
		ViewStack.Push(spawnOperationPanel);
		return spawnOperationPanel;
	}
	public void HideSpawnOperation()
	{
		if (spawnOperationPanel == null) return;
		ViewStack.Pop(spawnOperationPanel);
		spawnOperationPanel = null;
	}
	[Serializable]
	public class SpawnTroopsPanel : ControlPanelItem, IPanelTarget, IPanelFloating
	{
		public FloatingPanelItemUI FloatingPanelUI { get; set; }

		[SerializeField, FoldoutGroup("ViewItem"), InlineProperty, HideLabel]
		private SpawnPanel sectorPanel;
		public SpawnTroopsPanel(GameObject prefab, Transform root, StrategyControlPanelUI panelUI) : base(prefab, root, panelUI)
		{
			sectorPanel = null;
			FloatingPanelUI = null;
		}
		protected override void OnDispose()
		{
			sectorPanel?.Dispose();
			sectorPanel = null;
			FloatingPanelUI = null;
		}
		protected override void OnShow()
		{
			sectorPanel?.Visible();
		}
		protected override void OnHide()
		{
			sectorPanel?.Invisible();
		}
		void IPanelTarget.AddTarget(IStrategyElement element)
		{
			if (this is not IPanelFloating floating) return;
			floating.AddTarget(element);
		}

		void IPanelTarget.RemoveTarget(IStrategyElement element)
		{
			if (this is not IPanelFloating floating) return;
			floating.RemoveTarget(element);
		}
		void IPanelTarget.ClearTarget()
		{
			if (this is not IPanelFloating floating) return;
			floating.ClearTarget();
		}
		void IPanelFloating.AddTarget(IStrategyElement element)
		{
			if (!IsShow) return;
			if (element != null && element is SectorObject sector && sector != null)
			{
				if (sectorPanel == null || !sectorPanel.IsViewValid)
					sectorPanel = new SpawnPanel(sector, this, panelUI.ViewStack);
				else
					sectorPanel.ChangeValue(sector.CaptureFaction);
				FloatingPanelUI.SetTargetInMap(sector);
				sectorPanel.Visible();
			}
		}
		void IPanelFloating.RemoveTarget(IStrategyElement element)
		{
			if (!IsShow) return;
			sectorPanel?.Invisible();
			FloatingPanelUI.RemoveTargetInMap();
		}
		void IPanelFloating.ClearTarget()
		{
			if (!IsShow) return;
			sectorPanel?.Invisible();
			sectorPanel?.Dispose();
			sectorPanel = null;
		}

		[Serializable]
		private class SpawnPanel : ViewItem<Faction>
		{
			private SectorObject selectSector;
			private IViewStack viewStack;

			private List<(StrategyGamePlayData.UnitKey, KeyPairUnitInfo.UnitInfo, NumericSliderUI)> numericSliders;

			int 세력_병력_최대허용량;
			int 세력_병력_현재보유량;
			int 세력_병력_신규편제수;
			public SpawnPanel(SectorObject sector, ControlPanelItem panel, IViewStack viewStack) : base(sector.CaptureFaction, panel)
			{
				selectSector = sector;
				this.viewStack = viewStack;
				numericSliders = new List<(StrategyGamePlayData.UnitKey, KeyPairUnitInfo.UnitInfo, NumericSliderUI)>();
				ChangeValue(Value);
			}

			protected override void OnDispose()
			{
				numericSliders?.Clear();
				numericSliders = null;
			}
			protected override void OnInvisible()
			{
			}

			protected override void OnVisible()
			{
				if (KeyPair.TryFindPair<Button>("배치하기", out var confirm))
				{
					confirm.onClick.RemoveAllListeners();
					confirm.onClick.AddListener(OnClick_Confirm);
				}
				if (KeyPair.TryFindPair<Button>("취소", out var cancel))
				{
					cancel.onClick.RemoveAllListeners();
					cancel.onClick.AddListener(OnClick_Cancel);
				}
			}
			private void OnClick_Confirm()
			{
				if (selectSector == null) return;
				if (세력_병력_신규편제수 == 0 || 세력_병력_신규편제수 > 세력_병력_최대허용량)
				{
					// 편제 허용량 초과
					return;
				}
				if (!StrategyManager.Collector.TryFindFaction(Value.FactionID, out Faction faction))
				{
					// 유효하지 않은 세력
					return;
				}

				(UnitKey, int)[] spawnInfo = numericSliders
					.Select(i=>(i.Item1,Mathf.RoundToInt(i.Item3.Value)))
					.Where(i=>i.Item1 != UnitKey.None && i.Item2 > 0).ToArray();

				if (selectSector.Controller.OnConfirmButton_SpawnTroops(new SpawnTroopsInfo(Value.FactionID, spawnInfo)))
				{
					viewStack.ClearViewStack();
				}
				else
				{
					viewStack.Pop(ViewPanelUI);
				}
			}
			private void OnClick_Cancel()
			{
				viewStack.Pop(ViewPanelUI);
			}
			protected override void OnBeforeChangeValue()
			{
				ReleaseFaction();
				ReleaseContent();
				void ReleaseFaction()
				{
					Value.FactionStats.RemoveListener(OnChangeFactionStats);
				}
				void ReleaseContent()
				{
					int length = numericSliders.Count;
					for (int i = 0 ; i < length ; i++)
					{
						(_, _,NumericSliderUI ui) = numericSliders[i];
						if (ui == null) continue;
						ui.RemoveOnValueChange(OnChangeSliderValue);
						GameObject.Destroy(ui.gameObject);
					}
					numericSliders.Clear();
				}
			}
			protected override void OnAfterChangeValue()
			{
				UpdateContent();
				UpdateFaction();
				void UpdateContent()
				{
					if (!KeyPair.TryFindPair("Content", out var content)) return;
					Transform parent = content.transform;

					var unitKeyList = Value.GetAvailableUnitKeyList();
					int length = unitKeyList.Count;
					for (int i = 0 ; i < length ; i++)
					{
						var unitKey = unitKeyList[i];
						int findIndex = numericSliders.FindIndex((item) => item.Item1==unitKey);
						if (findIndex >= 0) continue;
						var info = StrategyManager.Key2UnitInfo.GetAsset(unitKey);
						KeyPair.FindPairChainAndCopy<NumericSliderUI>("SliderSample", parent, out NumericSliderUI sliderUI);
						sliderUI.gameObject.SetActive(true);
						numericSliders.Add((unitKey, info, sliderUI));
					}
				}
				void UpdateFaction()
				{
					Value.FactionStats.AddLateListener(OnChangeFactionStats);
					OnChangeFactionStats(new StatsValue());
				}
			}
			private void OnChangeFactionStats(StatsValue _)
			{
				var factionStatsList = Value.FactionStats.GetValueList(
					StatsType.세력_병력_최대허용량,
					StatsType.세력_병력_현재보유량);

				세력_병력_최대허용량 = factionStatsList[0].Value;
				세력_병력_현재보유량 = factionStatsList[1].Value;
				세력_병력_신규편제수 = 0;

				int length = numericSliders.Count;
				for (int i = 0 ; i < length ; i++)
				{
					(UnitKey _, UnitInfo _, NumericSliderUI slider) = numericSliders[i];
					세력_병력_신규편제수 += (int)slider.Value;
				}
				float 세력_병력_여유용량 = 세력_병력_최대허용량 - 세력_병력_현재보유량 - 세력_병력_신규편제수;
				for (int i = 0 ; i < length ; i++)
				{
					(UnitKey key, UnitInfo info, NumericSliderUI slider) = numericSliders[i];
					slider.Label = $"{info.DisplayName} : (+{info.UnitProfileObject.유닛_인력})";
					slider.SetMinMax(0, (int)(세력_병력_여유용량 /info.UnitProfileObject.유닛_인력), true);
					slider.AddOnValueChange(OnChangeSliderValue);
				}
				CheckSliderVaule();
			}
			private void OnChangeSliderValue(float _)
			{
				int length = numericSliders.Count;
				세력_병력_신규편제수 = 0;
				for (int i = 0 ; i < length ; i++)
				{
					(UnitKey _, UnitInfo info, NumericSliderUI slider) = numericSliders[i];
					세력_병력_신규편제수 += (int)slider.Value * info.UnitProfileObject.유닛_인력;
				}
				float 세력_병력_여유용량 = 세력_병력_최대허용량 - 세력_병력_현재보유량 - 세력_병력_신규편제수;
				for (int i = 0 ; i < length ; i++)
				{
					(UnitKey key, UnitInfo info, NumericSliderUI slider) = numericSliders[i];
					slider.SetHandleClamp(0, (int)(세력_병력_여유용량 + (slider.Value * info.UnitProfileObject.유닛_인력))/ info.UnitProfileObject.유닛_인력);
				}
				CheckSliderVaule();
			}
			private void CheckSliderVaule()
			{
				float 세력_병력_혀용용량 = 세력_병력_최대허용량 - 세력_병력_현재보유량;
				if (세력_병력_혀용용량 <= 0)
				{
					if (KeyPair.TryFindPair<FillRectUI>("TotalFill", out var fill))
					{
						fill.SetValueText(0, "편성 불가");
					}
					if (KeyPair.TryFindPair<Button>("배치하기", out var confirm))
					{
						confirm.interactable = false;
					}
				}
				else
				{
					if (KeyPair.TryFindPair<FillRectUI>("TotalFill", out var fill))
					{
						float rate = (float)세력_병력_신규편제수 / (float)세력_병력_혀용용량;
						fill.SetValueText(rate, $"{세력_병력_신규편제수} / {세력_병력_혀용용량}");
					}

					if (KeyPair.TryFindPair<Button>("배치하기", out var confirm))
					{
						confirm.interactable = 세력_병력_신규편제수 <= 세력_병력_혀용용량;
					}
				}
			}
		}
	}
}