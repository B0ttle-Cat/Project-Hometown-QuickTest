using System;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;

public partial class StrategyControlPanelUI : MonoBehaviour, IGamePanelUI, IViewStack, IStrategyStartGame
{
	private Canvas thisCanvas;

	public Stack<IPanelItemUI> ViewPanelUIStack { get; set; }
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
	public abstract class ControlPanelItem : IPanelItemUI
	{
		protected StrategyControlPanelUI panelUI;

		private GameObject panelPrefab;
		private Transform panelRoot;

		private GameObject panelObject;

		public bool IsShow { get; private set; }
		public bool IsDispose { get; private set; }
		public ControlPanelItem(GameObject prefab, Transform root, StrategyControlPanelUI panelUI)
		{
			this.panelUI = panelUI;
			this.panelPrefab = prefab;
			this.panelRoot = root;

			panelObject = null;
			IsShow = false;
			IsDispose = false;
		}
		public void Dispose()
		{
			if (IsDispose) return;
			IsDispose = true;

			Hide();
			OnDispose();
			if (panelObject != null)
			{
				Destroy(panelObject);
				panelObject = null;
			}
			panelPrefab = null;
			panelRoot = null;
			panelUI = null;
		}
		public void Show()
		{
			if (IsShow) return;
			IsShow = true;

			if (panelObject == null && panelPrefab != null)
			{
				panelObject = GameObject.Instantiate(panelPrefab, panelRoot);
				InstantiateFloatingPanelUI();
			}
			ShowFloatingPanelUI();
			OnShow();
		}
		public void Hide()
		{
			if (!IsShow) return;
			IsShow = false;

			if (ThgIsFloating)
			{
				HideFloatingPanelUI(_Hide);
			}
			else
			{
				_Hide();
			}
			void _Hide()
			{
				OnHide();
				if (this is IPanelFloating iPanel)
				{
					iPanel.ClearTarget();
				}
			}
		}
		private bool ThgIsFloating => this is IPanelFloating;
		private void InstantiateFloatingPanelUI()
		{
			if (this is not IPanelFloating panelFloating) return;

			panelFloating.FloatingPanelUI = panelObject.GetComponentInChildren<FloatingPanelItemUI>();
			if (panelFloating.FloatingPanelUI == null)
			{
				OffsetFloatingPanelItemUI offsetFloatingPanel = panelObject.AddComponent<OffsetFloatingPanelItemUI>();
				offsetFloatingPanel.Pivot = Vector2.one;
				offsetFloatingPanel.Offset = new Vector2(50f, 150f);
				panelFloating.FloatingPanelUI = offsetFloatingPanel;
			}
		}
		private void ShowFloatingPanelUI()
		{
			if (this is not IPanelFloating panelFloating) return;
			panelFloating.FloatingPanelUI.Show();
		}
		private bool HideFloatingPanelUI(Action hide)
		{
			if (this is not IPanelFloating panelFloating) return false;
			panelFloating.FloatingPanelUI.Hide(hide);
			return true;
		}
		protected abstract void OnShow();
		protected abstract void OnHide();
		protected abstract void OnDispose();

		[Serializable]
		protected abstract class ViewItem<TValue> : IViewItemUI, IDisposable
		{
			[SerializeField, ReadOnly]
			private TValue value;
			[SerializeField, ReadOnly]
			private ControlPanelItem panelUI;
			private IKeyPairChain keyPair;
			private bool isShow;
			private bool isDispose;
			public TValue Value => value;
			public IPanelItemUI ViewPanelUI => panelUI;
			public IKeyPairChain KeyPair => keyPair;
			public bool IsShow => isShow;
			public bool IsViewValid => keyPair != null;
			public ViewItem(TValue item, ControlPanelItem panel, bool callChangeValue = false)
			{
				value = item;
				panelUI = panel;
				keyPair = panel.panelObject.GetKeyPairChain();
				isShow = false;
				isDispose = false;
				if(callChangeValue) ChangeValue(Value);
			}
			public void Dispose()
			{
				if (isDispose) return;
				isDispose = true;

				Invisible();
				OnBeforeChangeValue();
				OnDispose();
				value = default;
				panelUI = null;
				keyPair = null;
			}
			public void Visible()
			{
				if (isShow) return;
				OnVisible();
			}
			public void Invisible()
			{
				if (!isShow) return;
				OnInvisible();
			}
			public void ChangeValue(TValue value)
			{
				if (this.value != null) OnBeforeChangeValue();
				this.value = value;
				if (this.value != null) OnAfterChangeValue();
			}
			protected abstract void OnDispose();
			protected abstract void OnVisible();
			protected abstract void OnInvisible();
			protected abstract void OnBeforeChangeValue();
			protected abstract void OnAfterChangeValue();
		}
	}
}
