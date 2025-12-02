using System;
using System.Collections.Generic;

using Pathfinding;
using Pathfinding.RVO;

using Sirenix.OdinInspector;

using UnityEngine;

using static StrategyGamePlayData;

[RequireComponent(typeof(Seeker))]
[RequireComponent(typeof(RVOController))]
[RequireComponent(typeof(FunnelModifier))]
[RequireComponent(typeof(RaycastModifier))]
public partial class UnitMovement : MonoBehaviour
{
	private IStateValueGetter StateValueGetter { get; set; }
	private IOperationBelonger OperationBelonger { get; set; }
	private IUnitCombatController CcombatController { get; set; }
	private IFSMController<UnitFSMType> FsmController { get; set; }

	private Vector3 operationMoveTarget;
	[ShowInInspector, ReadOnly]
	private Vector3 movePosition;
	[ShowInInspector, ReadOnly]
	private Vector3 moveVelocity = Vector3.zero;
	[ShowInInspector, ReadOnly]
	private float moveRadius = 0.5f;
	[ShowInInspector, ReadOnly]
	private float smoothTime = 0f;
	[ShowInInspector, ReadOnly]
	private float initLength = 0f;
	[ShowInInspector, ReadOnly]
	private float totalLength = 0f;
	[ShowInInspector, ReadOnly]
	private float tempLength = 0f;

	private Seeker seeker;
	private int movementIndex;
	private Vector3[] initPath;
	private List<Vector3> movePath;
	private List<Vector3> tempMovePath;
	private Queue<Vector3> findingPoints;
	private Action onMovePathUpdate;
	private Action<float> onMoveProgress;
	private Action onEndedMove;
	private Action onStartMove;

	private RVOController rvoController;

	public void Init(UnitObject unitObject)
	{
		StateValueGetter = unitObject;
		OperationBelonger = unitObject;
		CcombatController = unitObject;
		FsmController = unitObject;

		seeker = GetComponent<Seeker>();
		rvoController = GetComponent<RVOController>();
		moveRadius = rvoController.radius;

		movePath = new List<Vector3>();
		tempMovePath = null;
		findingPoints = new Queue<Vector3>();
		totalLength = 0;
		tempLength = 0f;

		isSerchingNavPath = false;

		InitPositionAndVelocity(out movePosition, out moveVelocity);
		UpdateMovementTransform();
	}
	public void Deinit()
	{
	}
	private bool HasOperation()
	{
		if (OperationBelonger == null) return false;
		return OperationBelonger.HasOperation;
	}
	private float GetStateValue(StatsType type)
	{
		if (StateValueGetter == null) return 0f;
		return StateValueGetter.GetStateValue(StrategyGamePlayData.StatsType.유닛_이동속도_c);
	}

	private void InitPositionAndVelocity(out Vector3 position, out Vector3 velocity)
	{
		position = transform.position;
		velocity = Vector2.zero;
	}
	Vector3 OperationModify(in Vector3 nextPosition, in float smoothTime, ref float maxSpeed, in float deltaTime)
	{
		if (!HasOperation()) return nextPosition;

		Vector3 operationLocalOffset = OperationBelonger.GetBelongedOperation().transform.TransformVector(OperationBelonger.OperationOffset);

		float distance = Vector3.Distance(ThisMovement.CurrentPosition, nextPosition + operationLocalOffset);
		float oneDistance = maxSpeed * (smoothTime + 0.2f);
		if (distance > oneDistance)
		{
			operationMoveTarget = nextPosition;
			maxSpeed *= distance / oneDistance;
		}

		return operationMoveTarget + operationLocalOffset;
	}
	void MainMovementPositionAndVelocity(in Vector3 nextPosition, in float deltaTime)
	{
		Vector3 currPosition = ThisMovement.CurrentPosition;
		Vector3 currVelocity = ThisMovement.CurrentVelocity;
		float smoothTime = ThisMovement.SmoothTime;
		float maxSpeed = ThisMovement.MaxSpeed;

		Vector3 moveTargetPosition = HasOperation() ? OperationModify(in nextPosition,in smoothTime, ref maxSpeed, in deltaTime) : nextPosition;

		currPosition = Vector3.SmoothDamp(currPosition, moveTargetPosition, ref currVelocity, smoothTime, maxSpeed, deltaTime);

		Vector3 delteMove = currPosition - transform.position;
		if (rvoController != null && rvoController.isActiveAndEnabled)
		{
			rvoController.SetTarget(moveTargetPosition, currVelocity.magnitude, maxSpeed, Vector3.positiveInfinity);
			delteMove = rvoController.CalculateMovementDelta(movePosition, deltaTime);
			movePosition += delteMove;
			moveVelocity = currVelocity;
		}
		else
		{
			movePosition = currPosition;
			moveVelocity = currVelocity;
		}

		UpdateMovementTransform();
	}
	void UpdateMovementTransform()
	{
		transform.position = movePosition;
		if (moveVelocity.sqrMagnitude > 0.0001f)
			transform.LookAt(movePosition + moveVelocity.normalized);
	}
}
public partial class UnitMovement : IMovement
{
	public IMovement ThisMovement => this;
	public Seeker ThisSeeker => seeker;
	public RVOController RVO => rvoController;
	Vector3 IMovement.CurrentPosition
	{
		get
		{
			if (transform.hasChanged)
			{
				transform.hasChanged = false;
				movePosition = transform.position;
			}
			return movePosition;
		}
	}
	Vector3 IMovement.CurrentVelocity => moveVelocity;
	float IMovement.CurrentRadius => moveRadius;
	Vector3 IMovement.NextMovePosition { get; set; }
	float IMovement.SmoothTime => smoothTime;
	float IMovement.MaxSpeed => GetStateValue(StrategyGamePlayData.StatsType.유닛_이동속도_c);
	int IMovement.MovementIndex { get => movementIndex; set => movementIndex = value; }
	Vector3[] IMovement.InitPath { get => initPath; set => initPath = value; }
	List<Vector3> IMovement.MovePath { get => movePath; set => movePath = value; }
	List<Vector3> IMovement.TempMovePath { get => tempMovePath; set => tempMovePath = value; }
	Queue<Vector3> IMovement.FindingPoints { get => findingPoints; set => findingPoints = value; }
	float IMovement.InitLength { get => initLength; set => initLength = value; }
	float IMovement.TotalLength { get => totalLength; set => totalLength = value; }
	float IMovement.TempLength { get => tempLength; set => tempLength = value; }
	Action IMovement.OnChangeMovePath { get => onMovePathUpdate; set => onMovePathUpdate = value; }
	Action<float> IMovement.OnChangeMoveProgress { get => onMoveProgress; set => onMoveProgress = value; }
	Action IMovement.OnStartMove { get => onStartMove; set => onStartMove = value; }
	Action IMovement.OnEndedMove { get => onEndedMove; set => onEndedMove = value; }
	void IMovement.OnMoveStart()
	{
		operationMoveTarget = Vector3.zero;
		moveVelocity = Vector3.zero;
		smoothTime = 0.5f;
	}
	void IMovement.OnMoveStop()
	{
		moveVelocity = Vector3.zero;
		smoothTime = 0.5f;
	}
}
public partial class UnitMovement : INodeMovement
{
	public INodeMovement ThisNodeMovement => this;
	public INodeMovement ParentMovement => OperationBelonger.GetBelongedOperation();
	bool INodeMovement.IsMovableState()
	{
		return FsmController.CurrentStateType == UnitFSMType.Idle;
	}
	void INodeMovement.SetPositionAndVelocity(in Vector3 position, in Vector3 delteMove, in Vector3 velocity, in float deltaTime)
	{
		if (HasOperation())
		{
			MainMovementPositionAndVelocity(in position, in deltaTime);
			return;
		}
		movePosition = position;
		moveVelocity = velocity;
		if (ThisMovement.HasTampPath)
		{
			tempLength -= delteMove.magnitude;
			if (tempLength < 0) tempLength = 0f;
		}

		UpdateMovementTransform();
	}
	void INodeMovement.OnStayUpdate(in float deltaTime)
	{
		if (HasOperation())
		{
			Vector3 operationPosition = ParentMovement.CurrentPosition;
			MainMovementPositionAndVelocity(in operationPosition, in deltaTime);
		}
		else
		{
			movePosition = transform.position;
			moveVelocity = Vector3.zero;
		}
	}
}
public partial class UnitMovement : INavMovement
{
	public INavMovement ThisNavMovement => this;
	private bool isSerchingNavPath;
	private Vector3 lastTargetPosition;
	bool INavMovement.IsMovableState()
	{
		return CcombatController.CurrentTarget != null
			&& FsmController.CurrentStateType == UnitFSMType.Chasing;
	}
	bool INavMovement.IsChangeTargetPositionCheck()
	{
		var delta = CcombatController.CurrentTarget.Position - lastTargetPosition;
		float sqrDistance  = delta.sqrMagnitude;
		if (sqrDistance > 0.0001f)
		{
			OnNewCombatMovementPath(CcombatController.CurrentTarget);
			return true;
		}
		return isSerchingNavPath;
	}
	void INavMovement.SetPositionAndVelocity(in Vector3 position, in Vector3 delteMove, in Vector3 velocity, in float deltaTime)
	{
		movePosition = position;
		moveVelocity = velocity;
		if (ThisMovement.HasTampPath)
		{
			if (tempLength < 0) tempLength = 0f;
		}

		UpdateMovementTransform();
	}
	void INavMovement.OnStayUpdate(in float deltaTime)
	{
		movePosition = transform.position;
		moveVelocity = Vector3.zero;
	}
	public void OnNewCombatMovementPath(ITargetableCombatant target)
	{
		if (target == null)
		{
			ThisNavMovement.ClearMovePath();
			isSerchingNavPath = false;
			lastTargetPosition = Vector3.zero;
			return;
		}

		lastTargetPosition = target.Position;

		isSerchingNavPath = true;
		ThisNavMovement.SetMovePath(CallbackSetMovePath, lastTargetPosition);
		void CallbackSetMovePath()
		{
			CutPathFromEnd(movePath, CcombatController.AttackStartRange.y - 0.01f, false);

			var delta = target.Position - lastTargetPosition;
			float sqrDistance  = delta.sqrMagnitude;
			if (sqrDistance > 0.0001f)
			{
				OnNewCombatMovementPath(CcombatController.CurrentTarget);
			}
			else
			{
				isSerchingNavPath = false;
			}
		}
	}
	private void CutPathFromEnd(List<Vector3> path, float distance, bool useRealPathLength)
	{
		if (path == null || path.Count < 2 || distance <= 0f)
			return;

		float remain = distance;

		// 뒤에서 앞으로 segment 검사
		for (int i = path.Count - 1 ; i > 0 ; i--)
		{
			Vector3 a = path[i];
			Vector3 b = path[i - 1];

			// 실제 경로 모양 그대로 cut
			if (useRealPathLength)
			{
				float seg = Vector3.Distance(a, b);

				if (remain <= seg)
				{
					// seg 구간 안에서 잘리는 위치
					float t = (seg - remain) / seg;
					path[i] = Vector3.Lerp(b, a, t);

					// i 이후는 모두 제거
					if (i + 1 < path.Count)
						path.RemoveRange(i + 1, path.Count - (i + 1));
					return;
				}

				remain -= seg;
			}
			else
			{
				// 단순 거리 cut: 경로 모양을 무시하고 a→b 방향 직선 기준 cut
				float seg = Vector3.Distance(a, b);

				if (remain <= seg)
				{
					// seg 구간 안에서 단순 직선 기준 cut
					float t = (seg - remain) / seg;

					// 여기서 "단순 cut" 은 경로의 실제 shape 를 무시하므로
					// b→a 방향 직선 보간만 유지한다.
					path[i] = Vector3.Lerp(b, a, t);

					if (i + 1 < path.Count)
						path.RemoveRange(i + 1, path.Count - (i + 1));
					return;
				}

				remain -= seg;
			}
		}

		// distance 가 시작 지점까지 넘어가는 상황
		path[path.Count - 1] = path[0];
		if (path.Count > 1)
			path.RemoveRange(1, path.Count - 1);
	}
}
#if UNITY_EDITOR
public partial class UnitMovement // MovementPathGizmo
{
	private const float headLength = 0.4f;    // 화살촉 길이
	private const float headAngle = 40f;      // 화살촉 각도 (deg)
	private const float pointRadius = 0.04f;  // 각 점 표시 반지름
	private const bool drawPoints = true;     // 정점 표시 여부
	void OnDrawGizmos_MovementPathGizmo()
	{
		var movePath = ThisMovement.MovePath;
		if (movePath == null || movePath.Count < 2)
			return;


		Gizmos.color = Color.green;

		for (int i = 0 ; i < movePath.Count - 1 ; i++)
		{
			Vector3 a = movePath[i];
			Vector3 b = movePath[i + 1];

			// 선분
			Gizmos.DrawLine(a, b);

			// 정점 표시
			if (drawPoints)
				Gizmos.DrawSphere(a, pointRadius);

			// 마지막 세그먼트이면 끝점도 표시
			if (i == movePath.Count - 2 && drawPoints)
				Gizmos.DrawSphere(b, pointRadius);

			// 화살촉: 각 세그먼트의 'b' 쪽에 화살촉 그림
			DrawArrowHead(a, b);
		}
	}
	void DrawArrowHead(Vector3 from, Vector3 to)
	{
		Vector3 dir = (to - from);
		float distance = dir.magnitude;
		if (distance <= Mathf.Epsilon) return;
		dir /= distance; // 정규화

		// forward = dir 로 하는 회전
		Quaternion rot = Quaternion.LookRotation(dir);

		// 두 방향의 화살촉 선 계산 (180도 뒤로 꺾인 방향에서 +- headAngle)
		Vector3 headDir1 = rot * Quaternion.Euler(0f, 180f + headAngle, 0f) * Vector3.forward;
		Vector3 headDir2 = rot * Quaternion.Euler(0f, 180f - headAngle, 0f) * Vector3.forward;

		Gizmos.DrawLine(to, to + headDir1 * headLength);
		Gizmos.DrawLine(to, to + headDir2 * headLength);
	}
}
#endif