using GameUI;

using Sirenix.Utilities;

public class UnitLabelGroupPanel : PanelGroupComponent<UnitLabelItemPanel>
{
	Faction playerFaction;

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
		InitObjects(StrategyManager.Collector.GetList<UnitObject>()
			.Convert<ITargetToPanelAPI>(i => i as ITargetToPanelAPI));
	}
	private void Deinit()
	{
		Clear();
	}
	private void ChangeValue(IStrategyElement element, bool added)
	{
		if (element is not UnitObject sector) return;

        if (added)
        {
			AddObject(sector);
        }
		else 
		{
			RemoveObject(sector);
		}
	}
}
