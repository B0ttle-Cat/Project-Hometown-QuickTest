namespace GameUI
{
	public interface ISetTargetPanel : IPanelItem
	{
		public ITargetToPanelAPI Target { get; }
		public bool SetTarget(ITargetToPanelAPI item)
		{
			if (Contains(item))
			{
				OnUpdateUI();
				return true;
			}
			OnReleaseUI();
			OnAttachUI(item);
			OnUpdateUI();
			return true;
		}
		protected bool Contains(ITargetToPanelAPI item);
		public void OnReleaseUI();
		public void OnAttachUI(ITargetToPanelAPI target);
		public void OnUpdateUI();
	}
}
