using System;
using System.Collections.Generic;

using GameUI;

using Sirenix.OdinInspector;

using TMPro;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using static StrategyGamePlayData;

public class LabelItemElementReferrer : MonoBehaviour
{
	[Serializable]
	public class ColorTarget
	{

		[ShowInInspector, OnValueChanged("ColorUpdate")]
		private Color targetColor;

		[SerializeField, InlineProperty]
		private TargetGraphic[] targetGraphics;

		public Color TargetColor { get => targetColor; set { targetColor = value; ColorUpdate(); } }

		[Serializable]
		public class TargetGraphic
		{
			public Graphic iamge;
			[HorizontalGroup("Color"), LabelText("S"), LabelWidth(20), Range(0,1), OnValueChanged("@TestColor($property.Parent.Parent.Parent.Parent.ValueEntry.WeakSmartValue)")]
			public float saturation = 1f;
			[HorizontalGroup("Color"), LabelText("V"), LabelWidth(20), Range(0,1), OnValueChanged("@TestColor($property.Parent.Parent.Parent.Parent.ValueEntry.WeakSmartValue)")]
			public float brightness = 1f;
			[HorizontalGroup("Color"), LabelText("A"), LabelWidth(20), Range(0,1),  OnValueChanged("@TestColor($property.Parent.Parent.Parent.Parent.ValueEntry.WeakSmartValue)")]
			public float alpha = 1f;
#if UNITY_EDITOR
			public void TestColor(object parentValue)
			{
				if (iamge.IsNullRef() || parentValue == null) return;
				if (parentValue is ColorTarget parent)
				{
					Color baseColor = parent.targetColor;
					Color.RGBToHSV(baseColor, out float h, out float s, out float v);
					SetColor(h, s, v, baseColor.a);
				}
			}
#endif
			public void SetColor(float h, float s, float v, float a)
			{
				if (iamge.IsNullRef()) return;
				var targetColor = Color.HSVToRGB(h, s * saturation, v * brightness);
				targetColor.a = a * alpha;
				iamge.color = targetColor;
			}
		}
		public void ColorUpdate()
		{
			Color.RGBToHSV(targetColor, out float h, out float s, out float v);
			float a = targetColor.a;
			int length = targetGraphics == null ? 0 : targetGraphics.Length;
			for (int i = 0 ; i < length ; i++)
			{
				var target = targetGraphics[i];
				if (target.IsNullRef()) continue;

				target.SetColor(h, s, v, a);
			}
		}
	}

	[Space]
	[SerializeField] private RectTransform showDetailRect;
#if UNITY_EDITOR
	private bool is_showDetailRect => showDetailRect.IsNotNullRef();
#endif
	[SerializeField,BoxGroup("DetailRect", VisibleIf ="is_showDetailRect")]  private Image mainIcon;
	[SerializeField,BoxGroup("DetailRect")] private Image subIcon;
	[Space]
	[SerializeField,BoxGroup("DetailRect")] private Button select;
	[Space]
	[SerializeField,BoxGroup("DetailRect")] private TMP_Text displayText;
	[Space]
	[SerializeField,BoxGroup("DetailRect")] private ValueToFillAPI FillShild;
	[SerializeField,BoxGroup("DetailRect")] private ValueToFillAPI FillPersonnel;
	[SerializeField,BoxGroup("DetailRect")] private ValueToFillAPI FillMaterial;
	[SerializeField,BoxGroup("DetailRect")] private ValueToFillAPI FillElectric;
	[Space]
	[SerializeField, InlineProperty, BoxGroup("DetailRect/Accent Color Target"), HideLabel]
	private ColorTarget accentColorTarget;
	[SerializeField, InlineProperty, BoxGroup("DetailRect/Text Color Target"), HideLabel]
	private ColorTarget textColorTarget;

	[Space]
	[SerializeField] private RectTransform showSimpleRect;
#if UNITY_EDITOR
	private bool is_showSimpleRect => showSimpleRect.IsNotNullRef();
#endif
	[SerializeField,BoxGroup("SimpleRect", VisibleIf = "is_showSimpleRect")] private ValueToFillAPI fillSimple;
	[SerializeField,BoxGroup("SimpleRect")] private Color simpleFillRectColor;
	[SerializeField,BoxGroup("SimpleRect")] private Color simpleFillBGColor;

	[Space]
	[SerializeField,BoxGroup("IconGroup")] private HorizontalLayoutGroup effectIconGroup;
	private Dictionary<StatusEffectsFlag,GameObject> effectIcons;
	[SerializeField,BoxGroup("IconGroup")] private HorizontalLayoutGroup guideIconPrefab;
	private Dictionary<StatusEffectsFlag,GameObject> guideIcons;

	public void Awake()
	{
		accentColorTarget.ColorUpdate();
		textColorTarget.ColorUpdate();
	}
	public void ShowSimpleElement()
	{
		showDetailRect.gameObject.SetActive(false);
		showSimpleRect.gameObject.SetActive(true);
	}
	public void ShowDetailElement()
	{
		showSimpleRect.gameObject.SetActive(false);
		showDetailRect.gameObject.SetActive(true);
	}
	public void SetMainIcon(Sprite sprite)
	{
		if (mainIcon.IsNotNullRef())
			mainIcon.sprite = sprite;
	}
	public void SetSubIcon(Sprite sprite)
	{
		if (subIcon.IsNotNullRef())
			subIcon.sprite = sprite;
	}
	internal void SetAccentColor(Color color)
	{
		accentColorTarget.TargetColor = color;
	}

	internal void SetTextColor(Color color)
	{
		textColorTarget.TargetColor = color;
	}
	public void SetDisplayText(string displayText)
	{
		if (this.displayText.IsNotNullRef())
			this.displayText.text = displayText;
	}
	public void SetShieldFillAmount(float fillAmount, int index = 0)
	{
		if (FillShild.IsNullRef()) return;
		FillShild.gameObject.SetActive(index >= 0);
		FillShild.SetValue(fillAmount, index);
	}
	public void SetPersonnelFillAmount(float fillAmount, int index = 0)
	{
		if (FillPersonnel.IsNullRef()) return;
		FillPersonnel.gameObject.SetActive(index >= 0);
		FillPersonnel.SetValue(fillAmount, index);
	}
	public void SetMaterialFillAmount(float fillAmount, int index = 0)
	{
		if (FillMaterial.IsNullRef()) return;
		FillMaterial.gameObject.SetActive(index >= 0);
		FillMaterial.SetValue(fillAmount, index);
	}
	public void SetElectricFillAmount(float fillAmount, int index = 0)
	{
		if (FillElectric.IsNullRef()) return;
		FillElectric.gameObject.SetActive(index >= 0);
		FillElectric.SetValue(fillAmount, index);
	}
	public void SetSimpleFillAmount(float fillAmount, int index = 0)
	{
		if (fillSimple.IsNullRef()) return;
		fillSimple.gameObject.SetActive(index >= 0);
		fillSimple.SetValue(fillAmount, index);
	}
	public void SetEffectIcon(StatusEffectsFlag effectFlag)
	{

	}
	public void SetGuideIcon(StatusEffectsFlag guideFlag)
	{

	}
	public void OnClickAddListener(UnityAction action)
	{
		if (select.IsNullRef()) return;
		select.onClick.AddListener(action);
	}
	public void OnClickRemoveListener(UnityAction action)
	{
		if (select.IsNullRef()) return;
		select.onClick.RemoveListener(action);
	}
	public void OnClickRemoveAllListeners()
	{
		if (select.IsNullRef()) return;
		select.onClick.RemoveAllListeners();
	}
}