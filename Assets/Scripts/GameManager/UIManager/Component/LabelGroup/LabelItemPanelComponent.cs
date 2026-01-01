using Sirenix.OdinInspector;

namespace GameUI
{
	public abstract class LabelItemPanelComponent : PanelItemComponent, IPanelItem, IShowHide, ISetTargetPanel
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

		[ShowInInspector, PropertyOrder(-100)]
		public ITargetToPanelAPI Target { get; set; }
		void ISetTargetPanel.OnReleaseUI() { OnReleaseUI(); }
		void ISetTargetPanel.OnAttachUI(ITargetToPanelAPI target) { OnAttachUI(target); }
		void ISetTargetPanel.OnUpdateUI() { OnUpdateUI(); }
		protected abstract void OnReleaseUI();
		protected abstract void OnAttachUI(ITargetToPanelAPI target);
		protected abstract void OnUpdateUI();
	}

}
