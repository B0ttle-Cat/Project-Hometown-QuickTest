using System.Collections.Generic;

using GameUI;

using Sirenix.OdinInspector;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

using static StrategyGamePlayData;

public class SectorCardItemPanel : CardItemPanelComponent, IFindUIObject
{
	private ISectorToPanelAPI sectorCard;
	public IFindUIObject ThisUIFinder => this;
	[SerializeField, PropertyOrder(-90)] private List<IFindUIObject.KeyPairObject> keyPairs;
	List<IFindUIObject.KeyPairObject> IFindUIObject.KeyPairs { get => keyPairs; set => keyPairs = value; }

	private Image titleImage;
	private TMP_Text titleText;
	private bool showShield;
	private bool showPersonnel;
	private bool showMaterial;
	private bool showElectric;
	private FillRectPanelUI shieldFillRect;
	private FillRectPanelUI personnelFillRect;
	private FillRectPanelUI materialFillRect;
	private FillRectPanelUI electricFillRect;

	protected override void OnAttachUI(ITargetToPanelAPI item)
	{
		if (item is not ISectorToPanelAPI sectorCard) return;
		this.sectorCard = sectorCard;

		if (titleImage.IsNotNullRef() || ThisUIFinder.TryFind<Image>("..TitleImage", out titleImage))
			titleImage.sprite = sectorCard.GetCardImage();

		if (titleText.IsNotNullRef() || ThisUIFinder.TryFind<TMP_Text>("..TitleText", out titleText))
			titleText.text = sectorCard.GetCardName();

		showShield = false;
		showPersonnel = false;
		showMaterial = false;
		showElectric = false;

		if (shieldFillRect.IsNotNullRef() || ThisUIFinder.TryFind<FillRectPanelUI>("../Shield", out shieldFillRect))
			showShield = shieldFillRect.gameObject.activeSelf;
		if (personnelFillRect.IsNotNullRef() || ThisUIFinder.TryFind<FillRectPanelUI>("../Personnel", out personnelFillRect))
			showPersonnel = personnelFillRect.gameObject.activeSelf;
		if (materialFillRect.IsNotNullRef() || ThisUIFinder.TryFind<FillRectPanelUI>("../Material", out materialFillRect))
			showMaterial = materialFillRect.gameObject.activeSelf;
		if (electricFillRect.IsNotNullRef() || ThisUIFinder.TryFind<FillRectPanelUI>("../Electric", out electricFillRect))
			showElectric = electricFillRect.gameObject.activeSelf;

		if (sectorCard is ISupplyStats supplyStats)
		{
			supplyStats.OnSupplyChange -= UpdateSupplyStats;
			supplyStats.OnSupplyChange += UpdateSupplyStats;
		}

	}
	protected override void OnReleaseUI()
	{
		if (sectorCard.IsNotNullRef())
		{
			if (sectorCard is ISupplyStats supplyStats)
			{
				supplyStats.OnSupplyChange -= UpdateSupplyStats;
			}
			sectorCard = null;
		}

		titleImage = null;
		titleText = null;
		shieldFillRect = null;
		personnelFillRect = null;
		materialFillRect = null;
		electricFillRect = null;
	}

	private void UpdateSupplyStats(ISupplyStats supplyStats)
	{
		OnUpdateUI();
	}


	protected override void OnUpdateUI()
	{
		if (sectorCard.IsNullRef()) return;

		RePainting_FillRect(shieldFillRect, ref showShield, sectorCard.GetShieldSimpleValue());
		RePainting_FillRect(personnelFillRect, ref showPersonnel, sectorCard.GetPersonnelSimpleValue());
		RePainting_FillRect(materialFillRect, ref showMaterial, sectorCard.GetMaterialSimpleValue());
		RePainting_FillRect(electricFillRect, ref showElectric, sectorCard.GetElectricSimpleValue());

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
