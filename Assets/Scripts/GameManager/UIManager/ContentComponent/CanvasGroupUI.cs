using System.Threading;

using GameUI;

using Sirenix.OdinInspector;

using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasGroupUI : PanelItemComponent, IShowHideAsync
{
	private CanvasGroup canvasGroup;
	public virtual CanvasGroup ThisCanvasGroup => canvasGroup.IsNotNullRef() ? canvasGroup : canvasGroup = GetComponent<CanvasGroup>();

#if UNITY_EDITOR
	private void Reset()
	{
		AlphaOnOffValue = Vector2.up;
		changeTime = 0.25f;
		timeMode = ChangeTimeMode.UnscaleTime;
		ControlInteractableMode = ControlMode.ZeroSwitch;
		ControlBlocksRaycastsMode = ControlMode.ZeroSwitch;
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
	public enum ChangeTimeMode { UnscaleTime = 0, DeltaTime, FixedMode, }
	public enum ControlMode { None, ZeroSwitch, OneSwitch, Custom }
	[SerializeField, LabelText("Interactable"), EnumToggleButtons]
	public ControlMode ControlInteractableMode;
	private bool controlInteractable => ControlInteractableMode != ControlMode.None;
	private bool customControlInteractable => ControlInteractableMode == ControlMode.Custom;
	[SerializeField, Range(0f, 1f), HorizontalGroup("controlInteractable", VisibleIf = "@customControlInteractable"), LabelText("On Alpha")]
	private float interactableOnAlpha;
	[SerializeField, Range(0f, 1f), HorizontalGroup("controlInteractable"), LabelText("Off Alpha")]
	private float interactableOffAlpha;

	[SerializeField, LabelText("Blocks Raycasts"), EnumToggleButtons]
	public ControlMode ControlBlocksRaycastsMode;
	private bool controlBlocksRaycasts => ControlBlocksRaycastsMode != ControlMode.None;
	private bool customControlBlocksRaycasts => ControlBlocksRaycastsMode == ControlMode.Custom;
	[SerializeField, Range(0f, 1f), HorizontalGroup("controlBlocksRaycasts", VisibleIf = "@customControlBlocksRaycasts"), LabelText("On Alpha")]
	private float blocksRaycastsOnAlpha;
	[SerializeField, Range(0f, 1f), HorizontalGroup("controlBlocksRaycasts"), LabelText("Off Alpha")]
	private float blocksRaycastsOffAlpha;


	void IShowHide.EndedHide() { Alpha = AlphaOnOffValue.x; }
	void IShowHide.EndedShow() { Alpha = AlphaOnOffValue.y; }

	async Awaitable IShowHideAsync.AsyncShow(CancellationToken cancellationToken)
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

	async Awaitable IShowHideAsync.AsyncHide(CancellationToken cancellationToken)
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
			if (isIncreasing) ThisCanvasGroup.interactable = alpha >= ControlInteractableMode switch
            {
                ControlMode.ZeroSwitch => 0,
                ControlMode.OneSwitch => 1,
                ControlMode.Custom => interactableOnAlpha,
				_ => 1,
            };
			else ThisCanvasGroup.interactable = alpha > ControlInteractableMode switch
			{
				ControlMode.ZeroSwitch => 0,
				ControlMode.OneSwitch => 1,
				ControlMode.Custom => interactableOffAlpha,
				_ => 1,
			}; ;
		}
		if (controlBlocksRaycasts)
		{
			if (isIncreasing) ThisCanvasGroup.blocksRaycasts = alpha >= ControlBlocksRaycastsMode switch
			{
				ControlMode.ZeroSwitch => 0,
				ControlMode.OneSwitch => 1,
				ControlMode.Custom => blocksRaycastsOnAlpha,
				_ => 1,
			}; 
			else ThisCanvasGroup.blocksRaycasts = alpha > ControlBlocksRaycastsMode switch
			{
				ControlMode.ZeroSwitch => 0,
				ControlMode.OneSwitch => 1,
				ControlMode.Custom => blocksRaycastsOffAlpha,
				_ => 1,
			};
		}
	}
}