using UnityEngine;

using static StrategyGamePlayData;
public partial class UnitObject : IStatsValueControl
{
	public IStatsValueControl StatsValue => this;

	int IStatsValueControl.GetStatsValue(StatsType type)
	{
		const int FLOAT_TO_INT_MULTIPLIER = 100;

		return type switch
		{
			// --- 💸 Cost (비용) Properties ---
			StatsType.유닛_인력 => StatsData.DeploymentCostPersonnel,
			StatsType.유닛_물자 => StatsData.DeploymentCostMaterial,
			StatsType.유닛_전력 => StatsData.DeploymentCostElectric,
			StatsType.유닛_공격소모_물자 => StatsData.AttackCostMaterial,
			StatsType.유닛_공격소모_전력 => StatsData.AttackCostElectric,

			// --- 🛡️ Common (공통) Properties ---
			StatsType.유닛_최대내구도 => StatsData.MaxDurability,
			StatsType.유닛_치유력 => StatsData.HealingPower,
			StatsType.유닛_회복력 => StatsData.RecoveryPower,
			StatsType.유닛_이동속도_c => (int)(StatsData.MovementSpeed * FLOAT_TO_INT_MULTIPLIER), // float -> int 변환
			StatsType.유닛_점령점수 => StatsData.CaptureScore,

			// --- 🔭 Range (범위) Properties ---
			StatsType.유닛_공격범위_종료최소_c => (int)(StatsData.AttackLimitRangeMin * FLOAT_TO_INT_MULTIPLIER), // float -> int 변환
			StatsType.유닛_공격범위_시작최소_c => (int)(StatsData.AttackStartRangeMin * FLOAT_TO_INT_MULTIPLIER), // float -> int 변환
			StatsType.유닛_공격범위_시작최대_c => (int)(StatsData.AttackStartRangeMax * FLOAT_TO_INT_MULTIPLIER), // float -> int 변환
			StatsType.유닛_공격범위_종료최대_c => (int)(StatsData.AttackLimitRangeMax * FLOAT_TO_INT_MULTIPLIER), // float -> int 변환
			StatsType.유닛_행동범위_c => (int)(StatsData.ActionRange * FLOAT_TO_INT_MULTIPLIER), // float -> int 변환
			StatsType.유닛_시야범위_c => (int)(StatsData.VisionRange * FLOAT_TO_INT_MULTIPLIER), // float -> int 변환

			// --- ⚙️ Cycle (공격 주기) Properties ---
			StatsType.유닛_조준지연시간_c => (int)(StatsData.AimDelayTime * FLOAT_TO_INT_MULTIPLIER), // float -> int 변환
			StatsType.유닛_연속공격지연시간_c => (int)(StatsData.ContinuousAttackDelayTime * FLOAT_TO_INT_MULTIPLIER), // float -> int 변환
			StatsType.유닛_재공격지연시간_c => (int)(StatsData.ReattackDelayTime * FLOAT_TO_INT_MULTIPLIER), // float -> int 변환
			StatsType.유닛_재장전시간_c => (int)(StatsData.ReloadTime * FLOAT_TO_INT_MULTIPLIER), // float -> int 변환

			// --- 彈 Ammo (탄약) Properties ---
			StatsType.유닛_탄용량 => StatsData.AmmunitionCapacity,
			StatsType.유닛_동시공격개수 => StatsData.ConcurrentAttackCount,
			StatsType.유닛_연속공격횟수 => StatsData.ContinuousAttackCount,

			// --- 💥 Offense (공격) Properties ---
			StatsType.유닛_공격력 => StatsData.AttackPower,
			StatsType.유닛_치명공격력 => StatsData.CriticalAttackPower,
			StatsType.유닛_치명피해율 => StatsData.CriticalDamageRatio,
			StatsType.유닛_관통레벨 => StatsData.PenetrationLevel,
			StatsType.유닛_EMP충격레벨 => StatsData.EMPImpactLevel,
			StatsType.유닛_상태이상적용레벨 => StatsData.StatusPotencyLevel,
			StatsType.유닛_공격명중기회 => StatsData.HitChanceScore,
			StatsType.유닛_치명명중기회 => StatsData.CriticalChanceScore,

			// --- 🛡️ Defense (방어) Properties ---
			StatsType.유닛_방어력 => StatsData.AntiAttackPower, // Defense는 AntiAttackPower에 매핑
			StatsType.유닛_치명방어력 => StatsData.AntiCriticalAttackPower,
			StatsType.유닛_장갑레벨 => StatsData.AntiPenetrationLevel, // 장갑은 AntiPenetrationLevel에 매핑
			StatsType.유닛_EMP방호레벨 => StatsData.AntiEMPImpactLevel,
			StatsType.유닛_상태이상저항레벨 => StatsData.AntiStatusPotencyLevel,
			StatsType.유닛_공격회피기회 => StatsData.AntiHitChanceScore,
			StatsType.유닛_치명회피기회 => StatsData.AntiCriticalChanceScore,

			// --- RuntimeData 값 (가장 마지막 배치) ---
			StatsType.유닛_현재내구도 => RuntimeData.CurrentDurability,
			StatsType.유닛_사용탄수 => RuntimeData.AmmunitionUsed,

			// 정의되지 않은 StatsType에 대한 기본값
			_ => 0,
		} + RuntimeData.DynamicKeyStatsList.GetValue(type);
	}

	float IStatsValueControl.GetStatsValuePrecent(StatsType type)
	{
		const float INT_TO_FLOAT_DIVISOR = 0.01f;

		return type switch
		{
			// --- 💸 Cost (비용) Properties ---
			StatsType.유닛_인력 => StatsData.DeploymentCostPersonnel * INT_TO_FLOAT_DIVISOR,
			StatsType.유닛_물자 => StatsData.DeploymentCostMaterial * INT_TO_FLOAT_DIVISOR,
			StatsType.유닛_전력 => StatsData.DeploymentCostElectric * INT_TO_FLOAT_DIVISOR,
			StatsType.유닛_공격소모_물자 => StatsData.AttackCostMaterial * INT_TO_FLOAT_DIVISOR,
			StatsType.유닛_공격소모_전력 => StatsData.AttackCostElectric * INT_TO_FLOAT_DIVISOR,

			// --- 🛡️ Common (공통) Properties ---
			StatsType.유닛_최대내구도 => StatsData.MaxDurability * INT_TO_FLOAT_DIVISOR,
			StatsType.유닛_치유력 => StatsData.HealingPower * INT_TO_FLOAT_DIVISOR,
			StatsType.유닛_회복력 => StatsData.RecoveryPower * INT_TO_FLOAT_DIVISOR,
			StatsType.유닛_이동속도_c => StatsData.MovementSpeed, 
			StatsType.유닛_점령점수 => StatsData.CaptureScore * INT_TO_FLOAT_DIVISOR,

			// --- 🔭 Range (범위) Properties ---
			StatsType.유닛_공격범위_종료최소_c => StatsData.AttackLimitRangeMin,
			StatsType.유닛_공격범위_시작최소_c => StatsData.AttackStartRangeMin,
			StatsType.유닛_공격범위_시작최대_c => StatsData.AttackStartRangeMax,
			StatsType.유닛_공격범위_종료최대_c => StatsData.AttackLimitRangeMax,
			StatsType.유닛_행동범위_c => StatsData.ActionRange, 
			StatsType.유닛_시야범위_c => StatsData.VisionRange, 

			// --- ⚙️ Cycle (공격 주기) Properties ---
			StatsType.유닛_조준지연시간_c => StatsData.AimDelayTime, 
			StatsType.유닛_연속공격지연시간_c => StatsData.ContinuousAttackDelayTime,
			StatsType.유닛_재공격지연시간_c => StatsData.ReattackDelayTime, 
			StatsType.유닛_재장전시간_c => StatsData.ReloadTime,

			// --- 彈 Ammo (탄약) Properties ---
			StatsType.유닛_탄용량 => StatsData.AmmunitionCapacity * INT_TO_FLOAT_DIVISOR,
			StatsType.유닛_동시공격개수 => StatsData.ConcurrentAttackCount * INT_TO_FLOAT_DIVISOR,
			StatsType.유닛_연속공격횟수 => StatsData.ContinuousAttackCount * INT_TO_FLOAT_DIVISOR,

			// --- 💥 Offense (공격) Properties ---
			StatsType.유닛_공격력 => StatsData.AttackPower * INT_TO_FLOAT_DIVISOR,
			StatsType.유닛_치명공격력 => StatsData.CriticalAttackPower * INT_TO_FLOAT_DIVISOR,
			StatsType.유닛_치명피해율 => StatsData.CriticalDamageRatio * INT_TO_FLOAT_DIVISOR,
			StatsType.유닛_관통레벨 => StatsData.PenetrationLevel * INT_TO_FLOAT_DIVISOR,
			StatsType.유닛_EMP충격레벨 => StatsData.EMPImpactLevel * INT_TO_FLOAT_DIVISOR,
			StatsType.유닛_상태이상적용레벨 => StatsData.StatusPotencyLevel * INT_TO_FLOAT_DIVISOR,
			StatsType.유닛_공격명중기회 => StatsData.HitChanceScore * INT_TO_FLOAT_DIVISOR,
			StatsType.유닛_치명명중기회 => StatsData.CriticalChanceScore * INT_TO_FLOAT_DIVISOR,

			// --- 🛡️ Defense (방어) Properties ---
			StatsType.유닛_방어력 => StatsData.AntiAttackPower * INT_TO_FLOAT_DIVISOR,
			StatsType.유닛_치명방어력 => StatsData.AntiCriticalAttackPower * INT_TO_FLOAT_DIVISOR,
			StatsType.유닛_장갑레벨 => StatsData.AntiPenetrationLevel * INT_TO_FLOAT_DIVISOR,
			StatsType.유닛_EMP방호레벨 => StatsData.AntiEMPImpactLevel * INT_TO_FLOAT_DIVISOR,
			StatsType.유닛_상태이상저항레벨 => StatsData.AntiStatusPotencyLevel * INT_TO_FLOAT_DIVISOR,
			StatsType.유닛_공격회피기회 => StatsData.AntiHitChanceScore * INT_TO_FLOAT_DIVISOR,
			StatsType.유닛_치명회피기회 => StatsData.AntiCriticalChanceScore * INT_TO_FLOAT_DIVISOR,

			// --- RuntimeData 값 (가장 마지막 배치) ---
			StatsType.유닛_현재내구도 => RuntimeData.CurrentDurability * INT_TO_FLOAT_DIVISOR,
			StatsType.유닛_사용탄수 => RuntimeData.AmmunitionUsed * INT_TO_FLOAT_DIVISOR,

			// 정의되지 않은 StatsType에 대한 기본값
			_ => 0f,
		} + RuntimeData.DynamicKeyStatsList.GetValue(type) * INT_TO_FLOAT_DIVISOR;
	}

	void IStatsValueControl.SetStatsValue(StatsType type, int value)
	{
		// RuntimeData는 클래스 필드로 접근 가능하다고 가정합니다.
		switch (type)
		{
			case StatsType.유닛_현재내구도:
			RuntimeData.CurrentDurability = value;
			break;
			case StatsType.유닛_사용탄수:
			RuntimeData.AmmunitionUsed = value;
			break;
			default:
			RuntimeData.DynamicKeyStatsList.SetValue(type, value);
			break;
		}
	}

	void IStatsValueControl.SetStatsValuePrecent(StatsType type, float valuePercent)
    {
		// RuntimeData는 클래스 필드로 접근 가능하다고 가정합니다.
		switch (type)
		{
			case StatsType.유닛_현재내구도:
			RuntimeData.CurrentDurability = Mathf.FloorToInt(valuePercent * 100);
			break;
			case StatsType.유닛_사용탄수:
			RuntimeData.AmmunitionUsed = Mathf.FloorToInt(valuePercent * 100);
			break;
			default:
			RuntimeData.DynamicKeyStatsList.SetValue(type, Mathf.FloorToInt(valuePercent * 100));
			break;
		}
	}
}
public partial class UnitObject : ICombatCommon, ICombatOffense, ICombatDefance
{
	public ICombatCommon ThisCombatStats => this;
	public ICombatOffense ThisOffense => this;
	public ICombatDefance ThisDefance => this;

	public void TakeDamage(int damage, DamageCommander.DamageFlag flag)
	{
		int currentDurability = ThisCombatStats.CurrentDurability;
		currentDurability -= damage;
		ThisCombatStats.SetStatsValue(StatsType.유닛_현재내구도, currentDurability);

		// Show Demage Effect


		if (currentDurability <= 0)
		{
			DamageDeath();
		}
	}
}