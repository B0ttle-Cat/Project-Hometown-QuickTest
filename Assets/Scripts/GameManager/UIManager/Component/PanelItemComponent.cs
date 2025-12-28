using System;
using System.Threading;

using UnityEngine;

namespace GameUI
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class PanelItemComponent : MonoBehaviour, IPanelItem, IShowHideAsync
	{
		private RectTransform rectTransform;
		private GameUIController root;
		public IPanelItem ThisPanel => this;
		RectTransform IPanelItem.ThisRect => rectTransform.IsNotNullRef() ? rectTransform : rectTransform = GetComponent<RectTransform>();
		GameUIController IPanelItem.RootUI => root.IsNotNullRef() ? root : root = ThisPanel.FindRoot();
		public IShowHide ThisShowHide => this;
		bool IShowHide.IsShow { get; set; } = false;


		protected virtual void Awake()
		{
			try
			{
				ThisShowHide.PairingShowHide();
			}
			catch(Exception ex)
			{
				Debug.LogError(gameObject.name);
				Debug.LogException(ex);
			}
		}
		protected virtual void OnDestroy()
		{
			try
			{
				ThisShowHide.UnpairingShowHide();
			}
			catch (Exception ex)
			{
				Debug.LogError(gameObject.name);
				Debug.LogException(ex);
			}
		}


		void IShowHide.Show() => Show();
		void IShowHide.Hide() => Hide();
		async Awaitable IShowHideAsync.Show(CancellationToken cancellationToken) => await Show(cancellationToken);
		async Awaitable IShowHideAsync.Hide(CancellationToken cancellationToken) => await Hide(cancellationToken);
		protected abstract void Show();
		protected abstract void Hide();
		protected virtual async Awaitable Show(CancellationToken cancellationToken) { return; }
		protected virtual async Awaitable Hide(CancellationToken cancellationToken) { return; }
	}
}
