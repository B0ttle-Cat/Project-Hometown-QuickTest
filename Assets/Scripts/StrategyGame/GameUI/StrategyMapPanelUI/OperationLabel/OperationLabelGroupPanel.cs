using GameUI;

public class OperationLabelGroupPanel : PanelGroupComponent<OperationLabelItemPanel>
{
	Faction playerFaction;

	internal void SetTargetFaction(Faction faction)
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
		playerFaction.AddChangeOperation(ChangeValue, false);
		InitObjects(playerFaction.OperationList.ForPanel);
	}
	private void Deinit()
	{

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
