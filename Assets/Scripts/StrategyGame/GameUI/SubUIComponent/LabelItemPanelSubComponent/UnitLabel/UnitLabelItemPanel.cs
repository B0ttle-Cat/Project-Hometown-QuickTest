using GameUI;

using UnityEngine;
using UnityEngine.InputSystem;


public class UnitLabelItemPanel : LabelItemPanelComponent
{
	private LabelItemElementReferrer referrer;
	public LabelItemElementReferrer Referrer => referrer.IsNotNullRef() ? referrer : TryGetComponent<LabelItemElementReferrer>(out referrer) ? referrer : null;
	public UnitObject Unit { get; private set; }
	private bool? isShowDetail;
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

		referrer.SetPersonnelFillAmount(0, -1);
		referrer.SetMaterialFillAmount(0, -1);
		referrer.SetElectricFillAmount(0, -1);
		isShowDetail = true;
		referrer.ShowDetailElement();
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
		ratio = Mathf.Clamp01(ratio);
		referrer.SetShieldFillAmount(ratio);
		referrer.SetSimpleFillAmount(ratio);
	}
	protected override void OnChangedUI()
	{
		if (Unit.IsNullRef()) return;

		OnChangeDurability(Unit);
	}
	protected virtual void LateUpdate()
	{
		if (Unit.IsNullRef()) return;
		if (referrer.IsNullRef()) return;

		bool showSimpleKey = Keyboard.current.altKey.isPressed;
		
		if (StrategyManager.PlayerFactionID == Unit.FactionID || showSimpleKey)
		{
			if (!(isShowDetail ?? false))
			{
				isShowDetail = true;
				referrer.ShowDetailElement();
			}
		}
		else
		{
			if (isShowDetail ?? true)
			{
				isShowDetail = false;
				referrer.ShowSimpleElement();
			}
		}
	}
}
