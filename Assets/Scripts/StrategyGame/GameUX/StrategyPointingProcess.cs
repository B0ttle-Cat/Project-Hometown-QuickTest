using System;
using System.Collections.Generic;

using UnityEngine;

using static StrategyManagerModule.StrategyGameUX;

namespace StrategyManagerModule
{
	public partial class StrategyPointingProcess : MonoBehaviour, IStrategyProcess
	{
		public IStrategyProcess ThisProcess => this;
		public List<ProcessOverrider> OverriderList { get; set; } = new List<ProcessOverrider>();


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
		void IStrategyProcess.OnRemoveProcessOverride(ProcessOverrider processOverride)
		{
			OverriderList.Add(processOverride);
		}
		void IStrategyProcess.OnAddProcessOverride(ProcessOverrider processOverride)
		{
			OverriderList.Remove(processOverride);
		}
	}
	public partial class StrategyPointingProcess : MonoBehaviour
	{
		StrategyMouseSelectComputer.MouseState RightLastState;
		private void Mouse_OnChangeRightMouseState(StrategyMouseSelectComputer.MouseState state, StrategyMouseSelectComputer.InputData mouseInputData)
		{
			if (state == StrategyMouseSelectComputer.MouseState.Released)
			{
				if (RightLastState == StrategyMouseSelectComputer.MouseState.Click)
				{
					OnPointingAtGround(StrategyManager.Pathfinding.ScreenToWorldPoint(mouseInputData.mouseCurrPosition));
				}
			}
			RightLastState = state;
		}
		private void Selecter_OnSectorPointing(SectorObject sector)
		{
			OnPointingAtSector(sector);
		}
		private void OnPointingAtGround(Vector3 pointing)
		{
			if (ThisProcess.TryGetProcessOverrider<OverridePointingAtGround>(out var hack))
			{
				hack.OnPointingAtGround(pointing);
				return;
			}

			IList<UnitObject> unitList = StrategyManager.Selecter.SelectUnit;
			int length = unitList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				var item = unitList[i];
				item.ThisNavMovement.SetMovePath(pointing);
			}
		}
		private void OnPointingAtSector(SectorObject pointing)
		{
			if (ThisProcess.TryGetProcessOverrider<OverridePointingAtSector>(out var hack))
			{
				hack.OnPointingAtSector(pointing);
				return;
			}

			IList<UnitObject> unitList = StrategyManager.Selecter.SelectUnit;
			int length = unitList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				var item = unitList[i];
				item.ThisNodeMovement.SetMovePath(pointing);
			}
		}
	}
	public class OverridePointingAtGround : ProcessOverrider
	{
		readonly Action<Vector3> onPointingAtGround;

		public OverridePointingAtGround(Action<Vector3> onPointingAtGround) : base()
		{
			this.onPointingAtGround = onPointingAtGround;
		}
		protected override void OnOverride()
		{
			StrategyManager.GameUX.PointingProcess.OnAddProcessOverride(this);
		}
		protected override void OnDispose()
		{
			StrategyManager.GameUX.PointingProcess.OnAddProcessOverride(this);
		}

		internal void OnPointingAtGround(Vector3 pointing)
		{
			onPointingAtGround?.Invoke(pointing);
		}
	}
	public class OverridePointingAtSector : ProcessOverrider
	{
		readonly Action<SectorObject> onPointingAtSector;

		public OverridePointingAtSector(Action<SectorObject> onPointingAtSector) : base()
		{
			this.onPointingAtSector = onPointingAtSector;
		}
		protected override void OnOverride()
		{
			StrategyManager.GameUX.PointingProcess.OnAddProcessOverride(this);
		}
		protected override void OnDispose()
		{
			StrategyManager.GameUX.PointingProcess.OnAddProcessOverride(this);
		}

		internal void OnPointingAtSector(SectorObject pointing)
		{
			onPointingAtSector?.Invoke(pointing);
		}
	}

	public partial class StrategyGameUX
	{
		private StrategyPointingProcess pointingProcess;
		public IStrategyProcess PointingProcess
		{
			get
			{
				if (pointingProcess.IsNullRef())
				{
					pointingProcess = gameObject.GetComponent<StrategyPointingProcess>();
				}
				return pointingProcess;
			}
		}
	} 
}