using GameUI;

using UnityEngine;

using static StrategyGamePlayData;

public class StrategyResourcesView : PanelItemComponent
{
	private ISupplyStats supplyTarget;

	[SerializeField]
	private FillRectMultiPanelUI personnel;
	[SerializeField]
	private FillRectMultiPanelUI material;
	[SerializeField]
	private FillRectMultiPanelUI electric;

	[SerializeField]
	private IShowHide personnelShowHide;
	[SerializeField]
	private IShowHide materialShowHide;
	[SerializeField]
	private IShowHide electricShowHide;

	public void SetTarget(ISupplyStats target)
	{
		if (supplyTarget == target) return;

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
		if (personnelShowHide.IsNullRef())
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
		if (personnel.IsNotNullRef())
		{
			(float[] values, float total, float max) = statsValue.GetPersonnelDetailValue();
			personnel.MinMax = new Vector2(0, max);
			personnel.SetValue(values);
		}
		if (material.IsNotNullRef())
		{
			(float[] values, float total, float max) = statsValue.GetMaterialDetailValue();
			material.MinMax = new Vector2(0, max);
			material.SetValue(values);
		}
		if (electric.IsNotNullRef())
		{
			(float[] values, float total, float max) = statsValue.GetElectricDetailValue();
			electric.MinMax = new Vector2(0, max);
			electric.SetValue(values);
		}
	}
}
