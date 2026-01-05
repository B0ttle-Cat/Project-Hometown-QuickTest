using GameUI;

using UnityEngine;

using static StrategyGamePlayData;


public class SectorLabelItemPanel : LabelItemPanelComponent, ISetTargetPanel
{
	private LabelItemElementReferrer referrer;
	public LabelItemElementReferrer Referrer => referrer.IsNotNullRef() ? referrer : TryGetComponent<LabelItemElementReferrer>(out referrer) ? referrer : null;
	public SectorObject Sector { get; private set; }

	protected override void OnReleaseUI()
	{
		if (Sector.IsNullRef()) return;

		if (Sector is ICombatDefance defance)
		{
			defance.OnChangeDurability -= OnChangeDurability;
			defance.OnChangeDurability += OnChangeDurability;
		}

		if (Sector is ISupplyStats supplyStats)
		{
			supplyStats.OnSupplyChange -= UpdateSupplyStats;
			supplyStats.OnSupplyChange += UpdateSupplyStats;
		}

		Sector = null;
		referrer = null;
	}

	protected override void OnAttachUI(ITargetToPanelAPI target)
	{
		if (target is not SectorObject sector) return;
		Sector = sector;
		if (Sector.IsNullRef()) return;

		referrer = Referrer;
		if (referrer.IsNullRef()) return;

		if (Sector is ITargetToLabelAPI labelAPI)
		{
			referrer.SetMainIcon(labelAPI.GetLabelIcon());
			referrer.SetSubIcon(labelAPI.GetLabelIcon());
			referrer.SetDisplayText(labelAPI.GetLabelName());
			referrer.SetAccentColor(labelAPI.GetLabelAccentColor());
			referrer.SetTextColor(labelAPI.GetLabelTextColor());
		}

		if (Sector is ICombatDefance defance)
		{
			defance.OnChangeDurability -= OnChangeDurability;
			defance.OnChangeDurability += OnChangeDurability;
		}

		if (Sector is ISupplyStats supplyStats)
		{
			supplyStats.OnSupplyChange -= UpdateSupplyStats;
			supplyStats.OnSupplyChange += UpdateSupplyStats;
		}
	}

    private void OnChangeDurability((int current, int max) obj)
    {
		if (referrer.IsNullRef()) return;
		float ratio = obj.max <= 0 ? 0 : (obj.current / (float)obj.max);
		referrer.SetShieldFillAmount(Mathf.Clamp01(ratio * 0.5f));
	}

    private void UpdateSupplyStats(ISupplyStats supplyStats)
	{
		if (supplyStats.IsNullRef()) return;
		if (referrer.IsNullRef()) return;
		referrer.SetPersonnelFillAmount(FillAmount(supplyStats.GetPersonnelSimpleValue(), 0.5f));
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
		UpdateSupplyStats(Sector);
	}
}
