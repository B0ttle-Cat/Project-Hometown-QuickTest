using System;

using TMPro;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class NumericSliderUI : MonoBehaviour
{
	[SerializeField]
	private TMP_Text label;
	[SerializeField]
	private SliderFillRect slider;
	[SerializeField]
	private Button buttonSample;
	private CanvasGroup canvasGroup;

	[SerializeField]
	private Transform valueButtonLayout;
	[SerializeField]
	private Transform persantButtonLayout;
	private bool isShowPersantButton;
	public float Value
	{
		get => slider.GetValue() ;
		set => slider.SetValue(value);
	}
	public string Label
	{
		get => label.text;
		set => label.text = value;
	}
	private void InitCanvasGroup()
	{
		if (canvasGroup == null)
		{
			canvasGroup = GetComponent<CanvasGroup>();
			if (canvasGroup == null)
			{
				canvasGroup = gameObject.AddComponent<CanvasGroup>();
				canvasGroup.alpha = 1f;
				canvasGroup.interactable = true;
				canvasGroup.blocksRaycasts = true;
			}
		}
	}

	public void SetMinMax(float min, float max, bool wholeNumbers)
	{
		InitCanvasGroup();

		if (Mathf.Approximately(min, max))
		{
			canvasGroup.alpha = 0.75f;
			canvasGroup.interactable = false;
		}
		else
		{
			canvasGroup.alpha = 1f;
			canvasGroup.interactable = true;
		}

		slider.SetMinMax(min, max);
		slider.SetWholeNumbes(wholeNumbers);
	}
	public void SetHandleClamp(float min, float max)
	{
		InitCanvasGroup();

		if (Mathf.Approximately(min, max))
		{
			canvasGroup.alpha = 0.75f;
			canvasGroup.interactable = false;
		}
		else
		{
			canvasGroup.alpha = 1f;
			canvasGroup.interactable = true;
		}

		slider.SetHandleClamp(min, max);
	}
	public void Awake()
	{
		InitChangeValueButtons();
		InitCanvasGroup();
	}
	public void LateUpdate()
	{
		PersantButtonSwitch();
	}
	private void PersantButtonSwitch()
	{
		if (valueButtonLayout == null) return;
		if (persantButtonLayout == null) return;
		if (Keyboard.current.shiftKey.isPressed)
		{
			ShowPersantButton();
		}
		else
		{
			HidePresandButton();
		}
		void ShowPersantButton()
		{
			if (!isShowPersantButton)
			{
				isShowPersantButton = true;
				persantButtonLayout.gameObject.SetActive(true);
				valueButtonLayout.gameObject.SetActive(false);
			}
		}
		void HidePresandButton()
		{
			if (isShowPersantButton)
			{
				isShowPersantButton = false;
				persantButtonLayout.gameObject.SetActive(false);
				valueButtonLayout.gameObject.SetActive(true);
			}
		}
	}
	private void InitChangeValueButtons()
	{
		isShowPersantButton = persantButtonLayout.gameObject.activeSelf;
		valueButtonLayout.gameObject.SetActive(!isShowPersantButton);

		buttonSample.gameObject.SetActive(true);
		int[] valueArray = new int[] { int.MinValue, -100, -10, -1, 1,10,100,int.MaxValue};
		int length = valueArray.Length;
		for (int i = 0 ; i < length ; i++)
		{
			int value = valueArray[i];
			var button = GameObject.Instantiate<Button>(buttonSample, valueButtonLayout);
			string label = value == int.MinValue ? "Min" : value == int.MaxValue ? "Max" : $"{value:+#;-#;0}";
			TMP_Text text = button.gameObject.GetComponentInChildren<TMP_Text>();
			if (text != null)
			{
				text.text = label;
			}
			button.gameObject.name = label;
			button.onClick.AddListener(() => OnClickValueButton(value));
		}
		valueArray = new int[] { int.MinValue, -20, -10, -1, 1, 10, 20, int.MaxValue };
		length = valueArray.Length;
		for (int i = 0 ; i < length ; i++)
		{
			int value = valueArray[i];
			var button = GameObject.Instantiate<Button>(buttonSample, persantButtonLayout);
			string label = value == int.MinValue ? "Min" : value == int.MaxValue ? "Max" : $"{value:+#;-#;0}%";
			TMP_Text text = button.gameObject.GetComponentInChildren<TMP_Text>();
			if (text != null)
			{
				text.text = label;
			}
			button.gameObject.name = label;
			button.onClick.AddListener(() => OnClickPresantButton(value));
		}
		buttonSample.gameObject.SetActive(false);
	}
	public void OnClickValueButton(int value)
	{
		Value += value;
	}
	public void OnClickPresantButton(int value)
	{
		Value += slider.SliderRange * ((float)value / 100);
	}

	public void AddOnValueChange(Action<float> onValueChange)
	{
		slider.onValueChanged -= onValueChange;
		slider.onValueChanged += onValueChange;
	}
	public void RemoveOnValueChange(Action<float> onValueChange)
	{
		slider.onValueChanged -= onValueChange;
	}
}
