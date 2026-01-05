using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using static StrategyGamePlayData;

public class LabelItemElementReferrer : MonoBehaviour
{
	[SerializeField] private Image mainIcon;
	[SerializeField] private Image subIcon;
	[Space]
	[SerializeField] private Button select;
	[Space]
	[SerializeField] private TMP_Text displayText;
	[Space]
	[SerializeField] private Image FillShild;
	[SerializeField] private Image FillPersonnel;
	[SerializeField] private Image FillMaterial;
	[SerializeField] private Image FillElectric;
	[Space]
	[SerializeField] private HorizontalLayoutGroup effectIconGroup;
	private Dictionary<StatusEffectsFlag,GameObject> effectIcons;
	[SerializeField] private HorizontalLayoutGroup guideIconPrefab;
	private Dictionary<StatusEffectsFlag,GameObject> guideIcons;
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
	public void SetDisplayText(string displayText)
	{
		if (this.displayText.IsNotNullRef())
			this.displayText.text = displayText;
	}
	public void SetShieldFillAmount(float fillAmount)
	{
		if (FillShild.IsNotNullRef())
			FillShild.fillAmount = fillAmount;
	}
	public void SetPersonnelFillAmount(float fillAmount)
	{
		if (FillPersonnel.IsNotNullRef())
			FillPersonnel.fillAmount = fillAmount;
	}
	public void SetMaterialFillAmount(float fillAmount)
	{
		if (FillMaterial.IsNotNullRef())
			FillMaterial.fillAmount = fillAmount;
	}
	public void SetElectricFillAmount(float fillAmount)
	{
		if (FillElectric.IsNotNullRef())
			FillElectric.fillAmount = fillAmount;
	}
	public void SetEffectIcon(StatusEffectsFlag effectFlag)
	{

	}
	public void SetGuideIcon(StatusEffectsFlag guideFlag)
	{

	}
	public void OnClickAddListener(UnityAction action)
	{
		if(select.IsNullRef()) return;
		select.onClick.AddListener(action);
	}
	public void OnClickRemoveListener(UnityAction action)
	{
		if (select.IsNullRef()) return;
		select.onClick.RemoveListener(action);
	}
	public void OnClickRemoveAllListeners()
	{
		if(select.IsNullRef()) return;
		select.onClick.RemoveAllListeners();
	}
}