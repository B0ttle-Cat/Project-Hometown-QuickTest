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
		[SerializeField,Space, PropertyOrder(-97)]
		protected TabPanelGroup tabPanelGroup;

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

			tabPanelGroup = null;
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
			[SerializeField]
			protected Toggle toggle;
			protected IShowHide contentTarget;
			private RectTransform thisRect;
			private GameUIController root;
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
				this.contentTarget = content;

				if (contentTarget.IsNullRef()) return;
				if (init) contentTarget.OnShowImmediate();
				else contentTarget.OnHideImmediate();
			}

			public void OnChangeValue(bool newValue)
			{
				if (contentTarget.IsNullRef()) return;

				if (newValue) contentTarget.OnShow();
				else contentTarget.OnHide();
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
				contentTarget = null;
				thisRect = null;
				root = null;
			}
		}
	}
}
