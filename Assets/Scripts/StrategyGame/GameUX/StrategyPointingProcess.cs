using System.Collections.Generic;

using StrategyManagerModule;

using UnityEngine;

public class StrategyPointingProcess : MonoBehaviour, IStrategyProcess
{
	StrategyMouseSelectComputer.MouseState RightLastState;

	void IStrategyProcess.OnStart()
	{
		StrategyManager.Selecter.OnSectorPointing += Selecter_OnSectorPointing;
		StrategyManager.Selecter.Mouse.OnChangeRightMouseState += Mouse_OnChangeRightMouseState;
		RightLastState = StrategyManager.Selecter.Mouse.RightSelecterState;
	}
	void IStrategyProcess.OnStop()
	{
		StrategyManager.Selecter.OnSectorPointing -= Selecter_OnSectorPointing;
	}
	private void Mouse_OnChangeRightMouseState(StrategyMouseSelectComputer.MouseState state, StrategyMouseSelectComputer.InputData mouseInputData)
	{
		if (state == StrategyMouseSelectComputer.MouseState.Released)
		{
			if (RightLastState == StrategyMouseSelectComputer.MouseState.Click)
			{
				IList<UnitObject> unitList = StrategyManager.Selecter.SelectUnit;
				int length = unitList.Count;
				for (int i = 0 ; i < length ; i++)
				{
					var item = unitList[i];
					item.ThisNavMovement.SetMovePath(mouseInputData.mouseCurrPosition);
				}
			}
		}
		RightLastState = state;
	}
	private void Selecter_OnSectorPointing(SectorObject sector)
	{
		IList<SectorObject> sectorList = StrategyManager.Selecter.SelectSector;

		int length = sectorList.Count;
		for (int i = 0 ; i < length ; i++)
		{
			var item = sectorList[i];
		}

		IList<UnitObject> unitList = StrategyManager.Selecter.SelectUnit;

		length = unitList.Count;
		for (int i = 0 ; i < length ; i++)
		{
			var item = unitList[i];
			item.ThisNodeMovement.SetMovePath(sector);
		}
	}
}
