public partial class UnitObject : IFSMController<UnitFSMType>
{
	public IFSMController<UnitFSMType> FSMController => this;
	public IFSMInterface<UnitFSMType> FSMInterface { get; set; }

	partial void InitFSM()
	{
		if (!TryGetComponent<UnitFiniteStateMachine>(out var fsm))
		{
			fsm = gameObject.AddComponent<UnitFiniteStateMachine>();
		}
		FSMInterface = fsm;
		FSMController.InitState(OnStateEnterCallback, OnStateExitCallback, UnitFSMType.Idle, FSMController.GetStateList());
	}
	partial void DeinitFSM()
	{
		FSMController.DeinitState();
	}
	private void OnStateEnterCallback(UnitFSMType type)
	{

	}
	private void OnStateExitCallback(UnitFSMType type)
	{
		
	}
}