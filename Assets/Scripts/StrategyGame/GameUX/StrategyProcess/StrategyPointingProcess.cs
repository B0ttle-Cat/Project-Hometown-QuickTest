//using System.Collections.Generic;

//using UnityEngine;

//namespace StrategyManagerModule
//{
//	public partial class StrategyPointingProcess : IStrategyProcess
//	{
//		public IStrategyProcess ThisProcess => this;
//		public List<ProcessOverrider> OverriderList { get; set; } = new List<ProcessOverrider>();


//		void IStrategyProcess.OnStart()
//		{
//			StrategyManager.Selecter.OnSectorPointing += Selecter_OnSectorPointing;
//			StrategyManager.Selecter.Mouse.OnChangeRightMouseState += Mouse_OnChangeRightMouseState;
//			RightLastState = StrategyManager.Selecter.Mouse.RightSelecterState;
//		}
//		void IStrategyProcess.OnStop()
//		{
//			StrategyManager.Selecter.OnSectorPointing -= Selecter_OnSectorPointing;
//		}
//        void IStrategyProcess.Update()
//        {
            
//        }
//	}
//	public partial class StrategyPointingProcess
//	{
//		StrategyMouseSelectComputer.MouseState RightLastState;
//		private void Mouse_OnChangeRightMouseState(StrategyMouseSelectComputer.MouseState state, StrategyMouseSelectComputer.InputData mouseInputData)
//		{
//			if (state == StrategyMouseSelectComputer.MouseState.Released)
//			{
//				if (RightLastState == StrategyMouseSelectComputer.MouseState.Click)
//				{
//					OnPointingAtGround(StrategyManager.Pathfinding.ScreenToWorldPoint(mouseInputData.mouseCurrPosition));
//				}
//			}
//			RightLastState = state;
//		}
//		private void Selecter_OnSectorPointing(SectorObject sector)
//		{
//			OnPointingAtSector(sector);
//		}
//		private void OnPointingAtGround(Vector3 pointing)
//		{
//			if (ThisProcess.TryGetProcessOverrider<ProcessOverrider_OnPointingEmptyGround>(out var hack))
//			{
//				hack.InvokeOverrider(pointing);
//				return;
//			}

//			IList<UnitObject> unitList = StrategyManager.Selecter.SelectUnit;
//			int length = unitList.Count;
//			for (int i = 0 ; i < length ; i++)
//			{
//				var item = unitList[i];
//				item.ThisNavMovement.SetMovePath(pointing);
//			}
//		}
//		private void OnPointingAtSector(SectorObject pointing)
//		{
//			if (ThisProcess.TryGetProcessOverrider<ProcessOverrider_OnPointingTarget>(out var hack))
//			{
//				hack.InvokeOverrider(pointing);
//				return;
//			}

//			IList<UnitObject> unitList = StrategyManager.Selecter.SelectUnit;
//			int length = unitList.Count;
//			for (int i = 0 ; i < length ; i++)
//			{
//				var item = unitList[i];
//				item.ThisNodeMovement.SetMovePath(pointing);
//			}
//		}
//	}
	
//}