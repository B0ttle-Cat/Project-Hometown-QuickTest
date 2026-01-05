namespace GameUI
{
	public abstract class CardItemPanelComponent : PanelItemComponent, IPanelItem, IShowHide, ISetTargetPanel
	{
        public ITargetToPanelAPI Target { get; set; }

        protected override void Awake()
		{
			base.Awake();
			ThisShowHide.PairingShowHide();
		}
		protected override void OnDestroy()
		{
			base.OnDestroy();
			ThisShowHide.UnpairingShowHide();
			Target = null;
		}
		bool ISetTargetPanel.Contains(ITargetToPanelAPI item)
		{
			return this.Target == item;
		}
		void ISetTargetPanel.OnReleaseUI()
        {
        }

        void ISetTargetPanel.OnAttachUI(ITargetToPanelAPI target)
        {
        }

        void ISetTargetPanel.OnUpdateUI()
        {
        }

		protected abstract void OnReleaseUI();
		protected abstract void OnAttachUI(ITargetToPanelAPI card);
		protected abstract void OnUpdateUI();
	}
}
