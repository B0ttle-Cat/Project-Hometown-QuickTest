using System;

using Unity.Mathematics;

using UnityEngine;

using Random = UnityEngine.Random;

public class ProjectileMovement : MonoBehaviour, IProjectileMovement
{
	[SerializeField] protected ICombatHandler order;
	[SerializeField] protected ITargetableCombatant target;
	[SerializeField] protected Vector3 startPosition;
	[SerializeField] protected Vector3 targetPosition;
	[SerializeField] protected Vector3 prevPosition;
	[SerializeField] protected Vector3 currentPosition;
	[SerializeField] protected float moveSpeed;
	[SerializeField] protected Vector3 moveDirection;

	private bool resetJobDataFlag;
	public struct MovementJobData
	{
		public int ProjectileKey;

		public float3 Position;
		public float3 PrevPosition;
		public float3 TargetPosition;
		public float3 MoveDirection;
		public float MoveSpeed;

		public float DeltaTime;
		public float UpdateTime;

		public float3 CepOffset; // 초기 CEP 오프셋은 유지
		public uint RandomState; // Job 내부 난수 상태 유지
	}
	public struct MovmentConstantData
	{
		public bool IsShiftSpeed;
		public float MoveStartSpeed;
		public float MoveMaxSpeed;
		public float TimeFromStartToMaxSpeed;

		public bool HomingEnabled;
		public float HomingTurnSpeed;
		public float HomingTurnSpeedWhenMaxSpeed;
		public float HomingLimitAngleCosine;
		public float HomingLimitSqrDistance;
	}

	bool IProjectileMovement.ResetJobDataFlag => resetJobDataFlag;
	IProjectileMovement IProjectileMovement.ThisMovement => this;
	int IProjectileMovement.OrderElementID => order.ThisElement.ID;
	int IProjectileMovement.TargetElementID => target.ThisElement.ID;
	Vector3 IProjectileMovement.StartPosition => startPosition;
	Vector3 IProjectileMovement.TargetPosition => targetPosition;
	Vector3 IProjectileMovement.PrevPosition => prevPosition;
	Vector3 IProjectileMovement.CurrentPosition => currentPosition;
	float IProjectileMovement.MoveSpeed => moveSpeed;
	Vector3 IProjectileMovement.MoveDiraction => moveDirection;

	private ProjectileStatsData projectileStats;
	private Action onTransformUpdate;

	public void Init(ProjectileStatsData projectileStats, Action onTransformUpdate)
	{
		this.projectileStats = projectileStats;

		OnInit(projectileStats);

		resetJobDataFlag = true;
	}
	protected virtual void OnInit(ProjectileStatsData projectileStats) { }
	internal void Deinit()
	{
		order = null;
		target = null;
	}

	void IProjectileMovement.SetTarget(ICombatHandler order, ITargetableCombatant target)
	{
		this.order = order;
		this.target = target;

		startPosition = order.AttackStartPosition;
		targetPosition = target.HitTargetPosition;

		transform.position = startPosition;
		transform.LookAt(targetPosition);


		OnSetTarget();
		resetJobDataFlag = true;
	}
	protected virtual void OnSetTarget() { }

	public void InitMovementJobData(out MovementJobData pureMovementData)
	{
		startPosition = order.AttackStartPosition;
		targetPosition = target.HitTargetPosition;
		Vector3 cepOffset = projectileStats.CepEnabled ? GenerateCEPOffset(projectileStats.CepRadius, projectileStats.CepProbability) : Vector3.zero;

		prevPosition = startPosition;
		currentPosition = startPosition;
		moveDirection = (targetPosition + cepOffset - startPosition).normalized;
		moveSpeed = projectileStats.MoveStartSpeed;;

		transform.position = currentPosition;
		transform.LookAt(currentPosition + moveDirection);

		pureMovementData = new MovementJobData
		{
			ProjectileKey = (int)projectileStats.ProjectileKey, // ProjectileKey 필드가 ProjectileStatsData에 있다고 가정
			Position = currentPosition,
			PrevPosition = prevPosition,
			TargetPosition = targetPosition,
			MoveDirection = moveDirection,
			MoveSpeed = moveSpeed,
			DeltaTime = 0,
			UpdateTime = 0,
			CepOffset = cepOffset,
			RandomState = (uint)Random.Range(1, int.MaxValue),
		};

		resetJobDataFlag = false;

		// PrepareCurve 함수 및 NativeArray 할당 로직은 더 이상 이 함수 내부에서 호출하지 않습니다.
		// Curve Data는 StrategyUpdate_ProjectileMovement 클래스가 모든 키에 대해 미리 준비하여 Job에 전달해야 합니다.
	}

	public void ApplyJobResult(in MovementJobData pureMovementData)
	{
		prevPosition = pureMovementData.PrevPosition;
		currentPosition = pureMovementData.Position;
		moveSpeed = pureMovementData.MoveSpeed;
		moveDirection = pureMovementData.MoveDirection;

		transform.position = currentPosition;
		transform.LookAt(currentPosition + moveDirection);

		onTransformUpdate?.Invoke();
	}
	public void UpdateMovementJobData(ref MovementJobData pureMovementData)
	{
		if (target == null) return;
		pureMovementData.TargetPosition = target.HitTargetPosition;
	}
	protected static Vector3 GenerateCEPOffset(float cepRadius, float cepProbability)
	{
		if (cepRadius <= 0f) return Vector3.zero;
		float denom = Mathf.Sqrt(-2f * Mathf.Log(1f - cepProbability));
		if (denom < 1e-6f) return Vector3.zero;
		float sigma = cepRadius / denom;
		float u = UnityEngine.Random.value;
		float r = sigma * Mathf.Sqrt(-2f * Mathf.Log(u));
		float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
		float x = r * Mathf.Cos(angle);
		float z = r * Mathf.Sin(angle);
		return new Vector3(x, 0f, z);
	}


}

