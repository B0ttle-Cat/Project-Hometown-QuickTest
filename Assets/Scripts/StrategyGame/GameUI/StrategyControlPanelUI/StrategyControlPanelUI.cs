using System;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;

public partial class StrategyControlPanelUI : MonoBehaviour, IGamePanelUI, IViewStack, IStrategyStartGame
{
	private Canvas thisCanvas;

	public Stack<IViewPanelUI> ViewPanelUIStack { get; set; }
	public IViewStack ViewStack => this;
	public void OpenUI()
	{
		if (thisCanvas == null)
			thisCanvas = GetComponent<Canvas>();

		if (thisCanvas == null) return;
		thisCanvas.enabled = true;
	}
	public void CloseUI()
	{
		ViewStack.ClearViewStack();

		if (thisCanvas == null) return;
		thisCanvas.enabled = false;
	}
	void IStrategyStartGame.OnStartGame()
	{
		CloseUI();
	}
	void IStrategyStartGame.OnStopGame()
	{
		CloseUI();
	}
}

public partial class StrategyControlPanelUI
{
	public interface IControlPanel : IPanelFloating, IViewPanelUI
	{
	}

	public abstract class ControlPanelUI : IViewPanelUI
	{
		protected StrategyControlPanelUI panelUI;

		private GameObject panelPrefab;
		private Transform panelRoot;

		private GameObject panelObject;
		protected FloatingPanelItemUI FloatingPanelUI { get; private set; }

		public bool IsShow => panelObject != null && panelObject.activeSelf;
		private bool IsDispose => panelObject != null;
		public ControlPanelUI(GameObject prefab, Transform root, StrategyControlPanelUI panelUI)
		{
			this.panelUI = panelUI;
			this.panelPrefab = prefab;
			this.panelRoot = root;

			panelObject = null;
			FloatingPanelUI = null;
		}
		public void Dispose()
		{
			if (!IsDispose) return;

			Hide();
			OnDispose();
			if (panelObject != null)
			{
				Destroy(panelObject);
				panelObject = null;
			}
			FloatingPanelUI = null;
			panelPrefab = null;
			panelRoot = null;
			panelUI = null;
		}
		public void Show()
		{
			if (IsShow) return;
			if (panelPrefab == null) return;

			if (panelObject == null)
			{
				panelObject = GameObject.Instantiate(panelPrefab, panelRoot);
				FloatingPanelUI = panelObject.GetComponentInChildren<FloatingPanelItemUI>();
				if (FloatingPanelUI == null)
				{
					OffsetFloatingPanelItemUI offsetFloatingPanel = panelObject.AddComponent<OffsetFloatingPanelItemUI>();
					offsetFloatingPanel.Pivot = Vector2.one;
					offsetFloatingPanel.Offset = new Vector2(50f, 150f);
					FloatingPanelUI = offsetFloatingPanel;
				}
			}
			FloatingPanelUI.Show();
			OnShow();
		}
		public void Hide()
		{
			if (!IsShow) return;

			FloatingPanelUI.Hide(() =>
			{
				OnHide();
				if (this is IControlPanel iPanel)
				{
					iPanel.ClearTarget();
				}
			});
		}
		protected abstract void OnShow();
		protected abstract void OnHide();
		protected abstract void OnDispose();

		[Serializable]
		protected abstract class ViewItem<TValue> : IViewItemUI, IDisposable where TValue : class
		{
			[SerializeField, ReadOnly]
			private TValue value;
			[SerializeField, ReadOnly]
			private ControlPanelUI panelUI;
			private IKeyPairChain pairChain;
			private bool isShow;
			private bool isDispose;
			public TValue Value => value;
			public IViewPanelUI ViewPanelUI => panelUI;
			public IKeyPairChain PairChain => pairChain;
			public bool IsShow => isShow;
			public bool IsViewValid => pairChain != null;
			public ViewItem(ControlPanelUI panel, TValue sector)
			{
				value = sector;
				panelUI = panel;
				pairChain = panel.panelObject.GetPairChain();
				isShow = false;
				isDispose = false;
				OnInit();
				ChangeValue(Value);
			}
			public void Dispose()
			{
				if (!isDispose) return;
				isDispose = true;

				Unvisible();
				OnDispose();
				value = null;
				panelUI = null;
				pairChain = null;
			}
			public void Visible()
			{
				if (isShow) return;
				OnVisible();
			}
			public void Unvisible()
			{
				if (!isShow) return;
				OnUnvisible();
			}
			public void ChangeValue(TValue value)
			{
				if (this.value != null) OnBeforeChangeValue();
				this.value = value;
				if (this.value != null) OnAfterChangeValue();
			}
			protected abstract void OnDispose();
			protected abstract void OnInit();
			protected abstract void OnVisible();
			protected abstract void OnUnvisible();
			protected abstract void OnBeforeChangeValue();
			protected abstract void OnAfterChangeValue();
		}
	}
}