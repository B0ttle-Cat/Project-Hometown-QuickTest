using System;

using Sirenix.OdinInspector;

using StrategyManagerModule;

using UnityEngine;

using static StrategyGamePlayData;

[Serializable]
public record UnitStatsData
{
	// Private Fields (Odin Inspector attributes remain for editor visualization)
	[FoldoutGroup(nameof(Cost)), InlineProperty, HideLabel, SerializeField]
	private Cost cost;
	[FoldoutGroup(nameof(Common)), InlineProperty, HideLabel, SerializeField]
	private Common common;
	[FoldoutGroup(nameof(Range)), InlineProperty, HideLabel, SerializeField]
	private Range range;
	[FoldoutGroup(nameof(Cycle)), InlineProperty, HideLabel, SerializeField]
	private Cycle cycle;
	[FoldoutGroup(nameof(Ammo)), InlineProperty, HideLabel, SerializeField]
	private Ammo ammo;
	[FoldoutGroup(nameof(Offense)), InlineProperty, HideLabel, SerializeField]
	private Offense offense;
	[FoldoutGroup(nameof(Defense)), InlineProperty, HideLabel, SerializeField]
	private Defense defense;

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
[Serializable]
public record UnitInstanceData
{
	public UnitKey unitKey;     // 원본과 매칭되는 키
	public string displayName;  // 유닛 이름

	public int unitID;
	public int factionID;

	[SerializeField]
	private int lastVisiteSectorID;
	[SerializeField]
	private int currVisiteSectorID;
	public int VisiteSectorID
	{
		get
		{
			if (currVisiteSectorID == -1)
			{
				if (lastVisiteSectorID == -1)
				{
					return -1;
				}
				return lastVisiteSectorID;
			}
			return currVisiteSectorID;
		}
		set
		{
			if (currVisiteSectorID == -1)
			{
				lastVisiteSectorID = currVisiteSectorID = value;
			}
			else
			{
				lastVisiteSectorID = currVisiteSectorID;
				currVisiteSectorID = value;
			}
		}
	}
	public UnitInstanceData(UnitProfileObject data, int factionID = -1)
    {
		unitKey = data.unitKey;
		displayName = data.displayName;
		this.factionID = factionID;

		unitID = -1;
		lastVisiteSectorID = -1;
		currVisiteSectorID = -1;
	}
	public void Init(in StrategyStartSetterData.UnitData data)
	{
		this.factionID = data.factionID;
		lastVisiteSectorID = currVisiteSectorID = data.visiteSectorID;
	}

    internal void SetElementID(in int unitElementID)
    {
		unitID = unitElementID;
	}
}