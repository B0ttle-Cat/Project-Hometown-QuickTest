using System.Collections.Generic;

using UnityEngine;

using Collider = UnityEngine.Collider;



// ProjectileMovement.ProjectileKey와 ProjectileMovement.ProjectileConstantData를 사용

namespace StrategyManagerModule
{
	public partial class StrategyUpdate
	{
        public class StrategyUpdate_ProjectileHitCheck : StrategyUpdateSubClass<StrategyUpdate_ProjectileHitCheck.HitCheck>
		{
			public StrategyUpdate_ProjectileHitCheck(StrategyUpdate updater) : base(updater)
			{
			}

			protected override void Dispose()
			{
				StrategyManager.Pooling.RemoveChangeListener<ProjectileObject>(OnChangeValue);
			}

			protected override void Start()
			{
				StrategyManager.Pooling.AddChangeListener<ProjectileObject>(OnChangeValue, true);
			}

			private void OnChangeValue(GameObject element, bool added)
			{
				if (element == null || !element.TryGetComponent<ProjectileObject>(out var component)) return;

				if (added)
				{
					updateList.Add(new(component, this));
				}
				else
				{
					updateList.RemoveAll(f => f.thisProjectile == component);
				}
			}

			public class HitCheck : UpdateLogic
			{
				public readonly  ProjectileObject thisProjectile;
				public IProjectileMovement thisMovement => thisProjectile.ThisMovement;
				public IProjectileHitReporting hitReporting => thisProjectile;

				public readonly HashSet<Collider> alreadyHit;

				public LayerMask hitLater;
				private RaycastHit[] raycastHits = null;

				public HitCheck(ProjectileObject projectile, StrategyUpdate_ProjectileHitCheck thisSubClass) : base(thisSubClass)
				{
					thisProjectile = projectile;
					alreadyHit = new();
				}

				protected override void OnDispose()
				{
					alreadyHit.Clear();
					raycastHits = null;
				}

				protected override void OnUpdate(in float deltaTime)
				{
					// Job/ECS 를 사용하지 않을경우 다음과 같이 처리한다.

					var runtimeData = thisProjectile.RuntimeData;
					var statsData = thisProjectile.StatsData;
					if (runtimeData == null || statsData == null) return;


					int maxPiercingCount = Mathf.Max(statsData.PiercingMinMaxPoint.x, statsData.PiercingMinMaxPoint.y);
					int currPiercingCount = runtimeData.PiercingCount; // 관통 가능한 횟수
					if (maxPiercingCount <= 0 || currPiercingCount <= 0) return;

					if(raycastHits == null)
					{
						int hitMaxLength = maxPiercingCount * 2;
						if (hitMaxLength < 8) hitMaxLength = 8;
						raycastHits = new RaycastHit[hitMaxLength];
					}
					float collisionRadius = statsData.CollisionRadius; // 발사체의 반지름

					var prevPosition = thisMovement.PrevPosition; // 발사체 이전프레임 위치
					var currPosition = thisMovement.CurrentPosition; // 발사체 현제 프레임 위치
					var moveDiraction = thisMovement.MoveDiraction;
					float maxLength = Vector3.Distance(prevPosition, currPosition);

					int hitCount = UnityEngine.Physics.SphereCastNonAlloc(prevPosition, collisionRadius, moveDiraction, raycastHits, maxLength, hitLater);

					if (raycastHits.Length == hitCount)
					{
						// 놓친 Hit 가 있을 수 있음 대비할 것.
						raycastHits = UnityEngine.Physics.SphereCastAll(prevPosition, collisionRadius, moveDiraction, maxLength, hitLater);
						hitCount = raycastHits.Length;
					}

					for (int i = 0 ; i < hitCount ; i++)
					{
						var hitCollider = raycastHits[i].collider;
						if (!alreadyHit.Add(hitCollider)) continue;

						hitReporting.HitReporting(raycastHits[i].collider);

						// HitReporting 이후 적절한 수치만큼 관통 카운트가 감소될 것.
						currPiercingCount = runtimeData.PiercingCount;
						if (currPiercingCount <= 0)
						{
							// 더이상 관통할 수 없음;
							return;
						}
					}
				}
			}

			protected override void Update(in float deltaTime)
			{
				int length = UpdateList.Count;
				for (int i = 0 ; i < length ; i++)
				{
					var item = UpdateList[i];
					if (item == null) continue;
					item.Update(in deltaTime);
				}
			}
		}
	}
}