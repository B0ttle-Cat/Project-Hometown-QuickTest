using System;

using Sirenix.OdinInspector;

using UnityEngine;
using UnityEngine.UI;

public partial class StrategyControlPanelUI : MonoBehaviour, IGamePanelUI, IStartGame
{
	private Canvas canvas;
	private GraphicRaycaster graphicRaycaster;

	private ControlPanel currentControlPanel;
	private ISelectMouse currentSelected;

	public bool IsOpen { get; set; }
	private void Awake()
	{
		canvas = GetComponent<Canvas>();
		graphicRaycaster = GetComponent<GraphicRaycaster>();
		currentControlPanel = null;
		currentSelected = null;
		LastDeselectAnything(null);
	}
	public void OpenUI()
	{
		if (canvas != null) canvas.enabled = true;
		if (graphicRaycaster != null) graphicRaycaster.enabled = true;

		SwitchControlPanel(NewControlPanelType(currentSelected));
	}
	public void CloseUI()
	{
		if (canvas != null) canvas.enabled = false;
		if (graphicRaycaster != null) graphicRaycaster.enabled = false;

		currentControlPanel?.Hide();
		currentControlPanel = null;
	}
	void IStartGame.OnStartGame()
	{
		StrategyManager.Selecter.AddListener_OnFirstAndLast(FirstSelectAnything, LastDeselectAnything);
		if (StrategyManager.Selecter.GetCurrentSelectList.Count > 0)
		{
			OpenUI();
		}
	}
	void IStartGame.OnStopGame()
	{
		CloseUI();
		StrategyManager.Selecter.RemoveListener_OnFirsAndLast(FirstSelectAnything, LastDeselectAnything);
	}
	private void FirstSelectAnything(ISelectMouse select)
	{
		currentSelected = select;
		OpenUI();
	}
	private void LastDeselectAnything(ISelectMouse select)
	{
		CloseUI();
		currentSelected = null;
	}

	private ControlPanel NewControlPanelType(ISelectMouse select)
	{
		return select switch
		{
			SectorObject so => new SectorControlPanel(this),
			_ => null,
		};
	}
	private void SwitchControlPanel(ControlPanel newControlPanel)
	{
		currentControlPanel?.Hide();
		currentControlPanel = newControlPanel;
		currentControlPanel?.Show();
	}
}
public partial class StrategyControlPanelUI
{
	public abstract class ControlPanel : IDisposable
	{
		protected StrategyControlPanelUI ThisPanel { get; private set; }
		protected ISelectMouse Selected => ThisPanel.currentSelected;
		protected ControlPanel(StrategyControlPanelUI thisPanel)
		{
			ThisPanel = thisPanel;
		}

		public void Show()
		{
			OnShow();
		}
		public void Hide()
		{
			OnHide();
		}
		protected abstract void OnShow();
		protected abstract void OnHide();
		public void Dispose()
		{
			OnDispose();
			ThisPanel = null;
		}
		protected virtual void OnDispose()
		{

		}
	}
}

public partial class StrategyControlPanelUI
{
	[Serializable, InlineProperty,HideLabel]
	public struct SectorControlPanelUIPrefab
	{
		public RectTransform mainPanel;
	}

	[SerializeField, FoldoutGroup("SectorUI")]
	private SectorControlPanelUIPrefab sectorPrefab;
	public class SectorControlPanel : ControlPanel
	{
		private SectorControlPanelUIPrefab Prefabs => ThisPanel.sectorPrefab;
		private GameObject mainPanelPrefabs;

		private GameObject showMainPanel;
		private CanvasGroupUI canvasGroupUI;
		private SectorMainFloatingUI floatingUI;
		public SectorControlPanel(StrategyControlPanelUI thisPanel) : base(thisPanel)
		{
			mainPanelPrefabs = Prefabs.mainPanel != null ? Prefabs.mainPanel.gameObject : null;
		}
		protected override void OnShow()
		{
			if (mainPanelPrefabs == null) return;
			showMainPanel = GameObject.Instantiate(mainPanelPrefabs, ThisPanel.transform);

			if (showMainPanel.TryGetComponent<CanvasGroupUI>(out canvasGroupUI))
			{
				canvasGroupUI.OnShow();
			}
			if (showMainPanel.TryGetComponent<SectorMainFloatingUI>(out floatingUI))
			{
				floatingUI.enabled = true;

				var selecteSector = Selected as SectorObject;
				var sectorTransform = selecteSector.transform;
				Plane sectorPlane =  new Plane(sectorTransform.up, sectorTransform.position);

				var camera = Camera.main;
				var mouseCurrPosition = StrategyManager.Selecter.GetInputData.mouseCurrPosition;
				var mouseRay = camera.ScreenPointToRay(mouseCurrPosition);
				if (sectorPlane.Raycast(mouseRay, out float enter))
				{
					var rayHitPoint = mouseRay.GetPoint(enter);

					Transform anchor =  floatingUI.Anchor;
					if (anchor == null) anchor = new GameObject("anchor").transform;
					anchor.position = rayHitPoint;
					floatingUI.SetAnchor(anchor);
				}
			}
		}

		protected override void OnHide()
		{
			if (showMainPanel == null) return;
			WaitDestroy();

			showMainPanel = null;
			canvasGroupUI = null;
			floatingUI = null;

			async void WaitDestroy()
			{
				var waitObject = showMainPanel;
				var waitCanvasGroupUI = canvasGroupUI;

				waitCanvasGroupUI.OnHide();
				while (waitObject != null && waitCanvasGroupUI != null && waitCanvasGroupUI.IsSyncOn)
				{
					if (waitObject != null)
					{
						Destroy(waitObject);
						waitObject = null;
					}
					else
					{
						await Awaitable.NextFrameAsync();
					}
				}
			}
		}
	}
}
