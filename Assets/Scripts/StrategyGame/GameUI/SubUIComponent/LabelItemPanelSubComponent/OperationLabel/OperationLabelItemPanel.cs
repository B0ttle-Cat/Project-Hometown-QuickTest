using GameUI;

using UnityEngine;


[RequireComponent(typeof(LabelItemElementReferrer))]
public class OperationLabelItemPanel : LabelItemPanelComponent
{
	private LabelItemElementReferrer referrer;
	public LabelItemElementReferrer Referrer => referrer.IsNotNullRef() ? referrer : TryGetComponent<LabelItemElementReferrer>(out referrer) ? referrer : null;
	public OperationObject Operation { get; private set; }
	private bool isShow;
	private bool isShowDetail;

	protected override void OnReleaseUI()
	{
		if (Operation.IsNotNullRef())
		{

			Operation = null;
		}

		if (referrer.IsNotNullRef())
		{
			referrer.Clear();
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

		isShow = false;
		isShowDetail = false;
		ThisShowHide.OnHideImmediate();
		referrer.ShowSimpleElement();
	}

	private void OnClickLabel()
	{
		if (Operation.IsNullRef()) return;
		if (Operation is not ISelectable selectable) return;

		StrategyManager.Selecter.OnSystemSelectToggleObject(selectable);
	}

	protected override void OnChangedUI()
    {
	}
	private bool IsShowLabel()
	{
		if (StrategyManager.PlayerFactionID == Operation.FactionID)
		{
			return true;
		}
		return false;
	}
	private bool IsShowDetail()
	{
		if (StrategyManager.GameUX.OnKey_ShowDetail)
		{
			return true;
		}
		else if (StrategyManager.PlayerFactionID == Operation.FactionID)
		{
			return true;
		}
		return false;
	}
	protected virtual void LateUpdate()
	{
		if (Operation.IsNullRef()) return;
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
