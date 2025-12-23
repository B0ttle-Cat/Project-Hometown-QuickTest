using GameUI;

using UnityEngine;

using static StrategyGamePlayData;

public class StrategyResourcesView : PanelItemComponent
{
	private ISupplyStats supplyTarget;

	[SerializeField]
	private FillRectAndLabel personnel;
	[SerializeField]
	private FillRectAndLabel material;
	[SerializeField]
	private FillRectAndLabel electric;

	[SerializeField]
	private IShowHide personnelShowHide;
	[SerializeField]
	private IShowHide materialShowHide;
	[SerializeField]
	private IShowHide electricShowHide;


	public override IPanelItem ThisPanel => this;
	public override IShowHideAsync ThisShowHide => this;

    public void SetTarget(ISupplyStats target)
	{
		if(supplyTarget == target) return;

		if (supplyTarget.IsNotNullRef())
		{
			supplyTarget.OnSupplyChange -= OnSupplyChange;
		}

		supplyTarget = target;

		if (target.IsNotNullRef())
		{
			target.OnSupplyChange -= OnSupplyChange;
			target.OnSupplyChange += OnSupplyChange;
			OnSupplyChange(target);
		}
	}

    protected override void Hide()
    {
		if (personnelShowHide.IsNullRef())
			personnelShowHide = personnel.GetComponent<IShowHide>();
		if (personnelShowHide.IsNotNullRef())
			personnelShowHide.OnHide();

		if (materialShowHide.IsNullRef())
			materialShowHide = material.GetComponent<IShowHide>();
		if (materialShowHide.IsNotNullRef())
			materialShowHide.OnHide();

		if (electricShowHide.IsNullRef())
			electricShowHide = electric.GetComponent<IShowHide>();
		if (electricShowHide.IsNotNullRef())
			electricShowHide.OnHide();
	}

    protected override void Show()
    {
		if(personnelShowHide.IsNullRef())
			personnelShowHide = personnel.GetComponent<IShowHide>();
		if (personnelShowHide.IsNotNullRef())
			personnelShowHide.OnShow();

		if (materialShowHide.IsNullRef())
			materialShowHide = material.GetComponent<IShowHide>();
		if (materialShowHide.IsNotNullRef())
			materialShowHide.OnShow();

		if (electricShowHide.IsNullRef())
			electricShowHide = electric.GetComponent<IShowHide>();
		if (electricShowHide.IsNotNullRef())
			electricShowHide.OnShow();
	}

    private void OnSupplyChange(ISupplyStats statsValue)
	{
		if (personnel != null)
		{
			int max   = statsValue.GetStatsValue(StatsType.자원_인력_최대);
			int local = statsValue.GetStatsValue(StatsType.자원_인력_현재);
			float ratio = (float)local / (float)max;
			personnel.SetValueText(ratio, $"{local}/{max}");
		}
		if (material != null)
		{
			int max   = statsValue.GetStatsValue(StatsType.자원_재료_최대);
			int local = statsValue.GetStatsValue(StatsType.자원_재료_현재);
			float ratio = (float)local / (float)max;
			material.SetValueText(ratio, $"{local}/{max}");
		}
		if (electric != null)
		{
			int max   = statsValue.GetStatsValue(StatsType.자원_전력_최대);
			int local = statsValue.GetStatsValue(StatsType.자원_전력_현재);
			float ratio = (float)local / (float)max;
			electric.SetValueText(ratio, $"{local}/{max}");
		}
	}

}
