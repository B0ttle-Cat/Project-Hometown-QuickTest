using System;

using UnityEngine;

using Random = UnityEngine.Random;


public class DamageCommander : IDisposable
{
	[Flags]
	public enum DamageFlag
	{
		None        = 0,            // 0
		Miss        = 1 << 0,       // 빗나감
		Hit         = 1 << 1,       // 명중
		Critical    = 1 << 2,       // 치명
		Pierce      = 1 << 3,       // 관통 효과
		Explosion   = 1 << 4,       // 폭발 효과
		EMPShock    = 1 << 5,       // 에너지 효과
		Effective   = 1 << 6,       // 유효 상성
		Resist      = 1 << 7,       // 저항 상성
	}

	private readonly ICombatOffense offense;
	private readonly ICombatDefance defance;
	private readonly float projectileDemageFactor; // 폭심지와의 거리/관통횟수/감전깊이/등에 따라 주어지는 계수
	private DamageFlag flag;

	private float totalDamage;

	public DamageCommander(ICombatOffense offense, ICombatDefance defance, float projectileDemageFactor, DamageFlag flag)
	{
		this.offense = offense;
		this.defance = defance;
		this.projectileDemageFactor = projectileDemageFactor;
		this.flag = flag;

		totalDamage = 0;
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


	public virtual void ComputeDamage()
	{
		totalDamage = 0;
		if (flag == DamageFlag.None) return;
		if (flag.HasFlag(DamageFlag.Miss)) return;

		bool isCritical = CombatUtility.CheckChance(CombatUtility.CalculateCriticalChance(offense, defance));

		float baseDamage =  CombatUtility.CalculateDamage(offense, defance,isCritical, projectileDemageFactor);

		float typeFactor = CombatUtility.CalculateTypeFactor(offense,defance);

		totalDamage = baseDamage * typeFactor;
		if (totalDamage < 1f) totalDamage = 1f;
	}


	public virtual void InjectDamage()
	{
		if (flag == DamageFlag.None) return;

		if (flag.HasFlag(DamageFlag.Miss))
		{
			defance.TakeDamage(0, flag);
		}
		else
		{
			defance.TakeDamage(RandomDamage(), flag);
		}
		int RandomDamage()
		{
			float randomFactor = 1 + Random.insideUnitCircle.x * 0.1f;
			return Mathf.FloorToInt(Mathf.Max(1, totalDamage * randomFactor));
		}
	}
}
