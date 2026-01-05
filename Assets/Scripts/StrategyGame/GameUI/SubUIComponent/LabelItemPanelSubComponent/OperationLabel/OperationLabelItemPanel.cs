using GameUI;

using UnityEngine;


[RequireComponent(typeof(LabelItemElementReferrer))]
public class OperationLabelItemPanel : LabelItemPanelComponent, ISetTargetPanel
{
	private LabelItemElementReferrer referrer;
	public LabelItemElementReferrer Referrer => referrer.IsNotNullRef() ? referrer : TryGetComponent<LabelItemElementReferrer>(out referrer) ? referrer : null;
	public OperationObject Operation { get; private set; }

	protected override void OnReleaseUI()
	{
		if (Operation.IsNullRef()) return;

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
		referrer.SetPersonnelFillAmount(0);
		referrer.SetMaterialFillAmount(0);
		referrer.SetElectricFillAmount(0);
	}

    protected override void OnUpdateUI()
    {
    }
}
