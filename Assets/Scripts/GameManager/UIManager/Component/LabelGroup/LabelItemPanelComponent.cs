using Sirenix.OdinInspector;

using UnityEngine;

namespace GameUI
{
	public abstract class LabelItemPanelComponent : PanelItemComponent, IPanelItem, IShowHide, ISetTargetPanel
	{
		[ShowInInspector, PropertyOrder(-100)]
		public ITargetToPanelAPI Target { get; set; }
		public ITargetToLabelAPI LabelAPI { get; set; }

		public Vector2 labelOffset;

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
			LabelAPI = null;
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
		}
		void ISetTargetPanel.OnAttachUI(ITargetToPanelAPI target)
		{
			Target = target;
			if (Target.IsNullRef()) return;
			if (target is ITargetToLabelAPI)
			{
				LabelAPI = target as ITargetToLabelAPI;
			}
			OnAttachUI(target);
		}
		void ISetTargetPanel.OnUpdateUI()
		{
			if (Target.IsNullRef()) return;
			OnUpdateUI();
		}
		protected abstract void OnReleaseUI();
		protected abstract void OnAttachUI(ITargetToPanelAPI target);
		protected abstract void OnUpdateUI();
		public virtual void UpdateLabelPosition(Camera camera)
		{
			if (LabelAPI.IsNullRef()) return;
			if (camera.IsNullRef()) return;

			Vector3 labelWorldPosition = LabelAPI.LabelWorldPosition();
			Vector2 screenPoint = camera.WorldToScreenPoint(labelWorldPosition);
			screenPoint += labelOffset;

			ThisPanel.ThisRect.anchoredPosition = screenPoint;
		}


		[Button("Test UI Update")]
		private void TestUIUpdate()
		{
			var temp = Target;
			OnReleaseUI();
			OnAttachUI(temp);
			OnUpdateUI();
		}
	}
}

