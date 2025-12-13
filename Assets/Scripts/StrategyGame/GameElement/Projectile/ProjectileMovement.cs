using System;

using Unity.Mathematics;

using UnityEngine;

using Random = UnityEngine.Random;

public class ProjectileMovement : MonoBehaviour, IProjectileMovement
{
	[SerializeField] protected ICombatHandler order;
	[SerializeField] protected ITargetableCombatant target;
	[SerializeField] protected Vector3 prevPosition;

	private bool resetJobDataFlag;
	public struct MovementJobData
	{
		public int ProjectileKey;

		public float3 Position;
		public float3 PrevPosition;
		public float3 TargetPosition;
		public float3 EndedPosition;
		public float3 MoveDirection;
		public float MoveSpeed;

		public float DeltaTime;
		public float UpdateTime;

		public float3 CepOffset; 
		public uint RandomState;
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
	Vector3 IProjectileMovement.StartPosition => RuntimeData.StartPosition;
	Vector3 IProjectileMovement.TargetPosition => RuntimeData.TargetPosition;
	Vector3 IProjectileMovement.PrevPosition => prevPosition;
	Vector3 IProjectileMovement.CurrentPosition => RuntimeData.Position;
	float IProjectileMovement.MoveSpeed => RuntimeData.MoveSpeed;
	Vector3 IProjectileMovement.MoveDiraction => RuntimeData.MoveDiraction;

	private ProjectileRuntimeData RuntimeData;
	private ProjectileStatsData StatsData;
	private Action onTransformUpdate;
	private Action<Vector3> onArrive;

	public void Init(ProjectileRuntimeData runtimeData, ProjectileStatsData statsData, Action onTransformUpdate, Action<Vector3> onArrive)
	{
		this.RuntimeData = runtimeData;
		this.StatsData = statsData;

		OnInit(runtimeData, statsData);

		resetJobDataFlag = true;
	}
	protected virtual void OnInit(ProjectileRuntimeData runtimeData, ProjectileStatsData statsData) { }
	internal void Deinit()
	{
		order = null;
		target = null;
	}

	void IProjectileMovement.SetTarget(ICombatHandler order, ITargetableCombatant target)
	{
		this.order = order;
		this.target = target;

		RuntimeData.StartPosition = order.AttackStartPosition;
		RuntimeData.TargetPosition = target.HitTargetPosition;

		transform.position = RuntimeData.StartPosition;
		transform.LookAt(RuntimeData.TargetPosition);


		OnSetTarget();
		resetJobDataFlag = true;
	}
	protected virtual void OnSetTarget() { }

	public void InitMovementJobData(out MovementJobData pureMovementData)
	{
		RuntimeData.StartPosition = order.AttackStartPosition;
		RuntimeData.TargetPosition = target.HitTargetPosition;
		Vector3 cepOffset = StatsData.CepEnabled ? GenerateCEPOffset(StatsData.CepRadius, StatsData.CepProbability) : Vector3.zero;

		prevPosition = RuntimeData.StartPosition;
		RuntimeData.Position = RuntimeData.StartPosition;
		RuntimeData.EndedPosition = RuntimeData.TargetPosition + cepOffset;
		RuntimeData.MoveDiraction = (RuntimeData.EndedPosition - RuntimeData.Position).normalized;
		RuntimeData.MoveSpeed = StatsData.MoveStartSpeed;

		transform.position = RuntimeData.Position;
		transform.LookAt(RuntimeData.Position + RuntimeData.MoveDiraction);

		pureMovementData = new MovementJobData
		{
			ProjectileKey = (int)StatsData.ProjectileKey, // ProjectileKey 필드가 ProjectileStatsData에 있다고 가정
			Position = RuntimeData.Position,
			PrevPosition = prevPosition,
			TargetPosition = RuntimeData.TargetPosition,
			EndedPosition = RuntimeData.EndedPosition,
			MoveDirection = RuntimeData.MoveDiraction,
			MoveSpeed = RuntimeData.MoveSpeed,
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
		RuntimeData.Position = pureMovementData.Position;
		RuntimeData.EndedPosition = pureMovementData.EndedPosition;
		RuntimeData.MoveSpeed = pureMovementData.MoveSpeed;
		RuntimeData.MoveDiraction = pureMovementData.MoveDirection;

		transform.position = RuntimeData.Position;
		transform.LookAt(RuntimeData.Position + RuntimeData.MoveDiraction);

		onTransformUpdate?.Invoke();
		
		float sqrRemainingdistance = (RuntimeData.Position - RuntimeData.EndedPosition).sqrMagnitude;
		float deltaMoveDistance = RuntimeData.MoveSpeed * pureMovementData.DeltaTime;
		if (sqrRemainingdistance < deltaMoveDistance * deltaMoveDistance)
		{
			onArrive?.Invoke(RuntimeData.Position);
		}
	}
	public void UpdateMovementJobData(ref MovementJobData pureMovementData)
	{
		if (target == null) return;
		RuntimeData.TargetPosition = target.HitTargetPosition;
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

