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
	private Vector2Int piercingMinMaxPoint;
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
		weaponType = profile.WeaponType;
		moveStartSpeed = profile.MoveStartSpeed;
		isShiftSpeed = profile.IsShiftSpeed;
		moveMaxSpeed = profile.MoveMaxSpeed;
		moveSpeedCurve = profile.MoveSpeedCurve;
		timeFromStartToMaxSpeed = profile.TimeFromStartToMaxSpeed;
		homingEnabled = profile.HomingEnabled;
		homingTurnSpeed = profile.HomingTurnSpeed;
		homingActivationDelay = profile.HomingActivationDelay;
		lifeTime = profile.LifeTime;
		destroyDelayAfterHit = profile.DestroyDelayAfterHit;
		collisionRadius = profile.CollisionRadius;
		hitDamageMultiplier = profile.HitDamageMultiplier;
		hitEffectsFlag = profile.HitEffectsFlag;
		hitEffectsTimeMultiplier = profile.HitEffectsTimeMultiplier;
		piercingEnable = profile.PiercingEnable;
		piercingMinMaxPoint = profile.PiercingMinMaxPoint;
		piercingFalloffCurve = profile.PiercingFalloffCurve;
		explosionEnabled = profile.ExplosionEnabled;
		explosionMinMaxRadius = profile.ExplosionMinMaxRadius;
		explosionFalloffCurve = profile.ExplosionFalloffCurve;
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
			homingActivationDelay = this.homingActivationDelay,
			lifeTime = this.lifeTime,
			destroyDelayAfterHit = this.destroyDelayAfterHit,
			collisionRadius = this.collisionRadius,
			hitDamageMultiplier = this.hitDamageMultiplier,
			hitEffectsFlag = this.hitEffectsFlag,
			hitEffectsTimeMultiplier = this.hitEffectsTimeMultiplier,
			piercingEnable = this.piercingEnable,
			piercingMinMaxPoint = this.piercingMinMaxPoint,
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
	public float HomingActivationDelay => homingActivationDelay;
	public float LifeTime => lifeTime;
	public float DestroyDelayAfterHit => destroyDelayAfterHit;
	public float CollisionRadius => collisionRadius;
	public float HitDamageMultiplier => hitDamageMultiplier;
	public StatusEffectsFlag HitEffectsFlag => hitEffectsFlag;
	public float HitEffectsTimeMultiplier => hitEffectsTimeMultiplier;
	public bool PiercingEnable => piercingEnable;
	public Vector2Int PiercingMinMaxPoint => piercingMinMaxPoint;
	public AnimationCurve PiercingFalloffCurve => piercingFalloffCurve;
	public bool ExplosionEnabled => explosionEnabled;
	public Vector2 ExplosionMinMaxRadius => explosionMinMaxRadius;
	public AnimationCurve ExplosionFalloffCurve => explosionFalloffCurve;

	#endregion
}