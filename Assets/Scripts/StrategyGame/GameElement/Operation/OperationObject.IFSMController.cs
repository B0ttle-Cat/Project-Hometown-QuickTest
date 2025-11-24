using System;

using UnityEngine;

public partial class OperationObject : IFSMController<OperationFSMType>
{
	[Flags]
	public enum FSMFlag : int
	{
		None = 0,
		NodeMovement = 1 << 0,
		Fighting = 1 << 1,
	}
	[SerializeField]
	private FSMFlag fsmFlag;
	public FSMFlag FsmFlag => fsmFlag;
	public IFSMController<OperationFSMType> FSMController => this;
	public IFSMInterface<OperationFSMType> FSMInterface { get; set; }

	partial void InitFSM()
	{
		FSMController.InitState(OnStateEnterCallback, OnStateExitCallback, OperationFSMType.Idle, FSMController.GetStateList());
	}
	partial void DeinitFSM()
	{
		FSMController.DeinitState();
	}

	private void OnStateEnterCallback(OperationFSMType type)
	{
		fsmFlag |= type switch
		{
			OperationFSMType.Idle => FSMFlag.NodeMovement,
			OperationFSMType.Combat => FSMFlag.Fighting,
			_ => fsmFlag
		};
	}

	private void OnStateExitCallback(OperationFSMType type)
	{
		fsmFlag &= type switch
		{
			OperationFSMType.Idle => ~FSMFlag.NodeMovement,
			OperationFSMType.Combat => ~FSMFlag.Fighting,
			_ => fsmFlag
		};
	}
}