public interface IOperationController
{
	OperationObject This { get; }
    IOperationController ThisController => This;
	void On_SelectLabel();
	void DeleteThis();
    void OnMovementOrder_AvailableType(out bool execute, out bool pause, out bool cancel);
    void OnMovementOrder_SetTarget(SectorObject movementTarget);
	void OnMovementOrder_Cancel();
    void OnMovementOrder_Execute();
    void OnMovementOrder_Pause();
    bool OnOrganization_CheckValid(in SpawnTroopsInfo edit);
	bool OnOrganization_Edit(in SpawnTroopsInfo edit);
    bool OnOrganization_Divide(in SpawnTroopsInfo divide);
    bool OnOrganization_Merge(in SpawnTroopsInfo merge);
    void OnShowCloser();
}