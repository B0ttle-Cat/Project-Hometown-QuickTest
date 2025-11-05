using System;

using Sirenix.OdinInspector;

using TMPro;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public partial class StrategyMapPanelUI // SectorSelectPanel
{
	[SerializeField, FoldoutGroup("SectorSelect")]
	private GameObject sectorSelectPrefab;
	[SerializeField, FoldoutGroup("SectorSelect")]
	private Transform sectorSelectRoot;
	[SerializeField, FoldoutGroup("SectorSelect"), InlineProperty, HideLabel]
	private SectorSelectPanel sectorSelectPanel;
	public IMapPanelTargeting SectorSelectTargeting => sectorSelectPanel;

	private void EnableSectorSelectPanel()
	{
		sectorSelectPanel = new SectorSelectPanel(sectorSelectPrefab, sectorSelectRoot, this);
		sectorSelectPanel.Enable();
	}
	private void DisableSectorSelectPanel()
	{
		if (sectorSelectPanel == null) return;
		sectorSelectPanel.Disable();
		sectorSelectPanel.Dispose();
		sectorSelectPanel = null;
	}
	private void SectorSelectPanelUpdate()
	{
		if (sectorSelectPanel == null) return;
		sectorSelectPanel.Update();
	}


	[Serializable]
	public class SectorSelectPanel : MapPanelUI, IMapPanelTargeting
	{
		private GameObject panelPrefab;
		private Transform panelRoot;

		private GameObject panelObject;
		private MapPanelItemUI mapPanelItemUI;
		[SerializeField, FoldoutGroup("Panel"), InlineProperty, HideLabel]
		private SectorPanel sectorPanel;
		public SectorSelectPanel(GameObject panelPrefab, Transform panelRoot, StrategyMapPanelUI panel) : base(panel)
		{
			this.panelPrefab = panelPrefab;
			this.panelRoot = panelRoot;
			sectorPanel = null;
		}
		protected override void OnDispose()
		{
			if (sectorPanel != null)
			{
				sectorPanel.Dispose();
				sectorPanel = null;
			}
			if (panelObject != null)
			{
				Destroy(panelObject);
				panelObject = null;
			}
			if (sectorPanel != null)
			{
				sectorPanel.Dispose();
				sectorPanel = null;
			}
		}
		protected override void OnEnable()
		{
			panelObject = GameObject.Instantiate(panelPrefab, panelRoot);
			mapPanelItemUI = panelObject.GetComponentInChildren<MapPanelItemUI>();
			if (mapPanelItemUI == null)
			{
				OffsetMapPanelItemUI offsetMapPanelItemUI = panelObject.AddComponent<OffsetMapPanelItemUI>();
				offsetMapPanelItemUI.Pivot = Vector2.one;
				offsetMapPanelItemUI.Offset = new Vector2(50f, 150f);
				mapPanelItemUI = offsetMapPanelItemUI;
			}
			panelObject.SetActive(false);
		}
		protected override void OnDisable()
		{
			if (panelObject != null)
			{
				Destroy(panelObject);
				panelObject = null;
			}
			mapPanelItemUI = null;
			if (sectorPanel != null) sectorPanel.Dispose();
		}

		void IMapPanelTargeting.AddTarget(IStrategyElement element)
		{
			if (panelObject == null) return;
			if (element != null && element is SectorObject sector && sector != null)
			{
				if (sectorPanel == null || !sectorPanel.IsValid)
					sectorPanel = new SectorPanel(mapPanelItemUI);
				sectorPanel.ChangeSector(sector);
				sectorPanel.Show();
			}
		}
		void IMapPanelTargeting.RemoveTarget(IStrategyElement element)
		{
			if (panelObject == null) return;
			if(sectorPanel == null) return;
			sectorPanel.Hide();
		}
		void IMapPanelTargeting.ClearTarget()
		{
			if (panelObject == null) return;
			sectorPanel.Hide();
		}

		[Serializable]
		private class SectorPanel : IDisposable
		{
			[SerializeField,ReadOnly]
			private SectorObject sector;
			[SerializeField,ReadOnly]
			private MapPanelItemUI panel;
			private IKeyPairChain pairChain;
			private bool? isShow;
			[SerializeField, ReadOnly]
			private GameObject[] iconList;
			public bool IsValid => panel != null && pairChain != null;
			public event UnityAction onShowDetail;
			public event UnityAction onDeployCombatantsClick;
			public event UnityAction onConstructFacilitiesClick;
			public event UnityAction onMoveTroopsClick;
			public event UnityAction onUseFacilitiesSkillButton;
			public SectorPanel(MapPanelItemUI panel)
			{
				this.panel = panel;
				pairChain = panel.gameObject.GetPairChain();
				iconList = new GameObject[32];
				SetupButtons();
				isShow = null;
			}
			public SectorPanel(MapPanelItemUI panel, SectorObject sector) : this(panel)
			{
				ChangeSector(sector);
			}
			public void Dispose()
			{
				if (isShow ?? false) Hide();

				ChangeSector(null);
				RemoveButtons();

				sector = null;
				panel = null;
				pairChain = null;

				ClearIcon();
				iconList = null;
			}
			public void Show()
			{
				if (!IsValid) return;
				if (isShow ?? false) return;

				isShow = true;
				panel.Show();
			}
			public void Hide()
			{
				if (!IsValid) return;
				if (!isShow ?? true) return;
				isShow = false;
				panel.Hide();
			}
			public void ChangeSector(SectorObject sector)
			{
				if (!IsValid) return;

				Deinit(this.sector);
				Init(sector);
				void Init(SectorObject sector)
				{
					this.sector = sector;
					if (sector == null) return;

					panel.SetTargetInMap(sector);

					sector.Profile.AddListener(OnChangeProfile);
					sector.Stats.AddListener(OnChangeStats);
					sector.Facilities.AddListener(OnChangeFacilities);
					sector.Support.AddListener(OnChangeSupport);

					OnChangeProfile(sector.ProfileData);
					OnChangeStats(sector.StatsData);
					OnChangeFacilities(sector.FacilitiesData);
					OnChangeSupport(sector.SupportData);

					onShowDetail = () => sector.Controller.OnShowUI_Detail();
					onDeployCombatantsClick = () => sector.Controller.OnControlButton_DeployCombatants();
					onConstructFacilitiesClick = () => sector.Controller.OnControlButton_ConstructFacilities();
					onMoveTroopsClick = () => sector.Controller.OnControlButton_MoveTroops();
					onUseFacilitiesSkillButton = () => sector.Controller.OnControlButton_UseFacilitiesSkill();

				}
				void Deinit(SectorObject sector)
				{
					if (sector == null) return;

					sector.Profile.RemoveListener(OnChangeProfile);
					sector.Stats.RemoveListener(OnChangeStats);
					sector.Facilities.RemoveListener(OnChangeFacilities);
					sector.Support.RemoveListener(OnChangeSupport);

					onShowDetail = null;
					onDeployCombatantsClick = null;
					onConstructFacilitiesClick = null;
					onMoveTroopsClick = null;
					onUseFacilitiesSkillButton = null;
				}
			}
			private void OnChangeProfile(StrategyGamePlayData.SectorData.Profile.Data data)
			{
				pairChain
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
				SetFillRectUI("Fill Durability", sector.GetDurability());
				SetFillRectUI("Fill Garrison", sector.GetGarrison());
				SetFillRectUI("Fill Material", sector.GetMaterial());
				SetFillRectUI("Fill Electric", sector.GetElectric());
			}
			private void SetFillRectUI(string fillRectName, (int value, int max) item)
			{
				int value = item.value;
				int max = item.max;
				pairChain.FindPairChain<FillRectUI>(fillRectName, out var fillRect);
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
				if (!IsValid) return;

				if (pairChain.TryFindPair<Button>("ShowDetail", out var showDetail))
				{
					showDetail.onClick.RemoveAllListeners();
					showDetail.onClick.AddListener(()=>onShowDetail?.Invoke());
				}
				if (pairChain.TryFindPair<Button>("전투원 배치", out var deployButton))
				{
					deployButton.onClick.RemoveAllListeners();
					deployButton.onClick.AddListener(()=>onDeployCombatantsClick?.Invoke());
				}
				if (pairChain.TryFindPair<Button>("시설 건설", out var constructButton))
				{
					constructButton.onClick.RemoveAllListeners();
					constructButton.onClick.AddListener(()=>onConstructFacilitiesClick?.Invoke());
				}
				if (pairChain.TryFindPair<Button>("병력 이동", out var moveButton))
				{
					moveButton.onClick.RemoveAllListeners();
					moveButton.onClick.AddListener(()=>onMoveTroopsClick?.Invoke());
				}
				if (pairChain.TryFindPair<Button>("시설 장비 사용", out var useButton))
				{
					useButton.onClick.RemoveAllListeners();
					useButton.onClick.AddListener(()=>onUseFacilitiesSkillButton?.Invoke());
				}
			}
			private void RemoveButtons()
			{
				if (!IsValid) return;

				if (pairChain.TryFindPair<Button>("ShowDetail", out var showDetail))
				{
					showDetail.onClick.RemoveAllListeners();
				}
				if (pairChain.TryFindPair<Button>("전투원 배치", out var deployButton))
				{
					deployButton.onClick.RemoveAllListeners();
				}
				if (pairChain.TryFindPair<Button>("시설 건설", out var constructButton))
				{
					constructButton.onClick.RemoveAllListeners();
				}
				if (pairChain.TryFindPair<Button>("병력 이동", out var moveButton))
				{
					moveButton.onClick.RemoveAllListeners();
				}
				if (pairChain.TryFindPair<Button>("시설 장비 사용", out var useButton))
				{
					useButton.onClick.RemoveAllListeners();
				}
			}
        }
	}
}