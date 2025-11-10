using System;
using System.Collections;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public partial class StrategyMapPanelUI // SectorLabelGroup
{
	[SerializeField, FoldoutGroup("SectorLabel")]
	private GameObject sectorLabelPreafab;
	[SerializeField, FoldoutGroup("SectorLabel")]
	private Transform sectorLabelRoot;
	[SerializeField, FoldoutGroup("SectorLabel"), InlineProperty, HideLabel]
	private SectorLabelGroup sectorLabelGroup;

	private void ShowSectorLabelGroup()
	{
		sectorLabelGroup = new SectorLabelGroup(sectorLabelPreafab, sectorLabelRoot, this);
		sectorLabelGroup.Show();
	}
	private void HideSectorLabelGroup()
	{
		if (sectorLabelGroup == null) return;
		sectorLabelGroup.Hide();
		sectorLabelGroup.Dispose();
		sectorLabelGroup = null;
	}
	private void SectorLabelGroupUpdate()
	{
		if (sectorLabelGroup == null) return;
		sectorLabelGroup.Update();
	}

	public class SectorLabelGroup : MapLabelGroup<SectorLabelGroup.SectorLabel>
	{
		HashSet<CameraVisibilityGroup> aliveVisibility;

		public SectorLabelGroup(GameObject preafab, Transform parent, StrategyMapPanelUI panel) : base(preafab, parent, panel)
		{
			aliveVisibility = new HashSet<CameraVisibilityGroup>();
		}

		protected override void OnDispose()
		{
			if (aliveVisibility != null)
			{
				foreach (var visibility in aliveVisibility)
				{
					if (visibility == null) continue;
					visibility.OnVisibleEnter -= Visibility_OnVisibleEnter;
					visibility.OnVisibleExit -= Visibility_OnVisibleExit;
				}
				aliveVisibility.Clear();
				aliveVisibility = null;
			}
			base.OnDispose();
		}

		protected override void OnHide()
		{
			StrategyManager.Collector.RemoveChangeListener<SectorObject>(OnChangeList);
			int length = LabelList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				var label = LabelList[i];
				if (label.IsShow && label is IViewItemUI view)
				{
					view.Unvisible();
				}
			}
		}

		protected override void OnShow()
		{
			StrategyManager.Collector.AddChangeListener<SectorObject>(OnChangeList, out IList currentList);
			InitList(currentList);
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
				OnChangeList(sector, true);
			}
		}

		private void OnChangeList(IStrategyElement element, bool isAdded)
		{
			if (element is not SectorObject sector) return;

			if (isAdded) OnChangeList_Add();
			else OnChangeList_Remove();
			void OnChangeList_Add()
			{
				var visibility = sector.GetComponentInChildren<CameraVisibilityGroup>(true);
				if (visibility != null && aliveVisibility.Add(visibility))
				{
					visibility.OnVisibleEnter += Visibility_OnVisibleEnter;
					visibility.OnVisibleExit += Visibility_OnVisibleExit;
				}
				if (visibility.IsVisible) Visibility_OnVisibleEnter(sector);
				else Visibility_OnVisibleExit(sector);
			}
			void OnChangeList_Remove()
			{
				var visibility = sector.GetComponentInChildren<CameraVisibilityGroup>(true);
				if (visibility != null && aliveVisibility.Remove(visibility))
				{
					visibility.OnVisibleEnter -= Visibility_OnVisibleEnter;
					visibility.OnVisibleExit -= Visibility_OnVisibleExit;
				}
			}
		}
		private void Visibility_OnVisibleEnter(Component target)
		{
			if (target == null || target is not SectorObject sector) return;
			Visibility_OnVisibleEnter(sector);
		}
		private void Visibility_OnVisibleExit(Component target)
		{
			if (target == null || target is not SectorObject sector) return;
			Visibility_OnVisibleExit(sector);
		}
		private void Visibility_OnVisibleEnter(SectorObject sector)
		{
			if (sector == null) return;

			int length = LabelList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				var label = LabelList[i];
				if (label.Sector == sector)
				{
					if (!label.IsShow && label is IViewItemUI view)
					{
						view.Visible();
						return;
					}
				}
			}
			{
				var newLabel = new SectorLabel(sector, PopLabelUiObject(), this);
				LabelList.Add(newLabel);
				if (newLabel is IViewItemUI view)
				{
					view.Visible();
				}
			}
		}
		private void Visibility_OnVisibleExit(SectorObject sector)
		{
			if (sector == null) return;

			int length = LabelList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				var label = LabelList[i];
				if (label.Sector == sector)
				{
					if (label.IsShow && label is IViewItemUI view)
					{
						view.Unvisible();
						return;
					}
				}
			}
		}



		public class SectorLabel : MapLabel
		{
			private readonly SectorObject sector;
			public SectorObject Sector => sector;

			private TMP_Text labelText;
			private Button selectButton;
			private Transform iconParent;
			private List<GameObject> iconlist;
			public SectorLabel(SectorObject sector, GameObject uiObject, MapLabelGroup<SectorLabel> group) : base(uiObject, group)
			{
				this.sector = sector;

				KeyPair
					.FindPairChain<TMP_Text>("Name", out labelText)
					.FindPairChain<Button>("SelectButton", out selectButton)
					.FindPairChain("IconParent", out var iconParentObj);

				if (iconParentObj != null)
					iconParent = iconParentObj.transform;
			}
			protected override void OnDispose()
			{
				labelText = null;
				selectButton = null;
				iconParent = null;

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
			}

			protected override void Unvisible()
			{
				if (this is IMapPanel panel)
				{
					panel.RemoveTarget(Sector);
				}

				if (selectButton != null) selectButton.onClick.RemoveAllListeners();
				if (Sector != null) Sector.StatusEffectStatsGroup.RemoveListener(OnChangeGroupKey, OnRemoveGroupKey);
			}

			protected override void Visible()
			{
				if (this is IMapPanel panel)
				{
					panel.AddTarget(Sector);
				}

				labelText.text = Sector.ProfileData.sectorName;

				selectButton.onClick.RemoveAllListeners();
				selectButton.onClick.AddListener(() => StrategyManager.Selecter.OnSystemSelectObject(Sector));
				Sector.StatusEffectStatsGroup.AddListener(OnChangeGroupKey, OnRemoveGroupKey);
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
				if (iconParent != null)
				{
					while (iconParent.childCount != 0)
					{
						Destroy(iconParent.GetChild(0).gameObject);
					}
					iconlist ??= new List<GameObject>();
					iconlist.Clear();

					var keyList = Sector.StatusEffectStatsGroup.GetkeyList();
					int length = keyList.Count;
					for (int i = 0 ; i < length ; i++)
					{
						var key = $"Icon_status_effect_{keyList[i]}";
						if (StrategyManager.Key2Sprite.TryGetAsset(key, out var sprite) && sprite != null)
						{
							KeyPair.FindPairChainAndCopy<Image>("Icon", iconParent, out var iconImage);
							iconImage.sprite = sprite;
							iconlist.Add(iconImage.gameObject);
							iconImage.gameObject.SetActive(true);
						}
					}
				}
			}
		}
	}


	[Serializable]
	public class SectorLabelGroup2 : MapPanelUI, IMapPanel
	{

		[SerializeField]
		private List<SectorLabel> sectorPanelList;
		[SerializeField]
		private Stack<GameObject> disableUIObject;

		FloatingPanelItemUI IPanelFloating.FloatingPanelUI { get; }

		[Serializable]
		public class SectorLabel : IDisposable
		{
			private SectorObject sector;
			private CameraVisibilityGroup visibility;
			private Transform parent;

			private Func<GameObject> popUIObject;
			private Action<GameObject> pushUIObject;

			private GameObject uiObject;
			private FloatingPanelItemUI mapPanelItemUI;
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
					Visibility_OnVisibleEnter(sector);
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
					Visibility_OnVisibleExit(sector);
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
			private void Visibility_OnVisibleEnter(Component _)
			{
				if (popUIObject != null)
				{
					uiObject = popUIObject.Invoke();
					mapPanelItemUI = uiObject.GetComponentInChildren<FloatingPanelItemUI>();
					keyPairChain = uiObject.GetKeyPairChain();
					iconlist = new List<GameObject>();
				}

				Show();
			}
			private void Visibility_OnVisibleExit(Component _)
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
				Sector.StatusEffectStatsGroup.AddListener(OnChangeGroupKey, OnRemoveGroupKey);
			}
			public void Hide()
			{
				if (uiObject == null) return;
				if (mapPanelItemUI != null) mapPanelItemUI.Hide();
				if (selectButton != null) selectButton.onClick.RemoveAllListeners();
				if (Sector != null) Sector.StatusEffectStatsGroup.RemoveListener(OnChangeGroupKey, OnRemoveGroupKey);
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

					var keyList = Sector.StatusEffectStatsGroup.GetkeyList();
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
		public SectorLabelGroup2(GameObject preafab, Transform parent, StrategyMapPanelUI panel) : base(preafab, parent, panel)
		{
			sectorPanelList = new List<SectorLabel>();
			disableUIObject = new Stack<GameObject>();
		}
		protected override void OnDispose()
		{
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
		protected override void OnShow()
		{
			StrategyManager.Collector.AddChangeListener<SectorObject>(OnChangeList, out IList currentList);
			InitList(currentList);
		}
		protected override void OnHide()
		{
			StrategyManager.Collector.RemoveChangeListener<SectorObject>(OnChangeList);
			if (this is IPanelFloating target)
				target.ClearTarget();
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

				if (this is IPanelFloating target)
					target.AddTarget(sector);
			}
		}
		private void OnChangeList(IStrategyElement element, bool isAdded)
		{
			if (isAdded) OnChangeList_Add();
			else OnChangeList_Remove();
			void OnChangeList_Add()
			{
				if (this is IPanelFloating target)
					target.AddTarget(element);
			}
			void OnChangeList_Remove()
			{
				if (this is IPanelFloating target)
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
			if (Preafab == null) return null;

			GameObject newUIObject = GameObject.Instantiate(Preafab, Parent);
			return newUIObject;
		}
		private void PushUIObject(GameObject uiObject)
		{
			if (disableUIObject == null) disableUIObject = new Stack<GameObject>();

			if (disableUIObject.Count < 10) disableUIObject.Push(uiObject);
			else Destroy(uiObject);
		}

		void IPanelFloating.AddTarget(IStrategyElement element)
		{
			if (element == null || element is not SectorObject sector) return;
			int findIndex = sectorPanelList.FindIndex(p=>p.Sector == sector);
			if (findIndex < 0)
			{
				sectorPanelList.Add(new SectorLabel(sector, Parent, PopUIObject, PushUIObject));
			}
		}
		void IPanelFloating.RemoveTarget(IStrategyElement element)
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
		void IPanelFloating.ClearTarget()
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
