
using System;

public partial class OperationObject // Movement Order State
{
	public enum MovementOrderState
	{
		Wating = 0,     // 출발지 대기상태
		Execute,        // 목적지로 이동
		Pause,          // 현 위치에서 정지
		Cancel,         // 출발지로 귀환
	}
	
	private MovementOrderState movementOrder;
	private SectorObject movementTarget;
	public event Action<OperationObject> OnChangeMovementOrderState;


	public MovementOrderState MovementOrder => movementOrder;
    public SectorObject MovementTarget=> movementTarget;

	void InitMovementState(MovementOrderState movementState)
	{
		movementOrder = movementState;
		movementTarget = null;
	}

	void DeinitMovementState()
	{
		movementTarget = null;
	}

	void SetMovementTarget(SectorObject newTarget)
	{
		if (movementTarget == newTarget) return;
		movementTarget = newTarget;

		OnChangeMovementOrderState?.Invoke(this);
	}

	public void OnChangeOrder(MovementOrderState newOrder)
	{
		if (movementOrder == newOrder) return;
		movementOrder = newOrder;

		OnChangeMovementOrderState?.Invoke(this);
	}
}
