using System;
using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class DetailsPanelUI : MonoBehaviour
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

		private RectTransform contentPrefab;
		private RectTransform contentParent;
		private Action<RectTransform> onStartShow;

		public RectTransform Tab { get => tab; }
        public RectTransform Content { get => content; }

        public TabSlot(string tabName, RectTransform tab, RectTransform contentPrefab, RectTransform contentParent, Action<RectTransform> onStartShow)
		{
			this.tabName = tabName;
			this.tab = tab;
			this.contentPrefab = contentPrefab;
			this.contentParent = contentParent;
			this.onStartShow = onStartShow;
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
			onStartShow?.Invoke(Content);

			if (Content.TryGetComponent<CanvasGroupUI>(out var groupUI))
			{
				groupUI.OnShow();
			}
		}
		public void OnHideContent(bool remove = true)
		{
			if (Content == null) return;
			if (Content.TryGetComponent<CanvasGroupUI>(out var groupUI))
			{
				groupUI.OnHide();
			}
			else
			{
				groupUI.gameObject.SetActive(true);
			}
		}
	}
	[SerializeField]
	protected List<TabSlot> tabSlots;

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
	public void AddContnet(string tabName, RectTransform prefab, Action<RectTransform> onStartShow)
	{
		AddContnet(tabName, prefab, onStartShow, tabSlots.Count == 0);
	}
	public void AddContnet(string tabName, RectTransform prefab, Action<RectTransform> onStartShow, bool isOn = false)
	{
		if (tabButtonSample == null) return;

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
			return;
		}

		var slot = new TabSlot(tabName, tabRect, prefab, contentParent, onStartShow);
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
	}

	public void SelectContent(int index)
	{
		if (tabSlots == null) return;

		if (index < 0 || index >= tabSlots.Count) return;

		var tab = tabSlots[index];
		if (tab != null && tab.Tab.TryGetComponent<Toggle>(out var ui))
		{
			if (ui != null && !ui.isOn) ui.isOn = true;
		}
	}
	public int GetCurrentSelect()
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
