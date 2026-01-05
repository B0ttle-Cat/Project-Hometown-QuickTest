using GameUI;

using UnityEngine;

public class StrategyMapPanelUI : GameUIController
{
	[SerializeField]
	private Camera uiCamera;

	SectorLabelGroupPanel sectorLabelGroup;
	OperationLabelGroupPanel operationLabelGroup;
	UnitLabelGroupPanel unitLabelGroup;

	protected override void Hide()
	{
		DeinitLabelGroupPanel();

		int count = ThisUIFinder.TryFinds<IShowHide>(out var finds);
		for (int i = 0 ; i < count ; i++)
		{
			finds[i].OnHide();
		}
	}
	protected override void Show()
	{
		InitLabelGroupPanel();

		int count = ThisUIFinder.TryFinds<IShowHide>(out var finds);
		for (int i = 0 ; i < count ; i++)
		{
			finds[i].OnShow();
		}
	}
	private void InitLabelGroupPanel()
	{
		Faction playerFaction = FactionAPI.ID2Faction(StrategyManager.PlayerFactionID);

		if (ThisUIFinder.TryFind<SectorLabelGroupPanel>(out sectorLabelGroup))
			sectorLabelGroup.SetPlayerFaction(playerFaction);

		if (ThisUIFinder.TryFind<OperationLabelGroupPanel>(out operationLabelGroup))
			operationLabelGroup.SetPlayerFaction(playerFaction);

		if (ThisUIFinder.TryFind<UnitLabelGroupPanel>(out unitLabelGroup))
			unitLabelGroup.SetPlayerFaction(playerFaction);

	}
	private void DeinitLabelGroupPanel()
	{
		if (sectorLabelGroup.IsNotNullRef())
			sectorLabelGroup.SetPlayerFaction(null);

		if (operationLabelGroup.IsNotNullRef())
			operationLabelGroup.SetPlayerFaction(null);

		if (unitLabelGroup.IsNotNullRef())
			unitLabelGroup.SetPlayerFaction(null);
	}


	private void LateUpdate()
	{
		if (uiCamera.IsNullRef()) return;
	
		int length = 0;
		if (sectorLabelGroup.IsNotNullRef())
		{
			length = sectorLabelGroup.Count;
            for (int i = 0 ; i < length ; i++)
            {
				var item = sectorLabelGroup[i];
				item.UpdateLabelPosition(uiCamera);
			}
        }

		if (operationLabelGroup.IsNotNullRef())
		{
			length = operationLabelGroup.Count;
			for (int i = 0 ; i < length ; i++)
			{
				var item = operationLabelGroup[i];
				item.UpdateLabelPosition(uiCamera);
			}
		}

		if (unitLabelGroup.IsNotNullRef())
		{
			length = unitLabelGroup.Count;
			for (int i = 0 ; i < length ; i++)
			{
				var item = unitLabelGroup[i];
				item.UpdateLabelPosition(uiCamera);
			}
		}
	}
}
