using UnityEngine;

namespace GameUI
{
	public interface IPanelItem
	{
		IPanelItem ThisPanel { get; }
		RectTransform ThisRect { get; }
	}
}
