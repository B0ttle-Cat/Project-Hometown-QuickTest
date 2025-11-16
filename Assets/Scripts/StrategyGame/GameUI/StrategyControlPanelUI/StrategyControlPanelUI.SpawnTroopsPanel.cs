using System;
using System.Collections.Generic;
using System.Linq;

using Sirenix.OdinInspector;

using UnityEngine;
using UnityEngine.UI;

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

			private List<(StrategyGamePlayData.UnitKey, NumericSliderUI)> numericSliders;

			float 세력_병력_최대허용량;
			float 세력_병력_편제요구량;
			float 세력_병력_신규편재;
			public SpawnPanel(SectorObject sector, ControlPanelItem panel, IViewStack viewStack) : base(sector.CaptureFaction, panel)
			{
				selectSector = sector;
				this.viewStack = viewStack;
				numericSliders = new List<(StrategyGamePlayData.UnitKey, NumericSliderUI)>();
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
				if (세력_병력_신규편재 > 세력_병력_최대허용량) return;

				var infoList = numericSliders
					.Select(i=>(i.Item1,Mathf.RoundToInt(i.Item2.Value)))
					.Where(i=>i.Item1 != UnitKey.None && i.Item2 > 0);

				if(selectSector.Controller.OnConfirmButton_SpawnTroops(new SpawnTroopsInfo(Value.FactionID, infoList.ToArray())))
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
						(_, NumericSliderUI ui) = numericSliders[i];
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
						KeyPair.FindPairChainAndCopy<NumericSliderUI>("SliderSample", parent, out NumericSliderUI sliderUI);
						sliderUI.gameObject.SetActive(true);
						numericSliders.Add((unitKey, sliderUI));
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
					StatsType.세력_병력_편제요구량);

				세력_병력_최대허용량 = factionStatsList[0].Value;
				세력_병력_편제요구량 = factionStatsList[1].Value;
				세력_병력_신규편재 = 0;

				int length = numericSliders.Count;
				for (int i = 0 ; i < length ; i++)
				{
					(UnitKey _, NumericSliderUI slider) = numericSliders[i];
					세력_병력_신규편재 += slider.Value;
				}
				float 세력_병력_여유용량 = 세력_병력_최대허용량 - 세력_병력_편제요구량 - 세력_병력_신규편재;
				for (int i = 0 ; i < length ; i++)
				{
					(UnitKey key, NumericSliderUI slider) = numericSliders[i];
					var info = StrategyManager.Key2UnitInfo.GetAsset(key);
					slider.Label = info.DisplayName;
					slider.SetMinMax(0, 세력_병력_여유용량, true);
					slider.AddOnValueChange(OnChangeSliderValue);
				}
				CheckSliderVaule();
			}
			private void OnChangeSliderValue(float _)
			{
				int length = numericSliders.Count;
				세력_병력_신규편재 = 0;
				for (int i = 0 ; i < length ; i++)
				{
					(UnitKey _, NumericSliderUI slider) = numericSliders[i];
					세력_병력_신규편재 += slider.Value;
				}
				float 세력_병력_여유용량 = 세력_병력_최대허용량 - 세력_병력_편제요구량 - 세력_병력_신규편재;
				for (int i = 0 ; i < length ; i++)
				{
					(UnitKey key, NumericSliderUI slider) = numericSliders[i];
					slider.SetHandleClamp(0, 세력_병력_여유용량 + slider.Value);
				}
				CheckSliderVaule();
			}
			private void CheckSliderVaule()
			{
				if (세력_병력_최대허용량 <= 0)
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
					float 세력_병력_예상치 = 세력_병력_신규편재 +  세력_병력_편제요구량;

					if (KeyPair.TryFindPair<FillRectUI>("TotalFill", out var fill))
					{
						float rate = (float)세력_병력_예상치 / (float)세력_병력_최대허용량;
						fill.SetValueText(rate, $"{세력_병력_예상치} / {세력_병력_최대허용량}");
					}

					if (KeyPair.TryFindPair<Button>("배치하기", out var confirm))
					{
						confirm.interactable = 세력_병력_예상치 <= 세력_병력_최대허용량;
					}
				}
			}
		}
	}
}