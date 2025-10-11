using System;

using Sirenix.OdinInspector;

using UnityEngine;
using UnityEngine.UI;

public class DetailsPanelUI : MonoBehaviour
{
	[SerializeField]
	protected RectTransform tabList;
	[SerializeField]
	protected RectTransform tabButtonSample;

	[SerializeField]
	protected RectTransform contentPanel;

	protected class TabSlot : IDisposable
	{
		private readonly int index;
		private readonly RectTransform tab;
		private readonly RectUIBuilder rectUIBuilder;
		public int Index { get => index; }
		public RectTransform Tab { get => tab; }
		public RectUIBuilder RectUIBuilder => rectUIBuilder;

		public TabSlot(int index, RectTransform tab, RectUIBuilder rectUIBuilder)
		{
			this.index = index;
			this.tab = tab;
			this.rectUIBuilder = rectUIBuilder;
		}
		public void Dispose()
		{
			if (tab != null)
			{
				Destroy(tab.gameObject);
			}
			if (rectUIBuilder != null)
			{
				rectUIBuilder.Dispose();
			}
		}

		public void OnShowContent()
		{
			if (rectUIBuilder != null)
			{
				rectUIBuilder.OnShow();
			}
		}
		public void OnHideContent(bool remove = true)
		{
			if (rectUIBuilder != null)
			{
				if (remove) rectUIBuilder.ClearBuild();
				else rectUIBuilder.OnHide();
			}
		}
	}
	protected TabSlot[] tabSlots;

	protected void Init()
	{
		if (tabSlots != null) return;
		tabSlots = new TabSlot[20];
	}

	protected void Deinit()
	{
		if (tabSlots == null) return;

		int length = tabSlots.Length;
		for (int i = 0 ; i < length ; i++)
		{
			var tab = tabSlots[i];
			if (tab != null)
			{
				tabSlots[i] = null;
				tab.Dispose();
			}
		}
		tabSlots = null;
	}


	[Button]
	public void ClearAllContent()
	{
		ClearContent(-1);
	}
	[Button]
	public void ClearContent(int index)
	{
		if (index < 0 || index >= tabSlots.Length)
		{
			Deinit();
		}
		else
		{
			var tab = tabSlots[index];
			if (tab != null)
			{
				tabSlots[index] = null;
				tab.Dispose();
			}
		}
	}
	public void AddContnet(string tabName, RectUIBuilder contentBuilder)
	{
		int index = 0;
		int length = tabSlots.Length;
		for (int i = 0 ; i < length ; i++)
		{
			if (tabSlots[i] == null)
			{
				index = i;
				break;
			}
		}
		AddContnet(index, tabName, contentBuilder, index == 0);
	}
	public void AddContnet(int index, string tabName, RectUIBuilder contentBuilder, bool isOn = false)
	{
		if (index < 0 || index >= tabSlots.Length) return;
		if (tabButtonSample == null) return;

		Init();

		RectTransform tabRect = GameObject.Instantiate(tabButtonSample, tabList);
		Toggle tabButton = tabRect.GetComponent<Toggle>();

		if (tabButton == null)
		{
			Destroy(tabRect.gameObject);
			return;
		}

		var slot = tabSlots[index];
		if (slot != null) slot.Dispose();
		slot = new TabSlot(index, tabRect, contentBuilder);

		// Tab Sort For Index
		int length = tabSlots.Length;
		int sortIndex = 0;
		for (int i = 0 ; i < length ; i++)
		{
			if (tabSlots[i] == null) continue;
			var tab = tabSlots[i].Tab;
			if (tab == null) continue;
			if (sortIndex != tab.GetSiblingIndex())
			{
				tab.SetSiblingIndex(sortIndex++);
			}
		}

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

		if (index < 0 || index >= tabSlots.Length) return;

		var tab = tabSlots[index];
		if (tab != null && tab.Tab.TryGetComponent<Toggle>(out var ui))
		{
			if (ui != null && !ui.isOn) ui.isOn = true;
		}
	}
	public int GetCurrentSelect()
	{
		if (tabSlots == null) return -1;

		int length = tabSlots.Length;
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
