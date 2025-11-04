using System;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public interface ITabControl
{
	ITabControl ClearTab();
	ITabControl AddTab(params (string label, Action action)[] labels);
	ITabControl RemoveTab(params string[] labels);
	ITabControl GetTabControl();
}
public class TabPanelUI : MonoBehaviour, ITabControl
{
	[SerializeField]
	private ToggleGroup toggleGroup;
	[SerializeField]
	private GameObject toggleSample;

	[SerializeField, ReadOnly]
	private List<Tab> enableTabs;
	[ShowInInspector, ReadOnly]
	private Stack<Tab> disableTabs;

	private List<Tab> EnableTabs { get => enableTabs ??= new List<Tab>(); }
	private Stack<Tab> DisableTabs { get => disableTabs = new Stack<Tab>(); }

	[Serializable]
	private class Tab : IDisposable
	{
		[SerializeField, ReadOnly]
		private GameObject toggleObject;
		[SerializeField, ReadOnly]
		private Toggle toggle;
		[SerializeField, ReadOnly]
		private TMP_Text label;
		private Action action;
		private Action<Action> callback;

		[ShowInInspector, ReadOnly]
		public string LabelText => label != null ? label.text : "";

		public Tab(GameObject togglePrefab, ToggleGroup toggleGroup, Action<Action> callback)
		{
			toggleObject = GameObject.Instantiate(togglePrefab, toggleGroup.transform);
			this.toggle = toggleObject.GetComponentInChildren<Toggle>(true);
			this.label = toggleObject.GetComponentInChildren<TMP_Text>(true);
			action = null;
			this.callback = callback;

			toggle.group = toggleGroup;
			toggle.onValueChanged.RemoveAllListeners();
			toggle.onValueChanged.AddListener(OnChangeValue);
		}
		public void Dispose()
		{
			if (toggle != null)
			{
				toggle.onValueChanged.RemoveAllListeners();
				toggle = null;
			}
			label = null;
			action = null;
			callback = null;

			if (toggleObject != null)
			{
				GameObject.Destroy(toggleObject);
				toggleObject = null;
			}
		}
		public void Enable()
		{
			if (toggleObject != null)
			{
				toggleObject.SetActive(true);
				toggleObject.transform.SetAsLastSibling();
			}
		}
		public void Disable()
		{
			if (toggleObject != null)
			{
				toggleObject.SetActive(false);
			}
		}
		public void ChangeIndexAndLabel(Action action, string labelText)
		{
			this.action = action;
			if (label != null) label.text = labelText;
		}
		private void OnChangeValue(bool value)
		{
			if (!value || action == null) return;
			if (!toggleObject.activeInHierarchy) return;
			callback?.Invoke(action);
		}
	}

	public void OnDestroy()
	{
		if (enableTabs != null)
		{
			int length = enableTabs.Count;
			for (int i = 0 ; i < length ; i++)
			{
				enableTabs[i].Dispose();
			}
			enableTabs.Clear();
			enableTabs = null;
		}
		if (disableTabs != null)
		{
			while (disableTabs.TryPop(out Tab tab) && tab != null)
			{
				tab.Dispose();
			}
			disableTabs = null;
		}
		toggleGroup = null;
		toggleSample = null;
	}
	private void OnTabChange(Action action)
	{
		action.Invoke();
	}

	public ITabControl GetTabControl()
	{
		return this;
	}
	ITabControl ITabControl.ClearTab()
	{
		int length = EnableTabs.Count;
		for (int i = 0 ; i < length ; i++)
		{
			EnableTabs[i].Disable();
			DisableTabs.Push(EnableTabs[i]);
		}
		EnableTabs.Clear();
		return this;
	}

	ITabControl ITabControl.AddTab(params (string label, Action action)[] labels)
	{
		var list = EnableTabs;
		var stack = DisableTabs;

		int length = labels == null ? 0 : labels.Length;
		for (int i = 0 ; i < length ; i++)
		{
			string label = labels[i].label;
			Action action = labels[i].action;
			if(string.IsNullOrWhiteSpace(label)) continue;

			int index = list.Count;
			if (stack.TryPop(out Tab tab))
			{
				tab.ChangeIndexAndLabel(action, label);
				list.Add(tab);
				tab.Enable();
			}
			else
			{
				Tab newTab = new Tab(toggleSample, toggleGroup, OnTabChange);
				newTab.ChangeIndexAndLabel(action, label);
				list.Add(newTab);
				newTab.Enable();
			}
		}
		return this;
	}

	ITabControl ITabControl.RemoveTab(params string[] labels)
	{
		var list = EnableTabs;
		var stack = DisableTabs;

		int length = labels == null ? 0 : labels.Length;
		int tabCount = list.Count;
		for (int i = 0 ; i < length ; i++)
		{
			string label = labels[i];
			for (int ii = 0 ; ii < tabCount ; ii++)
			{
				Tab tab = list[ii];
				if (tab != null && label.Equals(tab.LabelText))
				{
					if (tabCount > ii)
					{
						list.RemoveAt(ii);
						if (stack.Count < 10)
						{
							tab.Disable();
							stack.Push(tab);
						}
						else
						{
							tab.Dispose();
						}
						break;
					}
				}
			}
		}
		return this;
	}
}
