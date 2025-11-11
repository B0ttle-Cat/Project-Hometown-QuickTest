public interface IOperationController
{
	OperationObject This { get; }
	IOperationController Controller { get; }

    void DeleteThis();
    void OnOrder_Cancel();
    void OnOrder_Execute();
    void OnOrder_Pause();
    bool OnOrganization_CheckValid(in SpawnTroopsInfo edit);
	bool OnOrganization_Edit(in SpawnTroopsInfo edit);
    bool OnOrganization_Divide(in SpawnTroopsInfo divide);
    bool OnOrganization_Merge(in SpawnTroopsInfo merge);
    void OnShowCloser();
}