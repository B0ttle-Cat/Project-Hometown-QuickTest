using UnityEngine;

public class StrategyGameUI : MonoBehaviour
{
	private StrategyControlPanelUI controlPanelUI;
	private StrategyMainPanelUI mainPanelUI;
	private StrategyDetailsPanelUI detailsPanelUI;

    private void Awake()
    {
		controlPanelUI = GetComponentInChildren<StrategyControlPanelUI>();
		mainPanelUI = GetComponentInChildren<StrategyMainPanelUI>();
		detailsPanelUI = GetComponentInChildren<StrategyDetailsPanelUI>();
	}

    public void OpenDetailsPanel_FieldInfo_Overview()
	{
		if (detailsPanelUI == null) return;
		detailsPanelUI.selectContent = StrategyDetailsPanelUI.StrategyDetailsPanelType.FieldInfo_Overview;
		detailsPanelUI.OpenUI();
	}
	public void CloseDetailsPanel() 
	{
	}
}
