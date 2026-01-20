using GameUI;

using UnityEngine;


public class UnitLabelItemPanel : LabelItemPanelComponent
{
	private LabelItemElementReferrer referrer;
	public LabelItemElementReferrer Referrer => referrer.IsNotNullRef() ? referrer : TryGetComponent<LabelItemElementReferrer>(out referrer) ? referrer : null;
	public UnitObject Unit { get; private set; }
	private bool isShow;
	private bool isShowDetail;
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
			referrer.Clear();
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
		isShow = false;
		isShowDetail = false;
		ThisShowHide.OnHideImmediate();
		referrer.ShowSimpleElement();
	}
	private void OnClickLabel()
	{
		if (Unit.IsNullRef()) return;
		if (Unit is not ISelectable selectable) return;

		StrategyManager.Selecter.OnSystemSelectToggleObject(Unit);
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

	private bool IsShowLabel()
	{
		if (StrategyManager.PlayerFactionID == Unit.FactionID)
		{
			if (Unit.OperationID < 0)
			{
				return false;
			}
			else if (Unit.ThisDestroyer.IsDestroy)
			{
				return false;
			}
		}
		return true;
	}
	private bool IsShowDetail()
	{
		if (StrategyManager.PlayerFactionID == Unit.FactionID)
		{
			if (StrategyManager.GameUX.OnKey_ShowDetail)
			{
				return true;
			}
		}
		return false;
	}
	protected virtual void LateUpdate()
	{
		if (Unit.IsNullRef()) return;
		if (referrer.IsNullRef()) return;

		bool nextShow = IsShowLabel();

		if (isShow == nextShow)
		{
			if (!isShow)
			{
				return;
			}
		}
		else
		{
			isShow = nextShow;
			if (isShow)
			{
				ThisShowHide.OnShow();
			}
			else
			{
				ThisShowHide.OnHide();
			}
		}

		bool nextShowDetail = IsShowDetail();
		if (isShowDetail == nextShowDetail)
		{
			return;
		}
		else
		{
			isShowDetail = nextShowDetail;
			if (isShowDetail) referrer.ShowDetailElement();
			else referrer.ShowSimpleElement();
		}
	}
}
