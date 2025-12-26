using GameUI;

using UnityEngine;

public class StrategyMainTabContentUI : TabPanelGroup
{
    [SerializeField]
    private CanvasGroupUI[] panels;

    public override void InitTab()
    {
		this.New();
		this.Clear();

		panels = GetComponentsInChildren<CanvasGroupUI>(true);

		int length = panels.Length;
        for (int i = 0 ; i < length ; i++)
        {
			this.Add(new MainTabPanelItem(panels[i]));
		}
    }

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
