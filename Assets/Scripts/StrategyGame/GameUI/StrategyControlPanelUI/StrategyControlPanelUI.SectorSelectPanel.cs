using System;

using Sirenix.OdinInspector;

using TMPro;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public partial class StrategyControlPanelUI // SectorSelectPanel
{
	[SerializeField, FoldoutGroup("SectorSelect")]
	private GameObject sectorSelectPrefab;
	[SerializeField, FoldoutGroup("SectorSelect")]
	private Transform sectorSelectRoot;
	[SerializeField, FoldoutGroup("SectorSelect"), InlineProperty, HideLabel]
	private SectorSelectPanel sectorSelectPanel;

	public IControlPanel ShowSectorSelectPanel()
	{
		sectorSelectPanel = new SectorSelectPanel(sectorSelectPrefab, sectorSelectRoot, this);
		ViewStack.Push(sectorSelectPanel);
		return sectorSelectPanel;
	}
	public void HideSectorSelectPanel()
	{
		if (sectorSelectPanel == null) return;
		ViewStack.Pop(sectorSelectPanel);
		sectorSelectPanel = null;
	}

	[Serializable]
	public class SectorSelectPanel : ControlPanelUI, IControlPanel
	{
		[SerializeField, FoldoutGroup("FloatingPanelUI"), InlineProperty, HideLabel]
		private SectorPanel sectorPanel;
		public SectorSelectPanel(GameObject prefab, Transform root, StrategyControlPanelUI panelUI) : base(prefab, root, panelUI)
		{
			sectorPanel = null;
		}
		protected override void OnDispose()
		{
            sectorPanel?.Dispose();
            sectorPanel = null;
        }
		protected override void OnShow()
		{
			sectorPanel?.Visible();
		}
		protected override void OnHide()
		{
			sectorPanel?.Unvisible();
		}

		void IPanelFloating.AddTarget(IStrategyElement element)
		{
			if (!IsShow) return;
			if (element != null && element is SectorObject sector && sector != null)
			{
				if (sectorPanel == null || !sectorPanel.IsViewValid)
					sectorPanel = new SectorPanel(this, sector);
				else 
					sectorPanel.ChangeValue(sector);

				FloatingPanelUI.SetTargetInMap(sector);
				sectorPanel.Visible();
			}
		}
		void IPanelFloating.RemoveTarget(IStrategyElement element)
		{
			if (!IsShow) return;
			sectorPanel?.Unvisible();
			FloatingPanelUI.RemoveTargetInMap();
		}
		void IPanelFloating.ClearTarget()
		{
			if (!IsShow) return;
			if (!IsShow) return;
			sectorPanel?.Unvisible();
			sectorPanel?.Dispose();
			sectorPanel = null;
		}

		[Serializable]
		private class SectorPanel : ViewItem<SectorObject>
		{
			[SerializeField, ReadOnly]
			private GameObject[] iconList;
			public event UnityAction onShowDetail;
			public event UnityAction onDeployUniqueUnit;
			public event UnityAction onConstructFacilities;
			public event UnityAction onPlanningTroopMovements;
			public event UnityAction onUseFacilitiesSkill;
			public SectorPanel(ControlPanelUI panel, SectorObject sector) : base(panel, sector) {}
			protected override void OnInit()
			{
				iconList = new GameObject[32];
				SetupButtons();
			}
			protected override void OnDispose()
			{
				ChangeValue(null);
				RemoveButtons();
				ClearIcon();
				iconList = null;
			}
			protected override void OnVisible()
			{

			}
			protected override void OnUnvisible()
			{
			
			}
			protected override void OnBeforeChangeValue()
			{
				Value.Profile.RemoveListener(OnChangeProfile);
				Value.Stats.RemoveListener(OnChangeStats);
				Value.Facilities.RemoveListener(OnChangeFacilities);
				Value.Support.RemoveListener(OnChangeSupport);

				onShowDetail = null;
				onDeployUniqueUnit = null;
				onConstructFacilities = null;
				onPlanningTroopMovements = null;
				onUseFacilitiesSkill = null;
			}
			protected override void OnAfterChangeValue()
			{
				Value.Profile.AddListener(OnChangeProfile);
				Value.Stats.AddListener(OnChangeStats);
				Value.Facilities.AddListener(OnChangeFacilities);
				Value.Support.AddListener(OnChangeSupport);

				OnChangeProfile(Value.ProfileData);
				OnChangeStats(Value.StatsData);
				OnChangeFacilities(Value.FacilitiesData);
				OnChangeSupport(Value.SupportData);

				onShowDetail = () => Value.Controller.OnShowUI_DetailUI();
				onDeployUniqueUnit = () => Value.Controller.OnControlButton_DeployUniqueUnit();
				onConstructFacilities = () => Value.Controller.OnControlButton_ConstructFacilities();
				onPlanningTroopMovements = () => Value.Controller.OnControlButton_SpawnTroops();
				onUseFacilitiesSkill = () => Value.Controller.OnControlButton_UseFacilitiesSkill();
			}
			private void OnChangeProfile(StrategyGamePlayData.SectorData.Profile.Data data)
			{
				PairChain
					.FindPairChain<TMP_Text>("Name", out var name)
					.FindPairChain("IconParent", out GameObject iconParent)
					.FindPairChain("IconSample", out GameObject iconSample)
					;

				if (name != null) name.text = data.sectorName;

				if (iconParent != null && iconSample != null)
				{
					SetIconList(iconParent.transform, iconSample, data.effects);
				}
				else
				{
					ClearIcon();
				}
			}
			private void ClearIcon()
			{
				if (iconList == null) return;
				int length = iconList.Length;
				for (int i = 0 ; i < length ; i++)
				{
					if (iconList[i] == null) continue;
					GameObject.Destroy(iconList[i]);
					iconList[i] = null;
				}
			}
			private void SetIconList(Transform parent, GameObject sample, StrategyGamePlayData.EffectsFlag effectFlag)
			{
				if (iconList == null) iconList = new GameObject[32];
				int effectFlagValue = (int)effectFlag;
				for (int i = 0 ; i < 32 ; i++)
				{
					int flagToCheck = 1 << i;
					if ((effectFlagValue & flagToCheck) != 0)
					{
						GameObject activeIcon = iconList[i];
						if (activeIcon == null)
						{
							StrategyGamePlayData.EffectsFlag flag = (StrategyGamePlayData.EffectsFlag)flagToCheck;
							activeIcon = GameObject.Instantiate(sample, parent);
							Image iconImage = activeIcon.GetComponentInChildren<Image>();
							iconImage.sprite = StrategyManager.Key2Sprite[$"Icon_status_effect_{flag}"] ?? iconImage.sprite;
							iconList[i] = activeIcon;
						}
						activeIcon.SetActive(true);
					}
					else
					{
						if (iconList[i] == null) continue;
						iconList[i].SetActive(false);
					}
				}
			}

			private void OnChangeStats(StrategyGamePlayData.SectorData.MainStats.Data data)
			{
				UpdateFillRectUI();
			}
			private void OnChangeFacilities(StrategyGamePlayData.SectorData.Facilities.Data data)
			{
				UpdateFillRectUI();
			}
			private void OnChangeSupport(StrategyGamePlayData.SectorData.Support.Data data)
			{
				UpdateFillRectUI();
			}
			private void UpdateFillRectUI()
			{
				SetFillRectUI("Fill Durability", Value.GetDurability());
				SetFillRectUI("Fill Troops", Value.GetTroops());
				SetFillRectUI("Fill Material", Value.GetMaterial());
				SetFillRectUI("Fill Electric", Value.GetElectric());
			}
			private void SetFillRectUI(string fillRectName, (int value, int max) item)
			{
				int value = item.value;
				int max = item.max;
				PairChain.FindPairChain<FillRectUI>(fillRectName, out var fillRect);
				if (fillRect == null) return;

				if (max > 0)
				{
					float rate = (float)value / (float)max;
					fillRect.gameObject.SetActive(true);
					fillRect.SetValueText(rate, $"{value,10} / {max,-10}");
				}
				else
				{
					fillRect.SetValueText(0, "");
				}
			}
			private void SetupButtons()
			{
				if (!IsViewValid) return;

				if (PairChain.TryFindPair<Button>("ShowDetail", out var showDetail))
				{
					showDetail.onClick.RemoveAllListeners();
					showDetail.onClick.AddListener(()=>onShowDetail?.Invoke());
				}
				if (PairChain.TryFindPair<Button>("전투원 배치", out var deployButton))
				{
					deployButton.onClick.RemoveAllListeners();
					deployButton.onClick.AddListener(()=>onDeployUniqueUnit?.Invoke());
				}
				if (PairChain.TryFindPair<Button>("시설 건설", out var constructButton))
				{
					constructButton.onClick.RemoveAllListeners();
					constructButton.onClick.AddListener(()=>onConstructFacilities?.Invoke());
				}
				if (PairChain.TryFindPair<Button>("병력 이동", out var moveButton))
				{
					moveButton.onClick.RemoveAllListeners();
					moveButton.onClick.AddListener(()=>onPlanningTroopMovements?.Invoke());
				}
				if (PairChain.TryFindPair<Button>("시설 장비 사용", out var useButton))
				{
					useButton.onClick.RemoveAllListeners();
					useButton.onClick.AddListener(()=>onUseFacilitiesSkill?.Invoke());
				}
			}
			private void RemoveButtons()
			{
				if (!IsViewValid) return;

				if (PairChain.TryFindPair<Button>("ShowDetail", out var showDetail))
				{
					showDetail.onClick.RemoveAllListeners();
				}
				if (PairChain.TryFindPair<Button>("전투원 배치", out var deployButton))
				{
					deployButton.onClick.RemoveAllListeners();
				}
				if (PairChain.TryFindPair<Button>("시설 건설", out var constructButton))
				{
					constructButton.onClick.RemoveAllListeners();
				}
				if (PairChain.TryFindPair<Button>("병력 이동", out var moveButton))
				{
					moveButton.onClick.RemoveAllListeners();
				}
				if (PairChain.TryFindPair<Button>("시설 장비 사용", out var useButton))
				{
					useButton.onClick.RemoveAllListeners();
				}
			}
        }
	}
}