public partial class OperationObject : IOperationController
{
	public IOperationController Controller => this;

	void IOperationController.DeleteThis()
    {
        // 스스로에게 파괴 명령
    }

    void IOperationController.OnOrder_Cancel()
    {
    }

    void IOperationController.OnOrder_Execute()
    {
    }

    void IOperationController.OnOrder_Pause()
    {
    }

    bool IOperationController.OnOrganization_CheckValid(in SpawnTroopsInfo edit)
    {
        return true;
    }
    bool IOperationController.OnOrganization_Edit(in SpawnTroopsInfo edit)
	{
		return true;
	}


    bool IOperationController.OnOrganization_Divide(in SpawnTroopsInfo divide)
	{
		return true;
	}

    bool IOperationController.OnOrganization_Merge(in SpawnTroopsInfo merge)
	{
		return true;
	}

    void IOperationController.OnShowCloser()
    {
		// 카메라 타겟 설정
		// StrategyManager.MainCamera

		// 모드 전환
		StrategyManager.ViewAndControl.ModeChange(ViewAndControlModeType.TacticsMode);
    }
}