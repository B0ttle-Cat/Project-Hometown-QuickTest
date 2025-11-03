using System;
using System.Collections;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public partial class StrategyMapPanelUI // SectorLabelPanel
{
	[SerializeField, FoldoutGroup("SectorLabel")]
	private GameObject sectorLabelPreafab;
	[SerializeField, FoldoutGroup("SectorLabel")]
	private Transform sectorLabelRoot;
	[SerializeField, FoldoutGroup("SectorLabel"), InlineProperty, HideLabel]
	private SectorLabelPanel sectorLabelPanel;

	private void EnableSectorLabelPanel()
	{
		sectorLabelPanel = new SectorLabelPanel(sectorLabelPreafab, sectorLabelRoot, this);
		sectorLabelPanel.Enable();
	}
	private void DisableSectorLabelPanel()
	{
		if (sectorLabelPanel == null) return;
		sectorLabelPanel.Disable();
		sectorLabelPanel.Dispose();
		sectorLabelPanel = null;
	}
	private void SectorLabelPanelUpdate()
	{
		if (sectorLabelPanel == null) return;
		sectorLabelPanel.Update();
	}


	[Serializable]
	public class SectorLabelPanel : MapPanelUI , IMapPanelTargeting
	{
		private GameObject mapPanelPreafab;
		private Transform mapPanelRoot;
		[SerializeField]
		private List<SectorLabel> sectorPanelList;
		[SerializeField]
		private Stack<GameObject> disableUIObject;

		[Serializable]
		public class SectorLabel : IDisposable
		{
			private SectorObject sector;
			private CameraVisibilityGroup visibility;
			private Transform parent;

			private Func<GameObject> popUIObject;
			private Action<GameObject> pushUIObject;

			private GameObject uiObject;
			private MapPanelItemUI mapPanelItemUI;
			private IKeyPairChain keyPairChain;
			private Button selectButton;

			private List<GameObject> iconlist;
			public SectorObject Sector { get => sector; private set => sector = value; }

#if UNITY_EDITOR
			[ShowInInspector]
			private string SectorName => sector == null ? "Null" : sector.ProfileData.sectorName;
#endif
			public SectorLabel(SectorObject sector, Transform parent, Func<GameObject> popUIObject, Action<GameObject> pushUIObject)
			{
				this.Sector = sector;
				this.parent = parent;
				this.popUIObject = popUIObject;
				this.pushUIObject = pushUIObject;
				this.visibility = sector.GetComponentInChildren<CameraVisibilityGroup>(true);

				if (visibility.IsVisible)
				{
					Visibility_OnVisibleEnter();
				}
				else
				{
					Hide();
				}

				visibility.OnVisibleEnter += Visibility_OnVisibleEnter;
				visibility.OnVisibleExit += Visibility_OnVisibleExit;
			}
			public void Dispose()
			{
				if (uiObject != null)
				{
					Visibility_OnVisibleExit();
					uiObject = null;
				}
				mapPanelItemUI = null;
				keyPairChain = null;
				selectButton = null;
				if (iconlist != null)
				{
					int length = iconlist.Count;
					for (int i = 0 ; i < length ; i++)
					{
						var icon = iconlist[i];
						if (icon == null) continue;
						Destroy(icon);
					}
					iconlist.Clear();
					iconlist = null;
				}

				popUIObject = null;
				pushUIObject = null;

				Sector = null;
				visibility = null;
				parent = null;
			}
			private void Visibility_OnVisibleEnter()
			{
				if (popUIObject != null)
				{
					uiObject = popUIObject.Invoke();
					mapPanelItemUI = uiObject.GetComponentInChildren<MapPanelItemUI>();
					keyPairChain = uiObject.GetPairChain();
					iconlist = new List<GameObject>();
				}

				Show();
			}
			private void Visibility_OnVisibleExit()
			{
				if (pushUIObject != null && uiObject != null)
				{
					pushUIObject.Invoke(uiObject);
				}

				Hide();
			}

			public void Show()
			{
				if (uiObject == null) return;
				mapPanelItemUI.SetTargetInMap(Sector);
				mapPanelItemUI.Show();

				keyPairChain
					.FindPairChain<TMP_Text>("Name", out var nameText)
					.FindPairChain<Button>("SelectButton", out selectButton);

				nameText.text = Sector.ProfileData.sectorName;

				selectButton.onClick.RemoveAllListeners();
				selectButton.onClick.AddListener(() => StrategyManager.Selecter.OnSystemSelectObject(Sector));
				Sector.EffectStatsGroup.AddListener(OnChangeGroupKey, OnRemoveGroupKey);
			}
			public void Hide()
			{
				if (uiObject == null) return;
				if (mapPanelItemUI != null) mapPanelItemUI.Hide();
				if (selectButton != null) selectButton.onClick.RemoveAllListeners();
				if (Sector != null) Sector.EffectStatsGroup.RemoveListener(OnChangeGroupKey, OnRemoveGroupKey);
			}

			public void OnChangeGroupKey(string key)
			{
				ChangeIconList();
			}
			public void OnRemoveGroupKey(string key)
			{
				ChangeIconList();
			}
			public void ChangeIconList()
			{
				keyPairChain.FindPairChain("IconParent", out var iconParent);
				if (iconParent != null)
				{
					var iconParenTr = iconParent.transform;

					while (iconParenTr.childCount != 0)
					{
						Destroy(iconParenTr.GetChild(0).gameObject);
					}
					iconlist ??= new List<GameObject>();
					iconlist.Clear();

					var keyList = Sector.EffectStatsGroup.GetkeyList();
					int length = keyList.Count;
					for (int i = 0 ; i < length ; i++)
					{
						var key = $"Icon_status_effect_{keyList[i]}";
						if (StrategyManager.Key2Sprite.TryGetAsset(key, out var sprite) && sprite != null)
						{
							keyPairChain.FindPairChainAndCopy<Image>("Icon", iconParent.transform, out var iconImage);
							iconImage.sprite = sprite;
							iconlist.Add(iconImage.gameObject);
							iconImage.gameObject.SetActive(true);
						}
					}
				}
			}
		}
		public SectorLabelPanel(GameObject panelPreafab, Transform panelRoot, StrategyMapPanelUI panel) : base(panel)
		{
			mapPanelPreafab = panelPreafab;
			mapPanelRoot = panelRoot;
			sectorPanelList = new List<SectorLabel>();
			disableUIObject = new Stack<GameObject>();
		}
		protected override void OnDispose()
		{
			mapPanelPreafab = null;
			mapPanelRoot = null;
			sectorPanelList = null;
			if (disableUIObject != null)
			{
				while (disableUIObject.TryPop(out var item))
				{
					Destroy(item);
				}
				disableUIObject.Clear();
				disableUIObject = null;
			}
		}
		protected override void OnEnable()
		{
			StrategyManager.Collector.AddChangeListener<SectorObject>(OnChangeList, out IList currentList);
			InitList(currentList);
		}
		protected override void OnDisable()
		{
			StrategyManager.Collector.RemoveChangeListener<SectorObject>(OnChangeList);
			if (this is IMapPanelTargeting target)
				target.ClearTarget();
		}
		protected override bool OnEnableCondition()
		{
			if (StrategyManager.MainCamera == null) return false;

			return StrategyManager.MainCamera.orthographicSize > 20f;
		}

		private void InitList(IList currentList)
		{
			if (currentList == null || currentList is not List<SectorObject> sectorList) return;
			if (sectorList.Count == 0) return;

			int length = sectorList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				var sector = sectorList[i];
				if (sector == null) continue;

				if (this is IMapPanelTargeting target)
					target.AddTarget(sector);
			}
		}
		private void OnChangeList(IStrategyElement element, bool isAdded)
		{
			if (isAdded) OnChangeList_Add();
			else OnChangeList_Remove();
			void OnChangeList_Add()
			{
				if(this is IMapPanelTargeting target)
					target.AddTarget(element);
			}
			void OnChangeList_Remove()
			{
				if (this is IMapPanelTargeting target)
					target.RemoveTarget(element);
			}
		}
		private GameObject PopUIObject()
		{
			if (disableUIObject == null) disableUIObject = new Stack<GameObject>();

			while (disableUIObject.TryPop(out var item))
			{
				if (item != null) return item;
			}
			if (mapPanelPreafab == null) return null;

			GameObject newUIObject = GameObject.Instantiate(mapPanelPreafab, mapPanelRoot);
			return newUIObject;
		}
		private void PushUIObject(GameObject uiObject)
		{
			if (disableUIObject == null) disableUIObject = new Stack<GameObject>();

			if (disableUIObject.Count < 10) disableUIObject.Push(uiObject);
			else Destroy(uiObject);
		}

        void IMapPanelTargeting.AddTarget(IStrategyElement element)
		{
			if (element == null || element is not SectorObject sector) return;
			int findIndex = sectorPanelList.FindIndex(p=>p.Sector == sector);
			if (findIndex < 0)
			{
				sectorPanelList.Add(new SectorLabel(sector, mapPanelRoot, PopUIObject, PushUIObject));
			}
		}
        void IMapPanelTargeting.RemoveTarget(IStrategyElement element)
		{
			if (element == null || element is not SectorObject sector) return;
			int findIndex = sectorPanelList.FindIndex(p=>p.Sector == sector);
			if (findIndex >= 0)
			{
				var item = sectorPanelList[findIndex];
				sectorPanelList.RemoveAt(findIndex);
				item.Dispose();
			}
		}
        void IMapPanelTargeting.ClearTarget()
		{
			if (sectorPanelList != null)
			{
				foreach (var item in sectorPanelList)
				{
					item?.Dispose();
				}
				sectorPanelList.Clear();
			}
		}
    }
}
