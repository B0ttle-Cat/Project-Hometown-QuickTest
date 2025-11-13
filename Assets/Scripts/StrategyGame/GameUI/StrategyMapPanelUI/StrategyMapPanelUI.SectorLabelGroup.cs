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
					visibility.OnChangeVisible -= Visibility_OnChangeVisible;
					visibility.OnChangeInvisible -= Visibility_OnChangeInvisible;
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
					view.Invisible();
				}
			}
		}

		protected override void OnShow()
		{
			StrategyManager.Collector.AddChangeListener<SectorObject>(OnChangeList, out IList currentList);
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
			if (element == null || element is not SectorObject sector || sector == null) return;

			if (isAdded) OnChangeList_Add();
			else OnChangeList_Remove();
			void OnChangeList_Add()
			{
				var visibility = sector.GetComponentInChildren<CameraVisibilityGroup>(true);
				if (visibility != null && aliveVisibility.Add(visibility))
				{
					visibility.OnChangeVisible += Visibility_OnChangeVisible;
					visibility.OnChangeInvisible += Visibility_OnChangeInvisible;
				}
				if (visibility.IsVisible) Visibility_OnVisibleEnter(sector);
				else Visibility_OnVisibleExit(sector);
			}
			void OnChangeList_Remove()
			{
				var visibility = sector.GetComponentInChildren<CameraVisibilityGroup>(true);
				if (visibility != null && aliveVisibility.Remove(visibility))
				{
					visibility.OnChangeVisible -= Visibility_OnChangeVisible;
					visibility.OnChangeInvisible -= Visibility_OnChangeInvisible;
				}
			}
		}
		private void Visibility_OnChangeVisible(Component target)
		{
			if (target == null || target is not SectorObject sector) return;
			Visibility_OnVisibleEnter(sector);
		}
		private void Visibility_OnChangeInvisible(Component target)
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
				if (label != null && label.Sector == sector)
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
						view.Invisible();
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

			protected override void Invisible()
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
}
