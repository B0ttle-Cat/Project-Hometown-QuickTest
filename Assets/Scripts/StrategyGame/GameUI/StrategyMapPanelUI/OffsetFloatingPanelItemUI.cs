using UnityEngine;

public class OffsetFloatingPanelItemUI : FloatingPanelItemUI
{
	[SerializeField]
	private Vector2 pivot;
	[SerializeField]
	private Vector2 offset;

	protected CameraVisibilityGroup visibility;

	public Vector2 Pivot { get => pivot; set => pivot = value; }
	public Vector2 Offset { get => offset; set => offset = value; }

	protected override void Reset()
	{
		base.Reset();
		Pivot = new Vector2(0.5f, 1f);
		Offset = Vector2.zero;
	}
	protected override void InitTarget(Transform mapTarget)
	{
		visibility = mapTarget.GetComponentInChildren<CameraVisibilityGroup>();
	}
	protected override void OnUpdate()
	{
		Camera camera = StrategyManager.MainCamera;
		if (camera == null) return;
		if (visibility != null)
		{
			Rect visibleScreenRect = visibility.VisibleScreenRect;

			Vector2 screenMapTarget = visibleScreenRect.center;
			float halfWidth = visibleScreenRect.width * 0.5f;
			float halfHeight = visibleScreenRect.height * 0.5f;

			Vector2 pivotNormal = (Pivot - (Vector2.one * 0.5f)) * 2f;

			Vector2 newPosition = screenMapTarget + (-pivotNormal * new Vector2(halfWidth, halfHeight)) +  Offset;
			rectTransform.pivot = Pivot;
			rectTransform.position = newPosition;
		}
		else
		{
			rectTransform.pivot = Pivot;
			Vector2 screenMapTarget = camera.WorldToScreenPoint(MapTarget.transform.position);
			rectTransform.position = screenMapTarget + Offset;
		}
	}
}
