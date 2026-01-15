using GameUI;

using Sirenix.Utilities;

using UnityEngine;

public class UnitLabelGroupPanel : PanelGroupComponent<UnitLabelItemPanel>
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
		InitObjects(StrategyManager.Collector.GetList<UnitObject>().Convert(i => i as ITargetToPanelAPI));
		StrategyManager.Collector.AddChangeListener<UnitObject>(ChangeValue, false);
	}
	private void Deinit()
	{
		StrategyManager.Collector.RemoveChangeListener<UnitObject>(ChangeValue);
		Clear();
	}
	private void ChangeValue(UnitObject element, bool added)
	{
        if (added)
        {
			AddObject(element);
        }
		else 
		{
			RemoveObject(element);
		}
	}
	protected override bool SetPanelObject(UnitLabelItemPanel newPanel, ITargetToPanelAPI item)
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
