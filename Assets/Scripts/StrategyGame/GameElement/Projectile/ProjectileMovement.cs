using System;

using Unity.Collections;
using Unity.Mathematics;

using UnityEngine;

using Random = UnityEngine.Random;

public class ProjectileMovement : MonoBehaviour, IProjectileMovement
{
	[SerializeField] protected IUnitCombatController order;
	[SerializeField] protected ITargetableCombatant target;
	[SerializeField] protected Vector3 startPosition;
	[SerializeField] protected Vector3 targetPosition;
	[SerializeField] protected Vector3 prevPosition;
	[SerializeField] protected Vector3 currentPosition;
	[SerializeField] protected float moveSpeed;
	[SerializeField] protected Vector3 moveDiraction;

	private bool PureUpdateFlag;
	public struct MovementJobData : IDisposable
	{
		// transform / motion
		public float3 Position;
		public float3 PrevPosition;
		public float3 TargetPosition;
		public float3 MoveDirection;
		public float MoveSpeed;

		// delta
		public float DeltaTime;
		public float UpdateTime; // original code had lifeTime decreased each frame

		// speed shift (curve)
		public bool IsShiftSpeed; // 0/1
		public float MoveStartSpeed;
		public float MoveMaxSpeed;
		public float TimeFromStartToMaxSpeed;

		// homing
		public bool HomingEnabled; // 0/1
		public float HomingTurnSpeed;
		public float HomingTurnSpeedWhenMaxSpeed;
		public float HomingLimitAngleCosine; // precomputed cosine
		public float HomingLimitSqrDistance;

		// Cep Offset
		public float3 CepOffset;

		// Random
		public uint RandomState;

		// Per-instance sampled animation curve table (owned by Mono)
		// IMPORTANT: Mono must Alloc/Dispose this; Job only reads it.
		public NativeArray<float> MoveSpeedCurveTable;

		public void Dispose()
		{
			MoveSpeedCurveTable.Dispose();
		}

		// Helper to sample MoveSpeedCurveTable (normalized t in [0,1])
		public float SampleSpeedCurve(float normalizedT)
		{
			if (!MoveSpeedCurveTable.IsCreated || MoveSpeedCurveTable.Length == 0) return 1f;
			float t = math.clamp(normalizedT, 0f, 1f);
			int len = MoveSpeedCurveTable.Length;
			float idxF = t * (len - 1);
			int idx = (int)math.floor(idxF);
			int idx1 = math.min(idx + 1, len - 1);
			float a = MoveSpeedCurveTable[idx];
			float b = MoveSpeedCurveTable[idx1];
			float frac = idxF - idx;
			return math.lerp(a, b, frac);
		}
	}
	bool IProjectileMovement.RawDataUpdateFlag => PureUpdateFlag;
	IProjectileMovement IProjectileMovement.ThisMovement => this;
	int IProjectileMovement.OrderElementID => order.ThisElement.ID;
	int IProjectileMovement.TargetElementID => target.ThisElement.ID;
	Vector3 IProjectileMovement.StartPosition => startPosition;
	Vector3 IProjectileMovement.TargetPosition => targetPosition;
	Vector3 IProjectileMovement.PrevPosition => prevPosition;
	Vector3 IProjectileMovement.CurrentPosition => currentPosition;
	float IProjectileMovement.MoveSpeed => moveSpeed;
	Vector3 IProjectileMovement.MoveDiraction => moveDiraction;

	private ProjectileStatsData projectileStats;

	public void Init(ProjectileStatsData projectileStats)
	{
		this.projectileStats = projectileStats;

		OnInit(projectileStats);

		PureUpdateFlag = true;
	}
	protected virtual void OnInit(ProjectileStatsData projectileStats) { }
	internal void Deinit()
	{
		order = null;
		target = null;
	}

	void IProjectileMovement.SetTarget(IUnitCombatController order, ITargetableCombatant target)
	{
		this.order = order;
		this.target = target;


		OnSetTarget();
		PureUpdateFlag = true;
	}
	protected virtual void OnSetTarget() { }
	public void InitPureMovementData(out MovementJobData pureMovementData)
	{
		startPosition = order.AttackStartPosition;
		targetPosition = target.HitTargetPosition;

		Vector3 cepOffset = projectileStats.CepEnabled ? GenerateCEPOffset(projectileStats.CepRadius, projectileStats.CepProbability) : Vector3.zero;

		pureMovementData = new MovementJobData
		{
			Position = startPosition,
			PrevPosition = startPosition,
			TargetPosition = targetPosition,
			MoveDirection = (targetPosition + cepOffset - startPosition).normalized,
			MoveSpeed = projectileStats.MoveStartSpeed,
			DeltaTime = 0,
			UpdateTime = 0,

			IsShiftSpeed = projectileStats.IsShiftSpeed,
			MoveStartSpeed = projectileStats.MoveStartSpeed,
			MoveMaxSpeed = projectileStats.MoveMaxSpeed,
			MoveSpeedCurveTable = PrepareCurve(projectileStats.MoveSpeedCurve),
			TimeFromStartToMaxSpeed = projectileStats.TimeFromStartToMaxSpeed,

			HomingEnabled = projectileStats.HomingEnabled,
			HomingTurnSpeed = projectileStats.HomingTurnSpeed,
			HomingTurnSpeedWhenMaxSpeed = projectileStats.HomingTurnSpeedWhenMaxSpeed,
			HomingLimitAngleCosine = projectileStats.HomingLimitAngleCosine,
			HomingLimitSqrDistance = projectileStats.HomingLimitSqrDistance,

			CepOffset = cepOffset,

			RandomState = (uint)Random.value*10000,
		};

		PureUpdateFlag = false;

		NativeArray<float> PrepareCurve(AnimationCurve curve, int resolution = 128)
		{
			var curveTable = new NativeArray<float>(resolution, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

			for (int i = 0 ; i < resolution ; i++)
			{
				float t = (float)i / (resolution - 1);
				curveTable[i] = curve.Evaluate(t);
			}

			return curveTable;
		}
	}
	public void ApplyJobResult(in MovementJobData pureMovementData)
	{
		prevPosition = pureMovementData.PrevPosition;
		currentPosition = pureMovementData.Position;
		moveSpeed = pureMovementData.MoveSpeed;
		moveDiraction = pureMovementData.MoveDirection;
	}

	public void UpdatePureMovementData(ref MovementJobData pureMovementData)
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

