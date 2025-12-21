using UnityEngine.UI;

namespace GameUI
{
    public abstract class ViewItemComponent : PanelItemComponent, IViewItem
    {
        public abstract IViewItem ThisView { get; }
        public abstract Graphic ThisGraphic { get; }
    }
}
