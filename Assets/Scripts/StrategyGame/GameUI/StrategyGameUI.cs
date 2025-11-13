using Sirenix.OdinInspector;

using UnityEngine;

public class StrategyGameUI : MonoBehaviour
{
	[SerializeField, ReadOnly] private StrategyMapPanelUI mapPanelUI;
	[SerializeField, ReadOnly] private StrategyControlPanelUI controlPanelUI;
	[SerializeField, ReadOnly] private StrategyMainPanelUI mainPanelUI;
	[SerializeField, ReadOnly] private StrategyDetailsPanelUI detailsPanelUI;
	[SerializeField, ReadOnly] private StrategyPopupPanelUI popupPanelUI;
	public StrategyMapPanelUI MapPanelUI { get => mapPanelUI; private set => mapPanelUI = value; }
	public StrategyControlPanelUI ControlPanelUI { get => controlPanelUI; private set => controlPanelUI = value; }
	public StrategyMainPanelUI MainPanelUI { get => mainPanelUI; private set => mainPanelUI = value; }
	public StrategyDetailsPanelUI DetailsPanelUI { get => detailsPanelUI; private set => detailsPanelUI = value; }
	public StrategyPopupPanelUI PopupPanelUI { get => popupPanelUI; private set => popupPanelUI = value; }
	private void Reset()
	{
		Init();
	}
    private void OnValidate()
    {
		Init();
	}
    private void Awake()
	{
		Init();
	}
	private void OnDestroy()
	{
		DeInit();
	}
	public void Init()
	{
		MapPanelUI = GetComponentInChildren<StrategyMapPanelUI>(true);
		ControlPanelUI = GetComponentInChildren<StrategyControlPanelUI>(true);
		MainPanelUI = GetComponentInChildren<StrategyMainPanelUI>(true);
		DetailsPanelUI = GetComponentInChildren<StrategyDetailsPanelUI>(true);
		PopupPanelUI = GetComponentInChildren<StrategyPopupPanelUI>(true);
	}
	public void DeInit()
	{
		MapPanelUI = null;
		ControlPanelUI = null;
		MainPanelUI = null;
		DetailsPanelUI = null;
		PopupPanelUI = null;
	}
}