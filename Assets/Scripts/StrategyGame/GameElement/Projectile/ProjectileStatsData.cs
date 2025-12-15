using System;

using Sirenix.OdinInspector;

using Unity.Collections;

using UnityEngine;

using static ProjectileMovement;
using static StrategyGamePlayData;


[Serializable]
public record ProjectileStatsData // ProfileStats
{
	#region ProfileState
	[SerializeField,HideIf("@true")]
	private ProjectileKey projectileKey;

	[Title("StatsData_old")]
	[LabelText("공격 속성"), SerializeField]
	private WeaponType weaponType;

	[BoxGroup("Movement")]
	[LabelText("이동 시작 속도"), SerializeField]
	private float moveStartSpeed = 10f;
	[ToggleGroup("isShiftSpeed", GroupID = "Movement/T", ToggleGroupTitle = "가속 여부"), SerializeField]
	private bool isShiftSpeed = false;
	[ToggleGroup("isShiftSpeed",GroupID = "Movement/T"), LabelText("최대 속도"), SerializeField]
	private float moveMaxSpeed = 20f;
	[ToggleGroup("isShiftSpeed",GroupID = "Movement/T"), LabelText("속도 커브"), SerializeField]
	private AnimationCurve moveSpeedCurve = AnimationCurve.Linear(0,0,1,1);
	[ToggleGroup("isShiftSpeed",GroupID = "Movement/T"), LabelText("최대 속도 도달 시간"), SerializeField]
	private float timeFromStartToMaxSpeed = 1f;

	[ToggleGroup("homingEnabled", GroupID = "Movement/H", ToggleGroupTitle ="유도 여부"), SerializeField]
	private bool homingEnabled = false;
	[ToggleGroup("homingEnabled", GroupID = "Movement/H"), LabelText("유도 활성 지연"), SerializeField]
	private float homingActivationDelay = 0f;
	[ToggleGroup("homingEnabled", GroupID = "Movement/H"), LabelText("유도 회전 속도"), SerializeField]
	private float homingTurnSpeed = 180f;
	[ToggleGroup("homingEnabled", GroupID = "Movement/H"), LabelText("MaxSpeed 일때 회전 속도"), ShowIf("isShiftSpeed"), SerializeField]
	private float homingTurnSpeedWhenMaxSpeed = 180f;
	[ToggleGroup("homingEnabled", GroupID = "Movement/H"), LabelText("유도 한계 각도"), SerializeField]
	[Range(0f,180f)]
	private float homingLimitAngle = 180;
	[ToggleGroup("homingEnabled", GroupID = "Movement/H"), LabelText("유도 한계 거리"), SerializeField]
	private float homingLimitDistance = float.PositiveInfinity;

	[ToggleGroup("cepEnabled", GroupID = "Movement/C", ToggleGroupTitle ="공산오차 적용 여부"), SerializeField]
	private bool cepEnabled;
	[ToggleGroup("cepEnabled", GroupID = "Movement/C"), LabelText("공산오차  기준거리"), SerializeField]
	private float cepDistance;
	[ToggleGroup("cepEnabled", GroupID = "Movement/C"), LabelText("공산오차 반경"), SerializeField]
	private float cepRadius;
	[ToggleGroup("cepEnabled", GroupID = "Movement/C"), LabelText("반경 내 들어갈 확률"), SerializeField]
	[Range(0f,1f)]
	private float cepProbability;
	[ToggleGroup("cepEnabled", GroupID = "Movement/C"), LabelText("가로 비율"), SerializeField]
	[Range(0f,1f)]
	[HorizontalGroup("Movement/C/H")]
	public float cepWidthScale = 1f;
	[ToggleGroup("cepEnabled", GroupID = "Movement/C"), LabelText("높이 비율"), SerializeField]
	[Range(0f,1f)]
	[HorizontalGroup("Movement/C/H")]
	public float cepHeaghtScale = 1f;
	[ToggleGroup("cepEnabled", GroupID = "Movement/C"), LabelText("길이 비율"), SerializeField]
	[Range(0f,1f)]
	[HorizontalGroup("Movement/C/H")]
	public float cepLengthScale = 1f;

	[BoxGroup("LifeCycle"), LabelText("생존 시간"), SerializeField]
	private float lifeTime = 5f;

	[BoxGroup("Collision"), LabelText("충돌 반경"), SerializeField]
	private float collisionRadius = 0.1f;

	[BoxGroup("Hit"), LabelText("명중시 상태이상 플래그"), SerializeField]
	private StatusEffectsFlag hitEffectsFlag = StatusEffectsFlag.None;
#if UNITY_EDITOR
	private bool _isHitStatusEffects => hitEffectsFlag != StatusEffectsFlag.None;
	[EnableIf("_isHitStatusEffects")]
#endif
	[BoxGroup("Hit"), LabelText("명중시 상태이상 시간 배율"), SerializeField]
	private float hitEffectsTimeMultiplier = 1;

	[BoxGroup("Hit"), LabelText("Last Hit Effect 타입"), SerializeField]
	private SubEffectKey lastHitEffectKey;
	public bool PiercingEnable => weaponType is WeaponType.관통 or WeaponType.관통특화;
	public bool ExplosionEnabled => weaponType is WeaponType.폭발 or WeaponType.폭발특화;
	public bool EmpShockEnabled => weaponType is WeaponType.에너지;

	[BoxGroup("Hit/P", GroupName ="관통", VisibleIf = "PiercingEnable"),  LabelText("관통 최대 점수"), SerializeField]
	private int piercingMaxCount;
	[BoxGroup("Hit/P"), LabelText("관통 효과 감쇠 커브(관통횟수)"), SerializeField]
	private AnimationCurve piercingFalloffCurve;

	[BoxGroup("Hit/E", GroupName ="폭발", VisibleIf = "ExplosionEnabled"), LabelText("폭발 최소/최대 반경"), SerializeField]
	private Vector2 explosionMinMaxRadius;
	[BoxGroup("Hit/E"), LabelText("폭발 지연시간"), SerializeField]
	private float explosionDelayAfterHit = 0.1f;
	[BoxGroup("Hit/E"), LabelText("폭발 효과 감쇠 커브(거리)"), SerializeField]
	private AnimationCurve explosionFalloffCurve;
	[BoxGroup("Hit/E"), LabelText("폭발 Effect 타입"), SerializeField]
	private SubEffectKey explosionEffectKey;


	[BoxGroup("Hit/S", GroupName ="EMP충격", VisibleIf = "EmpShockEnabled"), LabelText("전파 전달 거리"), SerializeField]
	private float empShockPropagationDistance;
	[BoxGroup("Hit/S"), LabelText("EMP 동시 전파 수"), SerializeField]
	private int empShockChainCount;
	[BoxGroup("Hit/S"), LabelText("EMP 전파 횟수"), SerializeField]
	private int empShockDepthCount;
	[BoxGroup("Hit/S"), LabelText("EMP 중첩 횟수"), SerializeField]
	private int empShockOverlapsCount;
	[BoxGroup("Hit/S"), LabelText("EMP 효과 감쇠 커브(Depth)"), SerializeField]
	private AnimationCurve empShockFalloffCurve;
	[BoxGroup("Hit/S"), LabelText("EMP 충격 Effect 타입"), SerializeField]
	private SubEffectKey empShockEffectKey;


	public ProjectileStatsData()
	{
	}
	public ProjectileStatsData(ProjectileProfileObject profile)
	{
		if (profile == null) return;
		var statsData = profile.statsData;
		if (statsData == null) return;

		projectileKey = statsData.ProjectileKey;

		weaponType = statsData.WeaponType;

		moveStartSpeed = statsData.MoveStartSpeed;
		isShiftSpeed = statsData.IsShiftSpeed;
		moveMaxSpeed = statsData.MoveMaxSpeed;
		moveSpeedCurve = statsData.MoveSpeedCurve;
		timeFromStartToMaxSpeed = statsData.TimeFromStartToMaxSpeed;

		homingEnabled = statsData.HomingEnabled;
		homingTurnSpeed = statsData.HomingTurnSpeed;
		homingTurnSpeedWhenMaxSpeed = statsData.HomingTurnSpeedWhenMaxSpeed;
		homingActivationDelay = statsData.HomingActivationDelay;
		homingLimitAngle = statsData.HomingLimitAngle;
		homingLimitDistance = statsData.HomingLimitDistance;

		cepEnabled = statsData.CepEnabled;
		cepRadius = statsData.CepRadius;
		cepProbability = statsData.CepProbability;

		lifeTime = statsData.LifeTime;

		collisionRadius = statsData.CollisionRadius;

		hitEffectsFlag = statsData.HitEffectsFlag;
		hitEffectsTimeMultiplier = statsData.HitEffectsTimeMultiplier;

		piercingMaxCount = statsData.PiercingMaxCount;
		piercingFalloffCurve = statsData.PiercingFalloffCurve;

		explosionMinMaxRadius = statsData.ExplosionMinMaxRadius;
		explosionDelayAfterHit = statsData.ExplosionDelayAfterHit;
		explosionFalloffCurve = statsData.ExplosionFalloffCurve;
		explosionEffectKey = statsData.ExplosionEffectKey;

		empShockPropagationDistance = statsData.EmpShockPropagationDistance;
		empShockChainCount = statsData.EmpShockChainCount;
		empShockDepthCount = statsData.EmpShockDepthCount;
		empShockOverlapsCount = statsData.EmpShockOverlapsCount;
		empShockFalloffCurve = statsData.EmpShockFalloffCurve;
		empShockEffectKey = statsData.EmpShockEffectKey;
	}

	public ProjectileStatsData(ProjectileKey projectileKey = ProjectileKey.None, WeaponType weaponType = WeaponType.일반,
		float moveStartSpeed = 10f, bool isShiftSpeed = false, float moveMaxSpeed = 20f, AnimationCurve moveSpeedCurve = null, float timeFromStartToMaxSpeed = 2f,
		bool homingEnabled = false, float homingActivationDelay = 0f, float homingTurnSpeed = 180f, float homingTurnSpeedWhenMaxSpeed = 180f, float homingLimitAngle = 180f, float homingLimitDistance = float.PositiveInfinity,
		bool cepEnabled = false, float cepDistance = 10f, float cepScale = 3, float cepRadius = 0.5f, float cepProbability = 0.9f, float cepWidthScale = 0.5f, float cepLengthScale = 1f, float cepHeaghtScale = 1f,
		float lifeTime = 1f,
		float collisionRadius = 0.1f,
		StatusEffectsFlag hitEffectsFlag = StatusEffectsFlag.None, float hitEffectsTimeMultiplier = 1f, SubEffectKey projectileHitEffectKey = SubEffectKey.None,
		bool piercingEnable = false, int PiercingMaxCount = default, AnimationCurve piercingFalloffCurve = null,
		bool explosionEnabled = false, Vector2 explosionMinMaxRadius = default, float explosionDelayAfterHit = 0f, AnimationCurve explosionFalloffCurve = null, SubEffectKey explosionEffectKey = SubEffectKey.폭발_소형,
		float empShockPropagationDistance = 5f, int empShockChainCount = 3, int empShockDepthCount = 5, int empShockOverlapsCount = 1, AnimationCurve empShockFalloffCurve = null, SubEffectKey empShockEffectKey = SubEffectKey.EMP충격_소형)
	{

		this.projectileKey = projectileKey;

		this.weaponType = weaponType;

		this.moveStartSpeed = moveStartSpeed;
		this.isShiftSpeed = isShiftSpeed;
		this.moveMaxSpeed = moveMaxSpeed;
		this.moveSpeedCurve = moveSpeedCurve ?? AnimationCurve.Linear(0, 0, 1, 1);
		this.timeFromStartToMaxSpeed = timeFromStartToMaxSpeed;

		this.homingEnabled = homingEnabled;
		this.homingActivationDelay = homingActivationDelay;
		this.homingTurnSpeed = homingTurnSpeed;
		this.homingTurnSpeedWhenMaxSpeed = homingTurnSpeedWhenMaxSpeed;
		this.homingLimitAngle = homingLimitAngle;
		this.homingLimitDistance = homingLimitDistance;

		this.cepEnabled = cepEnabled;
		this.cepDistance = cepDistance;
		this.cepRadius = cepRadius;
		this.cepProbability = cepProbability;
		this.cepWidthScale = cepWidthScale;
		this.cepHeaghtScale = cepHeaghtScale;
		this.cepLengthScale = cepLengthScale;

		this.lifeTime = lifeTime;
		this.collisionRadius = collisionRadius;

		this.hitEffectsFlag = hitEffectsFlag;
		this.hitEffectsTimeMultiplier = hitEffectsTimeMultiplier;
		this.lastHitEffectKey = projectileHitEffectKey;

		this.piercingMaxCount = PiercingMaxCount;
		this.piercingFalloffCurve = piercingFalloffCurve ?? AnimationCurve.Linear(0, 1, 1, 0);

		this.explosionMinMaxRadius = explosionMinMaxRadius;
		this.explosionDelayAfterHit = explosionDelayAfterHit;
		this.explosionFalloffCurve = explosionFalloffCurve ?? AnimationCurve.Linear(0, 1, 1, 0);
		this.explosionEffectKey = explosionEffectKey;

		this.empShockPropagationDistance = empShockPropagationDistance;
		this.empShockChainCount = empShockChainCount;
		this.empShockDepthCount = empShockDepthCount;
		this.empShockOverlapsCount = empShockOverlapsCount;
		this.empShockFalloffCurve = empShockFalloffCurve ?? AnimationCurve.Linear(0, 1, 1, 0);
		this.empShockEffectKey = empShockEffectKey;
	}

	public ProjectileStatsData Copy()
	{
		return new ProjectileStatsData()
		{
			projectileKey = this.projectileKey,

			weaponType = this.weaponType,

			moveStartSpeed = this.moveStartSpeed,
			isShiftSpeed = this.isShiftSpeed,
			moveMaxSpeed = this.moveMaxSpeed,
			moveSpeedCurve = this.moveSpeedCurve,
			timeFromStartToMaxSpeed = this.timeFromStartToMaxSpeed,

			homingEnabled = this.homingEnabled,
			homingTurnSpeed = this.homingTurnSpeed,
			homingTurnSpeedWhenMaxSpeed = this.homingTurnSpeedWhenMaxSpeed,
			homingActivationDelay = this.homingActivationDelay,
			homingLimitAngle = this.homingLimitAngle,
			homingLimitDistance = this.homingLimitDistance,

			cepEnabled =		this.cepEnabled,
			cepDistance =		this.cepDistance,
			cepRadius =		this.cepRadius,
			cepProbability =	this.cepProbability,
			cepWidthScale =		this.cepWidthScale,
			cepHeaghtScale =	this.cepHeaghtScale,
			cepLengthScale =	this.cepLengthScale,

			lifeTime = this.lifeTime,

			collisionRadius = this.collisionRadius,

			hitEffectsFlag = this.hitEffectsFlag,
			hitEffectsTimeMultiplier = this.hitEffectsTimeMultiplier,
			lastHitEffectKey = this.lastHitEffectKey,

			piercingMaxCount = this.piercingMaxCount,
			piercingFalloffCurve = this.piercingFalloffCurve,

			explosionMinMaxRadius = this.explosionMinMaxRadius,
			explosionDelayAfterHit = this.explosionDelayAfterHit,
			explosionFalloffCurve = this.explosionFalloffCurve,
			explosionEffectKey = this.explosionEffectKey,

			empShockPropagationDistance = this.empShockPropagationDistance,
			empShockChainCount = this.empShockChainCount,
			empShockDepthCount = this.empShockDepthCount,
			empShockOverlapsCount = this.empShockOverlapsCount,
			empShockFalloffCurve = this.empShockFalloffCurve,
			empShockEffectKey = this.empShockEffectKey,
		};
	}


	public ProjectileKey ProjectileKey => projectileKey;
	public WeaponType WeaponType => weaponType;
	public float MoveStartSpeed => moveStartSpeed;
	public bool IsShiftSpeed => isShiftSpeed;
	public float MoveMaxSpeed => moveMaxSpeed;
	public AnimationCurve MoveSpeedCurve => moveSpeedCurve;
	public float TimeFromStartToMaxSpeed => timeFromStartToMaxSpeed;
	public bool HomingEnabled => homingEnabled;
	public float HomingTurnSpeed => homingTurnSpeed;
	public float HomingTurnSpeedWhenMaxSpeed => homingTurnSpeedWhenMaxSpeed;
	public float HomingActivationDelay => homingActivationDelay;
	public float HomingLimitAngle => homingLimitAngle;
	public float HomingLimitDistance => homingLimitDistance;

	public float HomingLimitAngleCosine => Mathf.Cos(homingLimitAngle * Mathf.Deg2Rad);
	public float HomingLimitSqrDistance => float.IsPositiveInfinity(homingLimitDistance) ? float.PositiveInfinity : homingLimitDistance * homingLimitDistance;

	public bool CepEnabled => cepEnabled;
	public float CepDistance => Mathf.Max(cepDistance, 0.001f);
	public float CepRadius => Mathf.Max(cepRadius, 0.001f);
	public float CepProbability => Mathf.Clamp(cepProbability, 0.001f, 1f);
	public Vector3 CepScaleVector3 => new Vector3(cepWidthScale, cepHeaghtScale, cepLengthScale);
	public float LifeTime => lifeTime;
	public float CollisionRadius => collisionRadius;
	public StatusEffectsFlag HitEffectsFlag => hitEffectsFlag;
	public float HitEffectsTimeMultiplier => hitEffectsTimeMultiplier;
	public SubEffectKey ProjectileHitEffectKey => lastHitEffectKey;
	public int PiercingMaxCount => Mathf.Max(1, piercingMaxCount);
	public AnimationCurve PiercingFalloffCurve => piercingFalloffCurve;
	public Vector2 ExplosionMinMaxRadius => explosionMinMaxRadius;
	public float ExplosionDelayAfterHit => Mathf.Max(0, explosionDelayAfterHit);
	public AnimationCurve ExplosionFalloffCurve => explosionFalloffCurve;
	public SubEffectKey ExplosionEffectKey => explosionEffectKey;
	public float EmpShockPropagationDistance => empShockPropagationDistance;
	public int EmpShockChainCount => empShockChainCount;
	public int EmpShockDepthCount => empShockDepthCount;
	public int EmpShockOverlapsCount => empShockOverlapsCount;
	public AnimationCurve EmpShockFalloffCurve => empShockFalloffCurve;
	public SubEffectKey EmpShockEffectKey => empShockEffectKey;

	public float PiercingFalloffMultiplier(int currentCount)
	{
		if (!PiercingEnable) return 1f;
		int max = PiercingMaxCount;
		float rate = (float)currentCount / (float)max;
		return PiercingFalloffCurve.Evaluate(rate);
	}
	public float ExplosionFalloffMultiplier(float currentDistance)
	{
		if (!ExplosionEnabled) return 1f;
		Vector2 minMax = ExplosionMinMaxRadius;
		float min = Mathf.Min(minMax.x, minMax.y);
		float max = Mathf.Max(minMax.x, minMax.y);
		float point = currentDistance;
		if (Mathf.Approximately(min, max))
		{
			return 1f;
		}
		float rate = (point - min) / (max - min);
		return ExplosionFalloffCurve.Evaluate(rate);
	}
	public float EmpShockFalloffMultiplier(int currentDepth)
	{
		if (!EmpShockEnabled) return 1f;
		int max = EmpShockDepthCount;
		if (max <= 1) return 1f;

		float rate = (float)currentDepth / (float)max;
		return EmpShockFalloffCurve.Evaluate(rate);
	}
	/// <summary>
	/// 목표 방향에 공산 오차를 적용하여 최종 착탄 지점까지의 벡터를 계산합니다.
	/// </summary>
	/// <param name="targetDirection">목표물의 방향 (정규화될 필요는 없지만, 방향 정보가 있어야 함).</param>
	/// <param name="distanceD">단기 공산 위치 계산을 위한 기준 거리 (D).</param>
	/// <param name="radiusR">투사체의 N%가 착탄될 반경 비율 (R) [0, 1].</param>
	/// <param name="percentN">착탄될 투사체의 비율 (N) [0, 1].</param>
	/// <param name="cepScale">착탄될 범위의 Vector3 스케일. Z: 목표 방향 스케일, XY: 수직면 스케일.</param>
	/// <returns>원점에서 최종 착탄 지점으로 향하는 방향 벡터.</returns>
	public Vector3 CalculateCEPDiraction(Vector3 targetDirection)
	{
		return CalculateCEPDiraction(targetDirection, Vector3.up);
	}

	public Vector3 CalculateCEPDiraction(Vector3 targetDirection,Vector3 directionUp) 
	{
		if (!CepEnabled) return targetDirection;


		float distanceD = CepDistance;
		float radiusR = CepRadius;
		float percentN = CepProbability;
		Vector3 cepScale = CepScaleVector3;

		Vector3 V1 = targetDirection.normalized; // Z축 (Forward)
		if (V1.sqrMagnitude < 0.001f)
		{
			return Vector3.forward * distanceD;
		}

		// 2. 표준 편차 (Sigma) 계산
		float n_factor = GetSigmaFactor(percentN);
		float n_factor_inv = (n_factor > float.Epsilon) ? 1.0f / n_factor : 0f;

		float sigmaXY = radiusR * n_factor_inv;
		float sigmaZ = radiusR * n_factor_inv;

		// 3. 3D 가우시안 랜덤 샘플링 (클램핑 로직 추가됨)
		// N(0, 1)을 따르는 독립적인 Zx, Zy, Zz 생성

		// 99.99% 확률 반경에 해당하는 표준 계수
		const float N_FACTOR_MAX = 4.0f; // GetSigmaFactor(0.9999f)의 결과

		// Zx, Zy 쌍 생성 (2D Radial)
		float u1 = UnityEngine.Random.value;
		float u2 = UnityEngine.Random.value;
		float mag1 = Mathf.Sqrt(-2f * Mathf.Log(u1)); // 2D 표준 가우시안 반경

		// **2D Radial 클램핑**: 99.99% 반경(4.0 sigma)을 초과하는 샘플은 잘라냅니다.
		mag1 = Mathf.Min(mag1, N_FACTOR_MAX);

		float randX_std = mag1 * Mathf.Cos(2f * Mathf.PI * u2);
		float randY_std = mag1 * Mathf.Sin(2f * Mathf.PI * u2);

		// Zz 값 생성 (1D Normal)
		float u3 = UnityEngine.Random.value;
		float u4 = UnityEngine.Random.value;
		float mag2 = Mathf.Sqrt(-2f * Mathf.Log(u3));
		float randZ_std = mag2 * Mathf.Cos(2f * Mathf.PI * u4);

		// **1D Longitudinal 클램핑**: Zz의 절댓값을 4.0 sigma로 제한합니다.
		randZ_std = Mathf.Clamp(randZ_std, -N_FACTOR_MAX, N_FACTOR_MAX);


		// 4. 목표 지역 좌표계 (V1, V2, V3) 계산

		Vector3 V2; // X축 (Right, cepScale.x 적용)
		Vector3 V3; // Y축 (Up, cepScale.y 적용)

		// V2를 먼저 계산: V1과 directionUp에 모두 직교하는 벡터 (Right)
		V2 = Vector3.Cross(V1, directionUp).normalized;

		// V1과 directionUp이 평행하여 V2가 영벡터(Zero Vector)가 되는 경우 처리
		if (V2.sqrMagnitude < 0.001f)
		{
			if (Mathf.Abs(V1.y) > 0.99f)
				V2 = Vector3.right; // 월드 X축 사용
			else
				V2 = Vector3.Cross(V1, Vector3.up).normalized;
		}

		// V3를 다시 계산: V1과 V2에 모두 직교하는 벡터 (Up)
		V3 = Vector3.Cross(V2, V1).normalized;

		// 5. 표준 편차 및 스케일 적용하여 최종 오차 거리 계산
		float errorX = randX_std * sigmaXY * cepScale.x; // V2 방향 오차
		float errorY = randY_std * sigmaXY * cepScale.y; // V3 방향 오차
		float errorZ = randZ_std * sigmaZ * cepScale.z; // V1 방향 오차

		// 최종 착탄 지점의 오차 벡터
		Vector3 finalErrorVector = errorX * V2 + errorY * V3;

		// 6. 최종 착탄 지점 계산
		float finalDistance = distanceD + errorZ;

		Vector3 finalTargetVector = V1 * finalDistance + finalErrorVector;

		return finalTargetVector / distanceD;
	}
	public Vector3 CalculateCEPPosition(Vector3 startPosition, Vector3 targetPosition, Vector3 directionUp)
	{
		if (!CepEnabled) return targetPosition;

		Vector3 directionVector = targetPosition - startPosition;
		float actualDistance = directionVector.magnitude; // D_actual
		Vector3 targetDirection = directionVector.normalized; // V1

		if (actualDistance < 0.001) actualDistance = 0.001f;
		float stdDistance = CepDistance; // D_std

		float distanceFactorK = actualDistance / stdDistance;

		// 1. 표준 거리에서의 시뮬레이션 착탄 지점 (P_std) 획득
		Vector3 calculateDiraction = CalculateCEPDiraction(targetDirection, directionUp);
		return targetPosition + calculateDiraction * distanceFactorK;
	}


	// GetSigmaFactor는 그대로 사용합니다.
	public static float GetSigmaFactor(float probability)
	{
		// 1.0에 가까워지면 log(0)이 되어 무한대로 발산하므로, 최대 확률을 제한합니다.
		if (probability >= 0.9999f) return 4.0f;
		if (probability <= 0.0f) return 0.0f;
		return Mathf.Sqrt(-2f * Mathf.Log(1f - probability));
	}

	public MovmentConstantData GetMovementConstantData()
	{
		return new MovmentConstantData
		{
			IsShiftSpeed = this.isShiftSpeed,
			MoveStartSpeed = this.moveStartSpeed,
			MoveMaxSpeed = this.moveMaxSpeed,
			TimeFromStartToMaxSpeed = this.timeFromStartToMaxSpeed,

			HomingEnabled = this.homingEnabled,
			HomingTurnSpeed = this.homingTurnSpeed,
			HomingTurnSpeedWhenMaxSpeed = this.homingTurnSpeedWhenMaxSpeed,

			// 미리 계산된 코사인/제곱 거리를 사용합니다.
			HomingLimitAngleCosine = this.HomingLimitAngleCosine,
			HomingLimitSqrDistance = this.HomingLimitSqrDistance
		};
	}


	public static float[] PrepareAnimationCurve(AnimationCurve curve, int resolution = 128)
	{
		var curveTable = new float[resolution];

		for (int i = 0 ; i < resolution ; i++)
		{
			float t = (float)i / (resolution - 1);
			curveTable[i] = curve.Evaluate(t);
		}

		return curveTable;
	}
	public NativeArray<float> GetMovementCurveData(int resolution, Allocator allocator)
	{
		// PrepareAnimationCurve를 사용하여 float[]를 얻고 NativeArray로 변환
		float[] array = PrepareAnimationCurve(this.moveSpeedCurve, resolution);
		return new NativeArray<float>(array, allocator);
	}
	#endregion
}