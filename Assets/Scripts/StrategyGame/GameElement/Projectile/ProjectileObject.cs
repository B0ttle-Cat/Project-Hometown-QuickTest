using System.Collections.Generic;

using Sirenix.OdinInspector;

using StrategyManagerModule;

using UnityEngine;

public partial class ProjectileObject : MonoBehaviour
{
	[SerializeField, InlineProperty, HideLabel]
	private ProjectileRuntimeData runtimeData;
	[SerializeField, InlineProperty, HideLabel]
	private ProjectileStatsData statsData;
	public ProjectileRuntimeData RuntimeData { get => runtimeData; private set => runtimeData = value; }
	public ProjectileStatsData StatsData { get => statsData; private set => statsData = value; }


	public ICombatHandler Order { get; private set; }
	public ICombatCommon OrderCombatCommon => Order as ICombatCommon;

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
		InitScale();
		InitLifetime();
		InitMovement();
	}
	partial void InitLifetime();
	partial void InitMovement();
	partial void InitHitReporting();
	public void DeInit()
	{
		DeinitMovment();
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
public partial class ProjectileObject // Init Lifetime
{
	ProjectileLifetime objectLifetime;
	partial void InitLifetime()
	{
		if (objectLifetime == null || !TryGetComponent<ProjectileLifetime>(out objectLifetime))
		{
			objectLifetime = gameObject.AddComponent<ProjectileLifetime>();
		}
		objectLifetime.ResetTime(RuntimeData.LifeTime);
	}

}
public partial class ProjectileObject : IStrategyPoolingElement
{
	IStrategyElement IStrategyElement.ThisElement => this;
	int IStrategyElement.ID { get; set; }
	GameObject IStrategyPoolingElement.PrefabReference { get; set; }
	void IStrategyElement.InStrategyCollector()
	{
		runtimeData = null;
		statsData = null;
	}
	void IStrategyElement.OutStrategyCollector()
	{
		runtimeData = null;
		statsData = null;
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
	public LayerMask hitLayerMask;

	partial void InitHitReporting()
	{
		hitLayerMask = LayerMask.GetMask("Ground", "HardObject", "Hitable", "Unit");
	}
	partial void DeinitHitReporting()
	{

	}

	void IProjectileHit.ProjectileMoveCast(out int hitCount, ref RaycastHit[] raycastHits)
	{
		// 1. 초기 체크 및 변수 계산
		int currPiercingCount = RuntimeData.PiercingCount;
		int maxPiercingCount = StatsData.PiercingMinMaxCount.y;
		// 관통 횟수가 maxPiercingCount 를 이상이면 즉시 종료 (hitCount는 0 유지)
		if (currPiercingCount >= maxPiercingCount)
		{
			hitCount = 0;
			return;
		}

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

	void IProjectileHit.HitOtherObject(GameObject gameObject)
	{

	}

	void IProjectileHit.Hitable(IHitableCombatant hitable)
	{
		if (hitable is not ICombatDefance defance) return;
		if (defance == null) return;

		var order = OrderCombatCommon;
		var offense = order.ThisOffense;

		//offense.PenetrationLevel
		//
		//defance.AntiAttackPower
	}
	void IProjectileHit.HitOtherElement(IStrategyElement hit)
	{

	}


}