namespace GameUI
{
	public abstract class CardItemPanelComponent : PanelItemComponent, IPanelItem, IShowHide, ISetTargetPanel
	{
        public ITargetToPanelAPI Target { get; set; }
		public ITargetToCardAPI CardlAPI { get; set; }
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
			if (Target.IsNullRef()) return;
			OnReleaseUI();
			Target = null;
			CardlAPI = null;
		}
		void ISetTargetPanel.OnAttachUI(ITargetToPanelAPI target)
		{
			Target = target;
			if (Target.IsNullRef()) return;
			if (target is ITargetToCardAPI)
			{
				CardlAPI = target as ITargetToCardAPI;
			}
			OnAttachUI(target);
		}
		void ISetTargetPanel.OnChangedUI()
		{
			if (Target.IsNullRef()) return;
			OnUpdateUI();
		}
		protected abstract void OnReleaseUI();
		protected abstract void OnAttachUI(ITargetToPanelAPI target);
		protected abstract void OnUpdateUI();
	}
}
