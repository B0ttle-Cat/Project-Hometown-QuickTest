using GameUI;

public class UnitCardItemPanel : CardItemPanelComponent
{
	private CardItemElementReferrer referrer;
	private CardItemElementReferrer Referrer => referrer.IsNotNullRef() ? referrer : TryGetComponent<CardItemElementReferrer>(out referrer) ? referrer : null;

	public UnitObject CardTarget { get; private set; }

	protected override void OnAttachUI(ITargetToPanelAPI target)
	{
		if (target is not UnitObject unitCard) return;
		CardTarget = unitCard;
		if (CardTarget.IsNullRef()) return;

		referrer = Referrer;
		if (referrer.IsNullRef()) return;



		if (CardTarget is ITargetToCardAPI cardAPI)
		{
			referrer.SetTitleImage(cardAPI.GetCardImage());
			referrer.SetTItleText(cardAPI.GetCardName());
		}

		referrer.SetShildFillAmount(0,0);
		referrer.SetPersonnelFillAmount(0, 0);
		referrer.SetMaterialFillAmount(0, 0);
		referrer.SetElectricFillAmount(0, 0);

		if (CardTarget is IDurabilityValue durability)
		{
			durability.OnChangeDurability -= OnChangeDurability;
			durability.OnChangeDurability += OnChangeDurability;
		}
	}
	protected override void OnReleaseUI()
	{
		if (CardTarget.IsNotNullRef())
		{
			if (CardTarget is IDurabilityValue durability)
			{
				durability.OnChangeDurability -= OnChangeDurability;
			}
			CardTarget = null;
		}
		if (referrer.IsNotNullRef())
		{

			referrer = null;
		}
	}

	private void OnChangeDurability(IDurabilityValue durability)
	{
		if (durability.IsNullRef()) return;
		if (referrer.IsNullRef()) return;

		referrer.SetShildFillAmount(durability.CurrentDurability, durability.MaxDurability);
	}
	protected override void OnUpdateUI()
	{
		if (CardTarget.IsNullRef()) return;
		OnChangeDurability(CardTarget);
	}
	float FillRatio((float current, float max) value)
	{
		return Ratio(value.current, value.max);
	}
	private float Ratio(float current, float max)
	{
		if (max < 0) return 0;
		return current / max;
	}
}
