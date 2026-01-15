using GameUI;

using Sirenix.Utilities;

using UnityEngine;

public class SectorLabelGroupPanel : PanelGroupComponent<SectorLabelItemPanel>
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
		InitObjects(StrategyManager.Collector.GetList<SectorObject>()
			.Convert<ITargetToPanelAPI>(i => i as ITargetToPanelAPI));
	}
	private void Deinit()
	{
		Clear();
	}
	private void ChangeValue(IStrategyElement element, bool added)
	{
		if (element is not SectorObject sector) return;

		if (added)
		{
			AddObject(sector);
		}
		else
		{
			RemoveObject(sector);
		}
	}


	protected override bool SetPanelObject(SectorLabelItemPanel newPanel, ITargetToPanelAPI item)
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
