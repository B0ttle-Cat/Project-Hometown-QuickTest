using System.Collections.Generic;

using UnityEngine;

using Collider = UnityEngine.Collider;
using RaycastHit = UnityEngine.RaycastHit;



// ProjectileMovement.ProjectileKey와 ProjectileMovement.ProjectileConstantData를 사용

namespace StrategyManagerModule
{
	public partial class StrategyUpdate
	{
		public class StrategyUpdate_ProjectileHitCheck : StrategyUpdateSubClass<StrategyUpdate_ProjectileHitCheck.HitCheck>
		{
			private RaycastHit[] raycastHits = null;
			public StrategyUpdate_ProjectileHitCheck(StrategyUpdate updater) : base(updater)
			{

			}

			protected override void Dispose()
			{
				StrategyManager.Pooling.RemoveChangeListener<ProjectileObject>(OnChangeValue);
				raycastHits = null;
			}

			protected override void Start()
			{
				raycastHits = new RaycastHit[IProjectileHit.MIN_ARRAY_CAPACITY];
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
				private readonly HashSet<Collider> alreadyHit;
				public IProjectileHit hitReporting => thisProjectile;

				public HitCheck(ProjectileObject projectile, StrategyUpdate_ProjectileHitCheck thisSubClass) : base(thisSubClass)
				{
					thisProjectile = projectile;
					alreadyHit = new();
				}

				protected override void OnDispose()
				{
					alreadyHit.Clear();
				}

				protected override void OnUpdate(in float deltaTime)
				{

				}
				public void ProjectileMoveCast(out int hitCount, ref RaycastHit[] raycastHits)
				{
					hitReporting.ProjectileMoveCast(out hitCount, ref raycastHits);
				}
				public void SendMoveCastReport(in int hitCount, in RaycastHit[] raycastHits)
				{
					var runtimeData = thisProjectile.RuntimeData;
					var statsData = thisProjectile.StatsData;
					int currPiercingCount = runtimeData.PiercingCount; // 관통 가능한 횟수
					int maxPiercingCount = statsData.PiercingMinMaxCount.y;

					for (int i = 0 ; i < hitCount && currPiercingCount < maxPiercingCount ; i++)
					{
						var hitCollider = raycastHits[i].collider;
						if (!alreadyHit.Add(hitCollider)) continue;

						hitReporting.HitReporting(raycastHits[i].collider);

						// HitReporting 이후 적절한 수치만큼 관통 카운트가 감소될 것.
						currPiercingCount = runtimeData.PiercingCount;
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
					var projectile = item.thisProjectile;
					if (projectile == null) continue;

					item.ProjectileMoveCast(out int hitCount, ref raycastHits);
					if (hitCount > 0) item.SendMoveCastReport(in hitCount, in raycastHits);
				}
			}
		}
	}
}