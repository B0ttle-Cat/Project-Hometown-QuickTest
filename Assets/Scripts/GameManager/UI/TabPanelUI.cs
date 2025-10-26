using System;
using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class TabPanelUI : MonoBehaviour
{
	[SerializeField]
	protected RectTransform tabList;
	[SerializeField]
	protected RectTransform tabButtonSample;
	[Space]
	[SerializeField]
	protected TMP_Text contentTitle;
	[SerializeField]
	protected RectTransform contentParent;

	[Serializable]
	protected class TabSlot : IDisposable
	{
		public string tabName;
		private RectTransform tab;
		private RectTransform content;
		private ViewController contentView;

		private RectTransform contentPrefab;
		private RectTransform contentParent;

		public RectTransform Tab { get => tab; }
        public RectTransform Content { get => content; }

        public TabSlot(string tabName, RectTransform tab, RectTransform contentPrefab, RectTransform contentParent, ViewController contentView )
		{
			this.tabName = tabName;
			this.tab = tab;
			this.contentPrefab = contentPrefab;
			this.contentParent = contentParent;
			this.contentView = contentView;
		}
		void CreateContentUI()
		{
			if (content != null) return;
			if (contentPrefab == null) return;

			content = GameObject.Instantiate(contentPrefab, contentParent);
			content.gameObject.name = contentPrefab.name;

			content.localPosition = Vector3.zero;
			content.localRotation = Quaternion.identity;
			content.localScale = Vector3.one;

			content.anchorMin = Vector2.zero;
			content.anchorMax = Vector2.one;
			content.anchoredPosition = Vector2.zero;
			content.sizeDelta = Vector2.zero;
			content.pivot = Vector3.one * 0.5f;
		}
		public void Dispose()
		{
			if (tab != null)
			{
				Destroy(tab.gameObject);
				tab = null;
			}
			if(contentPrefab == null)
			{
				contentPrefab = null;
			}
			if (contentView != null)
			{
				contentView.Dispose();
			}
			if (content != null)
			{
				Destroy(content.gameObject);
				content = null;
			}
		}

		public void OnShowContent()
		{
			if (Content == null)
			{
				CreateContentUI();
			}

			if (Content == null) return;
			Content.gameObject.SetActive(true);
			if (Content.TryGetComponent<CanvasGroupUI>(out var groupUI))
			{
				groupUI.OnShow();
			}

			if (contentView != null)
			{
				contentView.OnShow(Content);
			}
		}
		public void OnHideContent()
		{
			if (Content == null) return;
			if (Content.TryGetComponent<CanvasGroupUI>(out var groupUI))
			{
				groupUI.OnHide();
			}
			else
			{
				Content.gameObject.SetActive(true);
			}

			if (contentView != null)
			{
				contentView.OnHide();
			}
		}
	}

	[SerializeField]
	protected List<TabSlot> tabSlots;

	[Serializable]
	public abstract class ContentController
	{
		protected TabPanelUI component;

		public ContentController(TabPanelUI component)
		{
			this.component = component;
		}
		public abstract void OnShow();
		public abstract void OnHide();
	}

	[Serializable]
	public abstract class ViewController : IDisposable
	{
		protected TabPanelUI component;
		protected ContentController viewController;
		public void Init(TabPanelUI component, ContentController viewModel) 
		{
			this.component = component;
			this.viewController  = viewModel;
		}
		public void Dispose()
		{
			component = null;
			viewController  = null;
			OnDispose();
		}
		public virtual void OnInit() { }
		public abstract void OnShow(RectTransform viewRect);
		public abstract void OnHide();
		public abstract void OnDispose();
    }


	protected void Init()
	{
		if (tabSlots == null) tabSlots = new List<TabSlot>();
	}
	protected void Deinit()
	{
		if (tabSlots == null) return;

		int length = tabSlots.Count;
		for (int i = 0 ; i < length ; i++)
		{
			var tab = tabSlots[i];
			if (tab != null)
			{
				tabSlots[i] = null;
				tab.Dispose();
			}
		}
		tabSlots.Clear();
	}

	public void ContentTitleText(string titleName)
	{
		if (contentTitle == null) return;
		contentTitle.text = titleName;
	}

	public void ClearAllContent()
	{
		ClearContent(-1);
	}
	public void ClearContent(string tabName)
	{
		if (tabSlots == null) return;

		int length = tabSlots.Count;
		for (int i = 0 ; i < length ; i++)
		{
			var tab = tabSlots[i];
			if (tab != null && tab.tabName.Equals(tabName))
			{
				tabSlots.RemoveAt(i);
				tab.Dispose();
				return;
			}
		}
	}
	public void ClearContent(int index)
	{
		if (index < 0 || index >= tabSlots.Count)
		{
			Deinit();
		}
		else
		{
			var tab = tabSlots[index];
			if (tab != null)
			{
				tabSlots.RemoveAt(index);
				tab.Dispose();
				return;
			}
		}
	}

	public TView AddTabAndContnet<TView>(string tabName, RectTransform prefab, ContentController thisModel, bool isOn = false) where TView : ViewController, new()
	{
		if (tabButtonSample == null) return null;
		Init();

		RectTransform tabRect = GameObject.Instantiate(tabButtonSample, tabList);
		tabRect.gameObject.name = tabName + "_tab";
		TMP_Text tabTitle = tabRect.GetComponentInChildren<TMP_Text>();
		if (tabTitle != null)
		{
			tabTitle.text = tabName;
		}

		Toggle tabButton = tabRect.GetComponent<Toggle>();

		if (tabButton == null)
		{
			Destroy(tabRect.gameObject);
			return null;
		}
		ViewController tabContentView = new TView();
		tabContentView.Init(this, thisModel);
		var slot = new TabSlot(tabName, tabRect, prefab, contentParent , tabContentView);
		tabSlots.Add(slot);
			
		// Tab Active
		tabRect.gameObject.SetActive(true);
		tabButton.onValueChanged.AddListener(TabValueChanged);
		void TabValueChanged(bool _isOn)
		{
			if (_isOn) slot.OnShowContent();
			else slot.OnHideContent();
		}
		if (tabButton.isOn == isOn)
		{
			TabValueChanged(isOn);
		}
		else
		{
			tabButton.isOn = isOn;
		}
		return tabContentView as TView;
	}

	public void SelectTabIndex(int index)
	{
		if (tabSlots == null) return;

		if (index < 0 || index >= tabSlots.Count) return;

		var tab = tabSlots[index];
		if (tab != null && tab.Tab.TryGetComponent<Toggle>(out var ui))
		{
			if (ui != null && !ui.isOn) ui.isOn = true;
		}
	}
	public int GetSelectTabIndex()
	{
		if (tabSlots == null) return -1;

		int length = tabSlots.Count;
		for (int i = 0 ; i < length ; i++)
		{
			var tab = tabSlots[i];
			if (tab != null && tab.Tab.TryGetComponent<Toggle>(out var ui))
			{
				if (ui.isOn) return i;
			}
		}
		return -1;
	}
}
