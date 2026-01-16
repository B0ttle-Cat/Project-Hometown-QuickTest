using Sirenix.OdinInspector;

using UnityEngine;
using UnityEngine.UI;

namespace GameUI
{
	public abstract class LabelItemPanelComponent : PanelItemComponent, IPanelItem, IShowHide, ISetTargetPanel
	{
		[ShowInInspector, PropertyOrder(-100)]
		public ITargetToPanelAPI Target { get; set; }
		public ITargetToLabelAPI LabelAPI { get; set; }

		[SerializeField, HorizontalGroup, HideLabel, Header("Offset")]
		private Vector2 labelOffset = Vector2.zero;
		[SerializeField, HorizontalGroup, HideLabel, Header("Pivot")]
		private Vector2 labelPivot = Vector2.one * 0.5f;
		[SerializeField]
		private LayoutElement rootLayoutElement;
		protected override void Reset()
		{
			base.Reset();
			labelOffset = Vector2.zero;
			labelPivot = Vector2.one * 0.5f;
		}
		public LabelPositionItem PositionItem { get; set; }

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
			LabelAPI = null;
		}
		void ISetTargetPanel.OnAttachUI(ITargetToPanelAPI target)
		{
			Target = target;
			if (Target.IsNullRef()) return;
			if (target is ITargetToLabelAPI)
			{
				LabelAPI = target as ITargetToLabelAPI;
			}

			if (PositionItem.IsNullRef() && TryGetComponent<LabelPositionItem>(out var positionItem))
			{
				PositionItem = positionItem;
			}
			OnAttachUI(target);
		}
		void ISetTargetPanel.OnChangedUI()
		{
			if (Target.IsNullRef()) return;
			OnChangedUI();
		}
		protected abstract void OnReleaseUI();
		protected abstract void OnAttachUI(ITargetToPanelAPI target);
		protected abstract void OnChangedUI();
		public virtual void UpdateLabelPosition(Camera camera)
		{
			if (LabelAPI.IsNullRef()) return;
			if (camera.IsNullRef()) return;

			Vector2 finalScreenPos;

			if (Target is ITargetBoundary boundary)
			{
				Rect targetRect = boundary.GetScreenRect(camera);

				float anchorX = Mathf.Lerp(targetRect.xMax, targetRect.xMin, labelPivot.x);
				float anchorY = Mathf.Lerp(targetRect.yMax, targetRect.yMin, labelPivot.y);

				finalScreenPos = new Vector2(anchorX, anchorY);

				if (rootLayoutElement.IsNotNullRef())
				{
					rootLayoutElement.minWidth = targetRect.width;
				}
			}
			else
			{
				Vector3 labelWorldPosition = LabelAPI.LabelWorldPosition();
				finalScreenPos = (Vector2)camera.WorldToScreenPoint(labelWorldPosition);
			}

			finalScreenPos += labelOffset;

			if (PositionItem.IsNotNullRef() && PositionItem.enabled)
			{
				ThisPanel.ThisRect.pivot = labelPivot;
				PositionItem.SetOriginalPosition(finalScreenPos);
			}
			else
			{
				ThisPanel.ThisRect.pivot = labelPivot;
				ThisPanel.ThisRect.anchoredPosition = finalScreenPos;
			}
			
		
		}


		[Button("Test UI Update")]
		private void TestUIUpdate()
		{
			var temp = Target;
			OnReleaseUI();
			OnAttachUI(temp);
			OnChangedUI();
		}
	}
}

