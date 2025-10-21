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

	[SerializeField, HorizontalGroup("Mask"), LabelText("BG Mask")]
	private RectMask2D bgMask;
	[SerializeField, HorizontalGroup("Mask"), LabelText("BG Mask")]
	private RectMask2D fillMask;

	[SerializeField, HorizontalGroup("Rect"), LabelText("BG Rect")]
	private RectTransform bgRect;
	[SerializeField, HorizontalGroup("Rect"), LabelText("Fill Rect")]
	private RectTransform fillRect;

	[SerializeField, HorizontalGroup("Image"), LabelText("BG Image")]
	private Image bgImage;
	[SerializeField, HorizontalGroup("Image"), LabelText("Fill Image")]
	private Image fillImage;

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
	[FoldoutGroup("Color"), ShowInInspector, ReadOnly, EnableGUI]
	private Color bgColor { get; set; }

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
		Init();
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
			if (bgRect != null)
			{
				bgRect.anchorMin = Vector2.zero;
				bgRect.anchorMax = Vector2.one;
				bgRect.anchoredPosition = Vector2.zero;
				bgRect.sizeDelta = Vector2.zero;
				bgRect.pivot = Vector2.one * 0.5f;
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
			if (fillRect != null)
			{
				fillRect.anchorMin = Vector2.zero;
				fillRect.anchorMax = Vector2.one;
				fillRect.anchoredPosition = Vector2.zero;
				fillRect.sizeDelta = Vector2.zero;
				fillRect.pivot = Vector2.one * 0.5f;
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
				case FillDiraction.RightToLeft:
				{
					float length = bgRect.rect.width;
					float fill = length * (Value);
					bgMask.padding = new Vector4(0f, 0f, fill, 0f);
				}
				break;
				case FillDiraction.LeftToRight:
				{
					float length = bgRect.rect.width;
					float fill = length * (Value);
					bgMask.padding = new Vector4(fill, 0f, 0f, 0f);
				}
				break;
				case FillDiraction.BottomToTop:
				{
					float length = bgRect.rect.height;
					float fill = length * (Value);
					bgMask.padding = new Vector4(0f, fill, 0f, 0f);
				}
				break;
				case FillDiraction.TopToBottom:
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
				case FillDiraction.LeftToRight:
				{
					float length = fillRect.rect.width;
					float fill = length * (1f-Value);
					fillMask.padding = new Vector4(0f, 0f, fill, 0f);
				}
				break;
				case FillDiraction.RightToLeft:
				{
					float length = fillRect.rect.width;
					float fill = length * (1f-Value);
					fillMask.padding = new Vector4(fill, 0f, 0f, 0f);
				}
				break;
				case FillDiraction.TopToBottom:
				{
					float length = fillRect.rect.height;
					float fill = length * (1f-Value);
					fillMask.padding = new Vector4(0f, fill, 0f, 0f);
				}
				break;
				case FillDiraction.BottomToTop:
				{
					float length = fillRect.rect.height;
					float fill = length * (1f-Value);
					fillMask.padding = new Vector4(0f, 0f, 0f, fill);
				}
				break;
			}
		}
	}
}
