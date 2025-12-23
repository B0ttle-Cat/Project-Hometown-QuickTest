using System.Threading;

using Sirenix.OdinInspector;

using UnityEngine;

namespace GameUI
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class PanelItemComponent : MonoBehaviour, IPanelItem, IShowHideAsync
	{
		private RectTransform rectTransform;
		public abstract IPanelItem ThisPanel { get; }
		public virtual RectTransform ThisRect
		{
			get
			{
				if(rectTransform.IsNullRef())
					rectTransform = GetComponent<RectTransform>();
				return rectTransform;
			}
		}
        public abstract IShowHideAsync ThisShowHide { get; }
		IShowHide IShowHide.ThisShowHide => ThisShowHide;
        CancellationTokenSource IShowHideAsync.ShowHideCancellationTokenSource { get; set; }
		[SerializeField, PropertyOrder(-10000)]
		private bool isShow = false;
		bool IShowHide.IsShow { get => isShow; set => isShow = value; }
		async Awaitable IShowHideAsync.Show(CancellationToken cancellationToken) => await Show(cancellationToken);
		async Awaitable IShowHideAsync.Hide(CancellationToken cancellationToken) => await Hide(cancellationToken);
		void IShowHide.Show() => Show();
		void IShowHide.Hide() => Hide();
		protected abstract void Show();
		protected abstract void Hide();

		protected virtual async Awaitable Show(CancellationToken cancellationToken) { return; }
		protected virtual async Awaitable Hide(CancellationToken cancellationToken) { return; }
	}
}
