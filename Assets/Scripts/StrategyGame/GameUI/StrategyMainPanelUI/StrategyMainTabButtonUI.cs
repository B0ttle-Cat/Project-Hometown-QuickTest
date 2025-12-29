using System;

using GameUI;

using Sirenix.OdinInspector;

using UnityEngine;
using UnityEngine.UI;

public class StrategyMainTabButtonUI : TabButtonGroup, IShowHideAsync
{
	[Serializable]
	public class ItemPair
	{
		[HorizontalGroup, LabelWidth(50)]
		public Toggle toggle;
		[HorizontalGroup, LabelWidth(50), PropertyOrder(1)]
		public PanelItemComponent panel;

		[ShowInInspector, HorizontalGroup(width: 50), LabelText("IsOn"), LabelWidth(30)]
		public bool IsOn
		{
			get => !toggle.IsNullRef() && toggle.isOn;
			set { if (toggle.IsNotNullRef()) toggle.isOn = value; }
		}
	}

	[SerializeField, HideInPlayMode]

	private ItemPair[] initItemPairs;
	void IShowHide.StartShow()
	{
		canvasGroupUI.ThisShowHide.OnShow();
		int length = initItemPairs.Length;

		for (int i = 0 ; i < length ; i++)
		{
			var pair = initItemPairs[i];
			var newButton = new MainTabButton(pair.toggle, toggleGroup, pair.panel, pair.toggle.isOn);
			this.Add(newButton);
		}
	}
	void IShowHide.EndedHide()
	{
		canvasGroupUI.ThisShowHide.OnHide();
		this.Clear();

	}

	[Serializable]
	public class MainTabButton : TabButton
	{
		public MainTabButton(Toggle toggle, ToggleGroup toggleGroup, IShowHide content, bool init) : base(toggle, toggleGroup, content, init) { }
	}
}
