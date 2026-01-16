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

	[SerializeField] private Image mainIcon;
	[SerializeField] private Image subIcon;
	[Space]
	[SerializeField] private Button select;
	[Space]
	[SerializeField] private TMP_Text displayText;
	[Space]
	[SerializeField] private ValueToFillAPI FillShild;
	[SerializeField] private ValueToFillAPI FillPersonnel;
	[SerializeField] private ValueToFillAPI FillMaterial;
	[SerializeField] private ValueToFillAPI FillElectric;
	[Space]
	[SerializeField] private HorizontalLayoutGroup effectIconGroup;
	private Dictionary<StatusEffectsFlag,GameObject> effectIcons;
	[SerializeField] private HorizontalLayoutGroup guideIconPrefab;
	private Dictionary<StatusEffectsFlag,GameObject> guideIcons;

	[Space]
	[SerializeField, InlineProperty, BoxGroup("Accent Color Target"), HideLabel]
	private ColorTarget accentColorTarget;
	[SerializeField, InlineProperty, BoxGroup("Text Color Target"), HideLabel]
	private ColorTarget textColorTarget;
	public void Awake()
	{
		accentColorTarget.ColorUpdate();
		textColorTarget.ColorUpdate();
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

	public void ShowSimpleElement()
	{

	}
	public void ShowDetailElement()
	{

	}
}