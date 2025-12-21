using UnityEngine.UI;

namespace GameUI
{
    public interface IViewItem : IPanelItem
	{
		IViewItem ThisView { get; }
		Graphic ThisGraphic { get; }
	}
}
