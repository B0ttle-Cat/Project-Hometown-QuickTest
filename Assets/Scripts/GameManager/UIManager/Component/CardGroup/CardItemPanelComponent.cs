using Sirenix.OdinInspector;

namespace GameUI
{
	public abstract class CardItemPanelComponent : PanelItemComponent, IPanelItem, IShowHide
	{
		public abstract void OnRelease();
		public abstract void OnAttach();
		public abstract void OnUpdateUI();
		public abstract bool SetTarget(ICardUIObject target);
	}
	public abstract class CardItemPanelComponent<T> : CardItemPanelComponent, IPanelItem, IShowHide
		where T : class, ICardUIObject
	{
		[ShowInInspector]
		public T Item { get; private set; }

		protected override void Awake()
		{
			base.Awake();
			ThisShowHide.PairingShowHide();
		}
		protected override void OnDestroy()
		{
			base.OnDestroy();
			ThisShowHide.UnpairingShowHide();
			Item = null;
		}
		sealed public override bool SetTarget(ICardUIObject target)
		{
			if (target is not T item) return false;

			if (this.Item == item)
			{
				OnUpdateUI();
				return true;
			}
			if (this.Item.IsNotNullRef())
			{
				OnRelease();
			}
			this.Item = item;
			if (this.Item.IsNotNullRef())
			{
				OnAttach();
				OnUpdateUI();
			}
			return true;
		}
		sealed public override void OnRelease()
		{
			if (Item.IsNullRef())
			{
				Item = null;
				return;
			}
			ReleaseUI();
			Item = null;
		}
		sealed public override void OnAttach()
		{
			if (Item.IsNullRef())
			{
				Item = null;
				return;
			}
			AttachUI(Item);
		}
		sealed public override void OnUpdateUI()
		{
			if (Item.IsNullRef())
			{
				OnRelease();
				return;
			}
			UpdateUI();
		}
		internal bool Contains(T item)
		{
			return this.Item == item;
		}
		protected abstract void ReleaseUI();
		protected abstract void AttachUI(ICardUIObject card);
		protected abstract void UpdateUI();
	}
}
