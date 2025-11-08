using System;

using Sirenix.OdinInspector;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class FillRectUI : MonoBehaviour
{
	[SerializeField, Range(0f,1f)]
	private float fillAmount;
	[SerializeField]
	private Slider.Direction fillDiraction;

	[SerializeField, HorizontalGroup("Mask"), LabelText("BG Mask")]
	private RectMask2D bgMask;
	[SerializeField, HorizontalGroup("Mask"), LabelText("BG Mask")]
	private RectMask2D fillMask;

	[SerializeField, HorizontalGroup("Rect"), LabelText("BG Rect")]
	protected RectTransform bgRect;
	[SerializeField, HorizontalGroup("Rect"), LabelText("Fill Rect")]
	private RectTransform fillRect;

	[SerializeField, HorizontalGroup("Image"), LabelText("BG Image")]
	private Image bgImage;
	[SerializeField, HorizontalGroup("Image"), LabelText("Fill Image")]
	private Image fillImage;

	private TMP_Text fillRectTextUI;

	[FoldoutGroup("ImageConfig"), SerializeField]
	private float pixelsPerUnit;
	[FoldoutGroup("ImageConfig"), SerializeField]
	private Color fillColor;
	[FoldoutGroup("ImageConfig"), SerializeField, Range(0f,1f)]
	private float fillAlpha;
	[FoldoutGroup("ImageConfig"), SerializeField, Range(0f,1f)]
	private float bgSaturation;
	[FoldoutGroup("ImageConfig"), SerializeField, Range(0f,1f)]
	private float bgBrightness;
	[FoldoutGroup("ImageConfig"), SerializeField, Range(0f,1f)]
	private float bgAlpha;
	[FoldoutGroup("ImageConfig"), ShowInInspector, ReadOnly, EnableGUI]
	private Color bgColor { get; set; }

	public float Value
	{
		get => Mathf.Clamp01(fillAmount);
		protected set {
			if (Mathf.Approximately(value, fillAmount)) return;
			fillAmount = value;
			FillUpdate(); 
		}
	}
	public Slider.Direction Diraction
	{
		get => fillDiraction;
		protected set
		{
			if (fillDiraction == value) return;
			fillDiraction = value;
			FillUpdate();
		}
	}
	public virtual string Text
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

	public void Reset()
	{
		fillAmount = 0.5f;
		fillDiraction = Slider.Direction.LeftToRight;
		Image image = GetComponentInChildren<Image>();
		if (image != null) fillColor = image.color;
		else fillColor = Color.white;
		bgSaturation = 0f;
		bgBrightness = 1f;
		fillAlpha = 1f;
		bgAlpha = .5f;
		pixelsPerUnit = image.pixelsPerUnitMultiplier;
		FillUpdate();
	}
	public virtual void OnValidate()
	{
		Init();
		FillUpdate();
		ColorUpdate();
	}
	public virtual void Awake()
	{
		Init();
		ColorUpdate();
	}

	private void Init()
	{
		var makss = GetComponentsInChildren<RectMask2D>();
		if (makss.Length == 0)
		{
			return;
		}
		if (bgMask == null)
		{
			bgMask = makss[0];
		}
		if (bgMask != null)
		{
			if (bgRect == null)
			{
				bgRect = bgMask.GetComponent<RectTransform>();
			}
			if (bgImage == null)
			{
				bgImage = bgMask.GetComponentInChildren<Image>();
			}
		}

		if (fillMask == null)
		{
			fillMask = makss[1];
		}
		if (fillMask != null)
		{
			if (fillRect == null)
			{
				fillRect = fillMask.GetComponent<RectTransform>();
			}
			if (fillImage == null)
			{
				fillImage = fillMask.GetComponentInChildren<Image>();
			}
			if (bgRect != null && fillRect != null)
			{
				fillRect.anchorMin = bgRect.anchorMin;
				fillRect.anchorMax = bgRect.anchorMax;
				fillRect.anchoredPosition = bgRect.anchoredPosition;
				fillRect.sizeDelta = bgRect.sizeDelta;
				fillRect.pivot = bgRect.pivot;
			}
		}
	}
	private void ColorUpdate()
	{
		if (fillImage == null) return;

		if (bgImage == null || fillImage == null) return;

		// 채워진 부분은 fillColor 그대로 적용
		fillColor.a = fillAlpha;
		fillImage.color = fillColor;

		// fillColor를 HSV로 변환
		Color.RGBToHSV(fillColor, out float h, out float s, out float v);

		// 배경용으로 채도를 낮추고 밝기를 조절
		float bgS = s * bgSaturation;
		float bgV = v * bgBrightness;

		var color= Color.HSVToRGB(h, bgS, bgV);
		color.a = bgAlpha;
		bgColor = color;
		bgImage.color = bgColor;

		if (pixelsPerUnit >= 0.01f)
		{
			fillImage.pixelsPerUnitMultiplier = pixelsPerUnit;
			bgImage.pixelsPerUnitMultiplier = pixelsPerUnit;
		}
		else
		{
			pixelsPerUnit = fillImage.pixelsPerUnitMultiplier;
			if (pixelsPerUnit < 0.01f)
			{
				pixelsPerUnit = 0.01f;
			}
		}
	}
	public void FillUpdate()
	{
		RenderBG();
		RenderFill();
		void RenderBG()
		{
			if (bgMask == null || bgRect == null || bgImage == null) return;
			switch (Diraction)
			{
				case Slider.Direction.RightToLeft:
				{
					float length = bgRect.rect.width;
					float fill = length * (Value);
					bgMask.padding = new Vector4(0f, 0f, fill, 0f);
				}
				break;
				case Slider.Direction.LeftToRight:
				{
					float length = bgRect.rect.width;
					float fill = length * (Value);
					bgMask.padding = new Vector4(fill, 0f, 0f, 0f);
				}
				break;
				case Slider.Direction.BottomToTop:
				{
					float length = bgRect.rect.height;
					float fill = length * (Value);
					bgMask.padding = new Vector4(0f, fill, 0f, 0f);
				}
				break;
				case Slider.Direction.TopToBottom:
				{
					float length = bgRect.rect.height;
					float fill = length * (Value);
					bgMask.padding = new Vector4(0f, 0f, 0f, fill);
				}
				break;
			}
		}
		void RenderFill()
		{
			if (fillMask == null || fillRect == null || fillImage == null) return;
			switch (Diraction)
			{
				case Slider.Direction.LeftToRight:
				{
					float length = fillRect.rect.width;
					float fill = length * (1f-Value);
					fillMask.padding = new Vector4(0f, 0f, fill, 0f);
				}
				break;
				case Slider.Direction.RightToLeft:
				{
					float length = fillRect.rect.width;
					float fill = length * (1f-Value);
					fillMask.padding = new Vector4(fill, 0f, 0f, 0f);
				}
				break;
				case Slider.Direction.TopToBottom:
				{
					float length = fillRect.rect.height;
					float fill = length * (1f-Value);
					fillMask.padding = new Vector4(0f, fill, 0f, 0f);
				}
				break;
				case Slider.Direction.BottomToTop:
				{
					float length = fillRect.rect.height;
					float fill = length * (1f-Value);
					fillMask.padding = new Vector4(0f, 0f, 0f, fill);
				}
				break;
			}
		}
	}

	public virtual void SetValue(float value)
	{
		Value = Mathf.Clamp01(value);
	}
	public virtual void SetValueText(float value, string text)
	{
		SetValue(value);
		Text = text;
	}
	public virtual float GetValue()
	{
		return Value;
	}
	public virtual void SetDirection(Slider.Direction direction)
	{
		Diraction = direction;
	}
}
