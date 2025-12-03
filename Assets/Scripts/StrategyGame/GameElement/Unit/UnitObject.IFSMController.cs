public partial class UnitObject : IFSMController<UnitMainFSMType>
{
	public IFSMController<UnitMainFSMType> FSMController => this;
	public IFSMInterface<UnitMainFSMType> FSMInterface { get; set; }

	partial void InitFSM()
	{
		if (!TryGetComponent<UnitFiniteStateMachine>(out var fsm))
		{
			fsm = gameObject.AddComponent<UnitFiniteStateMachine>();
		}
		FSMInterface = fsm;
		FSMController.InitState(OnStateEnterCallback, OnStateExitCallback, UnitMainFSMType.Idle, FSMController.GetStateList());
	}
	partial void DeinitFSM()
	{
		FSMController.DeinitState();
	}
	private void OnStateEnterCallback(UnitMainFSMType type)
	{

	}
	private void OnStateExitCallback(UnitMainFSMType type)
	{
		
	}
}