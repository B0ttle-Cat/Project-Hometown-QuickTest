using System;

using Sirenix.OdinInspector;

using Unity.VisualScripting;

using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasGroupUI : MonoBehaviour
{
	private CanvasGroup canvasGroup;
	[SerializeField]
    private bool isOn;
	private bool isAwaitOn;
	public float changeTime = 0;
	public bool scaledDeltaTime = false;
	[FoldoutGroup("Setting")]
	[SerializeField, InlineProperty, HideLabel,Header("On Setting")]
	private Setting on = Setting.On;
	[FoldoutGroup("Setting")]
	[SerializeField, InlineProperty, HideLabel,Header("Off Setting")]
	private Setting off = Setting.Off;
	public bool IsOn => isOn;
	public bool IsAwaitOn => isAwaitOn;
	public bool IsSyncOn => IsOn == IsAwaitOn;

	[Serializable]
	public struct Setting
	{
		[Range(0f,1f)]
		public float alpha;
		public bool interactable;
		public bool blocksRaycasts;
		public static Setting On => new Setting()
		{
			alpha = 1f,
			interactable = true,
			blocksRaycasts = true
		};
		public static Setting Off => new Setting()
		{
			alpha = 0f,
			interactable = false,
			blocksRaycasts = false
		};

		public float Alpha01 => Mathf.Clamp01(alpha);


		public void Set(CanvasGroup canvasGroup)
		{
			if (canvasGroup == null) return;
			canvasGroup.alpha = alpha;
			canvasGroup.interactable = interactable;
			canvasGroup.blocksRaycasts = blocksRaycasts;
		}
		public void SetWithoutAlpha(CanvasGroup canvasGroup)
		{
			if (canvasGroup == null) return;
			canvasGroup.interactable = interactable;
			canvasGroup.blocksRaycasts = blocksRaycasts;
		}
	}

	private void Reset()
	{
		Init();
		isOn = false;
		changeTime = 0;
		scaledDeltaTime = false;
		on = Setting.On;
		off = Setting.Off;

		Value(isOn, true);
	}
	private void OnValidate()
	{
		Init();
		Value(isOn, true);
	}
	private void Init()
	{
		if (canvasGroup != null) return;
		canvasGroup = GetComponent<CanvasGroup>();
		if (canvasGroup != null) return;
		canvasGroup = canvasGroup.AddComponent<CanvasGroup>();
	}
	[Button]
	public void OnShowImmediate()
	{
		Init();
		Value(true, true);
	}
	[Button]
	public void OnHideImmediate()
	{
		Init();
		Value(false, true);
	}
	public void OnShow(Action awaitCallback = null)
	{
		Init();
		Value(true);
		
		if (awaitCallback == null) return;
		Await();
		async void Await()
		{
			while (!IsSyncOn)
			{
				await Awaitable.NextFrameAsync();
				if(destroyCancellationToken.IsCancellationRequested) return;
			}
			awaitCallback?.Invoke();
		}
	}

	public void OnHide(Action awaitCallback = null)
	{
		Init();
		Value(false);

		Await();
		async void Await()
		{
			while (!IsSyncOn)
			{
				await Awaitable.NextFrameAsync();
				if (destroyCancellationToken.IsCancellationRequested) return;
			}
			awaitCallback?.Invoke();
		}
	}
	public void OnToggle(bool isOn)
	{
		Init();
		Value(isOn);
	}

	private async void Value(bool _isOn, bool isImmediate = false)
	{
		Init();
		if (canvasGroup == null) return;

		if (isImmediate)
		{
			if (_isOn) on.Set(canvasGroup);
			else off.Set(canvasGroup);
			isOn = _isOn;
			isAwaitOn = _isOn;
			return;
		}

		isOn = _isOn;
		isAwaitOn = !_isOn;
		while (!IsSyncOn && canvasGroup != null)
		{
			if (destroyCancellationToken.IsCancellationRequested) break;

			if (_isOn) on.SetWithoutAlpha(canvasGroup);
			else off.SetWithoutAlpha(canvasGroup);

			float value = canvasGroup.alpha;
			float target = _isOn ? on.Alpha01 : off.Alpha01;
			float deltaValue = (1f/changeTime)* (scaledDeltaTime ? Time.deltaTime : Time.unscaledDeltaTime);

			value = Mathf.MoveTowards(value, target, deltaValue);
			canvasGroup.alpha = value;
			if (Mathf.Approximately(value, target))
			{
				break;
			}
			await Awaitable.NextFrameAsync();
		}
		isAwaitOn = isOn;
	}
}
