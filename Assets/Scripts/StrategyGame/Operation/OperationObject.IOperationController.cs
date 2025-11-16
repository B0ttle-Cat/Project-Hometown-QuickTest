public partial class OperationObject : IOperationController
{
	public IOperationController ThisController => this;

	void IOperationController.DeleteThis()
	{
		// 스스로에게 파괴 명령
		RelaseAndDestroyAllUnit();
		StrategyElementUtility.Destroy(this);
	}

	void IOperationController.OnMovementOrder_AvailableType(out bool execute, out bool pause, out bool cancel)
	{
		if(MovementTarget == null)
		{
			execute = false; pause = false; cancel = false;
			return;
		}
		switch (MovementOrder)
		{
			case MovementOrderState.Wating: { execute = true; pause = false; cancel = false; } break;
			case MovementOrderState.Execute: { execute = false; pause = true; cancel = true; } break;
			case MovementOrderState.Pause: { execute = true; pause = false; cancel = true; } break;
			case MovementOrderState.Cancel: { execute = true; pause = true; cancel = false; } break;
			default: { execute = false; pause = false; cancel = false; } break;
		}
	}

	void IOperationController.OnMovementOrder_SetTarget(SectorObject movementTarget)
	{
		SetMovementTarget(movementTarget);
	}
	void IOperationController.OnMovementOrder_Cancel()
	{
		OnChangeOrder(MovementOrderState.Cancel);
	}

	void IOperationController.OnMovementOrder_Execute()
	{
		OnChangeOrder(MovementOrderState.Execute);
	}

	void IOperationController.OnMovementOrder_Pause()
	{
		OnChangeOrder(MovementOrderState.Pause);
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

    void IOperationController.On_SelectLabel()
    {
		StrategyManager.Selecter.OnSystemSelectObject(this);
    }
}