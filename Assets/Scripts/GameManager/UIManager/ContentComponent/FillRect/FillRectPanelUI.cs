using System;

using Sirenix.OdinInspector;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace GameUI
{
	public class FillRectPanelUI : PanelItemComponent, IFillItem, IShowHideAsync
	{
		#region Ref Value
		[SerializeField, FoldoutGroup("FillRext")]
		private RectTransform showMaskRect;
		[SerializeField, FoldoutGroup("Image")]
		private Image fillImage;
		[SerializeField, FoldoutGroup("Image"),PropertyOrder(10)]
		private Image backgroundFillImage;
		[SerializeField, FoldoutGroup("Text")]
		private TMP_Text textUI;
		private RectTransform textRect;
		#endregion

		#region Value
		public enum FillMethodType
		{
			[InspectorName("→")]
			Horizontal_Left,
			[InspectorName("←")]
			Horizontal_Right,
			[InspectorName("↑")]
			Vertical_Bottom,
			[InspectorName("↓")]
			Vertical_Top,
		}
		public enum FloatToIntType
		{
			Float = 0,
			FloorToInt,
			RountToInt,
			CeilToInt
		}
		[SerializeField, HideIf("@true")]
		private Vector2 minMax;
		[SerializeField, HideIf("@true")]
		private Vector2 valueRange;
		[SerializeField, HideIf("@true")]
		private FillMethodType fillMethod;
		[SerializeField, HideIf("@true")]
		private FloatToIntType floatToInt;
		[SerializeField, HideIf("@true")]
		private string textFormat;
		[SerializeField, HideIf("@true")]
		private bool textRectFitInFillRect;

		[ShowInInspector, BoxGroup("FillRext/Value", ShowLabel = false, VisibleIf = "@showMaskRect != null")]
		public Vector2 MinMax
		{
			get => minMax;
			set { minMax = value; ChangeShowMaskRect(); }
		}
		[ShowInInspector, BoxGroup("FillRext/Value")]
		[MinMaxSlider(minMaxValueGetter: "MinMax", true)]
		public Vector2 Value
		{
			get => valueRange;
			set { valueRange = value; ChangeShowMaskRect(); }
		}
		[ShowInInspector, BoxGroup("FillRext/Value"), EnumToggleButtons]
		public FillMethodType FillMethod
		{
			get => fillMethod;
			set { fillMethod = value; ChangeShowMaskRect(); }
		}
		[ShowInInspector, BoxGroup("FillRext/Value"), EnumToggleButtons]
		public FloatToIntType FloatToInt
		{
			get => floatToInt;
			set { floatToInt = value; ChangeShowMaskRect(); }
		}

		[ShowInInspector, BoxGroup("Text/Value", ShowLabel = false, VisibleIf = "@textUI != null")]
		[InfoBox(@"TextFormat Hint
	{0} == Value.y		== ""Value (end)""
	{1} == MinMax.y		== ""Max""
	{2} == MinMax.x		== ""Min""
	{4} == Value.x		== ""Value (start)""
	Ex) ""{0}/{1}"" 
			== ""{Value.y}/{MinMax.y}""
			== ""{Value}/{Max}""")]
		public string TextFormat
		{
			get => textFormat;
			set { textFormat = value; ChangeText(); }
		}
		[ShowInInspector, BoxGroup("Text/Value")]
		public bool TextRectFitInFillRect
		{
			get => textRectFitInFillRect;
			set { textRectFitInFillRect = value; ChangeTextRect(); }
		}

		[ShowInInspector, BoxGroup("Image/Value", ShowLabel = false, VisibleIf = "@fillImage != null")]
		public Sprite FillSprite
		{
			get => fillImage == null ? null : fillImage.sprite;
			set { if (fillImage == null) return; else fillImage.sprite = value; ApplyBackgroundImage(); }
		}
		[ShowInInspector, BoxGroup("Image/Value")]
		[ColorUsage(false)]
		public Color FillColor
		{
			get => fillImage == null ? Color.clear : fillImage.color;
			set { if (fillImage == null) return; else fillImage.color = value; }
		}
		[ShowInInspector, BoxGroup("Image/Value")]
		[Range(0f,1f)]
		public float FillAlpha
		{
			get => fillImage == null ? 0 : fillImage.color.a;
			set
			{
				if (fillImage == null) return;
				else
				{
					Color color = fillImage.color;
					color.a = value;
					fillImage.color = color;
				}
			}
		}

		[ShowInInspector, BoxGroup("Image/Value")]
		public float PixelsPerUnitMultiplier
		{
			get => fillImage == null ? 1 : fillImage.pixelsPerUnitMultiplier;
			set { if (fillImage == null) return; else fillImage.pixelsPerUnitMultiplier = value; ApplyBackgroundImage(); }
		}
		[ShowInInspector, BoxGroup("Image/Background", ShowLabel = false, VisibleIf = "@backgroundFillImage != null", Order = 11)]
		[ColorUsage(false)]
		public Color BGFillColor
		{
			get => backgroundFillImage == null ? Color.clear : backgroundFillImage.color;
			set { if (backgroundFillImage == null) return; else backgroundFillImage.color = value; }
		}
		[ShowInInspector, BoxGroup("Image/Background")]
		[Range(0f, 1f)]
		public float BGFillAlpha
		{
			get => backgroundFillImage == null ? 0 : backgroundFillImage.color.a;
			set
			{
				if (backgroundFillImage == null) return;
				else
				{
					Color color = backgroundFillImage.color;
					color.a = value;
					backgroundFillImage.color = color;
				}
			}
		}
		#endregion
		private void Reset()
		{
			if (transform.childCount < 2) return;
			showMaskRect = transform.GetChild(0).GetComponent<RectTransform>();
			fillImage = transform.GetChild(1).GetComponent<Image>();
			if (transform.childCount < 3) return;
			textUI = transform.GetChild(2).GetComponent<TMP_Text>();
		}
		public void ChangeShowMaskRect()
		{
			if (showMaskRect.IsNullRef()) return;

			float min = MinMax.x;
			float max = minMax.y;
			if (min > max)
			{
				max = MinMax.x;
				min = minMax.y;
			}
			float minValue = Value.x;
			float maxValue = Value.y;

			if (fillImage.IsNotNullRef())
			{
				if (Mathf.Approximately(minValue, maxValue))
				{
					fillImage.enabled = false;
					if (textUI.IsNotNullRef()) textUI.enabled = false;
					return;
				}
				fillImage.enabled = true;
				if (textUI.IsNotNullRef()) textUI.enabled = true;
			}
			if (minValue > maxValue)
			{
				maxValue = Value.x;
				minValue = Value.y;
			}
			if (floatToInt != FloatToIntType.Float)
			{
				min = ConvertFloatToInt(min);
				max = ConvertFloatToInt(max);
				minValue = ConvertFloatToInt(minValue);
				maxValue = ConvertFloatToInt(maxValue);
			}
			if (Mathf.Approximately(max, min))
			{
				max += 1f;
			}

			float delta = max - min;


			float minRatio = (minValue - min) / (delta);
			float maxRatio = (maxValue - min) / (delta);

			var anchorMin = showMaskRect.anchorMin;
			var anchorMax = showMaskRect.anchorMax;
			if (fillMethod == FillMethodType.Horizontal_Left)
			{
				anchorMin.x = minRatio;
				anchorMax.x = maxRatio;
				anchorMin.y = 0f;
				anchorMax.y = 1f;
			}
			else if (fillMethod == FillMethodType.Horizontal_Right)
			{
				anchorMin.x = 1f - maxRatio;
				anchorMax.x = 1f - minRatio;
				anchorMin.y = 0f;
				anchorMax.y = 1f;
			}
			else if (fillMethod == FillMethodType.Vertical_Bottom)
			{
				anchorMin.x = 0f;
				anchorMax.x = 1f;
				anchorMin.y = minRatio;
				anchorMax.y = maxRatio;
			}
			else
			{
				anchorMin.x = 0f;
				anchorMax.x = 1f;
				anchorMin.y = 1f - maxRatio;
				anchorMax.y = 1f - minRatio;
			}
			showMaskRect.anchorMin = anchorMin;
			showMaskRect.anchorMax = anchorMax;

			ChangeText();
			ChangeTextRect();
		}
		public void ChangeTextRect()
		{
			if (!textRectFitInFillRect) return;
			if (textUI.IsNullRef()) return;
			if (textRect.IsNullRef())
			{
				textRect = textUI.GetComponent<RectTransform>();
			}
			if (textRect.IsNullRef()) return;

			Transform textParent = textRect.parent;
			if (showMaskRect.parent != textParent)
			{
				textRect.parent = showMaskRect.parent;
			}
			textRect.anchoredPosition = showMaskRect.anchoredPosition;
			textRect.localRotation = showMaskRect.localRotation;
			textRect.localScale = showMaskRect.localScale;
			textRect.anchorMin = showMaskRect.anchorMin;
			textRect.anchorMax = showMaskRect.anchorMax;
			textRect.sizeDelta = showMaskRect.sizeDelta;
			textRect.pivot = showMaskRect.pivot;
			if (textRect.parent != textParent)
			{
				textRect.parent = textParent;
			}
		}
		public void ChangeText()
		{
			if (textUI.IsNullRef()) return;
			if (string.IsNullOrWhiteSpace(textFormat))
			{
				textUI.text = "";
				return;
			}

			float min = MinMax.x;
			float max = minMax.y;
			if (min > max)
			{
				max = MinMax.x;
				min = minMax.y;
			}
			float minValue = Value.x;
			float maxValue = Value.y;
			if (minValue > maxValue)
			{
				maxValue = Value.x;
				minValue = Value.y;
			}


			try
			{
				if (floatToInt != FloatToIntType.Float)
				{
					textUI.text = string.Format(textFormat,
						ConvertFloatToInt(maxValue),
						ConvertFloatToInt(max),
						ConvertFloatToInt(min),
						ConvertFloatToInt(minValue));
				}
				else
				{
					textUI.text = string.Format(textFormat, maxValue, max, min, minValue);
				}
			}
			catch (Exception ex)
			{
				textUI.text = textFormat;
				Debug.LogWarning(ex);
			}
		}
		private int ConvertFloatToInt(float value)
		{
			return floatToInt switch
			{
				FloatToIntType.FloorToInt => Mathf.FloorToInt(value),
				FloatToIntType.RountToInt => Mathf.RoundToInt(value),
				FloatToIntType.CeilToInt => Mathf.CeilToInt(value),
				_ => (int)value,
			};
		}
		private void ApplyBackgroundImage()
		{
			if (backgroundFillImage.IsNullRef()) return;
			backgroundFillImage.sprite = fillImage.sprite;
			backgroundFillImage.pixelsPerUnitMultiplier = fillImage.pixelsPerUnitMultiplier;
		}
	}
}
