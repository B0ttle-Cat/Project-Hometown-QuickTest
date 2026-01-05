using GameUI;

using static StrategyGamePlayData;

public class OperationCardItemPanel : CardItemPanelComponent
{
	private CardItemElementReferrer referrer;
	private CardItemElementReferrer Referrer => referrer.IsNotNullRef() ? referrer : TryGetComponent<CardItemElementReferrer>(out referrer) ? referrer : null;
	public OperationObject CardTarget { get; private set; }
	protected override void OnReleaseUI()
	{
		if (CardTarget.IsNotNullRef())
		{
			if (CardTarget is IPersonnelStats personnel)
			{

			}
			CardTarget = null;
		}
		if (referrer.IsNotNullRef())
		{

			referrer = null;
		}
	}
	protected override void OnAttachUI(ITargetToPanelAPI item)
	{
		if (item is not OperationObject operation) return;
		CardTarget = operation;
		if (CardTarget.IsNullRef()) return;

		referrer = Referrer;
		if (referrer.IsNullRef()) return;



		if (CardTarget is ITargetToCardAPI cardAPI)
		{
			referrer.SetTitleImage(cardAPI.GetCardImage());
			referrer.SetTItleText(cardAPI.GetCardName());
		}
		if (CardTarget is IPersonnelStats personnel)
		{
		}

		referrer.SetShildFillAmount(0,0);
		referrer.SetPersonnelFillAmount(0, 0);
		referrer.SetMaterialFillAmount(0, 0);
		referrer.SetElectricFillAmount(0, 0);
	}

	private void OnChangePersonnel(IPersonnelStats personnel)
	{
		if (personnel.IsNullRef()) return;
		if (referrer.IsNullRef()) return;

		referrer.SetPersonnelFillAmount(personnel.GetPersonnelSimpleValue());
	}
	protected override void OnUpdateUI()
	{
		if (CardTarget.IsNullRef()) return;
	}
}
