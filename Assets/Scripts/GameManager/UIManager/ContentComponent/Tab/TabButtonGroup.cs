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
		[SerializeField, PropertyOrder(-99)]
		protected ToggleGroup toggleGroup;
		[SerializeField, PropertyOrder(-99)]
		protected CanvasGroupUI canvasGroupUI;
		[SerializeField,Space, PropertyOrder(-99)]
		protected TabPanelGroup tabPanelGroup;

		public override IPanelGroup<TabButton> ThisPanel { get; }
		public override IShowHideAsync ThisShowHide { get; }

		protected virtual void Awake()
		{
			TryGetComponent<ToggleGroup>(out toggleGroup);
			TryGetComponent<CanvasGroupUI>(out canvasGroupUI);
		}
		protected virtual void OnDestroy()
		{
			int length = Count;
			for (int i = 0 ; i < length ; i++)
			{
				Items[i].Dispose();
			}

			tabPanelGroup = null;
			toggleGroup = null;
			canvasGroupUI = null;
			base.Clear();
		}

		public override void Clear()
		{
			int length = Count;
			for (int i = 0 ; i < length ; i++)
			{
				Items[i].Dispose();
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
			RectTransform rectTransform;
			[SerializeField]
			protected IShowHide contentTarget;
			public IPanelItem ThisPanel => this;
			RectTransform IPanelItem.ThisRect
			{
				get
				{
					if (rectTransform == null)
						toggle.TryGetComponent<RectTransform>(out rectTransform);
					return rectTransform;
				}
			}
			public TabButton(Toggle toggle, ToggleGroup toggleGroup, IShowHide content, bool init)
			{
				if (toggle == null) return;

				this.toggle = toggle;
				toggle.isOn = init;
				toggle.group = toggleGroup;
				toggle.TryGetComponent<RectTransform>(out rectTransform);
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

			public virtual void Dispose()
			{
				if (toggle.IsNullRef()) return;

				toggle.onValueChanged.RemoveListener(OnChangeValue);

				toggle = null;
				rectTransform = null;
				contentTarget = null;
			}
		}
	}
}
