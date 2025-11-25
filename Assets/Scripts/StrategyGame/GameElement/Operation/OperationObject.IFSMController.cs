using System;

using UnityEngine;

public partial class OperationObject : IFSMController<OperationFSMType>
{
	[Flags]
	public enum FSMFlag : int
	{
		None = 0,
		NodeMovement = 1 << 0,
		Combat = 1 << 1,
	}
	[SerializeField]
	private FSMFlag fsmFlag;
	public FSMFlag FsmFlag => fsmFlag;
	public IFSMController<OperationFSMType> FSMController => this;
	public IFSMInterface<OperationFSMType> FSMInterface { get; set; }
	partial void InitFSM()
	{
		if(TryGetComponent<OperationFiniteStateMachine>(out var fsm))
		{
			fsm = gameObject.AddComponent<OperationFiniteStateMachine>();
		}
		FSMInterface = fsm;
		FSMController.InitState(OnStateEnterCallback, OnStateExitCallback, OperationFSMType.Idle, FSMController.GetStateList());
	}
	partial void DeinitFSM()
	{
		if (FSMInterface == null) return;

		FSMController.DeinitState();
		FSMInterface = null;
	}

	private void OnStateEnterCallback(OperationFSMType type)
	{
		fsmFlag |= type switch
		{
			OperationFSMType.Idle => FSMFlag.NodeMovement,
			OperationFSMType.Combat => FSMFlag.Combat,
			_ => fsmFlag
		};
	}

	private void OnStateExitCallback(OperationFSMType type)
	{
		fsmFlag &= type switch
		{
			OperationFSMType.Idle => ~FSMFlag.NodeMovement,
			OperationFSMType.Combat => ~FSMFlag.Combat,
			_ => fsmFlag
		};
	}
}