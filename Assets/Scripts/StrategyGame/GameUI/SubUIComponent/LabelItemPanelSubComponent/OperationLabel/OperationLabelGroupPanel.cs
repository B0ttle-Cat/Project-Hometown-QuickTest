using GameUI;

using Sirenix.Utilities;

using UnityEngine;

public class OperationLabelGroupPanel : PanelGroupComponent<OperationLabelItemPanel>
{
	Faction playerFaction;
	[SerializeField]
	private int positionItemPriority;

	internal void SetPlayerFaction(Faction faction)
	{
		if (playerFaction.IsNotNullRef())
		{
			Deinit();
		}
		playerFaction = faction;
		if (playerFaction.IsNotNullRef())
		{
			Init();
		}
	}

	private void Init()
	{
		InitObjects(StrategyManager.Collector.GetList<OperationObject>()
			.Convert<ITargetToPanelAPI>(i => i as ITargetToPanelAPI));
		StrategyManager.Collector.AddChangeListener<OperationObject>(ChangeValue, false);
		//InitObjects(playerFaction.OperationList.ForPanel);
		//playerFaction.AddChangeOperation(ChangeValue, false);
	}
	private void Deinit()
	{
		Clear();
	}
	private void ChangeValue(IStrategyElement element, bool added)
	{
		if (element is not OperationObject operation) return;

        if (added)
        {
			AddObject(operation);
        }
		else 
		{
			RemoveObject(operation);
		}
	}
	protected override bool SetPanelObject(OperationLabelItemPanel newPanel, ITargetToPanelAPI item)
	{
		if (base.SetPanelObject(newPanel, item))
		{
			if (!newPanel.gameObject.TryGetComponent<LabelPositionItem>(out var positionItem))
			{
				positionItem = newPanel.gameObject.AddComponent<LabelPositionItem>();
			}
			newPanel.PositionItem = positionItem;
			positionItem.Priority = positionItemPriority;
			return true;
		}
		return false;
	}
}
