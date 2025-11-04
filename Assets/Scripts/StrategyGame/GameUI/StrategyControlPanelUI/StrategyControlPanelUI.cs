using System;
using System.Collections.Generic;

using UnityEngine;

public partial class StrategyControlPanelUI : MonoBehaviour, IGamePanelUI, IStartGame
{
	public void OpenUI()
	{
	}
	public void CloseUI()
	{
	}

	void IStartGame.OnStartGame()
	{
		CloseUI();
	}

	void IStartGame.OnStopGame()
	{
		CloseUI();
	}
}

public partial class StrategyControlPanelUI
{
	public abstract class ControlPanel : IDisposable
	{
		protected StrategyControlPanelUI parentUI;
		private bool isShow;
		private bool isDispose;
		public ControlPanel(StrategyControlPanelUI parentUI)
		{
			this.parentUI = parentUI;
			isShow = false;
			isDispose = false;
		}
		public void Dispose()
		{
			if (!isDispose) return;
			isDispose = true;

			parentUI = null;
			OnDispose();
		}
		public void Show()
		{
			if (isShow) return;
			isShow = true;

			OnShow();
		}
		public void Hide()
		{
			if (!isShow) return;
			isShow = false;

			OnHide();
		}

		public abstract void OnShow();
		public abstract void OnHide();
		protected abstract void OnDispose();
	}
}
public partial class StrategyControlPanelUI
{
	public class MoveTroopsControlPanelUI
	{
		StrategyControlPanelUI ThisPanel;
		public MoveTroopsControlPanelUI(StrategyControlPanelUI thisPanel)
		{
			ThisPanel = thisPanel;
		}

		public class MoveTroops : ControlPanel
		{
			public List<SectorObject> fromSectors;
			public Queue<SectorObject> waypoints;
			public SectorObject toSector;
			public MoveTroops(StrategyControlPanelUI parentUI) : base(parentUI)
			{
			}
			public override void OnShow()
			{
			}
			public override void OnHide()
			{
			}
			protected override void OnDispose()
			{
			}
		}
	}
}