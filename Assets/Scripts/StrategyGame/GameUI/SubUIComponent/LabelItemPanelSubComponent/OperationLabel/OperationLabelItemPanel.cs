using GameUI;

using UnityEngine;

using static StrategyGamePlayData;


[RequireComponent(typeof(LabelItemElementReferrer))]
public class OperationLabelItemPanel : LabelItemPanelComponent, ISetTargetPanel
{
	private LabelItemElementReferrer referrer;
	public LabelItemElementReferrer Referrer => referrer.IsNotNullRef() ? referrer : TryGetComponent<LabelItemElementReferrer>(out referrer) ? referrer : null;
	public OperationObject Operation { get; private set; }

	protected override void OnReleaseUI()
	{
		if (Operation.IsNullRef()) return;

		if (Operation is ISupplyStats supplyStats)
		{
			supplyStats.OnSupplyChange -= UpdateSupplyStats;
		}

		Operation = null;
		referrer = null;
	}

	protected override void OnAttachUI(ITargetToPanelAPI target)
	{
		if (target is not OperationObject operation) return;
		Operation = operation;
		if (Operation.IsNullRef()) return;

		referrer = Referrer;
		if (referrer.IsNullRef()) return;

		if (Operation is ITargetToLabelAPI labelAPI)
		{
			referrer.SetMainIcon(labelAPI.GetLabelIcon());
			referrer.SetSubIcon(labelAPI.GetLabelIcon());
			referrer.SetDisplayText(labelAPI.GetLabelName());
			referrer.SetAccentColor(labelAPI.GetLabelAccentColor());
			referrer.SetTextColor(labelAPI.GetLabelTextColor());
		}

		referrer.SetShieldFillAmount(0);

		if (Operation is ISupplyStats supplyStats)
		{
			supplyStats.OnSupplyChange -= UpdateSupplyStats;
			supplyStats.OnSupplyChange += UpdateSupplyStats;
		}
	}
	private void UpdateSupplyStats(ISupplyStats supplyStats)
	{
		if (supplyStats.IsNullRef()) return;

		referrer.SetPersonnelFillAmount(FillAmount(supplyStats.GetPersonnelSimpleValue(), 1f));
		referrer.SetMaterialFillAmount(FillAmount(supplyStats.GetMaterialSimpleValue(), 0.5f));
		referrer.SetElectricFillAmount(FillAmount(supplyStats.GetElectricSimpleValue(), 0.5f));

		float FillAmount((float total, float max) value, float fillScale)
		{
			float ratio = value.max <= 0 ? 0 : (value.total / value.max);
			return Mathf.Clamp01(ratio * fillScale);
		}
	}
	protected override void OnUpdateUI()
	{
		UpdateSupplyStats(Operation);
	}
}
