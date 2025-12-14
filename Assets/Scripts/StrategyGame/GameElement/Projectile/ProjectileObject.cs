using System.Collections.Generic;

using Sirenix.OdinInspector;

using StrategyManagerModule;

using UnityEngine;

using static StrategyGamePlayData;

public partial class ProjectileObject : MonoBehaviour
{
	[SerializeField, InlineProperty, HideLabel]
	private ProjectileRuntimeData runtimeData;
	[SerializeField, InlineProperty, HideLabel]
	private ProjectileStatsData statsData;
	public ProjectileRuntimeData RuntimeData { get => runtimeData; private set => runtimeData = value; }
	public ProjectileStatsData StatsData { get => statsData; private set => statsData = value; }


	public ICombatHandler Order { get; private set; }
	public ICombatCommon OrderCombatCommon => Order == null ? null : Order as ICombatCommon;
	public void Init()
	{
		runtimeData = null;
		statsData = null;
	}
	public void Init(StrategyStartSetterData.ProjectileData.Info setterData)
	{
		RuntimeData = new ProjectileRuntimeData(setterData);
	}
	public void Init(ProjectileProfileObject profile)
	{
		if (StatsData == null)
		{
			StatsData = new ProjectileStatsData(profile);
		}
		if (RuntimeData == null)
		{
			RuntimeData = new ProjectileRuntimeData(profile);
		}
	}


	public void InitOther()
	{
		InitLife();
		InitScale();
		InitMovement();
		InitHitReporting();
	}
	partial void InitMovement();
	partial void InitHitReporting();
	public void DeInit()
	{
		DeinitMovment();
		DeinitHitReporting();
		runtimeData = null;
	}
	partial void DeinitMovment();
	partial void DeinitHitReporting();
	private void InitScale()
	{
		transform.localScale = Vector3.one * statsData.CollisionRadius;
	}
	public void SetTarget(ICombatHandler order, ITargetableCombatant target)
	{
		Order = order;
		ThisMovement.SetTarget(order, target);
	}
}
public partial class ProjectileObject : IStrategyElementDestroyer
{
	public IStrategyElementDestroyer ThisDestroyer => this;
	public bool IsDestroy { get; set; }

	ObjectLifetime objectLifetime;
	public void InitLife()
	{
		IsDestroy = false;
		if (objectLifetime == null || !TryGetComponent<ObjectLifetime>(out objectLifetime))
		{
			objectLifetime = gameObject.AddComponent<ObjectLifetime>();
		}
		objectLifetime.enabled = true;
		objectLifetime.ResetTime(RuntimeData.LifeTime, LifeTimeDeath);
	}
	void IStrategyElementDestroyer.OnDestroy()
	{
		StrategyElementFactory.Destroy(this);
	}
	private void LifeTimeDeath()
	{
		if (IsDestroy) return;
		objectLifetime.enabled = false;

		ThisDestroyer.ReservationDestroy();
	}

	private void HitableDeath(ICombatOffense offense, ICombatDefance defance)
	{
		if (IsDestroy) return;
		objectLifetime.enabled = false;

		var projectileHitEffectKey =  StatsData.ProjectileHitEffectKey;
		if (projectileHitEffectKey != SubEffectKey.None)
		{
			// HitEffect 생성
		}

		if (StatsData.ExplosionEnabled)
		{
			var explosionEffectKey = StatsData.ExplosionEffectKey;
			if (offense != null && explosionEffectKey != SubEffectKey.None)
			{
				Vector2 explosionMinMaxRadius = StatsData.ExplosionMinMaxRadius;
				float explosionDelayAfterHit = StatsData.ExplosionDelayAfterHit;
				AnimationCurve explosionFalloffCurve = StatsData.ExplosionFalloffCurve;

				// ExplosionEffect 생성
			}
		}
		else if (StatsData.EmpShockEnabled)
		{
			var empShockEffectKey = StatsData.EmpShockEffectKey;
			if (offense != null && empShockEffectKey != SubEffectKey.None)
			{
				float empShockPropagationDistance = StatsData.EmpShockPropagationDistance;
				int empShockChainCount = StatsData.EmpShockChainCount;
				int empShockDepthCount =StatsData.EmpShockDepthCount;
				int empShockOverlapsCount = StatsData.EmpShockOverlapsCount;
				AnimationCurve empShockFalloffCurve = StatsData.EmpShockFalloffCurve;

				// EmpChainEffec 생성
			}
		}

		ThisDestroyer.ReservationDestroy();
	}
	private void MovementDeath(Vector3 deathPosition)
	{
		if (IsDestroy) return;
		objectLifetime.enabled = false;


		if (StatsData.ExplosionEnabled)
		{
			var offense = OrderCombatCommon.ThisOffense;
			var explosionEffectKey = StatsData.ExplosionEffectKey;
			if (offense != null && explosionEffectKey != SubEffectKey.None)
			{
				Vector2 explosionMinMaxRadius = StatsData.ExplosionMinMaxRadius;
				float explosionDelayAfterHit = StatsData.ExplosionDelayAfterHit;
				AnimationCurve explosionFalloffCurve = StatsData.ExplosionFalloffCurve;

				// ExplosionEffect 생성
			}
		}
		else if (StatsData.EmpShockEnabled)
		{
			var offense = OrderCombatCommon.ThisOffense;

			var empShockEffectKey = StatsData.EmpShockEffectKey;
			if (offense != null && empShockEffectKey != SubEffectKey.None)
			{
				float empShockPropagationDistance = StatsData.EmpShockPropagationDistance;
				int empShockChainCount = StatsData.EmpShockChainCount;
				int empShockDepthCount =StatsData.EmpShockDepthCount;
				int empShockOverlapsCount = StatsData.EmpShockOverlapsCount;
				AnimationCurve empShockFalloffCurve = StatsData.EmpShockFalloffCurve;

				// EmpChainEffec 생성
			}
		}

		ThisDestroyer.ReservationDestroy();
	}
}

public partial class ProjectileObject : IStrategyPoolingElement
{
	IStrategyElement IStrategyElement.ThisElement => this;
	int IStrategyElement.ID { get; set; }
	GameObject IStrategyPoolingElement.PrefabReference { get; set; }
	void IStrategyElement.InStrategyCollector()
	{

	}
	void IStrategyElement.OutStrategyCollector()
	{

	}
	void IStrategyStartGame.OnStartGame()
	{
	}
	void IStrategyStartGame.OnStopGame()
	{
	}
}
public partial class ProjectileObject : IProjectileHit
{
	public IProjectileHit ThisProjectileHit => this;

	public LayerMask hitLayerMask;
	[ShowInInspector]
	private HashSet<Collider> alreadyHit;
	partial void InitHitReporting()
	{
		hitLayerMask = LayerMask.GetMask("Ground", "HardObject", "Hitable", "Unit");
		alreadyHit = new HashSet<Collider>();
	}
	partial void DeinitHitReporting()
	{
		alreadyHit.Clear();
	}
	void IProjectileHit.ProjectileOverlap(out int overlapCount, ref Collider[] colliders)
	{
		if (colliders == null || colliders.Length < IProjectileHit.MIN_ARRAY_CAPACITY)
		{
			colliders = new Collider[IProjectileHit.MIN_ARRAY_CAPACITY];
		}

		float collisionRadius = statsData.CollisionRadius;
		var prevPosition = PrevPosition;

		int currentCapacity = colliders.Length;
		if (!UnityEngine.Physics.CheckSphere(prevPosition, collisionRadius))
		{
			overlapCount = 0;
			return;
		}

		overlapCount = UnityEngine.Physics.OverlapSphereNonAlloc(prevPosition, collisionRadius, colliders, hitLayerMask);

		// NonAlloc으로 감지된 개수가 배열 크기와 같다면, 충돌체가 더 있을 가능성이 높음.
		if (overlapCount == currentCapacity)
		{
			Collider[] allOverlap = UnityEngine.Physics.OverlapSphere(prevPosition, collisionRadius, hitLayerMask);

			// 실제 충돌 개수
			overlapCount = allOverlap.Length;

			// Step A: 실제 충돌 개수(allHits.Length)가 기존 배열 크기보다 큰지 확인
			if (overlapCount > currentCapacity)
			{
				// 새로운 크기를 MIN_ARRAY_CAPACITY 단위로 계산하여 확장합니다.
				int numUnits = Mathf.CeilToInt((float)overlapCount / IProjectileHit.MIN_ARRAY_CAPACITY);
				int newCapacity = numUnits * IProjectileHit.MIN_ARRAY_CAPACITY;

				// 기존 배열의 내용을 새 배열로 옮기지 않고, allHits 배열 자체를 새 버퍼로 사용
				colliders = new Collider[newCapacity];
				System.Array.Copy(allOverlap, colliders, overlapCount);
				Debug.LogWarning($"ProjectileOverlap 에 사용중인 베열 크기가 부족합니다. ({currentCapacity} -> {newCapacity}). 배열을 확장합니다.");
			}
			else if (overlapCount == currentCapacity)
			{
				int newCapacity = currentCapacity + IProjectileHit.MIN_ARRAY_CAPACITY;
				colliders = new Collider[newCapacity];
				System.Array.Copy(allOverlap, colliders, overlapCount);
				Debug.LogWarning($"ProjectileOverlap 에 사용중인 베열 크기가 부족합니다. ({currentCapacity} -> {newCapacity}). 배열을 확장합니다.");
			}
			else
			{
				// 크기는 그대로 유지하고, 정확성을 위해 Alloc 결과를 기존 버퍼에 복사합니다.
				System.Array.Copy(allOverlap, colliders, overlapCount);
			}
		}
	}

	void IProjectileHit.ProjectileMoveCast(out int hitCount, ref RaycastHit[] raycastHits)
	{
		// 2. 배열 최소 크기 보장 로직
		if (raycastHits == null || raycastHits.Length < IProjectileHit.MIN_ARRAY_CAPACITY)
		{
			raycastHits = new RaycastHit[IProjectileHit.MIN_ARRAY_CAPACITY];
		}

		float collisionRadius = statsData.CollisionRadius;
		var prevPosition = PrevPosition;
		var moveDiraction = MoveDiraction;
		float maxLength = Vector3.Distance(prevPosition, CurrentPosition);

		// 2. NonAlloc으로 충돌 검사 (GC 없음)
		int currentCapacity = raycastHits.Length;
		hitCount = UnityEngine.Physics.SphereCastNonAlloc(prevPosition, collisionRadius, moveDiraction, raycastHits, maxLength, hitLayerMask);

		// 3. 충돌 누락 검사 및 배열 확장 폴백 (GC 발생 영역)
		// NonAlloc으로 감지된 개수가 배열 크기와 같다면, 충돌체가 더 있을 가능성이 높음.
		if (hitCount == currentCapacity)
		{
			RaycastHit[] allHits = UnityEngine.Physics.SphereCastAll(prevPosition,collisionRadius,moveDiraction,maxLength,hitLayerMask);

			// 실제 충돌 개수
			hitCount = allHits.Length;

			// Step A: 실제 충돌 개수(allHits.Length)가 기존 배열 크기보다 큰지 확인
			if (hitCount > currentCapacity)
			{
				// 새로운 크기를 MIN_ARRAY_CAPACITY 단위로 계산하여 확장합니다.
				int numUnits = Mathf.CeilToInt((float)hitCount / IProjectileHit.MIN_ARRAY_CAPACITY);
				int newCapacity = numUnits * IProjectileHit.MIN_ARRAY_CAPACITY;

				// 기존 배열의 내용을 새 배열로 옮기지 않고, allHits 배열 자체를 새 버퍼로 사용
				raycastHits = new RaycastHit[newCapacity];
				System.Array.Copy(allHits, raycastHits, hitCount);
				Debug.LogWarning($"ProjectileMoveCast 에 사용중인 베열 크기가 부족합니다. ({currentCapacity} -> {newCapacity}). 배열을 확장합니다.");
			}
			else if (hitCount == currentCapacity)
			{
				int newCapacity = currentCapacity + IProjectileHit.MIN_ARRAY_CAPACITY;
				raycastHits = new RaycastHit[newCapacity];
				System.Array.Copy(allHits, raycastHits, hitCount);
				Debug.LogWarning($"ProjectileMoveCast 에 사용중인 베열 크기가 부족합니다. ({currentCapacity} -> {newCapacity}). 배열을 확장합니다.");
			}
			else
			{
				// 크기는 그대로 유지하고, 정확성을 위해 Alloc 결과를 기존 버퍼에 복사합니다.
				System.Array.Copy(allHits, raycastHits, hitCount);
			}
		}

		// 순서를 보장하기 위해 거리 순으로 정렬합니다.
		if (hitCount > 1)
		{
			System.Array.Sort(raycastHits, 0, hitCount, Comparer<RaycastHit>.Create((a, b) => a.distance.CompareTo(b.distance)));
		}
	}
	void IProjectileHit.SendHitReporting(in int overlapCount, in Collider[] overlaps, in int hitCount, in RaycastHit[] raycastHits)
	{
		if (hitCount <= 0) return;

		var statsData = StatsData;


		for (int i = 0 ; i < overlapCount ; i++)
		{
			var collider = overlaps[i];
			if (!alreadyHit.Add(collider))
			{
				continue;
			}

			ThisProjectileHit.HitReporting(collider);

			if (IsDestroy)
			{
				return;
			}
		}
		for (int i = 0 ; i < hitCount ; i++)
		{
			var collider = raycastHits[i].collider;
			if (!alreadyHit.Add(collider))
			{
				continue;
			}

			ThisProjectileHit.HitReporting(collider);

			if (IsDestroy)
			{
				return;
			}
		}
	}


	void IProjectileHit.HitOtherObject(GameObject gameObject)
	{

	}

	void IProjectileHit.Hitable(IHitableCombatant hitable)
	{
		if (hitable is not ICombatDefance defance) return;
		if (defance == null) return;
		var offense = OrderCombatCommon?.ThisOffense;
		if (offense == null) return;

		if (offense.FactionID == defance.FactionID) return;


		float projectileDamageFactor = 1f;
		if (!CombatUtility.CheckChance(CombatUtility.CalculateHitChance(offense, defance)))
		{
			new CombatUtility.DamageCommander(offense, defance, projectileDamageFactor, CombatUtility.DamageFlag.Miss);
			return;
		}

		if (StatsData.PiercingEnable)
		{
			RuntimeData.PiercingCount = Hitable_관통(RuntimeData.PiercingCount);
			if (runtimeData.PiercingCount >= statsData.PiercingMaxCount)
			{
				HitableDeath(offense, defance);
			}
			int Hitable_관통(int piercingCount)
			{
				projectileDamageFactor = statsData.PiercingFalloffMultiplier(piercingCount);

				int baseDifference = offense.PenetrationLevel - (defance.AntiPenetrationLevel + (defance.ProtectionType is ProtectionType.강화장갑?1:0));
				if (baseDifference > 0)
				{
					piercingCount += baseDifference + 1;
					// 관통됨 2회의 데미지를 준다.
					new CombatUtility.DamageCommander(offense, defance, projectileDamageFactor, CombatUtility.DamageFlag.Hit | CombatUtility.DamageFlag.Pierce);
					new CombatUtility.DamageCommander(offense, defance, projectileDamageFactor, CombatUtility.DamageFlag.Hit | CombatUtility.DamageFlag.Pierce);
				}
				else if (baseDifference == 0)
				{
					piercingCount += 1;
					new CombatUtility.DamageCommander(offense, defance, projectileDamageFactor, CombatUtility.DamageFlag.Hit | CombatUtility.DamageFlag.Pierce);
				}
				else
				{
					piercingCount = int.MaxValue;
					new CombatUtility.DamageCommander(offense, defance, projectileDamageFactor, CombatUtility.DamageFlag.Hit | CombatUtility.DamageFlag.Pierce);
				}
				return piercingCount;
			}
		}
		else if (StatsData.ExplosionEnabled)
		{
			HitableDeath(offense, defance);
		}
		else if (StatsData.EmpShockEnabled)
		{
			HitableDeath(offense, defance);
		}
		else
		{
			new CombatUtility.DamageCommander(offense, defance, projectileDamageFactor, CombatUtility.DamageFlag.Hit);
			HitableDeath(offense, defance);
		}
	}
	void IProjectileHit.HitOtherElement(IStrategyElement hit)
	{

	}
}