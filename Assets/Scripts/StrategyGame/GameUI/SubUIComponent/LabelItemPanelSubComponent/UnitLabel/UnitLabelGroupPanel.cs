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
			return true;
		}
		return false;
	}
}
