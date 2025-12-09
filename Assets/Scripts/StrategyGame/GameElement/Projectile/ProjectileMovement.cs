using Unity.Collections;

using UnityEngine;

using static StrategyManagerModule.StrategyUpdate.StrategyUpdate_ProjectileMovement;

public interface IProjectileMovement
{
	IProjectileMovement ThisMovement { get; }
	int OrderElementID { get; }
	int TargetElementID { get; }
	Vector3 StartPosition { get; }
	Vector3 TargetPosition { get; }
	Vector3 PrevPosition { get; }
	Vector3 CurrentPosition { get; }
	float MoveSpeed { get; }
	Vector3 MoveDiraction { get; }
	public void SetTarget(IUnitCombatController order, ITargetableCombatant target);
	public void ApplyJobResult(in PureMovementData pureMovementData);
	public bool PureUpdateFlag { get; }
	public void InitPureMovementData(out PureMovementData pureMovementData);
	public void UpdatePureMovementData(ref PureMovementData pureMovementData);
}

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
	bool IProjectileMovement.PureUpdateFlag => PureUpdateFlag;
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
	public void InitPureMovementData(out PureMovementData pureMovementData)
	{
		startPosition = order.AttackStartPosition;
		targetPosition = target.HitTargetPosition;

		Vector3 cepOffset = projectileStats.CepEnabled ? GenerateCEPOffset(projectileStats.CepRadius, projectileStats.CepProbability) : Vector3.zero;

		pureMovementData = new PureMovementData
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

			RandomState = (uint)Random.value,
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
	public void ApplyJobResult(in PureMovementData pureMovementData)
	{
		prevPosition = pureMovementData.PrevPosition;
		currentPosition = pureMovementData.Position;
		moveSpeed = pureMovementData.MoveSpeed;
		moveDiraction = pureMovementData.MoveDirection;
	}

	public void UpdatePureMovementData(ref PureMovementData pureMovementData)
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

