using System;

using Sirenix.OdinInspector;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
[RequireComponent(typeof(CanvasGroup))]
public class SliderFillRect : FillRectUI
{
	private Slider slider;
	private TMP_InputField inputField;

	public event Action<float> onValueChanged;
	private float lastChangeValue;
	private Vector2 handleClamp;

	public Slider Slider
	{
		get
		{
			if (slider == null)
				slider = GetComponent<Slider>();
			return slider;
		}
	}
	public float SliderRange => Mathf.Abs(Slider.maxValue - Slider.minValue);

	[FoldoutGroup("SliderConfig"), SerializeField]
	private float handelSizeOffset;

	private bool IsChanging;
	private record ChangeUIHandle : IDisposable
	{
		SliderFillRect handle;
		public ChangeUIHandle(SliderFillRect handle)
		{
			this.handle = handle;
			handle.IsChanging = true;
		}
		void IDisposable.Dispose()
		{
			handle.IsChanging = false;
			handle = null;
		}
	}

	public override void OnValidate()
	{
		base.OnValidate();
		SliderInit();
	}
	public override void Awake()
	{
		base.Awake();
		SliderInit();
		Text = Slider.value.ToString();
		lastChangeValue = Slider.value;
	}
	private void SliderInit()
	{
		handleClamp = new Vector2(Slider.minValue, Slider.maxValue);

		var handleRect = Slider.handleRect;
		var handleRectArea = handleRect.parent.GetComponent<RectTransform>();
		Diraction = slider.direction;

		var rect = bgRect.rect;

		if (bgRect != null && handleRectArea != null)
		{
			handleRectArea.anchorMin = bgRect.anchorMin;
			handleRectArea.anchorMax = bgRect.anchorMax;
			handleRectArea.anchoredPosition = bgRect.anchoredPosition;
			handleRectArea.sizeDelta = bgRect.sizeDelta;
			handleRectArea.pivot = bgRect.pivot;

			float offsetSize = Diraction == Slider.Direction.LeftToRight || Diraction == Slider.Direction.RightToLeft
				? rect.height * 0.5f
				: rect.width  * 0.5f;

			handleRectArea.offsetMin += Diraction == Slider.Direction.LeftToRight || Diraction == Slider.Direction.RightToLeft
				? new Vector2(offsetSize, 0f)
				: new Vector2(0f, offsetSize);
			handleRectArea.offsetMax += Diraction == Slider.Direction.LeftToRight || Diraction == Slider.Direction.RightToLeft
				? new Vector2(-offsetSize, 0f)
				: new Vector2(0f, -offsetSize);
		}

		handleRect.sizeDelta = Diraction == Slider.Direction.LeftToRight || Diraction == Slider.Direction.RightToLeft
			? new Vector2(rect.height + handelSizeOffset, handelSizeOffset)
			: new Vector2(handelSizeOffset, rect.width + handelSizeOffset);
	}
	public override string Text
	{
		get
		{
			if (inputField == null) inputField = GetComponentInChildren<TMP_InputField>(true);
			if (inputField == null) return "0";
			return inputField.text;
		}
		set
		{
			if (inputField == null) inputField = GetComponentInChildren<TMP_InputField>(true);
			if (inputField == null) return;
			if (!float.TryParse(value, out _)) return;
			inputField.text = value;
		}
	}
	public void OnChangeFromInputField(string text)
	{
		if (IsChanging) return;
		using var _ = new ChangeUIHandle(this);

		if (!float.TryParse(text, out float value)) return;
		if (SliderRange == 0) return;
		value = Mathf.Clamp(value, handleClamp.x, handleClamp.y);
		Value = value / SliderRange;
		ChangeToSlider(value);
#if UNITY_EDITOR
		if(!UnityEditor.EditorApplication.isPlaying)
		{
			SliderInit();
		}
#endif
		OnValueChanged(value);
	}
	public void OnChangeFromInputField_EndEdit(string text)
	{
		if (!float.TryParse(text, out float value)) return;

		value = Mathf.Clamp(value, handleClamp.x, handleClamp.y);
		Text = value.ToString();
	}
	public void OnChangeFromSlider(float value)
	{
		float clampValue = Mathf.Clamp(value, handleClamp.x, handleClamp.y);
		if (clampValue != value)
		{
			ChangeToSlider(clampValue);
			return;
		}

		if (IsChanging) return;
		using var _ = new ChangeUIHandle(this);

		if (SliderRange == 0) return;
		Value = value / SliderRange;
		OnValueChanged(value);
#if UNITY_EDITOR
		if (!UnityEditor.EditorApplication.isPlaying)
		{
			SliderInit();
		}
#endif
		Text = value.ToString();
	}
	private void ChangeToSlider(float value)
	{
		value = Mathf.Clamp(value, handleClamp.x, handleClamp.y);
		Slider.value = value;
	}
	public void SetMinMax(float min, float max)
	{
		Slider.minValue = Mathf.Min(min, max);
		Slider.maxValue = Mathf.Max(min, max);
		handleClamp = new Vector2(Slider.minValue, Slider.maxValue);
	}
	public void SetHandleClamp(float min, float max)
	{
		float _min = Mathf.Min(min, max);
		float _max = Mathf.Max(min, max);
		_min = Mathf.Max(_min, Slider.minValue);
		_max = Mathf.Min(_max, Slider.maxValue);

		handleClamp = new Vector2(_min, _max);
		float sliderValue = Slider.value;
		float clampValue = Mathf.Clamp(sliderValue, handleClamp.x, handleClamp.y);
		if (sliderValue == clampValue) return;
		ChangeToSlider(clampValue);
	}
	public void SetWholeNumbes(bool wholeNumbers)
	{
		Slider.wholeNumbers = wholeNumbers;
	}
	public override void SetValue(float value)
	{
		ChangeToSlider(value);
	}
	public override void SetValueText(float value, string text)
	{
		SetValue(value);
	}
	public override void SetDirection(Slider.Direction direction)
	{
		Diraction = direction;
		Slider.direction = direction;
	}
	public override float GetValue()
	{
		return Slider.value;
	}

	private void OnValueChanged(float changeValue)
	{
		if (lastChangeValue == changeValue) return;
		lastChangeValue = changeValue;
		if (onValueChanged != null) onValueChanged(lastChangeValue);
	}
}

