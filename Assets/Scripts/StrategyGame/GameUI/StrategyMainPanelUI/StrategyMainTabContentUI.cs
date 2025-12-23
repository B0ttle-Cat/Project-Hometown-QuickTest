using GameUI;

using UnityEngine;

public class StrategyMainTabContentUI : TabPanelGroup
{
    protected override void Hide()
    {
    }

    protected override void Show()
    {
    }

    public class MainTabPanelItem : TabPanelItem
    {
        public MainTabPanelItem(GameObject uiObject) : base(uiObject)
        {
        }

		public MainTabPanelItem(CanvasGroupUI canvasGroupUI) : base(canvasGroupUI)
        {
        }

		public MainTabPanelItem(RectTransform rectTransform) : base(rectTransform)
        {
        }
    }
}
