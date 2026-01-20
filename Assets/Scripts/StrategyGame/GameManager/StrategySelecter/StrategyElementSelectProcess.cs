using System.Collections.Generic;

using Sirenix.OdinInspector;

using StrategyManagerModule;

using UnityEngine;

using static StrategyElementSelectProcess;
public partial class StrategyElementSelectProcess : FiniteStateMachine<ElementSelectType>, IFSMInterface<ElementSelectType>
{

	public enum ElementSelectType
	{
		None, Movement,
	}
	[ShowInInspector]
	private SelecterState CurrentState => currentState as SelecterState;
	public override IState<ElementSelectType>[] GetStateList()
	{
		return new IState<ElementSelectType>[]
		{
			new EmptySelecter(this, ElementSelectType.None),
			new MovementSelecter(this, ElementSelectType.Movement),
		};
	}
	public abstract class SelecterState : BaseState
	{
		#region SelecterState
		protected StrategyElementSelectProcess This { get; private set; }
		protected SelecterState(StrategyElementSelectProcess fsm, ElementSelectType type) : base(fsm, type)
		{
			This = fsm;
		}
		protected override void OnDispose()
		{
		}
		protected override void OnStateAwake()
		{
		}
		protected override void OnStateEnter()
		{
		}
		protected override void OnStateExit()
		{
		}
		protected override void OnStateStart()
		{
		}
		protected override ElementSelectType OnStateUpdate(in float deltaTime)
		{
			if (This.selectableList.Count == 0)
			{
				return ElementSelectType.None;
			}
			if (This.selectMovement.Count > 0)
			{
				return ElementSelectType.Movement;
			}


			return ThisType;
		}
		#endregion
		public abstract void OnSelect(ISelectable element);
		public abstract void OnDeselect(ISelectable element);
		public abstract void OnPointing(ISelectable element);
		public abstract void OnSelectingEmptyGround(Vector3 ground);
		public abstract void OnPointingEmptyGround(Vector3 ground);
	}
	public class EmptySelecter : SelecterState
	{
		public EmptySelecter(StrategyElementSelectProcess fsm, ElementSelectType type) : base(fsm, type)
		{
		}

		public override void OnSelect(ISelectable element)
		{
		}
		public override void OnDeselect(ISelectable element)
		{
		}
		public override void OnPointing(ISelectable element)
		{
		}
		public override void OnSelectingEmptyGround(Vector3 ground)
		{

		}
		public override void OnPointingEmptyGround(Vector3 ground)
		{

		}
	}
	public class MovementSelecter : SelecterState
	{
		public MovementSelecter(StrategyElementSelectProcess fsm, ElementSelectType type) : base(fsm, type)
		{
		}
		public override void OnSelect(ISelectable element)
		{
		}
		public override void OnDeselect(ISelectable element)
		{
		}
		public override void OnPointing(ISelectable element)
		{
			if (StrategyManager.ViewAndControl.CurrentMode == ViewAndControlModeType.OperationsMode)
			{
				if (element is not SectorObject moveTarget) return;
				foreach (var item in This.selectMovement)
				{
					if(item.Value is INodeMovement movement)
					{
						movement.SetMovePath(moveTarget);
					}
				}
			}
			else
			{
				if (element is not IMapSelectable moveTarget) return;
				foreach (var item in This.selectMovement)
				{
					if (item.Value is INavMovement movement)
					{
						movement.SetMovePath(moveTarget.SelectCenter);
					}
				}
			}
		}
		public override void OnSelectingEmptyGround(Vector3 ground)
		{

		}
		public override void OnPointingEmptyGround(Vector3 ground)
		{
			if (StrategyManager.ViewAndControl.CurrentMode == ViewAndControlModeType.OperationsMode)
			{

			}
			else
			{
				foreach (var item in This.selectMovement)
				{
					if (item.Value is INavMovement movement)
					{
						movement.SetMovePath(ground);
					}
				}
			}
		}
	}

}

public partial class StrategyElementSelectProcess : IStrategyProcess
{
	public IStrategyProcess ThisProcess => this;
	public List<ProcessOverrider> OverriderList { get; } = new List<ProcessOverrider>();
	void IStrategyProcess.OnInit()
	{
		InitSelectablesList();
	}
	void IStrategyProcess.OnStart()
	{
		InitState(OnStateEnterCallback, OnStateExitCallback, ElementSelectType.None, GetStateList());
	}
	void IStrategyProcess.OnStop()
	{
		DeinitState();
	}
	void IStrategyProcess.Update()
	{
		float deltaTime = Time.deltaTime;
		StateUpdate(deltaTime);
	}
	private void OnStateEnterCallback(ElementSelectType type)
	{

	}
	private void OnStateExitCallback(ElementSelectType type)
	{

	}
}
public partial class StrategyElementSelectProcess
{
	HashSet<ISelectable> selectableList;
	Dictionary<ISelectable, IStrategyElement> selectElement;
	Dictionary<ISelectable, IMovement> selectMovement;

	void InitSelectablesList()
	{
		selectableList = new HashSet<ISelectable>();
		selectElement = new Dictionary<ISelectable, IStrategyElement>();
		selectMovement = new Dictionary<ISelectable, IMovement>();
	}

	internal void OnSelect(ISelectable selectable)
	{
		if (selectable is not IStrategyElement element) return;
		if (!selectableList.Add(selectable)) return;

		AddDic(selectElement, selectable);
		AddDic(selectMovement, selectable);
		StateUpdate();

		selectable.OnSelect();
		CurrentState.OnSelect(selectable);
		

		static void AddDic<T>(Dictionary<ISelectable, T> list, ISelectable target)
		{
			if (target is T t) list.Add(target, t);
		}
	}
	internal void OnDeselect(ISelectable selectable)
	{
		if (selectable is not IStrategyElement element) return;
		if (!selectableList.Remove(selectable)) return;

		Remove(selectElement, selectable);
		Remove(selectMovement, selectable);
		StateUpdate();

		selectable.OnDeselect();
		CurrentState.OnDeselect(selectable);

		static void Remove<T>(Dictionary<ISelectable, T> list, ISelectable target)
		{
			if (target is T) list.Remove(target);
		}
	}
	internal void OnPointing(ISelectable selectable)
	{
		if (selectable is not IStrategyElement element) return;

		selectable.OnPointing();
		CurrentState.OnPointing(selectable);
	}
	internal void OnSelectingEmptyGround(Vector3 ground)
	{

	}
	internal void OnPointingEmptyGround(Vector3 ground)
	{
	}
}
