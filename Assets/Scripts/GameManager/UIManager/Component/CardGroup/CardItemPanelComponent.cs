using Sirenix.OdinInspector;

namespace GameUI
{
	public abstract class CardItemPanelComponent : PanelItemComponent, IPanelItem, IShowHide, IPanelSetObject
	{
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

		[ShowInInspector]
		public IObjectForPanel Target { get; private set; }

		public bool SetTarget(IObjectForPanel item)
		{
			if (item.IsNotNullRef()) return false;

			if (this.Target == item)
			{
				OnUpdateUI();
				return true;
			}
			if (this.Target.IsNotNullRef())
			{
				OnRelease();
			}
			this.Target = item;
			if (this.Target.IsNotNullRef())
			{
				OnAttach();
				OnUpdateUI();
			}
			return true;
		}
		public void OnRelease()
		{
			if (Target.IsNullRef())
			{
				Target = null;
				return;
			}
			ReleaseUI();
			Target = null;
		}
		public void OnAttach()
		{
			if (Target.IsNullRef())
			{
				Target = null;
				return;
			}
			AttachUI(Target);
		}
		public void OnUpdateUI()
		{
			if (Target.IsNullRef())
			{
				OnRelease();
				return;
			}
			UpdateUI();
		}
		internal bool Contains(IObjectForPanel item)
		{
			return this.Target == item;
		}
		protected abstract void ReleaseUI();
		protected abstract void AttachUI(IObjectForPanel card);
		protected abstract void UpdateUI();
	}
}
