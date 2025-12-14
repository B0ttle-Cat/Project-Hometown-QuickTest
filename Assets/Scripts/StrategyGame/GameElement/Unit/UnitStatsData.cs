using System;

using Sirenix.OdinInspector;

using UnityEngine;

using static StrategyGamePlayData;

[Serializable]
public record UnitStatsData
{
	// Private Fields (Odin Inspector attributes remain for editor visualization)
	[InlineProperty, HideLabel, SerializeField, Header(nameof(Cost))]
	private readonly Cost cost;
	[InlineProperty, HideLabel, SerializeField, Header(nameof(Common))]
	private readonly Common common;
	[InlineProperty, HideLabel, SerializeField, Header(nameof(Range))]
	private readonly Range range;
	[InlineProperty, HideLabel, SerializeField, Header(nameof(Cycle))]
	private readonly Cycle cycle;
	[InlineProperty, HideLabel, SerializeField, Header(nameof(Ammo))]
	private readonly Ammo ammo;
	[InlineProperty, HideLabel, SerializeField, Header(nameof(Offense))]
	private readonly Offense offense;
	[InlineProperty, HideLabel, SerializeField, Header(nameof(Defense))]
	private readonly Defense defense;

	public Cost GetCost => cost;
	public Common GetCommon => common;
	public Range GetRange => range;
	public Cycle GetCycle => cycle;
	public Ammo GetAmmo => ammo;
	public Offense GetOffense => offense;
	public Defense GetDefense => defense;


	public UnitStatsData()
	{
		cost = new();
		common = new();
		range = new();
		cycle = new();
		ammo = new();
		offense = new();
		defense = new();
	}

	public UnitStatsData(UnitProfileObject data)
	{
		var stats = data.stats;
		this.cost = stats.cost with { };
		this.common = stats.common with { };
		this.range = stats.range with { };
		this.cycle = stats.cycle with { };
		this.ammo = stats.ammo with { };
		this.offense = stats.offense with { };
		this.defense = stats.defense with { };
	}

    public UnitStatsData(UnitStatsData stats)
    {
        this.cost = stats.cost with { };
        this.common = stats.common with { };
        this.range = stats.range with { };
        this.cycle = stats.cycle with { };
        this.ammo = stats.ammo with { };
        this.offense = stats.offense with { };
        this.defense = stats.defense with { };
    }

    // --- 💸 Cost (비용) Properties ---
    [Serializable]
	public record Cost
	{
		public int DeploymentCostPersonnel;
		public int DeploymentCostMaterial;
		public int DeploymentCostPower;

		public int AttackCostMaterial;
		public int AttackCostPower;
	}
	public int DeploymentCostPersonnel => cost.DeploymentCostPersonnel;
	public int DeploymentCostMaterial => cost.DeploymentCostMaterial;
	public int DeploymentCostPower => cost.DeploymentCostPower;
	public int AttackCostMaterial => cost.AttackCostMaterial;
	public int AttackCostPower => cost.AttackCostPower;

	// --- 🛡️ Common (공통) Properties ---
	[Serializable]
	public record Common
	{
		public int MaxDurability;
		public int HealingPower;
		public int RecoveryPower;
		public float MovementSpeed;
		public int CaptureScore;
	}
	public int MaxDurability => common.MaxDurability;
	public int HealingPower => common.HealingPower;
	public int RecoveryPower => common.RecoveryPower;
	public float MovementSpeed => common.MovementSpeed;
	public int CaptureScore => common.CaptureScore;

	// --- 🔭 Range (범위) Properties ---
	[Serializable]
	public record Range
	{
		public Vector4 AttackRange;
		public float ActionRange;
		public float VisionRange;
	}
	public float AttackRangeLimitMin => range.AttackRange.x;
	public float AttackRangeStartMin => range.AttackRange.y;
	public float AttackRangeStartMax => range.AttackRange.z;
	public float AttackRangeLimitMax => range.AttackRange.w;
	public float ActionRange => range.ActionRange;
	public float VisionRange => range.VisionRange;

	// --- ⚙️ Cycle (공격 주기) Properties ---
	[Serializable]
	public record Cycle
	{
		public float AimDelayTime;
		public float ContinuousAttackDelayTime;
		public float ReattackDelayTime;
		public float ReloadTime;
	}

	public float AimDelayTime => cycle.AimDelayTime;
	public float ContinuousAttackDelayTime => cycle.ContinuousAttackDelayTime;
	public float ReattackDelayTime => cycle.ReattackDelayTime;
	public float ReloadTime => cycle.ReloadTime;

	// --- 彈 Ammo (탄약) Properties ---
	[Serializable]
	public record Ammo
	{
		public int AmmunitionCapacity;
		public int ConcurrentAttackCount;
		public int ContinuousAttackCount;
	}
	public int AmmunitionCapacity => ammo.AmmunitionCapacity;
	public int ConcurrentAttackCount => ammo.ConcurrentAttackCount;
	public int ContinuousAttackCount => ammo.ContinuousAttackCount;

	// --- 💥 Offense (공격) Properties ---
	[Serializable]
	public record Offense
	{
		public ProjectileKey projectileKey;

		public int AttackPower;
		public int CriticalAttackPower;
		public int CriticalDamageRatio;

		public int PenetrationLevel;
		public int EMPImpactLevel;
		public int StatusPotencyLevel;

		public int HitChanceScore;
		public int CriticalChanceScore;
	}
	public ProjectileKey ProjectileKey => offense.projectileKey;
	// 이 Property는 이미 Setter가 없었습니다.
	public WeaponType WeaponType => StrategyManager.Key2Projectile.TryGetAsset(ProjectileKey, out var info) ? info.WeaponType : WeaponType.None;
	public int AttackPower => offense.AttackPower;
	public int CriticalAttackPower => offense.CriticalAttackPower;
	public int CriticalDamageRatio => offense.CriticalDamageRatio;
	public int PenetrationLevel => offense.PenetrationLevel;
	public int EMPImpactLevel => offense.EMPImpactLevel;
	public int StatusPotencyLevel => offense.StatusPotencyLevel;
	public int HitChanceScore => offense.HitChanceScore;
	public int CriticalChanceScore => offense.CriticalChanceScore;

	// --- 🛡️ Defense (방어) Properties ---
	[Serializable]
	public record Defense
	{
		public ProtectionType protectType;

		public int AntiAttackPower;
		public int AntiCriticalAttackPower;

		public int AntiPenetrationLevel;
		public int AntiEMPImpactLevel;
		public int AntiStatusPotencyLevel;

		public int AntiHitChanceScore;
		public int AntiCriticalChanceScore;
	}
	// 이 Property는 이미 Setter가 없었습니다.
	public ProtectionType ProtectionType => defense.protectType;
	public int AntiAttackPower => defense.AntiAttackPower;
	public int AntiCriticalAttackPower => defense.AntiCriticalAttackPower;
	public int AntiPenetrationLevel => defense.AntiPenetrationLevel;
	public int AntiEMPImpactLevel => defense.AntiEMPImpactLevel;
	public int AntiStatusPotencyLevel => defense.AntiStatusPotencyLevel;
	public int AntiHitChanceScore => defense.AntiHitChanceScore;
	public int AntiCriticalChanceScore => defense.AntiCriticalChanceScore;

	[Button]
	void SetTestStatsValue()
	{
		// --- 💸 Cost (비용) ---
		// 유닛_인력, 유닛_물자, 유닛_전력은 '배치 비용'에 해당합니다.
		cost.DeploymentCostPersonnel = 1;
		cost.DeploymentCostMaterial = 1;
		cost.DeploymentCostPower = 1;

		// 유닛_공격소모_물자, 유닛_공격소모_전력은 '공격 비용'에 해당합니다.
		cost.AttackCostMaterial = 1;
		cost.AttackCostPower = 1;

		// --- 🛡️ Common (공통) ---
		common.MaxDurability = 1000;      // 유닛_최대내구도
		common.HealingPower = 10;         // 유닛_치유력
		common.RecoveryPower = 1;         // 유닛_회복력
		common.MovementSpeed = 1.00f;     // 유닛_이동속도 (float 형변환 필요)
		common.CaptureScore = 1;          // 유닛_점령점수

		// --- 🔭 Range (범위) ---
		// Vector4(x: LimitMin, y: StartMin, z: StartMax, w: LimitMax)
		range.AttackRange = new Vector4(
			0f,     // 유닛_공격범위_종료최소 (x)
			0f,     // 유닛_공격범위_시작최소 (y)
			8.00f,  // 유닛_공격범위_시작최대 (z)
			10.00f  // 유닛_공격범위_종료최대 (w)
		);
		range.ActionRange = 11.00f;       // 유닛_행동범위
		range.VisionRange = 15.00f;       // 유닛_시야범위

		// --- ⚙️ Cycle (공격 주기) ---
		cycle.AimDelayTime = 1.00f;       // 유닛_조준지연시간
		cycle.ContinuousAttackDelayTime = 0.10f; // 유닛_연속공격지연시간
		cycle.ReattackDelayTime = 0.50f;    // 유닛_재공격지연시간
		cycle.ReloadTime = 3.00f;         // 유닛_재장전시간

		// --- 彈 Ammo (탄약) ---
		ammo.AmmunitionCapacity = 8;      // 유닛_탄용량
		ammo.ConcurrentAttackCount = 1;   // 유닛_동시공격개수
		ammo.ContinuousAttackCount = 3;   // 유닛_연속공격횟수

		// --- 💥 Offense (공격) ---
		offense.projectileKey = ProjectileKey.일반탄_소형;

		offense.AttackPower = 10;           // 유닛_공격력
		offense.CriticalAttackPower = 30;   // 유닛_치명공격력
		offense.CriticalDamageRatio = 200;  // 유닛_치명피해율

		offense.PenetrationLevel = 1;       // 유닛_관통레벨
		offense.EMPImpactLevel = 1;         // 유닛_EMP충격레벨
		offense.StatusPotencyLevel = 1;     // 유닛_상태이상적용레벨

		offense.HitChanceScore = 70;        // 유닛_공격명중기회
		offense.CriticalChanceScore = 30;   // 유닛_치명명중기회

		// --- 🛡️ Defense (방어) ---
		defense.protectType = ProtectionType.일반;

		defense.AntiAttackPower = 1;        // 유닛_방어력
		defense.AntiCriticalAttackPower = 10; // 유닛_치명방어력

		defense.AntiPenetrationLevel = 1;   // 유닛_장갑레벨 (Anti-Penetration)
		defense.AntiEMPImpactLevel = 1;     // 유닛_EMP방호레벨
		defense.AntiStatusPotencyLevel = 1; // 유닛_상태이상저항레벨

		defense.AntiHitChanceScore = 10;    // 유닛_공격회피기회 (Anti-Hit Chance)
		defense.AntiCriticalChanceScore = 20; // 유닛_치명회피기회 (Anti-Critical Chance)
	}
}
[Serializable]
public record UnitRuntimeData
{
	public int CurrentDurability;       // 현재 내구도
	public int AmmunitionUsed;          // 사용 탄수

	public StatusEffectsFlag Status;   // 상태이상 효과

	public StatsList DynamicKeyStatsList;

    public UnitRuntimeData(UnitStatsData statsData)
    {
		CurrentDurability = statsData.MaxDurability;
		AmmunitionUsed = 0;

		Status = StatusEffectsFlag.None;
		DynamicKeyStatsList = new StatsList();
	}
}