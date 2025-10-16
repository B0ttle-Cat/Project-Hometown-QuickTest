using UnityEngine;

public class StrategyGameUI : MonoBehaviour
{
	private StrategyDetailsPanelUI detailsPanelUI;

    private void Awake()
    {
		detailsPanelUI = GetComponent<StrategyDetailsPanelUI>();
	}

    public void OpenDetailsPanel_FieldInfo_Overview()
	{
		if (detailsPanelUI == null) return;
		detailsPanelUI.OpenUI(StrategyDetailsPanelUI.StrategyDetailsPanelType.FieldInfo_Overview);
	}
	public void CloseDetailsPanel() 
	{
	}
}
