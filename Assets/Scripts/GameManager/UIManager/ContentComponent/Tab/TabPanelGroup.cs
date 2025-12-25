using System;

using UnityEngine;

namespace GameUI
{
	//<T> : PanelGroupComponent<T> where T : TabButtonGroup<T>.TabButton
	public abstract class TabPanelGroup : PanelGroupComponent<TabPanelGroup.TabPanelItem>
	{
		public override IPanelGroup<TabPanelItem> ThisPanel => this;
		public override IShowHideAsync ThisShowHide => this;


		protected virtual void OnDestroy()
		{
			int length = Count;
			for (int i = 0 ; i < length ; i++)
			{
				Items[i].Dispose();
			}
		}

		[Serializable]
		public class TabPanelItem : IPanelItem, IShowHide, IDisposable
		{
			[SerializeField]
			protected RectTransform rectTransform;
			protected CanvasGroupUI canvasGroupUI;
			public IPanelItem ThisPanel => this;
			public IShowHide ThisShowHide => this;
			RectTransform IPanelItem.ThisRect => rectTransform;
			bool IShowHide.IsShow { get; set; }

			public TabPanelItem(GameObject uiObject)
			{
				if (uiObject == null) return;
				uiObject.TryGetComponent<RectTransform>(out rectTransform);
				uiObject.TryGetComponent<CanvasGroupUI>(out canvasGroupUI);
			}
			public TabPanelItem(RectTransform rectTransform)
			{
				if (rectTransform == null) return;
				this.rectTransform = rectTransform;
				rectTransform.TryGetComponent<CanvasGroupUI>(out canvasGroupUI);
			}
			public TabPanelItem(CanvasGroupUI canvasGroupUI)
			{
				if (canvasGroupUI == null) return;
				this.canvasGroupUI = canvasGroupUI;
				rectTransform = canvasGroupUI.ThisRect;
			}

			void IShowHide.Show()
			{
				if (canvasGroupUI != null) canvasGroupUI.ThisShowHide.OnShow(Show);
				else if (rectTransform != null) rectTransform.gameObject.SetActive(true);
			}
			void IShowHide.Hide()
			{
				if (canvasGroupUI != null) canvasGroupUI.ThisShowHide.OnHide(Hide);
				else if (rectTransform != null) rectTransform.gameObject.SetActive(false);
			}

			protected virtual void Show() { }
			protected virtual void Hide() { }
			public virtual void Dispose()
			{
				rectTransform = null;
				canvasGroupUI = null;
			}
		}
	}
}
