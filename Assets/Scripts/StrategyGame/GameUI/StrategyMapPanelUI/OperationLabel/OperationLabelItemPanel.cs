using System.Collections.Generic;

using GameUI;

using Sirenix.OdinInspector;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

using static StrategyGamePlayData;


public class OperationLabelItemPanel : LabelItemPanelComponent, IFindUIObject, ISetTargetPanel
{
	public IFindUIObject ThisUIFinder => this;
	[SerializeField, PropertyOrder(-90)] private List<IFindUIObject.KeyPairObject> keyPairs;
	List<IFindUIObject.KeyPairObject> IFindUIObject.KeyPairs { get => keyPairs; set => keyPairs = value; }
	public IOperationForPanel Operation { get; private set; }

	private Image iconImage;
	private Button select;
	private TMP_Text displayText;
	private FillRectPanelUI personnel;
	private FillRectPanelUI material;
	private FillRectPanelUI electric;

	protected override void OnReleaseUI()
	{

	}

	protected override void OnAttachUI(ITargetToPanelAPI target)
	{
		if (target is not IOperationForPanel operation) return;
		Operation = operation;

		if (iconImage.IsNotNullRef() || ThisUIFinder.TryFind<Image>("..Icon", out iconImage))
			iconImage.sprite = Operation.GetLabelIcon();

		if (displayText.IsNotNullRef() || ThisUIFinder.TryFind<TMP_Text>("..TitleText", out displayText))
			displayText.text = Operation.GetLabelName();

		if (personnel.IsNotNullRef() || ThisUIFinder.TryFind<FillRectPanelUI>("../Personnel", out personnel)) { }
		if (material.IsNotNullRef() || ThisUIFinder.TryFind<FillRectPanelUI>("../Material", out material)) { }
		if (electric.IsNotNullRef() || ThisUIFinder.TryFind<FillRectPanelUI>("../Electric", out electric)) { }

		if (Operation is ISupplyStats supplyStats)
		{
			supplyStats.OnSupplyChange -= UpdateSupplyStats;
			supplyStats.OnSupplyChange += UpdateSupplyStats;
		}
	}
	private void UpdateSupplyStats(ISupplyStats supplyStats)
	{
		OnUpdateUI();
	}

	protected override void OnUpdateUI()
	{
		if (Operation.IsNullRef()) return;

		FillRect(personnel, Operation.GetPersonnelSimpleValue());
		FillRect(material, Operation.GetMaterialSimpleValue());
		FillRect(electric, Operation.GetMaterialSimpleValue());

		void FillRect(FillRectPanelUI fillRect, (float total, float max) value)
		{
			if(fillRect.IsNullRef()) return;
			fillRect.MinMax = new Vector2(0, value.max);
			fillRect.Value = new Vector2(0, value.total);
		}
	}
}
