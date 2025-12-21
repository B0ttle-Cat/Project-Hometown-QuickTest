using UnityEngine;

namespace GameUI
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class PanelItemComponent : MonoBehaviour, IPanelItem
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
	}
}
