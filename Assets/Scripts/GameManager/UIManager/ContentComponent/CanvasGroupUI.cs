using System.Threading;

using GameUI;

using Sirenix.OdinInspector;

using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasGroupUI : PanelItemComponent
{
	private CanvasGroup canvasGroup;
	public virtual CanvasGroup ThisCanvasGroup { get { if (canvasGroup.IsNullRef()) canvasGroup = GetComponent<CanvasGroup>(); return canvasGroup; } }
	public override IPanelItem ThisPanel => this;
	public override IShowHideAsync ThisShowHide => this;

#if UNITY_EDITOR
	private void Reset()
	{
		AlphaOnOffValue = Vector2.up;
		changeTime = 0.25f;
		timeMode = ChangeTimeMode.UnscaleTime;
		controlBlocksRaycasts = false;
		controlInteractable = false;
		interactableOnAlpha = 0.0f;
		interactableOffAlpha = 0.0f;
		blocksRaycastsOnAlpha = 0.0f;
		blocksRaycastsOffAlpha = 0.0f;
		Alpha = 1f;
	}
#endif

	[ShowInInspector, Range(0f, 1f), PropertyOrder(-1)]
	public float Alpha
	{
		get => ThisCanvasGroup.alpha;
		set
		{
			float prev = ThisCanvasGroup.alpha;
			if (Mathf.Approximately(prev, value)) return;
			ThisCanvasGroup.alpha = value;
			UpdateState(value, value - prev);
		}
	}

	[SerializeField, MinMaxSlider(0f, 1f, true)]
	private Vector2 AlphaOnOffValue;
	[SerializeField, HorizontalGroup, Range(0f, 10f)]
	private float changeTime;
	[SerializeField, HorizontalGroup, HideLabel, EnumToggleButtons]
	private ChangeTimeMode timeMode;
	public enum ChangeTimeMode { UnscaleTime = 0, DeltaTime, FixedMode,}

	[SerializeField]
	private bool controlInteractable;
	[SerializeField, Range(0f, 1f), HorizontalGroup("controlInteractable", VisibleIf = "@controlInteractable"), LabelText("On Alpha")]
	private float interactableOnAlpha;
	[SerializeField, Range(0f, 1f), HorizontalGroup("controlInteractable"), LabelText("Off Alpha")]
	private float interactableOffAlpha;

	[SerializeField]
	private bool controlBlocksRaycasts;
	[SerializeField, Range(0f, 1f), HorizontalGroup("controlBlocksRaycasts", VisibleIf = "@controlBlocksRaycasts"), LabelText("On Alpha")]
	private float blocksRaycastsOnAlpha;
	[SerializeField, Range(0f, 1f), HorizontalGroup("controlBlocksRaycasts"), LabelText("Off Alpha")]
	private float blocksRaycastsOffAlpha;


	protected override void Hide() { Alpha = AlphaOnOffValue.x; }
	protected override void Show() { Alpha = AlphaOnOffValue.y; }

	protected override async Awaitable Show(CancellationToken cancellationToken)
	{
		float endAlpha = AlphaOnOffValue.y;
		float speed = GetSpeed();
		while (!Mathf.Approximately(Alpha, endAlpha))
		{
			if (cancellationToken.IsCancellationRequested) break;
			Alpha = Mathf.MoveTowards(Alpha, endAlpha, speed * GetDeltaTime());
			await Awaitable.NextFrameAsync(cancellationToken);
		}
		if (!cancellationToken.IsCancellationRequested) Alpha = endAlpha;
	}

	protected override async Awaitable Hide(CancellationToken cancellationToken)
	{
		float endAlpha = AlphaOnOffValue.x;
		float speed = GetSpeed();
		while (!Mathf.Approximately(Alpha, endAlpha))
		{
			if (cancellationToken.IsCancellationRequested) break;
			Alpha = Mathf.MoveTowards(Alpha, endAlpha, speed * GetDeltaTime());
			await Awaitable.NextFrameAsync(cancellationToken);
		}
		if (!cancellationToken.IsCancellationRequested) Alpha = endAlpha;
	}
	private float GetSpeed()
	{
		float range = Mathf.Abs(AlphaOnOffValue.y - AlphaOnOffValue.x);
		return changeTime > 0 ? range / changeTime : 100f;
	}

	private float GetDeltaTime()
	{
#if UNITY_EDITOR
		if (!UnityEditor.EditorApplication.isPlaying)
		{
			return Time.fixedDeltaTime;
		}
#endif
		return timeMode switch
		{
			ChangeTimeMode.DeltaTime => Time.deltaTime,
			ChangeTimeMode.UnscaleTime => Time.unscaledDeltaTime,
			ChangeTimeMode.FixedMode => Time.fixedDeltaTime,
			_ => 100,
		};
	}
	private void UpdateState(float alpha, float delta)
	{
		// 한계값 도달 시 강제 업데이트 (delta 무시)
		if (alpha >= AlphaOnOffValue.y)
		{
			if (controlInteractable) ThisCanvasGroup.interactable = true;
			if (controlBlocksRaycasts) ThisCanvasGroup.blocksRaycasts = true;
			return;
		}
		if (alpha <= AlphaOnOffValue.x)
		{
			if (controlInteractable) ThisCanvasGroup.interactable = false;
			if (controlBlocksRaycasts) ThisCanvasGroup.blocksRaycasts = false;
			return;
		}

		// 범위 내에서는 방향(delta)에 따른 조건부 업데이트
		bool isIncreasing = delta > 0;
		if (controlInteractable)
		{
			if (isIncreasing) ThisCanvasGroup.interactable = alpha >= interactableOnAlpha;
			else ThisCanvasGroup.interactable = alpha > interactableOffAlpha;
		}
		if (controlBlocksRaycasts)
		{
			if (isIncreasing) ThisCanvasGroup.blocksRaycasts = alpha >= blocksRaycastsOnAlpha;
			else ThisCanvasGroup.blocksRaycasts = alpha > blocksRaycastsOffAlpha;
		}
	}
}