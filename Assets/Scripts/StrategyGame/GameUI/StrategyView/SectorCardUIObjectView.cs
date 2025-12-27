using System.Collections.Generic;

using GameUI;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class SectorCardUIObjectView : PanelItemComponent, IFindUIObject
{
	public IFindUIObject ThisUIFinder => this;
	[SerializeField]
	private List<IFindUIObject.KeyPairObject> keyPairs;
	private ISectorCardUIObject sectorCard;
	List<IFindUIObject.KeyPairObject> IFindUIObject.KeyPairs => keyPairs;

	public void SetUITarget(SectorObject sectorObject)
	{
		sectorCard = sectorObject;
	}

	protected override void Hide()
	{
	}

	protected override void Show()
	{
	}

	internal void Attach(ISectorCardUIObject item)
	{
		sectorCard = item;

		if (ThisUIFinder.TryFind<Image>("..TitleImage", out var titleImage))
		{
			titleImage.sprite = sectorCard.GetTitleImage();
		}
		if (ThisUIFinder.TryFind<TMP_Text>("..TitleText", out var titleText))
		{
			titleText.text = sectorCard.GetTitleName();
		}

		if (ThisUIFinder.TryFind<FillRectPanelUI>("../Personnel/FillRectPanelUI", out var personnelFillRect))
		{
			var (total, max) = sectorCard.GetPersonnelSimpleValue();
			personnelFillRect.MinMax = new Vector2(0f,max);
			personnelFillRect.Value = new Vector2(0f, total);
		}
		if (ThisUIFinder.TryFind<FillRectPanelUI>("../Material/FillRectPanelUI", out var materialFillRect))
		{
			var (total, max) = sectorCard.GetMaterialSimpleValue();
			materialFillRect.MinMax = new Vector2(0f, max);
			materialFillRect.Value = new Vector2(0f, total);
		}
		if (ThisUIFinder.TryFind<FillRectPanelUI>("../Electric/FillRectPanelUI", out var electricFillRect))
		{
			var (total, max) = sectorCard.GetElectricSimpleValue();
			electricFillRect.MinMax = new Vector2(0f, max);
			electricFillRect.Value = new Vector2(0f, total);
		}

		// TODO :: 이어서 작업 진행


	}

	internal void ClearUI()
	{
	}

	internal void Release()
	{
		sectorCard = null;
	}

	internal void RePating()
	{
	}
}
