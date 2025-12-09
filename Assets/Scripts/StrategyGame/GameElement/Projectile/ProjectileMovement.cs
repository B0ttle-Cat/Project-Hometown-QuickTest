using UnityEngine;

public interface IProjectileMovement
{
	IProjectileMovement ThisMovement { get; }
	int OrderElementID { get; }
	int TargetElementID { get; }
	Vector3 StartPosition { get; }
	Vector3 TargetPosition { get; }
	Vector3 CurrentPosition { get; }
	float MoveSpeed { get; }
	Vector3 MoveDiraction { get; }
	public void SetTarget(IUnitCombatController order, ITargetableCombatant target);
	public void MovmentUpdate(in float deltaTime);
}

public class ProjectileMovement : MonoBehaviour, IProjectileMovement
{
	protected IUnitCombatController order;
	protected ITargetableCombatant target;
	protected Vector3 startPosition;
	protected Vector3 targetPosition;

	protected Vector3 currentPosition;
	protected float moveSpeed;
	protected Vector3 moveDiraction;
	IProjectileMovement IProjectileMovement.ThisMovement => this;
	int IProjectileMovement.OrderElementID => order.ThisElement.ID;
	int IProjectileMovement.TargetElementID => target.ThisElement.ID;
	Vector3 IProjectileMovement.StartPosition => startPosition;
	Vector3 IProjectileMovement.TargetPosition => targetPosition;
	Vector3 IProjectileMovement.CurrentPosition => currentPosition;
	float IProjectileMovement.MoveSpeed => moveSpeed;
	Vector3 IProjectileMovement.MoveDiraction => moveDiraction;

	protected float moveStartSpeed = 10f;
	protected bool isShiftSpeed = false;
	protected float moveMaxSpeed = 20f;
	protected AnimationCurve moveSpeedCurve = AnimationCurve.Linear(0,0,1,1);
	protected float timeFromStartToMaxSpeed = 1f;
	protected bool homingEnabled = false;
	protected float homingActivationDelay = 0f;
	protected float homingTurnSpeed = 180f;
	protected float homingTurnSpeedWhenMaxSpeed = 180f;
	protected float homingLimitAngle = 180;
	protected float homingLimitDistance = float.PositiveInfinity;
	protected bool cepEnabled = false;
	protected float cepRadius = 0f;
	protected float cepProbability = 50f;
	protected bool cepReapply = false;
	protected Vector2 cepReapplyMinMaxTime = new Vector2(0.5f, 1.5f);


	protected float updateTime;
	protected float homingLimitAngleCosine;
	protected float homingLimitSqrDistance;
	protected Vector3 cepOffsetXZ = Vector3.zero;
	protected float cepTimer = 0f;

	public void Init(ProjectileStatsData projectileStats)
	{
		moveStartSpeed = projectileStats.MoveStartSpeed;
		isShiftSpeed = projectileStats.IsShiftSpeed;
		moveMaxSpeed = projectileStats.MoveMaxSpeed;
		moveSpeedCurve = projectileStats.MoveSpeedCurve;
		timeFromStartToMaxSpeed = projectileStats.TimeFromStartToMaxSpeed;
		homingEnabled = projectileStats.HomingEnabled;
		homingActivationDelay = projectileStats.HomingActivationDelay;
		homingTurnSpeed = projectileStats.HomingTurnSpeed;
		homingTurnSpeedWhenMaxSpeed = projectileStats.HomingTurnSpeedWhenMaxSpeed;
		homingLimitAngle = projectileStats.HomingLimitAngle;
		homingLimitDistance = projectileStats.HomingLimitDistance;
		cepEnabled = projectileStats.CepEnabled;
		cepRadius = projectileStats.CepRadius;
		cepProbability = projectileStats.CepProbability;
		cepReapply = projectileStats.CepReapply;
		cepReapplyMinMaxTime = projectileStats.CepReapplyMinMaxTime;

		updateTime = 0;
		homingLimitAngleCosine = Mathf.Cos(homingLimitAngle * Mathf.Deg2Rad);
		homingLimitSqrDistance = float.IsPositiveInfinity(homingLimitDistance) ? float.PositiveInfinity : homingLimitDistance * homingLimitDistance;
		cepOffsetXZ = Vector3.zero;
		cepTimer = 0f;
		
		OnInit(projectileStats);
	}
	protected virtual void OnInit(ProjectileStatsData projectileStats){}
	internal void Deinit()
	{
		order = null;
		target = null;;
	}

	void IProjectileMovement.SetTarget(IUnitCombatController order, ITargetableCombatant target)
	{
		this.order = order;
		this.target = target;

		if (cepEnabled)
		{
			cepOffsetXZ = GenerateCEPOffset();
			cepTimer = Random.Range(cepReapplyMinMaxTime.x, cepReapplyMinMaxTime.y);
		}
		else
		{
			cepOffsetXZ = Vector3.zero;
			cepTimer = 0f;
		}

		startPosition = order.AttackStartPosition;
		targetPosition = target.HitTargetPosition + cepOffsetXZ;
		transform.position = currentPosition = startPosition;

		moveDiraction = (startPosition - targetPosition).normalized;
		moveSpeed = moveStartSpeed;

		OnSetTarget();
	}
	protected virtual void OnSetTarget(){}

	void IProjectileMovement.MovmentUpdate(in float deltaTime)
	{
		updateTime -= deltaTime;
		currentPosition = transform.position;

		UpdateMoveSpeed(in deltaTime);
		UpdateCepOffset(in deltaTime);
		UpdateTargetPosition(in deltaTime);

		UpdateTransform(in deltaTime);
	}
	protected virtual void UpdateMoveSpeed(in float deltaTime)
	{
		if (!isShiftSpeed) return;

		if (updateTime >= timeFromStartToMaxSpeed || timeFromStartToMaxSpeed < 0.01f)
		{
			moveSpeed = moveMaxSpeed;
		}
		else if (moveSpeed <= 0f)
		{
			moveSpeed = moveStartSpeed;
		}
		else
		{
			float timeRate = updateTime / timeFromStartToMaxSpeed;
			float shiftSpeed = moveSpeedCurve.Evaluate(timeRate);
			moveSpeed = Mathf.Lerp(moveStartSpeed, moveMaxSpeed, shiftSpeed);
		}
	}
	protected virtual void UpdateCepOffset(in float deltaTime)
	{
		if (!cepEnabled)
			return;

		// 재계산 OFF면, 초기에 만든 오프셋만 사용한다.
		if (!cepReapply)
			return;

		cepTimer -= deltaTime;
		if (cepTimer > 0f)
			return;

		// 재계산
		cepOffsetXZ = GenerateCEPOffset();

		// 다음 타이머 설정
		cepTimer = Random.Range(cepReapplyMinMaxTime.x, cepReapplyMinMaxTime.y);
	}
	protected virtual void UpdateTargetPosition(in float deltaTime)
	{
		if (!homingEnabled) return;

		targetPosition = target.HitTargetPosition + cepOffsetXZ;
		Vector3 toTarget = targetPosition - currentPosition;

		// 거리 제한 (무한이면 제한 없음)
		if (!float.IsPositiveInfinity(homingLimitDistance))
		{
			float dist = toTarget.magnitude;
			if (dist <= homingLimitDistance) return;
		}

		Vector3 newDir = toTarget.normalized;

		// 각도 제한 (-1 에 매우 근접 하거나, 보다 작으면 제한 없음 = 제한각이 180도 = 사실상 제한 없음)
		if (homingLimitAngleCosine >= -0.9999f)
		{
			float dot = Vector3.Dot(moveDiraction, newDir);
			if (dot < homingLimitAngleCosine) return;
		}

		// 회전 속도 계산
		float turnSpeed = homingTurnSpeed; // 기본값
		if (isShiftSpeed)
		{
			float denom = moveMaxSpeed - moveStartSpeed;
			float rateTurnSpeed = (denom > 0f)
				? Mathf.Clamp01((moveSpeed - moveStartSpeed) / denom)
				: 0f;

			turnSpeed = Mathf.Lerp(homingTurnSpeed, homingTurnSpeedWhenMaxSpeed, rateTurnSpeed);
		}

		// 초당 turnSpeed 도 만큼 회전 가능 → 라디안 변환
		float maxRadiansDelta = turnSpeed * Mathf.Deg2Rad * deltaTime;

		// 회전 적용
		moveDiraction = Vector3.RotateTowards(moveDiraction, newDir, maxRadiansDelta,0f);
	}
	protected virtual void UpdateTransform(in float deltaTime)
	{
		currentPosition += deltaTime * moveSpeed * moveDiraction;
		transform.SetPositionAndRotation(currentPosition, Quaternion.LookRotation(moveDiraction));
	}
	protected Vector3 GenerateCEPOffset()
	{
		if (cepRadius <= 0f)
			return Vector3.zero;

		// CEP 확률을 0~1 로
		float p = Mathf.Clamp01(cepProbability / 100f);

		// Rayleigh 분포 기반 scaling factor
		// r_p = σ * sqrt( -2 ln(1-p) )
		// 여기서 cepRadius = r_p
		//
		// → σ = cepRadius / sqrt(-2 ln(1-p))
		float denom = Mathf.Sqrt(-2f * Mathf.Log(1f - p));
		if (denom < 1e-6f)
			return Vector3.zero;

		float sigma = cepRadius / denom;

		// Rayleigh 난수: r = σ * sqrt(-2 ln(u))
		float u = Random.value;
		float r = sigma * Mathf.Sqrt(-2f * Mathf.Log(u));

		// 랜덤 방향
		float angle = Random.Range(0f, Mathf.PI * 2f);

		float x = r * Mathf.Cos(angle);
		float z = r * Mathf.Sin(angle);

		return new Vector3(x, 0f, z);
	}
}
