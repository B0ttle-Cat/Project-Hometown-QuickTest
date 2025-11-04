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

		EnableSectorLabelPanel();
		EnableSectorSelectPanel();
	}
	public void CloseUI()
	{
		IsOpen = false;

		DisableSectorLabelPanel();
		DisableSectorSelectPanel();
	}
	public void Update()
	{
		if (!IsOpen) return;

		SectorLabelPanelUpdate();
	}
	void IStartGame.OnStartGame()
	{
		EnableSectorLabelPanel();
		EnableSectorSelectPanel();
	}
	void IStartGame.OnStopGame()
	{
		CloseUI();
	}
}
public partial class StrategyMapPanelUI
{
	public abstract class MapPanelUI : IDisposable 
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
			if (isShow ?? false) Disable();
		
			OnDispose();
			ThisPanel = null;
			isShow = null;
		}
		public void Enable()
		{
			if (isShow ?? false) return;
			isShow = true;
			OnEnable();
		}
		public void Disable()
		{
			if (!isShow ?? true) return;
			isShow = false;
			OnDisable();
		}
		public void Update()
		{
			if (OnEnableCondition())
			{
				Enable();
			}
			else
			{
				Disable();
			}
		}
		protected virtual bool OnEnableCondition() => true;
		protected abstract void OnEnable();
		protected abstract void OnDisable();
		protected abstract void OnDispose();
    }
	public interface IMapPanelTargeting
	{
		public void AddTarget(IStrategyElement element);
		public void RemoveTarget(IStrategyElement element);
		public void ClearTarget();
	}
}