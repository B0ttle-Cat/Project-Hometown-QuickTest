using System;

using UnityEngine;

using static StrategyGamePlayData;

using Random = UnityEngine.Random;

public interface ICombatHandler : ICombatCommon
{
	ICombatHandler ThisCombatHandler { get; }
	IStrategyElement ThisElement { get; }
	int FactionID { get; }

	Transform transform { get; }
	Vector3 Position { get; }
	Vector3 AttackStartPosition { get; }
	Vector2 AttackStartRange { get; }
	Vector2 AttackLimitRange { get; }
	bool IsCombatState { get; }
	ITargetableCombatant CurrentTarget { get; set; }
	bool HasCurrentTarget => CurrentTarget != null;
	bool TargetInStartAttackRange { get; }
	bool TargetInLimitAttackRange { get; }
	bool TargetInActionRange { get; }

	bool IsRootCombatState { get; set; }
	ITargetableCombatant RootCurrentTarget { get; set; }
	bool HasRootCurrentTarget => RootCurrentTarget != null;

	event Action<ITargetableCombatant> OnChangeCurrentCombatTarget;
	void UpdateParameters();
	bool IsKeepingTargetAllowed();
	bool SearchingNewTarget(out ITargetableCombatant newTarget);
	void ChangeCombatTarget(in ITargetableCombatant newTarget);
}

public interface ICombatCommon : IStatsValueControl
{
	ICombatCommon ThisCombatStats { get; }
	ICombatOffense ThisOffense { get; }
	ICombatDefance ThisDefance { get; }

	// 🛡️ 내구도 및 회복 스탯 (Durability & Recovery)
	int MaxDurability => GetStatsValue(StatsType.유닛_최대내구도);
	int CurrentDurability => GetStatsValue(StatsType.유닛_현재내구도);
	int HealingPower => GetStatsValue(StatsType.유닛_치유력);
	int RecoveryPower => GetStatsValue(StatsType.유닛_회복력);

	// 💨 이동 및 점령 스탯 (Movement & Capture)
	float MovementSpeed => GetStatsValuePercent(StatsType.유닛_이동속도_c);
	float CaptureScore => GetStatsValue(StatsType.유닛_점령점수);

	// ⚙️ 공격 시스템 계수 (Attack System Multipliers)
	int AmmunitionCapacity => GetStatsValue(StatsType.유닛_탄용량);
	int AmmunitionUsed => GetStatsValue(StatsType.유닛_사용탄수);
	int HitDamageCount => GetStatsValue(StatsType.유닛_명중피격수);
	int ContinuousAttackCount => GetStatsValue(StatsType.유닛_연속공격횟수);
	int ConcurrentProjectileCount => GetStatsValue(StatsType.유닛_동시공격개수);

	// ⏱️ 딜레이 스탯 (Time Delays)
	float AimDelayTime => GetStatsValuePercent(StatsType.유닛_조준지연시간_c);
	float ContinuousAttackDelayTime => GetStatsValuePercent(StatsType.유닛_연속공격지연시간_c);
	float ReattackDelayTime => GetStatsValuePercent(StatsType.유닛_재공격지연시간_c);
	float ReloadTime => GetStatsValuePercent(StatsType.유닛_재장전시간_c);

	// 💸 공격 소모 자원 스탯 (Attack Cost)
	float AttackCostMaterial => GetStatsValue(StatsType.유닛_공격소모_물자);
	float AttackCostPower => GetStatsValue(StatsType.유닛_공격소모_전력);

	// 🔭 범위 스탯 (Range)
	float AttackRangeLimitMin => GetStatsValuePercent(StatsType.유닛_공격범위_종료최소_c);
	float AttackRangeStartMin => GetStatsValuePercent(StatsType.유닛_공격범위_시작최소_c);
	float AttackRangeStartMax => GetStatsValuePercent(StatsType.유닛_공격범위_시작최대_c);
	float AttackRangeLimitMax => GetStatsValuePercent(StatsType.유닛_공격범위_종료최대_c);
	float ActionRange => GetStatsValuePercent(StatsType.유닛_행동범위_c);
	float VisionRange => GetStatsValuePercent(StatsType.유닛_시야범위_c);
}
public interface ICombatOffense : IStatsValueControl
{
	ICombatOffense ThisOffense { get; }
	WeaponType WeaponType => WeaponType.일반;

	// 💥 기본 피해 스탯 (Base Damage)
	int AttackPower => GetStatsValue(StatsType.유닛_공격력);

	// 🎯 치명타 스탯 (Critical)
	float CriticalDamageRatio => GetStatsValuePercent(StatsType.유닛_치명피해율);
	int CriticalAttackPower => GetStatsValue(StatsType.유닛_치명공격력);

	// 🛡️ 관통 및 적용 스탯 (Penetration & Application)
	int PenetrationLevel => GetStatsValue(StatsType.유닛_관통레벨);
	int EMPImpactLevel => GetStatsValue(StatsType.유닛_EMP충격레벨);
	int StatusPotencyLevel => GetStatsValue(StatsType.유닛_상태이상적용레벨);

	// 📈 확률 기회 스탯 (Chance Score)
	int HitChanceScore => GetStatsValue(StatsType.유닛_공격명중기회);
	int CriticalChanceScore => GetStatsValue(StatsType.유닛_치명명중기회);
}
public interface ICombatDefance : IStatsValueControl
{
	ICombatDefance ThisDefance { get; }
	ProtectionType ProtectionType => ProtectionType.일반;

	// 🛡️ 기본 방어 스탯 (Base Defense)
	int AntiAttackPower => GetStatsValue(StatsType.유닛_방어력);

	// 🎯 치명타 방어 스탯 (Critical Defense)
	int AntiCriticalAttackPower => GetStatsValue(StatsType.유닛_치명방어력);

	// 🛡️ 장갑 및 방호 스탯 (Armor & Protection)
	int AntiPenetrationLevel => GetStatsValue(StatsType.유닛_장갑레벨);
	int AntiEMPImpactLevel => GetStatsValue(StatsType.유닛_EMP방호레벨);
	int AntiStatusPotencyLevel => GetStatsValue(StatsType.유닛_상태이상저항레벨);

	// 확률 회피 스탯 (Evasion Score)
	int AntiHitChanceScore => GetStatsValue(StatsType.유닛_공격회피기회);
	int AntiCriticalChanceScore => GetStatsValue(StatsType.유닛_치명회피기회);
}
public static partial class CombatUtility
{
	private const float FACTOR_VERY_LOW = 0.5f;  // (--)
	private const float FACTOR_LOW = 0.75f;       // (- )
	private const float FACTOR_NORMAL = 1.00f;    // ( )
	private const float FACTOR_HIGH = 1.50f;      // (+ )
	private const float FACTOR_VERY_HIGH = 2.00f; // (++)

	public static bool CheckChance(float chance)
	{
		return Random.value <= chance;
	}
	#region Calculate
	/// <summary>
	/// 공격자의 명중 확률 (0.0f ~ 1.0f)을 계산합니다.
	/// </summary>
	/// <param name="offense">공격 유닛의 전투 스탯</param>
	/// <param name="defance">방어 유닛의 전투 스탯</param>
	/// <returns>명중 확률 (0.0f ~ 1.0f)</returns>
	public static float CalculateHitChance(ICombatOffense offense, ICombatDefance defance)
	{
		float offenseScore = offense.HitChanceScore;
		float defanceScore = defance.AntiHitChanceScore;

		// 회피 기회 스탯이 0 미만일 수 없으므로 안전 장치를 추가합니다.
		if (offenseScore < 0) offenseScore = 0;
		if (defanceScore < 0) defanceScore = 0;

		// 분모가 0이면 100% 명중 (회피 불가능)
		float denominator = offenseScore + defanceScore;

		if (denominator <= 0)
		{
			return 1f;
		}

		// 명중률 공식: 공격 기여도 / (공격 기여도 + 방어 기여도)
		return offenseScore / denominator;
	}
	/// <summary>
	/// 공격자의 치명타 확률 (0.0f ~ 1.0f)을 계산합니다.
	/// </summary>
	/// <param name="offense">공격 유닛의 전투 스탯</param>
	/// <param name="defance">방어 유닛의 전투 스탯</param>
	/// <returns>치명타 확률 (0.0f ~ 1.0f)</returns>
	public static float CalculateCriticalChance(ICombatOffense offense, ICombatDefance defance)
	{
		float offenseScore = offense.CriticalChanceScore;
		float defanceScore = defance.AntiCriticalChanceScore;

		if (offenseScore < 0) offenseScore = 0;
		if (defanceScore < 0) defanceScore = 0;

		float denominator = offenseScore + defanceScore;

		if (denominator <= 0)
		{
			return 1f;
		}

		return offenseScore / denominator;
	}

	/// <summary>
	/// 공격자의 관통 레벨과 방어자의 장갑 레벨의 차이를 계산합니다.
	/// (PenetrationLevel - AntiPenetrationLevel)
	/// </summary>
	/// <param name="offense">공격 유닛의 전투 스탯</param>
	/// <param name="defance">방어 유닛의 전투 스탯</param>
	/// <returns>레벨 차이 값 (int)</returns>
	public static int GetPenetrationLevelDifference(ICombatOffense offense, ICombatDefance defance)
	{
		// 일반 & 관통타입 & 에너지 타입 
		// => baseDifference >=0 일 경우 관통했다고 봄.
		// => baseDifference >= 0 에서 100% 
		// => baseDifference < 0 일수록 더 작은 데미지
		// => 관통특화 타입은 baseDifference 에 +1  
		// => 상대가 강화장갑일 경우 baseDifference 에 -1
		// 폭발타입
		// => baseDifference를 일단 절대값으로 취급한다.
		// => baseDifference == 0 일 경우 내부 에서 정확히 폭발했다고 봄
		// => baseDifference == 0 일 경우 100%
		// => baseDifference 가 0 에서 멀어질수록 거 적은 데미지
		// => 폭발특화 타입 은 baseDifference 의 유효범위를 1 늘린다.
		// => 상대가 강화장갑일 경우 baseDifference 유효범위를 1 줄인다.
		// baseDifference 에 의한 상성 변화는 수치별 5% 씩 감소한다.
		WeaponType wType = offense.WeaponType;
		ProtectionType pType = defance.ProtectionType;
		int baseDifference = offense.PenetrationLevel - defance.AntiPenetrationLevel;

		if (wType is WeaponType.폭발 or WeaponType.폭발특화)
		{
			baseDifference = -Mathf.Abs(baseDifference);
			if (wType is WeaponType.폭발특화)
			{
				baseDifference += 1;
			}
			if (pType is ProtectionType.강화장갑)
			{
				baseDifference -= 1;
			}
			if (baseDifference > 0) baseDifference = 0;
		}
		else
		{
			if (wType is WeaponType.관통특화)
			{
				baseDifference += 1;
			}
			if (pType is ProtectionType.강화장갑)
			{
				baseDifference -= 1;
			}
		}

		Mathf.Clamp(baseDifference, -5, 5);
		return baseDifference;
	}
	public static int CalculatePiercingCount(ICombatOffense offense, ICombatDefance defance, int piercingCount)
	{
		WeaponType wType = offense.WeaponType;
		ProtectionType pType = defance.ProtectionType;
		if (wType is WeaponType.관통 or WeaponType.관통특화)
		{
			int baseDifference = offense.PenetrationLevel - (defance.AntiPenetrationLevel + (pType is ProtectionType.강화장갑?1:0));
			if (baseDifference < 0)
			{
				return int.MaxValue;
			}
			piercingCount += baseDifference + 1;
			return piercingCount;
		}
		else
		{
			return int.MaxValue;
		}
	}
	/// <summary>
	/// 공격자의 EMP 충격 레벨과 방어자의 EMP 방호 레벨의 차이를 계산합니다.
	/// (EMPImpactLevel - AntiEMPImpactLevel)
	/// </summary>
	/// <param name="offense">공격 유닛의 전투 스탯</param>
	/// <param name="defance">방어 유닛의 전투 스탯</param>
	/// <returns>레벨 차이 값 (int)</returns>
	public static int GetEMPLevelDifference(ICombatOffense offense, ICombatDefance defance)
	{
		return offense.EMPImpactLevel - defance.AntiEMPImpactLevel;
	}

	/// <summary>
	/// 공격자의 상태 이상 강도 레벨과 방어자의 상태 이상 저항 레벨의 차이를 계산합니다.
	/// (StatusPotencyLevel - AntiStatusPotencyLevel)
	/// </summary>
	/// <param name="offense">공격 유닛의 전투 스탯</param>
	/// <param name="defance">방어 유닛의 전투 스탯</param>
	/// <returns>레벨 차이 값 (int)</returns>
	public static int GetStatusPotencyLevelDifference(ICombatOffense offense, ICombatDefance defance)
	{
		return offense.StatusPotencyLevel - defance.AntiStatusPotencyLevel;
	}

	public static float GetWeaponEffectivenessFactor(ICombatOffense offense, ICombatDefance defance)
	{
		WeaponType wType = offense.WeaponType;
		ProtectionType pType = defance.ProtectionType;

		return (wType, pType) switch
		{
			// ----------------------------------------------------------------------
			// 📦 일반 무기 상성 (경장갑( ), 중장갑( ), 강화장갑(- ), 역장( ), 건물( ))
			// ----------------------------------------------------------------------
			(WeaponType.일반, ProtectionType.경장갑) => FACTOR_NORMAL,
			(WeaponType.일반, ProtectionType.중장갑) => FACTOR_NORMAL,
			(WeaponType.일반, ProtectionType.강화장갑) => FACTOR_LOW,
			(WeaponType.일반, ProtectionType.역장) => FACTOR_NORMAL,
			(WeaponType.일반, ProtectionType.건물) => FACTOR_NORMAL,

			// ----------------------------------------------------------------------
			// ⚔️ 관통 무기 상성 (경장갑( ), 중장갑(+ ), 강화장갑( ), 역장(- ), 건물( ))
			// ----------------------------------------------------------------------
			(WeaponType.관통, ProtectionType.경장갑) => FACTOR_NORMAL,
			(WeaponType.관통, ProtectionType.중장갑) => FACTOR_HIGH,
			(WeaponType.관통, ProtectionType.강화장갑) => FACTOR_NORMAL,
			(WeaponType.관통, ProtectionType.역장) => FACTOR_LOW,
			(WeaponType.관통, ProtectionType.건물) => FACTOR_NORMAL,

			// ----------------------------------------------------------------------
			// 💥 폭발 무기 상성 (경장갑(+ ), 중장갑( ), 강화장갑(- ), 역장(- ), 건물( ))
			// ----------------------------------------------------------------------
			(WeaponType.폭발, ProtectionType.경장갑) => FACTOR_HIGH,
			(WeaponType.폭발, ProtectionType.중장갑) => FACTOR_NORMAL,
			(WeaponType.폭발, ProtectionType.강화장갑) => FACTOR_LOW,
			(WeaponType.폭발, ProtectionType.역장) => FACTOR_LOW,
			(WeaponType.폭발, ProtectionType.건물) => FACTOR_NORMAL,

			// ----------------------------------------------------------------------
			// 🔨 관통특화 무기 상성 (경장갑(- ), 중장갑(++), 강화장갑(++), 역장(- ), 건물(+ ))
			// ----------------------------------------------------------------------
			(WeaponType.관통특화, ProtectionType.경장갑) => FACTOR_LOW,
			(WeaponType.관통특화, ProtectionType.중장갑) => FACTOR_VERY_HIGH,
			(WeaponType.관통특화, ProtectionType.강화장갑) => FACTOR_VERY_HIGH,
			(WeaponType.관통특화, ProtectionType.역장) => FACTOR_LOW,
			(WeaponType.관통특화, ProtectionType.건물) => FACTOR_HIGH,

			// ----------------------------------------------------------------------
			// 💣 폭발특화 무기 상성 (경장갑(++), 중장갑(--), 강화장갑(--), 역장( ), 건물(+ ))
			// ----------------------------------------------------------------------
			(WeaponType.폭발특화, ProtectionType.경장갑) => FACTOR_VERY_HIGH,
			(WeaponType.폭발특화, ProtectionType.중장갑) => FACTOR_VERY_LOW,
			(WeaponType.폭발특화, ProtectionType.강화장갑) => FACTOR_VERY_LOW,
			(WeaponType.폭발특화, ProtectionType.역장) => FACTOR_NORMAL,
			(WeaponType.폭발특화, ProtectionType.건물) => FACTOR_HIGH,

			// ----------------------------------------------------------------------
			// ⚛️ 에너지 무기 상성 (경장갑(- ), 중장갑(--), 강화장갑(--), 역장(++), 건물(--))
			// ----------------------------------------------------------------------
			(WeaponType.에너지, ProtectionType.경장갑) => FACTOR_LOW,
			(WeaponType.에너지, ProtectionType.중장갑) => FACTOR_VERY_LOW,
			(WeaponType.에너지, ProtectionType.강화장갑) => FACTOR_VERY_LOW,
			(WeaponType.에너지, ProtectionType.역장) => FACTOR_VERY_HIGH,
			(WeaponType.에너지, ProtectionType.건물) => FACTOR_VERY_LOW,

			// ----------------------------------------------------------------------
			// 🏷️ 정의되지 않은 모든 조합 처리 (ProtectionType.일반 포함)
			// ----------------------------------------------------------------------
			_ => FACTOR_NORMAL // 안전 장치: 정의되지 않은 모든 조합은 100% (보통)
		};
	}
	/// <summary>
	/// 공격력과 방어력을 기반으로 최소 피해량 1을 보장하는 기본 상쇄 피해량을 계산합니다.
	/// </summary>
	public static float CalculateBaseDamage(ICombatOffense offense, ICombatDefance defance)
	{
		// 최소 피해는 1로 설정하여 무력화 방지 (Damage Floor)
		return Mathf.Max(1f, offense.AttackPower - defance.AntiAttackPower);
	}

	/// <summary>
	/// 아래의 두 요소에 대한 영향값을 구합니다.
	/// 관통 레벨과 장갑 레벨 차이에서 발생하는 피해 증폭/감소 계수
	/// 공격 타입과 장갑 타입에서 나타나는 피해 증폭/감소 계수
	/// </summary>
	public static float CalculateTypeFactor(ICombatOffense offense, ICombatDefance defance)
	{

		float levelDamageFactor =  1f - (GetPenetrationLevelDifference(offense, defance) * 0.05f);
		levelDamageFactor = Mathf.Clamp(levelDamageFactor, FACTOR_LOW, 1f);


		float weaponEffectivenessFactor = GetWeaponEffectivenessFactor(offense, defance);


		// levelDifference 의한 수치 * 상성표에 대한 수치
		return levelDamageFactor * weaponEffectivenessFactor;
	}


	#endregion
}
public static partial class CombatUtility // Command
{
	[Flags]
	public enum DamageFlag
	{
		Miss = 0,
		Hit			 = 0 << 1,
		Pierce	 = 0 << 2,
	}
	public class DamageCommander : IDisposable
	{
		private readonly ICombatOffense offense;
		private readonly ICombatDefance defance;
		private readonly float projectileDemageFactor;
		private DamageFlag flag;

        public DamageCommander(ICombatOffense offense, ICombatDefance defance, float projectileDemageFactor, DamageFlag flag)
        {
            this.offense = offense;
            this.defance = defance;
            this.flag = flag;

			StrategyManager.Collector.Add<DamageCommander>(this);
		}
		public void ChangeFlag(DamageFlag flag)
		{
			this.flag = flag;
		}
		public DamageFlag GetFlag() => flag;
        public void Dispose()
        {
			StrategyManager.Collector.Remove<DamageCommander>(this);
		}


		public virtual void Compute()
		{
			// TODO : 데미지  계산 작업 진행

		}
	}
}
