using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class SliderHandlePivot : MonoBehaviour
{
	private Slider slider;

	private void Init()
	{
		if (slider != null) return;
		slider = GetComponent<Slider>();

	}

	public void OnChangeValue(float value)
	{
		Init();
		if (slider == null) return;

		float minValue = slider.minValue;
		float maxValue = slider.maxValue;

		float sliderRate = Mathf.Clamp01((value - minValue) / (maxValue - minValue));

		slider.handleRect.pivot = slider.direction switch
		{
			Slider.Direction.LeftToRight => new Vector2(sliderRate, 0f),
			Slider.Direction.RightToLeft => new Vector2(1f - sliderRate, 0f),
			Slider.Direction.BottomToTop => new Vector2(0f, sliderRate),
			Slider.Direction.TopToBottom => new Vector2(0f, 1f - sliderRate),
			_ => slider.handleRect.pivot,
		};
	}
}
