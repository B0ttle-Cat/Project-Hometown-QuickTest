using GameUI;

using UnityEngine;


public class UnitLabelItemPanel : LabelItemPanelComponent
{
	private LabelItemElementReferrer referrer;
	public LabelItemElementReferrer Referrer => referrer.IsNotNullRef() ? referrer : TryGetComponent<LabelItemElementReferrer>(out referrer) ? referrer : null;
	public UnitObject Unit { get; private set; }

	protected override void OnReleaseUI()
	{
		if (Unit.IsNotNullRef())
		{
			if (Unit is IDurabilityValue durability)
			{
				durability.OnChangeDurability -= OnChangeDurability;
				durability.OnChangeDurability += OnChangeDurability;
			}
			Unit = null;
		}

		if (referrer.IsNotNullRef())
		{
			referrer.OnClickRemoveListener(OnClickLabel);
			referrer = null;
		}
	}

	protected override void OnAttachUI(ITargetToPanelAPI target)
	{
		if (target is not UnitObject unit) return;
		Unit = unit;
		if (Unit.IsNullRef()) return;

		referrer = Referrer;
		if (referrer.IsNullRef()) return;

		referrer.OnClickAddListener(OnClickLabel);

		if (Unit is ITargetToLabelAPI labelAPI)
		{
			referrer.SetMainIcon(labelAPI.GetLabelIcon());
			referrer.SetSubIcon(labelAPI.GetLabelIcon());
			referrer.SetDisplayText(labelAPI.GetLabelName());
			referrer.SetAccentColor(labelAPI.GetLabelAccentColor());
			referrer.SetTextColor(labelAPI.GetLabelTextColor());
		}

		if (Unit is IDurabilityValue durability)
		{
			durability.OnChangeDurability -= OnChangeDurability;
			durability.OnChangeDurability += OnChangeDurability;
		}

		referrer.SetPersonnelFillAmount(0);
		referrer.SetMaterialFillAmount(0);
		referrer.SetElectricFillAmount(0);
	}
	private void OnClickLabel()
	{

	}
	private void OnChangeDurability(IDurabilityValue durability)
	{
		if (referrer.IsNullRef()) return;
		float max = durability.MaxDurability;
		float current = durability.CurrentDurability;
		float ratio = max <= 0 ? 0 : (current /max);
		referrer.SetShieldFillAmount(Mathf.Clamp01(ratio * 0.5f));
	}

	protected override void OnUpdateUI()
	{
		if (Unit.IsNullRef()) return;

		OnChangeDurability(Unit);
	}
}
