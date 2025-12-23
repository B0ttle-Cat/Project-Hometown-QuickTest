using System;

using GameUI;

using Sirenix.OdinInspector;

using UnityEngine;
using UnityEngine.UI;

public class StrategyMainTabButtonUI : TabButtonGroup
{
	[Serializable]
	struct ItemPair
	{
		[HorizontalGroup, LabelWidth(50)]
		public Toggle toggle;
		[HorizontalGroup, LabelWidth(50)]
		public RectTransform panel;
	}

	[SerializeField, HideInPlayMode]

	private ItemPair[] itemPairs;
	protected override void Show()
	{
		if (tabPanelGroup.IsNullRef()) return;


		int length = itemPairs.Length;
        for (int i = 0 ; i < length ; i++)
        {
			var pair = itemPairs[i];

			var newItem = new StrategyMainTabContentUI.MainTabPanelItem(pair.panel);
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
		public MainTabButton(Toggle toggle, ToggleGroup toggleGroup, IShowHide content, bool init) : base(toggle, toggleGroup, content, init)
		{
		}
	}
}
