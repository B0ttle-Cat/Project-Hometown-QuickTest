using System.Collections.Generic;

using GameUI;

using Sirenix.OdinInspector;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class UnitCardItemPanel : PanelItemComponent, IFindUIObject
{
	private IUnitCardUIObject unitCard;
	public IFindUIObject ThisUIFinder => this;
	[SerializeField, PropertyOrder(-90)] private List<IFindUIObject.KeyPairObject> keyPairs;
	List<IFindUIObject.KeyPairObject> IFindUIObject.KeyPairs { get => keyPairs; set => keyPairs = value; }
	public void SetUITarget(UnitObject operationObject)
	{
		unitCard = operationObject;
	}

	private Image titleImage;
	private TMP_Text titleText;
	private bool showShield;
	private bool showMaterial;
	private bool showElectric;
	private FillRectPanelUI shieldFillRect;
	private FillRectPanelUI materialFillRect;
	private FillRectPanelUI electricFillRect;

	internal void Attach(IUnitCardUIObject item)
	{
		unitCard = item;

		if (titleImage.IsNotNullRef() || ThisUIFinder.TryFind<Image>("..TitleImage", out titleImage))
			titleImage.sprite = unitCard.GetTitleImage();

		if (titleText.IsNotNullRef() || ThisUIFinder.TryFind<TMP_Text>("..TitleText", out titleText))
			titleText.text = unitCard.GetTitleName();

		showShield = false;
		showMaterial = false;
		showElectric = false;

		if (shieldFillRect.IsNotNullRef() || ThisUIFinder.TryFind<FillRectPanelUI>("../Shield", out shieldFillRect))
			showShield = shieldFillRect.gameObject.activeSelf;
		if (materialFillRect.IsNotNullRef() || ThisUIFinder.TryFind<FillRectPanelUI>("../Material", out materialFillRect))
			showMaterial = materialFillRect.gameObject.activeSelf;
		if (electricFillRect.IsNotNullRef() || ThisUIFinder.TryFind<FillRectPanelUI>("../Electric", out electricFillRect))
			showElectric = electricFillRect.gameObject.activeSelf;

		RePainting();
	}

	internal void ClearUI()
	{
	}

	internal void Release()
	{
		unitCard = null;

		titleImage = null;
		titleText = null;
		shieldFillRect = null;
		materialFillRect = null;
		electricFillRect = null;
	}

	internal void RePainting()
	{
		RePainting_FillRect(shieldFillRect, ref showShield, unitCard.GetShieldSimpleValue());
		RePainting_FillRect(materialFillRect, ref showMaterial, unitCard.GetMaterialSimpleValue());
		RePainting_FillRect(electricFillRect, ref showElectric, unitCard.GetElectricSimpleValue());

		static void RePainting_FillRect(FillRectPanelUI fillRect, ref bool isShow, (float total, float max) value)
		{
			if (fillRect.IsNotNullRef())
			{
				var (total, max) = value;
				bool nextShow = max > 0f;
				if (isShow != nextShow)
				{
					isShow = nextShow;
					fillRect.gameObject.SetActive(nextShow);
				}
				if (!isShow) return;

				fillRect.MinMax = new Vector2(0f, max);
				fillRect.Value = new Vector2(0f, total);
			}
		}
	}
}
