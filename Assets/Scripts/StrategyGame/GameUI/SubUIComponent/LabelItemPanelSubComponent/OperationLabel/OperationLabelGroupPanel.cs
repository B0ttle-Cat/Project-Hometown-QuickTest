using GameUI;

using Sirenix.Utilities;

public class OperationLabelGroupPanel : PanelGroupComponent<OperationLabelItemPanel>
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
}
