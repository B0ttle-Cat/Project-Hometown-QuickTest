using System.Collections.Generic;

using GameUI;

using Sirenix.OdinInspector;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class OperationCardItemPanel : PanelItemComponent, IFindUIObject
{
	private IOperationCardUIObject operationCard;
	public IFindUIObject ThisUIFinder => this;
	[SerializeField, PropertyOrder(-90)] private List<IFindUIObject.KeyPairObject> keyPairs;
	List<IFindUIObject.KeyPairObject> IFindUIObject.KeyPairs { get => keyPairs; set => keyPairs = value; }


	private Image titleImage;
	private TMP_Text titleText;
	private bool showPersonnel;
	private bool showMaterial;
	private bool showElectric;
	private FillRectPanelUI personnelFillRect;
	private FillRectPanelUI materialFillRect;
	private FillRectPanelUI electricFillRect;

	internal void Attach(IOperationCardUIObject item)
	{
		operationCard = item;

		if (titleImage.IsNotNullRef() || ThisUIFinder.TryFind<Image>("..TitleImage", out titleImage))
			titleImage.sprite = operationCard.GetTitleImage();

		if (titleText.IsNotNullRef() || ThisUIFinder.TryFind<TMP_Text>("..TitleText", out titleText))
			titleText.text = operationCard.GetTitleName();

		showPersonnel = false;
		showMaterial = false;
		showElectric = false;

		if (personnelFillRect.IsNotNullRef() || ThisUIFinder.TryFind<FillRectPanelUI>("../Personnel", out personnelFillRect))
			showPersonnel = personnelFillRect.gameObject.activeSelf;
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
		operationCard = null;

		titleImage = null;
		titleText = null;
		personnelFillRect = null;
		materialFillRect = null;
		electricFillRect = null;
	}

	internal void RePainting()
	{
		RePainting_FillRect(personnelFillRect, ref showPersonnel, operationCard.GetPersonnelSimpleValue());
		RePainting_FillRect(materialFillRect, ref showMaterial, operationCard.GetMaterialSimpleValue());
		RePainting_FillRect(electricFillRect, ref showElectric, operationCard.GetElectricSimpleValue());

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
