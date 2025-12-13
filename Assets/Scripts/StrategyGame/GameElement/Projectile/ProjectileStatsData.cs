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

	[Title("StatsData")]
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
	[ToggleGroup("cepEnabled", GroupID = "Movement/C"), LabelText("공산오차 반경"), SerializeField]
	private float cepRadius;
	[ToggleGroup("cepEnabled", GroupID = "Movement/C"), LabelText("반경 내 들어갈 확률"), SerializeField]
	private float cepProbability; 

	[BoxGroup("LifeCycle"), LabelText("생존 시간"), SerializeField]
	private float lifeTime = 5f;

	[BoxGroup("Collision"), LabelText("충돌 반경"), SerializeField]
	private float collisionRadius = 0.1f;

	[BoxGroup("Hit"), LabelText("명중시 피해 배율"), SerializeField]
	private float hitDamageMultiplier = 1f;
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
	public bool ElectromagneticEnabled => weaponType is WeaponType.에너지;

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


	[BoxGroup("Hit/Emp", GroupName ="EMP충격", VisibleIf = "ElectromagneticEnabled"), LabelText("전파 전달 거리"), SerializeField]
	private float empChainPropagationDistance;
	[BoxGroup("Hit/Emp"), LabelText("EMP 동시 전파 수"), SerializeField]
	private int empChainPropagationCount;
	[BoxGroup("Hit/Emp"), LabelText("EMP 전파 횟수"), SerializeField]
	private int empChainDepthCount;
	[BoxGroup("Hit/Emp"), LabelText("EMP 중첩 횟수"), SerializeField]
	private int empChainOverlapsCount;
	[BoxGroup("Hit/Emp"), LabelText("EMP 효과 감쇠 커브(Depth)"), SerializeField]
	private AnimationCurve empChainFalloffCurve;
	[BoxGroup("Hit/Emp"), LabelText("EMP 충격 Effect 타입"), SerializeField]
	private SubEffectKey empChainEffectKey;


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

		hitDamageMultiplier = statsData.HitDamageMultiplier;
		hitEffectsFlag = statsData.HitEffectsFlag;
		hitEffectsTimeMultiplier = statsData.HitEffectsTimeMultiplier;

		piercingMaxCount = statsData.PiercingMaxCount;
		piercingFalloffCurve = statsData.PiercingFalloffCurve;

		explosionMinMaxRadius = statsData.ExplosionMinMaxRadius;
		explosionDelayAfterHit = statsData.ExplosionDelayAfterHit;
		explosionFalloffCurve = statsData.ExplosionFalloffCurve;
		explosionEffectKey = statsData.ExplosionEffectKey;

		empChainPropagationDistance = statsData.EmpChainPropagationDistance;
		empChainPropagationCount = statsData.EmpChainPropagationCount;
		empChainDepthCount = statsData.EmpChainDepthCount;
		empChainOverlapsCount = statsData.EmpChainOverlapsCount;
		empChainFalloffCurve = statsData.EmpChainFalloffCurve;
		empChainEffectKey = statsData.EmpChainEffectKey;
	}

	public ProjectileStatsData(ProjectileKey projectileKey = ProjectileKey.None, WeaponType weaponType = WeaponType.일반,
		float moveStartSpeed = 10f, bool isShiftSpeed = false, float moveMaxSpeed = 20f, AnimationCurve moveSpeedCurve = null, float timeFromStartToMaxSpeed = 2f,
		bool homingEnabled = false, float homingActivationDelay = 0f, float homingTurnSpeed = 180f, float homingTurnSpeedWhenMaxSpeed = 180f, float homingLimitAngle = 180f, float homingLimitDistance = float.PositiveInfinity,
		bool cepEnabled = false, float cepRadius = 3f, float cepProbability = 0.9f,
		float lifeTime = 1f,
		float collisionRadius = 0.1f,
		float hitDamageMultiplier = 1f, StatusEffectsFlag hitEffectsFlag = StatusEffectsFlag.None, float hitEffectsTimeMultiplier = 1f, SubEffectKey projectileHitEffectKey = SubEffectKey.None,
		bool piercingEnable = false, int PiercingMaxCount = default, AnimationCurve piercingFalloffCurve = null,
		bool explosionEnabled = false, Vector2 explosionMinMaxRadius = default, float explosionDelayAfterHit = 0f, AnimationCurve explosionFalloffCurve = null, SubEffectKey explosionEffectKey = SubEffectKey.폭발_소형,
		float empChainPropagationDistance = 5f, int empChainPropagationCount = 3, int empChainDepthCount = 5, int empChainOverlapsCount = 1, AnimationCurve empChainFalloffCurve = null, SubEffectKey empChainEffectKey = SubEffectKey.전격_소형)
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
		this.cepRadius = cepRadius;
		this.cepProbability = cepProbability;

		this.lifeTime = lifeTime;
		this.collisionRadius = collisionRadius;

		this.hitDamageMultiplier = hitDamageMultiplier;
		this.hitEffectsFlag = hitEffectsFlag;
		this.hitEffectsTimeMultiplier = hitEffectsTimeMultiplier;
		this.lastHitEffectKey = projectileHitEffectKey;

		this.piercingMaxCount = PiercingMaxCount;
		this.piercingFalloffCurve = piercingFalloffCurve ?? AnimationCurve.Linear(0, 1, 1, 0);

		this.explosionMinMaxRadius = explosionMinMaxRadius;
		this.explosionDelayAfterHit = explosionDelayAfterHit;
		this.explosionFalloffCurve = explosionFalloffCurve ?? AnimationCurve.Linear(0, 1, 1, 0);
		this.explosionEffectKey = explosionEffectKey;

		this.empChainPropagationDistance = empChainPropagationDistance;
		this.empChainPropagationCount = empChainPropagationCount;
		this.empChainDepthCount = empChainDepthCount;
		this.empChainOverlapsCount = empChainOverlapsCount;
		this.empChainFalloffCurve = empChainFalloffCurve ?? AnimationCurve.Linear(0, 1, 1, 0);
		this.empChainEffectKey = empChainEffectKey;
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

			cepEnabled = this.cepEnabled,
			cepRadius = this.cepRadius,
			cepProbability = this.cepProbability,

			lifeTime = this.lifeTime,

			collisionRadius = this.collisionRadius,

			hitDamageMultiplier = this.hitDamageMultiplier,
			hitEffectsFlag = this.hitEffectsFlag,
			hitEffectsTimeMultiplier = this.hitEffectsTimeMultiplier,
			lastHitEffectKey = this.lastHitEffectKey,

			piercingMaxCount = this.piercingMaxCount,
			piercingFalloffCurve = this.piercingFalloffCurve,

			explosionMinMaxRadius = this.explosionMinMaxRadius,
			explosionDelayAfterHit = this.explosionDelayAfterHit,
			explosionFalloffCurve = this.explosionFalloffCurve,
			explosionEffectKey = this.explosionEffectKey,
	
			empChainPropagationDistance = this.empChainPropagationDistance,
			empChainPropagationCount = this.empChainPropagationCount,
			empChainDepthCount = this.empChainDepthCount,
			empChainOverlapsCount = this.empChainOverlapsCount,
			empChainFalloffCurve = this.empChainFalloffCurve ,
			empChainEffectKey = this.empChainEffectKey,
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
	public float CepRadius => cepRadius;
	public float CepProbability => cepProbability;
	public float LifeTime => lifeTime;
	public float CollisionRadius => collisionRadius;
	public float HitDamageMultiplier => hitDamageMultiplier;
	public StatusEffectsFlag HitEffectsFlag => hitEffectsFlag;
	public float HitEffectsTimeMultiplier => hitEffectsTimeMultiplier;
	public SubEffectKey ProjectileHitEffectKey => lastHitEffectKey;
	public int PiercingMaxCount => Mathf.Max(1,piercingMaxCount);
	public AnimationCurve PiercingFalloffCurve => piercingFalloffCurve;
	public Vector2 ExplosionMinMaxRadius => explosionMinMaxRadius;
	public float ExplosionDelayAfterHit => Mathf.Max(0,explosionDelayAfterHit);
	public AnimationCurve ExplosionFalloffCurve => explosionFalloffCurve;
	public SubEffectKey ExplosionEffectKey => explosionEffectKey;
	public float EmpChainPropagationDistance => empChainPropagationDistance;
	public int EmpChainPropagationCount => empChainPropagationCount;
	public int EmpChainDepthCount => empChainDepthCount;
	public int EmpChainOverlapsCount => empChainOverlapsCount;
	public AnimationCurve EmpChainFalloffCurve => empChainFalloffCurve;
	public SubEffectKey EmpChainEffectKey => empChainEffectKey;

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
	public float EmpFalloffMultiplier(int currentDepth)
	{
		if (!ElectromagneticEnabled) return 1f;
		int max = EmpChainDepthCount;
		if (max <= 1) return 1f;

		float rate = (float)currentDepth / (float)max;
		return EmpChainFalloffCurve.Evaluate(rate);
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