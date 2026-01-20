using GameUI;

using UnityEngine;

using static StrategyGamePlayData;


public class SectorLabelItemPanel : LabelItemPanelComponent
{
	private LabelItemElementReferrer referrer;
	public LabelItemElementReferrer Referrer => referrer.IsNotNullRef() ? referrer : TryGetComponent<LabelItemElementReferrer>(out referrer) ? referrer : null;
	public SectorObject Sector { get; private set; }
	private bool isShow;
	private bool isShowDetail;

	protected override void OnReleaseUI()
	{
		if (Sector.IsNotNullRef())
		{

			if (Sector is IDurabilityValue durability)
			{
				durability.OnChangeDurability -= OnChangeDurability;
			}
			if (Sector is ISupplyStats supplyStats)
			{
				supplyStats.OnSupplyChange -= UpdateSupplyStats;
			}
			if (Sector is IChangeOrderFaction changeOrderFaction)
			{
				changeOrderFaction.OnChangeFaction -= OnChangeFaction;
			}
			Sector = null;
		}

		if (referrer.IsNotNullRef())
		{
			referrer.Clear();
			referrer = null;
		}
	}
	protected override void OnAttachUI(ITargetToPanelAPI target)
	{
		if (target is not SectorObject sector) return;
		Sector = sector;
		if (Sector.IsNullRef()) return;

		referrer = Referrer;
		if (referrer.IsNullRef()) return;
		
		referrer.OnClickAddListener(OnClickLabel);


		if (Sector is ITargetToLabelAPI labelAPI)
		{
			referrer.SetMainIcon(labelAPI.GetLabelIcon());
			referrer.SetSubIcon(labelAPI.GetLabelIcon());
			referrer.SetDisplayText(labelAPI.GetLabelName());
			referrer.SetAccentColor(labelAPI.GetLabelAccentColor());
			referrer.SetTextColor(labelAPI.GetLabelTextColor());
		}

		if (Sector is IDurabilityValue durability)
		{
			durability.OnChangeDurability -= OnChangeDurability;
			durability.OnChangeDurability += OnChangeDurability;
		}

		if (Sector is ISupplyStats supplyStats)
		{
			supplyStats.OnSupplyChange -= UpdateSupplyStats;
			supplyStats.OnSupplyChange += UpdateSupplyStats;
		}

		if (Sector is IChangeOrderFaction changeOrderFaction)
		{
			changeOrderFaction.OnChangeFaction -= OnChangeFaction;
			changeOrderFaction.OnChangeFaction += OnChangeFaction;
		}
		isShow = false;
		isShowDetail = false;
		ThisShowHide.OnHideImmediate();
		referrer.ShowSimpleElement();
	}
	private void OnClickLabel()
	{
		if (Sector.IsNullRef()) return;
		if (Sector is not ISelectable selectable) return;

		StrategyManager.Selecter.OnSystemSelectToggleObject(selectable);
	}
	private void OnChangeFaction(IStrategyElement element, int factionID)
	{
		if (StrategyManager.PlayerFactionID == factionID)
		{
			referrer.ShowDetailElement();

		}
		else
		{
			referrer.ShowSimpleElement();
		}
	}
	private void OnChangeDurability(IDurabilityValue durability)
	{
		if (durability.IsNullRef()) return;
		if (referrer.IsNullRef()) return;
		float current = durability.CurrentDurability;
		float max = durability.MaxDurability;
		float ratio = max <= 0 ? 0 : (current / max);
		float shield = Mathf.Clamp01(ratio);
		referrer.SetShieldFillAmount(shield);
		referrer.SetSimpleFillAmount(shield);
	}
	private void UpdateSupplyStats(ISupplyStats supplyStats)
	{
		if (supplyStats.IsNullRef()) return;
		if (referrer.IsNullRef()) return;
		referrer.SetPersonnelFillAmount(FillAmount(supplyStats.GetPersonnelSimpleValue()));
		referrer.SetMaterialFillAmount(FillAmount(supplyStats.GetMaterialSimpleValue()));
		referrer.SetElectricFillAmount(FillAmount(supplyStats.GetElectricSimpleValue()));

		float FillAmount((float total, float max) value)
		{
			float ratio = value.max <= 0 ? 0 : (value.total / value.max);
			return Mathf.Clamp01(ratio);
		}
	}
	protected override void OnChangedUI()
	{
		if (Sector.IsNullRef()) return;
		if (referrer.IsNullRef()) return;

		OnChangeDurability(Sector);
		UpdateSupplyStats(Sector);
		OnChangeFaction(Sector, Sector.CaptureFactionID);
	}
	private bool IsShowLabel()
	{
		return true;
	}
	private bool IsShowDetail()
	{
		if (StrategyManager.GameUX.OnKey_ShowDetail)
		{
			return true;
		}
		else if (StrategyManager.PlayerFactionID == Sector.CaptureFactionID)
		{
			return true;
		}
		return false;
	}
	protected virtual void LateUpdate()
	{
		if (Sector.IsNullRef()) return;
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