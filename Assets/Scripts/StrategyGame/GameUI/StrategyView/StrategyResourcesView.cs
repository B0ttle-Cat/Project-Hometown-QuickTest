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

	public override IPanelItem ThisPanel => this;


    public void SetTarget(ISupplyStats target)
	{
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
