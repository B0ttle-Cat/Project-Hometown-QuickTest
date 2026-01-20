using GameUI;

using static StrategyGamePlayData;

public class SectorCardItemPanel : CardItemPanelComponent
{
	private CardItemElementReferrer referrer;
	private CardItemElementReferrer Referrer => referrer.IsNotNullRef() ? referrer : TryGetComponent<CardItemElementReferrer>(out referrer) ? referrer : null;
	public SectorObject CardTarget { get; private set; }
	protected override void OnReleaseUI()
	{
		if (CardTarget.IsNotNullRef())
		{
			if (CardTarget is IDurabilityValue durability)
			{
				durability.OnChangeDurability -= OnChangeDurability;
			}
			if (CardTarget is ISupplyStats supplyStats)
			{
				supplyStats.OnSupplyChange -= OnSupplyChange;
			}
			CardTarget = null;
		}
		if (referrer.IsNotNullRef())
		{
			referrer.Clear();
			referrer = null;
		}
	}
	protected override void OnAttachUI(ITargetToPanelAPI item)
	{
		if (item is not SectorObject sector) return;
		CardTarget = sector;
		if (CardTarget.IsNullRef()) return;

		referrer = Referrer;
		if (referrer.IsNullRef()) return;

		referrer.OnClickAddListener(OnClickCard);

		if (CardTarget is ITargetToCardAPI cardAPI)
		{
			referrer.SetTitleImage(cardAPI.GetCardImage());
			referrer.SetTItleText(cardAPI.GetCardName());
		}

		referrer.SetShildFillAmount(0,0);
		referrer.SetPersonnelFillAmount(0, 0);
		referrer.SetMaterialFillAmount(0, 0);
		referrer.SetElectricFillAmount(0, 0);

		referrer.OnClickAddListener(() =>
		{
			StrategyManager.Selecter.OnSystemSelectObject(CardTarget);
		});

		if (CardTarget is IDurabilityValue durability)
		{
			durability.OnChangeDurability -= OnChangeDurability;
			durability.OnChangeDurability += OnChangeDurability;
		}
		if (CardTarget is ISupplyStats supplyStats)
		{
			supplyStats.OnSupplyChange -= OnSupplyChange;
			supplyStats.OnSupplyChange += OnSupplyChange;
		}
	}
	private void OnClickCard()
	{
		if (CardTarget.IsNullRef()) return;
		if (CardTarget is not ISelectable selectable) return;

		StrategyManager.Selecter.OnSystemSelectToggleObject(CardTarget);
	}
	private void OnChangeDurability(IDurabilityValue durability)
	{
		if (durability.IsNullRef()) return;
		if (referrer.IsNullRef()) return;

		referrer.SetShildFillAmount(durability.CurrentDurability, durability.MaxDurability);
	}
	private void OnSupplyChange(ISupplyStats supplyStats)
	{
		if (supplyStats.IsNullRef()) return;
		if (referrer.IsNullRef()) return;


		referrer.SetPersonnelFillAmount(supplyStats.GetPersonnelSimpleValue());
		referrer.SetMaterialFillAmount(supplyStats.GetMaterialSimpleValue());
		referrer.SetElectricFillAmount(supplyStats.GetElectricSimpleValue());
		
	}
	protected override void OnUpdateUI()
	{
		if (CardTarget.IsNullRef()) return;
		OnSupplyChange(CardTarget);
	}
}
