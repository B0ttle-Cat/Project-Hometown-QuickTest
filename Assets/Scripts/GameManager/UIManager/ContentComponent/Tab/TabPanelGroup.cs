//using System;

//using UnityEngine;

//namespace GameUI
//{
//	//<T> : PanelGroupComponent<T> where T : TabButtonGroup<T>.TabButton
//	public abstract class TabPanelGroup : PanelGroupComponent<TabPanelGroup.TabPanelItem>
//	{
//		protected override void OnDestroy()
//		{
//			base.OnDestroy();
//			int length = Count;
//			for (int i = 0 ; i < length ; i++)
//			{
//				this[i].Dispose();
//			}
//		}

//		[Serializable]
//		public class TabPanelItem : IPanelItem, IShowHide, IDisposable
//		{
//			[SerializeField]
//			protected readonly CanvasGroupUI canvasGroupUI;
//			public IPanelItem ThisPanel => canvasGroupUI;
//			RectTransform IPanelItem.ThisRect => canvasGroupUI.ThisPanel.ThisRect;
//			GameUIController IPanelItem.RootUI => canvasGroupUI.ThisPanel.RootUI;
//			public IShowHide ThisShowHide => this;
//			bool IShowHide.IsShow { get ; set ; }

//			public TabPanelItem(GameObject uiObject)
//			{
//				if (uiObject == null) return;
//				uiObject.TryGetComponent<CanvasGroupUI>(out canvasGroupUI);
//				ThisShowHide.PairingShowHide();
//			}
//			public TabPanelItem(RectTransform rectTransform)
//			{
//				if (rectTransform == null) return;
//				rectTransform.TryGetComponent<CanvasGroupUI>(out canvasGroupUI);
//				ThisShowHide.PairingShowHide();
//			}
//			public TabPanelItem(CanvasGroupUI canvasGroupUI)
//			{
//				if (canvasGroupUI == null) return;
//				this.canvasGroupUI = canvasGroupUI;
//				ThisShowHide.PairingShowHide();
//			}

//			void IShowHide.Show()=> Show();
//			void IShowHide.Hide()=> Hide();

//			protected virtual void Show() { }
//			protected virtual void Hide() { }
//			public virtual void Dispose() 
//			{
//				ThisShowHide.UnpairingShowHide();
//			}
//		}

//		public IPanelItem GetPanelItem(int i)
//		{
//			if (i >= 0 && i < Count)
//				return this[i].ThisPanel;
//			else return null;
//		}
//    }
//}
