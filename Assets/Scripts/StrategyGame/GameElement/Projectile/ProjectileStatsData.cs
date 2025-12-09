using System;

using Sirenix.OdinInspector;

using UnityEngine;

using static StrategyGamePlayData;


[Serializable]
public record ProjectileStatsData // ProfileStats
{
	#region ProfileState
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
	private float cepProbability; // 0~100
	[ToggleGroup("cepEnabled", GroupID = "Movement/C"), LabelText("재계산 여부"), SerializeField]
	private bool cepReapply;
	[ToggleGroup("cepEnabled", GroupID = "Movement/C"), LabelText("재계산 간격"), SerializeField, ShowIf("cepReapply")]
	private Vector2 cepReapplyMinMaxTime;

	[BoxGroup("LifeCycle"), LabelText("생존 시간"), SerializeField]
	private float lifeTime = 5f;
	[BoxGroup("LifeCycle"), LabelText("명중 후 삭제 지연"), SerializeField]
	private float destroyDelayAfterHit = 0.1f;

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

	[ToggleGroup("piercingEnable", GroupID = "Hit/P", ToggleGroupTitle ="관통 사용 여부"), SerializeField]
	private bool piercingEnable = false;
	[ToggleGroup("piercingEnable", GroupID = "Hit/P"), LabelText("관통 최소/최대 점수"), SerializeField]
	private Vector2Int piercingMinMaxCount;
	[ToggleGroup("piercingEnable", GroupID = "Hit/P"), LabelText("관통 효과 감쇠 커브"), SerializeField]
	private AnimationCurve piercingFalloffCurve;

	[ToggleGroup("explosionEnabled", GroupID = "Hit/E", ToggleGroupTitle ="폭발 사용 여부"), SerializeField]
	private bool explosionEnabled = false;
	[ToggleGroup("explosionEnabled", GroupID = "Hit/E"), LabelText("폭발 최소/최대 반경"), SerializeField]
	private Vector2 explosionMinMaxRadius;
	[ToggleGroup("explosionEnabled", GroupID = "Hit/E"),  LabelText("폭발 효과 감쇠 커브"), SerializeField]
	private AnimationCurve explosionFalloffCurve;

	public ProjectileStatsData()
	{
	}
	public ProjectileStatsData(ProjectileProfileObject profile)
	{
		if (profile == null) return;
		var statsData = profile.statsData;
		if (statsData == null) return;

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
		destroyDelayAfterHit = statsData.DestroyDelayAfterHit;

		collisionRadius = statsData.CollisionRadius;

		hitDamageMultiplier = statsData.HitDamageMultiplier;
		hitEffectsFlag = statsData.HitEffectsFlag;
		hitEffectsTimeMultiplier = statsData.HitEffectsTimeMultiplier;

		piercingEnable = statsData.PiercingEnable;
		piercingMinMaxCount = statsData.PiercingMinMaxPoint;
		piercingFalloffCurve = statsData.PiercingFalloffCurve;

		explosionEnabled = statsData.ExplosionEnabled;
		explosionMinMaxRadius = statsData.ExplosionMinMaxRadius;
		explosionFalloffCurve = statsData.ExplosionFalloffCurve;
	}

	public ProjectileStatsData(WeaponType weaponType = WeaponType.일반,
		float moveStartSpeed = 10f, bool isShiftSpeed = false, float moveMaxSpeed = 20f, AnimationCurve moveSpeedCurve = null, float timeFromStartToMaxSpeed = 2f,
		bool homingEnabled = false, float homingActivationDelay = 0f, float homingTurnSpeed = 180f, float homingTurnSpeedWhenMaxSpeed = 180f, float homingLimitAngle = 180f, float homingLimitDistance = float.PositiveInfinity,
		bool cepEnabled = false, float cepRadius = 3f, float cepProbability = 0.9f, bool cepReapply = false, Vector2 cepReapplyMinMaxTime = default,
		float lifeTime = 10f, float destroyDelayAfterHit = 0.1f,
		float collisionRadius = 0.1f,
		float hitDamageMultiplier = 1f, StatusEffectsFlag hitEffectsFlag = StatusEffectsFlag.None, float hitEffectsTimeMultiplier = 1f,
		bool piercingEnable = false, Vector2Int piercingMinMaxCount = default, AnimationCurve piercingFalloffCurve = null,
		bool explosionEnabled = false, Vector2 explosionMinMaxRadius = default, AnimationCurve explosionFalloffCurve = null)
	{
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
		this.cepReapply = cepReapply;
		this.cepReapplyMinMaxTime = cepReapplyMinMaxTime;

		this.lifeTime = lifeTime;
		this.destroyDelayAfterHit = destroyDelayAfterHit;
		this.collisionRadius = collisionRadius;

		this.hitDamageMultiplier = hitDamageMultiplier;
		this.hitEffectsFlag = hitEffectsFlag;
		this.hitEffectsTimeMultiplier = hitEffectsTimeMultiplier;

		this.piercingEnable = piercingEnable;
		this.piercingMinMaxCount = piercingMinMaxCount;
		this.piercingFalloffCurve = piercingFalloffCurve ?? AnimationCurve.Linear(0, 1, 1, 0);

		this.explosionEnabled = explosionEnabled;
		this.explosionMinMaxRadius = explosionMinMaxRadius;
		this.explosionFalloffCurve = explosionFalloffCurve ?? AnimationCurve.Linear(0, 1, 1, 0);
	}

	public ProjectileStatsData Copy()
	{
		return new ProjectileStatsData()
		{
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
			cepReapply = this.cepReapply,
			cepReapplyMinMaxTime = this.cepReapplyMinMaxTime,

			lifeTime = this.lifeTime,
			destroyDelayAfterHit = this.destroyDelayAfterHit,

			collisionRadius = this.collisionRadius,

			hitDamageMultiplier = this.hitDamageMultiplier,
			hitEffectsFlag = this.hitEffectsFlag,
			hitEffectsTimeMultiplier = this.hitEffectsTimeMultiplier,

			piercingEnable = this.piercingEnable,
			piercingMinMaxCount = this.piercingMinMaxCount,
			piercingFalloffCurve = this.piercingFalloffCurve,

			explosionEnabled = this.explosionEnabled,
			explosionMinMaxRadius = this.explosionMinMaxRadius,
			explosionFalloffCurve = this.explosionFalloffCurve
		};
	}



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
	public float DestroyDelayAfterHit => destroyDelayAfterHit;
	public float CollisionRadius => collisionRadius;
	public float HitDamageMultiplier => hitDamageMultiplier;
	public StatusEffectsFlag HitEffectsFlag => hitEffectsFlag;
	public float HitEffectsTimeMultiplier => hitEffectsTimeMultiplier;
	public bool PiercingEnable => piercingEnable;
	public Vector2Int PiercingMinMaxPoint => piercingMinMaxCount;
	public AnimationCurve PiercingFalloffCurve => piercingFalloffCurve;
	public bool ExplosionEnabled => explosionEnabled;
	public Vector2 ExplosionMinMaxRadius => explosionMinMaxRadius;
	public AnimationCurve ExplosionFalloffCurve => explosionFalloffCurve;
	public float PiercingFalloffMultiplier(int currentCount)
	{
		if (!PiercingEnable) return 1f;
		Vector2Int minMax = PiercingMinMaxPoint;
		float min = Mathf.Min(minMax.x, minMax.y);
		float max = Mathf.Max(minMax.x, minMax.y);
		float point = (float)currentCount;
		if (Mathf.Approximately(min, max))
		{
			return 1f;
		}
		float rate = (point - min) / (max - min);
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

	#endregion
}