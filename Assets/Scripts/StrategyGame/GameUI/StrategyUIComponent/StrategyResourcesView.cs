using GameUI;

using TMPro;

using UnityEngine;

using static StrategyGamePlayData;

public class StrategyResourcesView : PanelItemComponent, IShowHideAsync
{
	private ISupplyStats supplyTarget;

	[SerializeField]
	private FillRectMultiPanelUI personnel;
	[SerializeField]
	private FillRectMultiPanelUI material;
	[SerializeField]
	private FillRectMultiPanelUI electric;

	[SerializeField]
	private TMP_Text personnelDetailInfoText;
	[SerializeField]
	private TMP_Text materialDetailInfoText;
	[SerializeField]
	private TMP_Text electricDetailInfoText;


	private IShowHide personnelShowHide;
	private IShowHide materialShowHide;
	private IShowHide electricShowHide;


	private const string personnelStringFormat = "{0} {1..:(+0);(-0);#} / {max}";
	private const string materialStringFormat = "{..} / {max}";
	private const string electricStringFormat = "{..} / {max}";

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

	void IShowHide.EndedHide()
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


		if (personnel.IsNotNullRef())
		{
			personnel.TextFormat = personnelStringFormat;
		}
		if (material.IsNotNullRef())
		{
			material.TextFormat = materialStringFormat;
		}
		if (electric.IsNotNullRef())
		{
			electric.TextFormat = electricStringFormat;
		}
	}

	void IShowHide.StartShow()
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
			(int[] values, int total, int max) = statsValue.GetPersonnelDetailValue();
			personnel.MinMax = new Vector2(0, max);
			personnel.SetValue(values);
		}
		if (material.IsNotNullRef())
		{
			(int[] values, int total, int max) = statsValue.GetMaterialDetailValue();
			material.MinMax = new Vector2(0, max);
			material.SetValue(values);
		}
		if (electric.IsNotNullRef())
		{
			(int[] values, int total, int max) = statsValue.GetElectricDetailValue();
			electric.MinMax = new Vector2(0, max);
			electric.SetValue(values);
		}
	}
}
