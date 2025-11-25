using System;

using UnityEngine;


public partial class UnitObject : IFSMController<UnitFSMType>
{
	[Flags]
	public enum FSMFlag : int
	{
		None = 0,
		NodeMovement = 1 << 0,
		FreeMovement = 1 << 1,
		Fighting = 1 << 2,
	}
	[SerializeField]
	private FSMFlag fsmFlag;
	public FSMFlag FsmFlag => fsmFlag;
	public IFSMController<UnitFSMType> FSMController { get; }
	IFSMInterface<UnitFSMType> IFSMController<UnitFSMType>.FSMInterface { get; set; }

	partial void InitFSM()
	{
		FSMController.InitState(OnStateEnterCallback, OnStateExitCallback, UnitFSMType.Idle, FSMController.GetStateList());
	}
	partial void DeinitFSM()
	{
		FSMController.DeinitState();
	}

	private void OnStateEnterCallback(UnitFSMType type)
	{
		fsmFlag = type switch
		{
			UnitFSMType.Idle => FSMFlag.NodeMovement,
			UnitFSMType.Fighting => FSMFlag.Fighting,
			UnitFSMType.Chasing => FSMFlag.FreeMovement | FSMFlag.Fighting,
			_ => FSMFlag.None
		};
	}

	private void OnStateExitCallback(UnitFSMType type)
	{
		fsmFlag = FSMFlag.None;
	}

}