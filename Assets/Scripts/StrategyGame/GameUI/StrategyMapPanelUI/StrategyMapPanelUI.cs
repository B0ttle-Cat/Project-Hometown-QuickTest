using System;
using System.Collections.Generic;

using UnityEngine;

public partial class StrategyMapPanelUI : MonoBehaviour, IGamePanelUI, IStrategyStartGame
{
	private Canvas canvas;
	private RectUICollisionAvoidance rectUICollisionAvoidance;

	public bool IsOpen { get; set; }

	public void Awake()
	{
		canvas = GetComponent<Canvas>();
		rectUICollisionAvoidance = GetComponent<RectUICollisionAvoidance>();
		CloseUI();
	}

	public void OpenUI()
	{
		IsOpen = true;
		canvas.enabled = true;
	}
	public void CloseUI()
	{
		IsOpen = false;
		canvas.enabled = false;
	}
	public void LateUpdate()
	{
		if (!IsOpen) return;

		SectorLabelGroupUpdate();
		OperationLabelGroupUpdate();
		RectUICollisionAvoidance();
	}
	void IStrategyStartGame.OnStartGame()
	{
		OpenUI();
	}
	void IStrategyStartGame.OnStopGame()
	{
		CloseUI();
	}
	public void RectUICollisionAvoidance()
	{
		if (rectUICollisionAvoidance == null) return;

		//rectUICollisionAvoidance.SetAvoidanceRecttransform()

		rectUICollisionAvoidance.AvoidanceUpdate();
	}
}
public partial class StrategyMapPanelUI : IViewAndControlModeChange
{
	void IViewAndControlModeChange.OnChangeMode(ViewAndControlModeType changeMode)
	{
		if (changeMode == ViewAndControlModeType.None)
		{
			HideSectorLabelGroup();
			HideOperationLabelGroup();
			return;
		}

		if (changeMode == ViewAndControlModeType.OperationsMode)
		{
			ShowSectorLabelGroup();
			ShowOperationLabelGroup();
		}
		else if (changeMode == ViewAndControlModeType.TacticsMode)
		{
			HideSectorLabelGroup();
			HideOperationLabelGroup();
		}
	}
}
public partial class StrategyMapPanelUI // MapPanelUI
{
	public interface IMapPanel : IPanelFloating
	{
	}
	public abstract class MapPanelUI : IDisposable, IViewPanelUI
	{
		protected StrategyMapPanelUI ThisPanel { get; private set; }
		protected GameObject Preafab { get; private set; }
		protected Transform Parent { get; private set; }
		private bool isShow = false;
		public bool IsShow => isShow;

		protected MapPanelUI(GameObject preafab, Transform parent, StrategyMapPanelUI panel)
		{
			ThisPanel = panel;
			this.Preafab = preafab;
			this.Parent = parent;
			isShow = false;
		}
		public void Dispose()
		{
			if (ThisPanel == null) return;
			if (isShow) Hide();

			OnDispose();
			Preafab = null;
			Parent = null;
			ThisPanel = null;
			isShow = false;
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
		public void Update()
		{
			if (isShow)
			{
				OnUpdate();
			}
		}
		protected abstract void OnShow();
		protected abstract void OnHide();
		protected abstract void OnDispose();
		protected virtual void OnUpdate() { }
	}

	public abstract class MapLabelGroup<T> : MapPanelUI where T : MapLabelGroup<T>.MapLabel
	{
		private List<T> labelList;
		private Stack<GameObject> diableLabelUI;
		protected List<T> LabelList { get => labelList; private set => labelList = value; }

		protected MapLabelGroup(GameObject preafab, Transform parent, StrategyMapPanelUI panel) : base(preafab, parent, panel)
		{
			diableLabelUI = new Stack<GameObject>();
			LabelList = new List<T>();
		}
		protected override void OnDispose()
		{
			if (LabelList != null)
			{
				int length = LabelList.Count;
				for (int i = 0 ; i < length ; i++)
				{
					var label = LabelList[i];
					if (label == null) continue;
					label.Dispose();
				}
				LabelList.Clear();
				LabelList = null;
			}

			if (diableLabelUI != null)
			{
				foreach (var item in diableLabelUI)
				{
					if (item == null) continue;
					GameObject.Destroy(item);
				}
				diableLabelUI.Clear();
				diableLabelUI = null;
			}
		}
		protected GameObject PopLabelUiObject(bool setActive = true)
		{
			if (diableLabelUI.TryPop(out var uiObject) && uiObject != null)
			{
				uiObject.SetActive(setActive);
				return uiObject;
			}

			if (Preafab.activeSelf != setActive)
			{
				Preafab.SetActive(setActive);
			}
			uiObject = GameObject.Instantiate(Preafab, Parent);
			uiObject.SetActive(setActive);
			if (Preafab.activeSelf != setActive)
			{
				Preafab.SetActive(!setActive);
			}
			return uiObject;
		}
		protected void PushLabelUiObject(T labelUI)
		{
			if (labelUI == null) return;
			if (LabelList.Remove(labelUI))
			{
				var uiObject = labelUI.UIObject;
				if (uiObject == null) return;
				if (diableLabelUI.Count < 10)
				{
					uiObject.SetActive(false);
					diableLabelUI.Push(uiObject);
				}
				else
				{
					GameObject.Destroy(uiObject);
				}
			}
			else
			{
				var uiObject = labelUI.UIObject;
				if (uiObject == null) return;
				GameObject.Destroy(uiObject);
			}
		}

		protected override void OnUpdate()
		{
			int length = LabelList.Count;
			bool isSizeChange = false;
			for (int i = 0 ; i < length ; i++)
			{
				OnLabelUpdate(LabelList[i], ref isSizeChange);
				if (isSizeChange)
				{
					length = LabelList.Count;
					isSizeChange = false;
				}
			}
		}
		protected virtual void OnLabelUpdate(T target, ref bool isSizeChange)
		{
			if (target != null)
			{
				target.Update(ref isSizeChange);
			}
		}

		public abstract class MapLabel : IDisposable, IMapPanel, IViewItemUI
		{
			protected MapLabelGroup<T> ThisGroup { get; private set; }
			public GameObject UIObject { get; private set; }
			public IKeyPairChain KeyPair { get; private set; }
			public FloatingPanelItemUI FloatingPanelUI { get; set; }

			private bool isShow = false;
			private float invisibleTimeLimit;
			private float invisibleTime;
			public bool IsShow => isShow;

			protected MapLabel(GameObject uiObject, MapLabelGroup<T> group)
			{
				ThisGroup = group;
				UIObject = uiObject;
				FloatingPanelUI = UIObject.GetComponent<FloatingPanelItemUI>();
				KeyPair = UIObject.GetKeyPairChain();
				invisibleTime = invisibleTimeLimit = 10;
				isShow = false;
			}
			public void Dispose()
			{
				if (ThisGroup == null) return;
				OnDispose();

				ThisGroup?.PushLabelUiObject(this as T);

				if (isShow && this is IViewItemUI view) view.Invisible();
				ThisGroup = null;
				UIObject = null;
				if (this is IPanelFloating floating) floating.ClearTarget();
				FloatingPanelUI = null;
				KeyPair = null;
				isShow = false;

			}
			public void Update(ref bool isSizeChange)
			{
				if (ThisGroup == null) return;

				if (this is IPanelFloating floating)
					floating.FloatingUpdate();
				OnUpdate();

				if (!isShow)
				{
					invisibleTime -= Time.unscaledDeltaTime;
					if (invisibleTime < 0)
					{
						Dispose();
						isSizeChange = true;
					}
				}
			}
			void IViewItemUI.Visible()
			{
				if (isShow) return;
				isShow = true;
				invisibleTime = invisibleTimeLimit;
				if (FloatingPanelUI != null) FloatingPanelUI.Show();
				Visible();
			}
			void IViewItemUI.Invisible()
			{
				if (!isShow) return;
				isShow = false;
				invisibleTime = invisibleTimeLimit;
				if (FloatingPanelUI != null) FloatingPanelUI.Hide();
				Invisible();
			}
			protected abstract void OnDispose();
			protected abstract void Visible();
			protected abstract void Invisible();
			protected virtual void OnUpdate() { }
		}
	}
}