using System;

using Sirenix.OdinInspector;

using UnityEngine;
using UnityEngine.UI;

namespace GameUI
{
	[RequireComponent(typeof(ToggleGroup))]
	[RequireComponent(typeof(CanvasGroupUI))]
	public abstract class TabButtonGroup : PanelGroupComponent<TabButtonGroup.TabButton>
	{
		[SerializeField, HorizontalGroup, PropertyOrder(-99)]
		protected ToggleGroup toggleGroup;
		[ShowInInspector, HorizontalGroup(width: 120), PropertyOrder(-99), ToggleLeft]
		protected bool AllowSwitchOff
		{
			get => !toggleGroup.IsNullRef() && toggleGroup.allowSwitchOff;
			set { if (toggleGroup.IsNotNullRef()) toggleGroup.allowSwitchOff = value; }
		}
		[SerializeField, PropertyOrder(-98)]
		protected CanvasGroupUI canvasGroupUI;

		protected override void Reset()
		{
			base.Reset();
			InitTab();
		}
		protected virtual void OnValidate()
        {
			InitTab();
		}
		protected override void Awake()
		{
			base.Awake();
			InitTab();
		}
		protected override void OnDestroy()
		{
			base.OnDestroy();

			int length = Count;
			for (int i = 0 ; i < length ; i++)
			{
				this[i].Dispose();
			}

			toggleGroup = null;
			canvasGroupUI = null;

			base.Clear();
		}

		protected virtual void InitTab()
		{
			TryGetComponent<ToggleGroup>(out toggleGroup);
			TryGetComponent<CanvasGroupUI>(out canvasGroupUI);
		}
		public override void Clear()
		{
			int length = Count;
			for (int i = 0 ; i < length ; i++)
			{
				this[i].Dispose();
			}
			base.Clear();
		}

		protected override void Show()
		{
			canvasGroupUI.ThisShowHide.OnShow();
		}
		protected override void Hide()
		{
			canvasGroupUI.ThisShowHide.OnHide();
		}

		[Serializable]
		public class TabButton : IPanelItem, IDisposable
		{
			[SerializeField, HorizontalGroup, LabelWidth(50)]
			protected Toggle toggle;
			[SerializeField, HorizontalGroup, LabelWidth(50), PropertyOrder(1)]
			protected IShowHide panel;
			private RectTransform thisRect;
			private GameUIController root;

			[ShowInInspector, HorizontalGroup(width: 50), LabelText("IsOn"), LabelWidth(30)]
			public bool IsOn
			{
				get => !toggle.IsNullRef() && toggle.isOn;
				set { if (toggle.IsNotNullRef()) toggle.isOn = value; }
			}



			public IPanelItem ThisPanel => this;
			RectTransform IPanelItem.ThisRect => thisRect.IsNotNullRef() ? thisRect : thisRect = toggle.GetComponent<RectTransform>();
			GameUIController IPanelItem.RootUI => root.IsNotNullRef() ? root : root = ThisPanel.FindRoot();



			public TabButton(Toggle toggle, ToggleGroup toggleGroup, IShowHide content, bool init)
			{
				if (toggle == null) return;

				this.toggle = toggle;
				toggle.isOn = init;
				toggle.group = toggleGroup;
				toggle.onValueChanged.AddListener(OnChangeValue);
				this.panel = content;

				if (panel.IsNullRef()) return;
				if (init) panel.OnShowImmediate();
				else panel.OnHideImmediate();
			}

			public void OnChangeValue(bool newValue)
			{
				if (panel.IsNullRef()) return;

				if (newValue) panel.OnShow();
				else panel.OnHide();
			}
			public void OnChangeValue()
			{
				OnChangeValue(!toggle.IsNullRef() && toggle.isOn);
			}
			public virtual void Dispose()
			{
				if (toggle.IsNullRef()) return;

				toggle.onValueChanged.RemoveListener(OnChangeValue);

				toggle = null;
				panel = null;
				thisRect = null;
				root = null;
			}
		}
	}
}
