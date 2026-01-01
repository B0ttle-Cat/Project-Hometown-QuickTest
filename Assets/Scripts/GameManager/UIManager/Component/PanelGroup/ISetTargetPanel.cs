namespace GameUI
{
    public interface ISetTargetPanel : IPanelItem
    {
		public ITargetToPanelAPI Target { get; set; }
		public bool SetTarget(ITargetToPanelAPI item)
		{
			if (item.IsNotNullRef()) return false;

			if (Contains(item))
			{
				UpdateUI();
				return true;
			}
			if (this.Target.IsNotNullRef())
			{
				Release();
			}
			this.Target = item;
			if (this.Target.IsNotNullRef())
			{
				Attach();
				UpdateUI();
			}
			return true;
		}
		public void Release()
		{
			if (Target.IsNullRef())
			{
				Target = null;
				return;
			}
			OnReleaseUI();
			Target = null;
		}
		public void Attach()
		{
			if (Target.IsNullRef())
			{
				Target = null;
				return;
			}
			OnAttachUI(Target);
		}
		public void UpdateUI()
		{
			if (Target.IsNullRef())
			{
				Release();
				return;
			}
			OnUpdateUI();
		}
		public bool Contains(ITargetToPanelAPI item)
		{
			return this.Target == item;
		}

		protected void OnReleaseUI();
		protected void OnAttachUI(ITargetToPanelAPI target);
		protected void OnUpdateUI();
	}
}
