using Sirenix.OdinInspector;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class FillRectUI : MonoBehaviour
{
	[SerializeField, Range(0f,1f)]
	private float fillAmount;
	[SerializeField]
	private FillDiraction fillDiraction;

	[SerializeField]
	private RectMask2D fillRectMask2D;
	private RectTransform fillRect;
	
	[FoldoutGroup("Color"), SerializeField]
	private Color fillColor;
	[FoldoutGroup("Color"), SerializeField, Range(0f,1f)]
	private float fillAlpha;
	[FoldoutGroup("Color"), SerializeField, Range(0f,1f)]
	private float bgSaturation;
	[FoldoutGroup("Color"), SerializeField, Range(0f,1f)]
	private float bgBrightness;
	[FoldoutGroup("Color"), SerializeField, Range(0f,1f)]
	private float bgAlpha;

	private TMP_Text fillRectTextUI;
	public enum FillDiraction
	{
		LeftToRight,
		RightToLeft,
		TopToBottom,
		BottomToTop,
	}

	public void Reset()
	{
		fillAmount = 0.5f;
		fillDiraction = FillDiraction.LeftToRight;
		Image image = GetComponentInChildren<Image>();
		if (image != null) fillColor = image.color;
		else fillColor = Color.white;
		bgSaturation = 0f;
		bgBrightness = 1f;
		fillAlpha = 1f;
		bgAlpha = 1f;
		FillUpdate();
	}
	public void OnValidate()
	{
		FillUpdate();
		ColorUpdate();
	}

	public float Value
	{
		get => Mathf.Clamp01(fillAmount);
		set { fillAmount = value; FillUpdate(); }
	}
	public FillDiraction Diraction
	{
		get => fillDiraction;
		set { fillDiraction = value; FillUpdate(); }
	}
	public string Text
	{
		get
		{
			if (fillRectTextUI == null) fillRectTextUI = GetComponentInChildren<TMP_Text>();
			if (fillRectTextUI == null) return "";
			return fillRectTextUI.text;
		}
		set
		{
			if (fillRectTextUI == null) fillRectTextUI = GetComponentInChildren<TMP_Text>();
			if (fillRectTextUI == null) return;
			fillRectTextUI.text = value;
		}
	}

	public void SetValueText(float value, string text)
	{
		Value = Mathf.Clamp01(value);
		Text = text;
	}

    public void Awake()
    {
		Init();
		ColorUpdate();
    }

    private void Init()
	{
		if (fillRectMask2D == null)
		{
			fillRectMask2D = GetComponentInChildren<RectMask2D>();
		}
		if (fillRectMask2D != null && fillRect == null)
		{
			fillRect = fillRectMask2D.GetComponent<RectTransform>();
			fillRect.anchorMin = Vector2.zero;
			fillRect.anchorMax = Vector2.one;
			fillRect.anchoredPosition = Vector2.zero;
			fillRect.sizeDelta = Vector2.zero;
			fillRect.pivot = Vector2.one * 0.5f;
		}
	}
	private void ColorUpdate()
	{
		if (fillRect == null) return;

		// 각각의 이미지 가져오기
		var bgImage = GetComponent<Image>();
		var fillImage = fillRect.GetComponentInChildren<Image>();
		if (bgImage == null || fillImage == null) return;

		// 채워진 부분은 fillColor 그대로 적용
		fillColor.a = fillAlpha;
		fillImage.color = fillColor;

		// fillColor를 HSV로 변환
		Color.RGBToHSV(fillColor, out float h, out float s, out float v);

		// 배경용으로 채도를 낮추고 밝기를 조절
		float bgS = s * bgSaturation;
		float bgV = v * bgBrightness;

		Color bgColor = Color.HSVToRGB(h, bgS, bgV);

		bgColor.a = bgAlpha;
		bgImage.color = bgColor;
	}
	public void FillUpdate()
	{
		Init();
		if (fillRectMask2D == null || fillRect == null) return;

		/// <summary>
		/// Padding to be applied to the masking
		/// X = Left
		/// Y = Bottom
		/// Z = Right
		/// W = Top
		/// </summary>
		switch (Diraction)
		{
			case FillDiraction.LeftToRight:
			{
				float length = fillRect.rect.width;
				float fill = length * (1f-Value);
				fillRectMask2D.padding = new Vector4(0f, 0f, fill, 0f);
			}
			break;
			case FillDiraction.RightToLeft:
			{
				float length = fillRect.rect.width;
				float fill = length * (1f-Value);
				fillRectMask2D.padding = new Vector4(fill, 0f, 0f, 0f);
			}
			break;
			case FillDiraction.TopToBottom:
			{
				float length = fillRect.rect.height;
				float fill = length * (1f-Value);
				fillRectMask2D.padding = new Vector4(0f, fill, 0f, 0f);
			}
			break;
			case FillDiraction.BottomToTop:
			{
				float length = fillRect.rect.height;
				float fill = length * (1f-Value);
				fillRectMask2D.padding = new Vector4(0f, 0f, 0f, fill);
			}
			break;
		}
	}
}
