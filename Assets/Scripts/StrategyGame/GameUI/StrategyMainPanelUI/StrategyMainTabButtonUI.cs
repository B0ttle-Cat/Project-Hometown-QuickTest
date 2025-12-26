using System;

using GameUI;

using Sirenix.OdinInspector;

using UnityEngine;
using UnityEngine.UI;

public class StrategyMainTabButtonUI : TabButtonGroup
{
	[Serializable]
	public class ItemPair
	{
		[HorizontalGroup, LabelWidth(50)]
		public Toggle toggle;
		[HorizontalGroup, LabelWidth(50), PropertyOrder(1)]
		public RectTransform rect;

		[ShowInInspector, HorizontalGroup(width: 50), LabelText("IsOn"), LabelWidth(30)]
		public bool IsOn
		{
			get => !toggle.IsNullRef() && toggle.isOn;
			set { if (toggle.IsNotNullRef()) toggle.isOn = value; }
		}
	}

	[SerializeField, HideInPlayMode]

	private ItemPair[] itemPairs;

	protected override void InitTab()
	{
		base.InitTab();
		if (tabPanelGroup.IsNotNullRef())
		{
			tabPanelGroup.InitTab();
		}

		Toggle[] toggles = GetComponentsInChildren<Toggle>(true);
		int length = toggles.Length;
		itemPairs = new ItemPair[length];
		for (int i = 0 ; i < length ; i++)
		{
			var panel = tabPanelGroup.IsNullRef() ? null : tabPanelGroup.GetPanelItem(i);
			itemPairs[i] = new()
			{
				toggle = toggles[i],
				rect = panel.IsNullRef() ? null : panel.ThisRect
			};
			if (panel.IsNotNullRef())
			{
				
				if (panel.ThisRect.TryGetComponent<IShowHide>(out var showHide))
				{
					if (itemPairs[i].IsOn)
					{
						showHide.OnShow();
					}
					else
					{
						showHide.OnHide();
					}
				}
				else
				{
					panel.ThisRect.gameObject.SetActive(itemPairs[i].IsOn);
				}
			}
		}
	}


	protected override void Show()
	{
		if (tabPanelGroup.IsNullRef()) return;


		int length = itemPairs.Length;
		for (int i = 0 ; i < length ; i++)
		{
			var pair = itemPairs[i];

			var newItem = new StrategyMainTabContentUI.MainTabPanelItem(pair.rect);
			tabPanelGroup.Add(newItem);

			var newButton = new MainTabButton(pair.toggle, toggleGroup, newItem, pair.toggle.isOn);
			this.Add(newButton);
		}
	}

	protected override void Hide()
	{
		tabPanelGroup.Clear();
		this.Clear();
	}

	[Serializable]
	public class MainTabButton : TabButton
	{
		public MainTabButton(Toggle toggle, ToggleGroup toggleGroup, IShowHide content, bool init) : base(toggle, toggleGroup, content, init) { }
	}
}
