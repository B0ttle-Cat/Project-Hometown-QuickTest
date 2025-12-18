using System;

using UnityEngine;

using static StrategyGamePlayData;

public interface ICombatHandler : ICombatCommon, IStrategyElement
{
	ICombatHandler ThisCombatHandler { get; }
	Transform transform { get; }
	Vector3 Position { get; }
	Vector3 AttackStartPosition { get; }
	bool IsCombatState { get; }
	ITargetableCombatant ActionTarget { get; }
	ITargetableCombatant AttackTarget { get; }
	bool HasActionTarget => ActionTarget.IsNotNullRef();
	bool HasAttackTarget => AttackTarget.IsNotNullRef();
	ITargetableCombatant CurrentTarget => (HasAttackTarget ? AttackTarget : (HasActionTarget ? ActionTarget : null));
	bool HasCurrentTarget => HasActionTarget || HasActionTarget;
	//[Obsolete("remove", true)]
	//bool TargetInStartAttackRange { get; }
	//[Obsolete("remove", true)]
	//bool TargetInLimitAttackRange { get; }
	//
	//[Obsolete("remove", true)]
	//bool IsOperationCombatState { get; set; }
	//[Obsolete("remove", true)]
	//ITargetableCombatant OperationCurrentTarget { get; set; }
	//[Obsolete("remove", true)]
	//bool HasOperationCurrentTarget => OperationCurrentTarget.IsNotNullRef();

	//[Obsolete("remove", true)]
	//void UpdateParameters();
	bool SomthingInActionRange();
	bool SomthingInAttackRange();
	bool HasKeepAttackTarget();
	void UpdateNewNearbyTarget();
	//void ChangeCombatAttackTarget(in ITargetableCombatant newTarget);
	//void ChangeCombatActionTarget(in ITargetableCombatant newTarget);
	event Action<ITargetableCombatant> OnChangeCurrentCombatTarget;
}
public interface ICombatCommon : IStatsValueControl, IStrategyElement
{
	ICombatCommon ThisCombatStats { get; }
	ICombatOffense ThisOffense { get; }
	ICombatDefance ThisDefance { get; }
	int FactionID { get; }

	// 🛡️ 내구도 및 회복 스탯 (Durability & Recovery)
	int MaxDurability => GetStatsValue(StatsType.유닛_최대내구도);
	int CurrentDurability => GetStatsValue(StatsType.유닛_현재내구도);
	int HealingPower => GetStatsValue(StatsType.유닛_치유력);
	int RecoveryPower => GetStatsValue(StatsType.유닛_회복력);

	// 💨 이동 및 점령 스탯 (Movement & Capture)
	float MovementSpeed => GetStatsValuePrecent(StatsType.유닛_이동속도_c);
	float CaptureScore => GetStatsValue(StatsType.유닛_점령점수);

	// ⚙️ 공격 시스템 계수 (Cycle System Multipliers)
	int AmmunitionCapacity => GetStatsValue(StatsType.유닛_탄용량);
	int AmmunitionUsed => GetStatsValue(StatsType.유닛_사용탄수);
	int ConcurrentProjectileCount => GetStatsValue(StatsType.유닛_동시공격개수);
	int ContinuousAttackCount => GetStatsValue(StatsType.유닛_연속공격횟수);

	// ⏱️ 딜레이 스탯 (Time Delays)
	float AimDelayTime => GetStatsValuePrecent(StatsType.유닛_조준지연시간_c);
	float ContinuousAttackDelayTime => GetStatsValuePrecent(StatsType.유닛_연속공격지연시간_c);
	float ReattackDelayTime => GetStatsValuePrecent(StatsType.유닛_재공격지연시간_c);
	float ReloadTime => GetStatsValuePrecent(StatsType.유닛_재장전시간_c);

	// 💸 공격 소모 자원 스탯 (Cycle Cost)
	float AttackCostMaterial => GetStatsValue(StatsType.유닛_공격소모_물자);
	float AttackCostPower => GetStatsValue(StatsType.유닛_공격소모_전력);

	// 🔭 범위 스탯 (Range)
	float AttackRangeLimitMin => GetStatsValuePrecent(StatsType.유닛_공격범위_종료최소_c);
	float AttackRangeStartMin => GetStatsValuePrecent(StatsType.유닛_공격범위_시작최소_c);
	float AttackRangeStartMax => GetStatsValuePrecent(StatsType.유닛_공격범위_시작최대_c);
	float AttackRangeLimitMax => GetStatsValuePrecent(StatsType.유닛_공격범위_종료최대_c);
	float ActionRange => GetStatsValuePrecent(StatsType.유닛_행동범위_c);
	float VisionRange => GetStatsValuePrecent(StatsType.유닛_시야범위_c);
}
public interface ICombatOffense : IStatsValueControl, IStrategyElement
{
	ICombatOffense ThisOffense { get; }
	int FactionID { get; }
	WeaponType WeaponType => WeaponType.일반;

	// 💥 기본 피해 스탯 (Base Damage)
	int AttackPower => GetStatsValue(StatsType.유닛_공격력);

	// 🎯 치명타 스탯 (Critical)
	float CriticalDamageRatio => GetStatsValuePrecent(StatsType.유닛_치명피해율);
	int CriticalAttackPower => GetStatsValue(StatsType.유닛_치명공격력);

	// 🛡️ 관통 및 적용 스탯 (Penetration & Application)
	int PenetrationLevel => GetStatsValue(StatsType.유닛_관통레벨);
	int EMPImpactLevel => GetStatsValue(StatsType.유닛_EMP충격레벨);
	int StatusPotencyLevel => GetStatsValue(StatsType.유닛_상태이상적용레벨);

	// 📈 확률 기회 스탯 (Chance Score)
	int HitChanceScore => GetStatsValue(StatsType.유닛_공격명중기회);
	int CriticalChanceScore => GetStatsValue(StatsType.유닛_치명명중기회);
}
public interface ICombatDefance : IStatsValueControl, IStrategyElement
{
	ICombatDefance ThisDefance { get; }
	int FactionID { get; }
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


	void TakeDamage(int damage, DamageCommander.DamageFlag flag);
}
