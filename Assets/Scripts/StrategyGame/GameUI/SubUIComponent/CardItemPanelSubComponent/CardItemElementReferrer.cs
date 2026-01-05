using System;

using GameUI;

using Sirenix.OdinInspector;

using TMPro;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using Button = UnityEngine.UI.Button;

public class CardItemElementReferrer : MonoBehaviour
{

	[Serializable, InlineProperty, HideLabel]
	public class IconSpriteSwap
	{
		public Image iconImage;
		[ShowIf("@iconImage!=null")]
		public Gradient iconColor;
		[ListDrawerSettings(ShowFoldout = false, ShowPaging = false), ShowIf("@iconImage!=null")]
		public IconProgress[] progressSwap;
		[Serializable, InlineProperty, HideLabel]
		public class IconProgress
		{
			[HorizontalGroup, HideLabel]
			public Sprite icon;
			[HorizontalGroup, HideLabel, Range(0,1f)]
			public float profress;
		}

		public void SetProgress(float progress)
		{
			if (iconImage.IsNullRef() || iconColor.IsNullRef()) return;
			
			iconImage.color = iconColor.Evaluate(progress);

			if (progressSwap.IsNotNullRef() && progressSwap.Length > 0)
			{
				int selectIndex = -1;
				int length = progressSwap.Length;
				for (int i = 0 ; i < length ; i++)
				{
					var prog = progressSwap[i];
					if (prog.profress <= progress)
					{
						selectIndex = i;
					}
					else
					{
						break;
					}
				}
				iconImage.sprite = (selectIndex < 0 ? progressSwap[^1] : progressSwap[selectIndex]).icon;
			}
		}
	}

	[SerializeField] private TMP_Text titleText;
	[SerializeField] private Image mainImage;
	[SerializeField] private Button select;

	[SerializeField, FoldoutGroup("Shild")] private FillRectPanelUI fillShild;
	[SerializeField, FoldoutGroup("Shild"), ShowIf("@fillShild!=null")] private IconSpriteSwap shildIcon;

	[SerializeField, FoldoutGroup("Personnel")] private FillRectPanelUI fillPersonnel;
	[SerializeField, FoldoutGroup("Personnel"), ShowIf("@fillPersonnel!=null")] private IconSpriteSwap personnelIcon;

	[SerializeField, FoldoutGroup("Material")] private FillRectPanelUI fillMaterial;
	[SerializeField, FoldoutGroup("Material"), ShowIf("@fillMaterial!=null")] private IconSpriteSwap materialIcon;

	[SerializeField, FoldoutGroup("Electric")] private FillRectPanelUI fillElectric;
	[SerializeField, FoldoutGroup("Electric"), ShowIf("@fillElectric!=null")] private IconSpriteSwap electricIcon;

	public void SetTItleText(string text)
	{
		if (titleText.IsNotNullRef())
			titleText.text = text;
	}
	public void SetTitleImage(Sprite sprite)
	{
		if (mainImage.IsNotNullRef())
			mainImage.sprite = sprite;
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

	public void SetShildFillAmount((int value, int max)value) => SetShildFillAmount(value.max, value.max);
	public void SetShildFillAmount(int value, int max)
	{
		if (fillShild.IsNotNullRef())
		{
			fillShild.MinMax = new Vector2(0, max);
			fillShild.Value = new Vector2(0, value);
			shildIcon.SetProgress(Ratio(value, max));
		}
	}
	public void SetPersonnelFillAmount((int value, int max) value) => SetPersonnelFillAmount(value.max, value.max);
	public void SetPersonnelFillAmount(int value, int max)
	{
		if (fillPersonnel.IsNotNullRef())
		{
			fillPersonnel.MinMax = new Vector2(0, max);
			fillPersonnel.Value = new Vector2(0, value);
			personnelIcon.SetProgress(Ratio(value, max));
		}
	}
	public void SetMaterialFillAmount((int value, int max) value) => SetMaterialFillAmount(value.max, value.max);
	public void SetMaterialFillAmount(int value, int max)
	{
		if (fillMaterial.IsNotNullRef())
		{
			fillMaterial.MinMax = new Vector2(0, max);
			fillMaterial.Value = new Vector2(0, value);
			materialIcon.SetProgress(Ratio(value, max));
		}
	}
	public void SetElectricFillAmount((int value, int max) value) => SetElectricFillAmount(value.max, value.max);
	public void SetElectricFillAmount(int value, int max)
	{
		if (fillElectric.IsNotNullRef())
		{
			fillElectric.MinMax = new Vector2(0, max);
			fillElectric.Value = new Vector2(0, value);
			electricIcon.SetProgress(Ratio(value, max));
		}
	}

	private float Ratio(int value, int max)
	{
		if (max <= 0) return 0f;
		return (float)value / (float)max;
	}
}
