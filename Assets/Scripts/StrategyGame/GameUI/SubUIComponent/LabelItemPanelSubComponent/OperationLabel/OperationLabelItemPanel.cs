using GameUI;

using UnityEngine;


[RequireComponent(typeof(LabelItemElementReferrer))]
public class OperationLabelItemPanel : LabelItemPanelComponent
{
	private LabelItemElementReferrer referrer;
	public LabelItemElementReferrer Referrer => referrer.IsNotNullRef() ? referrer : TryGetComponent<LabelItemElementReferrer>(out referrer) ? referrer : null;
	public OperationObject Operation { get; private set; }

	protected override void OnReleaseUI()
	{
		if (Operation.IsNotNullRef())
		{

			Operation = null;
		}

		if (referrer.IsNotNullRef())
		{
			referrer.OnClickRemoveListener(OnClickLabel);
			referrer = null;
		}
	}

	protected override void OnAttachUI(ITargetToPanelAPI target)
	{
		if (target is not OperationObject operation) return;
		Operation = operation;
		if (Operation.IsNullRef()) return;

		referrer = Referrer;
		if (referrer.IsNullRef()) return;

		referrer.OnClickAddListener(OnClickLabel);

		if (Operation is ITargetToLabelAPI labelAPI)
		{
			referrer.SetMainIcon(labelAPI.GetLabelIcon());
			referrer.SetSubIcon(labelAPI.GetLabelIcon());
			referrer.SetDisplayText(labelAPI.GetLabelName());
			referrer.SetAccentColor(labelAPI.GetLabelAccentColor());
			referrer.SetTextColor(labelAPI.GetLabelTextColor());
		}

		referrer.SetShieldFillAmount(0,-1);
		referrer.SetPersonnelFillAmount(0, -1);
		referrer.SetMaterialFillAmount(0, -1);
		referrer.SetElectricFillAmount(0, -1);
	}

	private void OnClickLabel()
	{

	}

	protected override void OnUpdateUI()
    {
    }
}
