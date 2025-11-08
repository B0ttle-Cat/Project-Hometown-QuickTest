using System;

using UnityEngine;

public partial class StrategyMapPanelUI : MonoBehaviour, IGamePanelUI, IStartGame
{
	public bool IsOpen { get; set; }

	public void Awake()
	{
		CloseUI();
	}

	public void OpenUI()
	{
		IsOpen = true;
		ShowSectorLabelPanel();
	}
	public void CloseUI()
	{
		IsOpen = false;
		HideSectorLabelPanel();
	}
	public void Update()
	{
		if (!IsOpen) return;

		SectorLabelPanelUpdate();
	}
	void IStartGame.OnStartGame()
	{
		ShowSectorLabelPanel();
	}
	void IStartGame.OnStopGame()
	{
		CloseUI();
	}
}
public partial class StrategyMapPanelUI
{
	public interface IMapPanel : IPanelFloating
	{
	}
	public abstract class MapPanelUI : IDisposable, IViewPanelUI
	{
		protected StrategyMapPanelUI ThisPanel { get; private set; }
		private bool? isShow = null;
		protected MapPanelUI(StrategyMapPanelUI panel)
		{
			ThisPanel = panel;
			isShow = null;
		}
		public void Dispose()
		{
			if (isShow ?? false) Hide();
		
			OnDispose();
			ThisPanel = null;
			isShow = null;
		}
		public void Show()
		{
			if (isShow ?? false) return;
			isShow = true;
			OnShow();
		}
		public void Hide()
		{
			if (!isShow ?? true) return;
			isShow = false;
			OnHide();
		}
		public void Update()
		{
			if (OnEnableCondition())
			{
				Show();
			}
			else
			{
				Hide();
			}
		}
		protected virtual bool OnEnableCondition() => true;
		protected abstract void OnShow();
		protected abstract void OnHide();
		protected abstract void OnDispose();
    }
}