using UnityEngine;

using RaycastHit = UnityEngine.RaycastHit;



// ProjectileMovement.ProjectileKey와 ProjectileMovement.ProjectileConstantData를 사용

namespace StrategyManagerModule
{
	public partial class StrategyUpdate
	{
		public class StrategyUpdate_ProjectileHitCheck : StrategyUpdateSubClass<StrategyUpdate_ProjectileHitCheck.HitCheck>
		{
			private RaycastHit[] raycastHits = null;
			private Collider[] overlaps = null;
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
				overlaps = new Collider[IProjectileHit.MIN_ARRAY_CAPACITY];
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

				public IProjectileHit hitReporting => thisProjectile;

				StrategyGamePlayData.WeaponType weaponType;

				public HitCheck(ProjectileObject projectile, StrategyUpdate_ProjectileHitCheck thisSubClass) : base(thisSubClass)
				{
					thisProjectile = projectile;
				}

				protected override void OnDispose()
				{

				}

				protected override void OnUpdate(in float deltaTime)
				{

				}
				public void ProjectileOverlap(out int overlapCount, ref Collider[] overlaps)
				{
					hitReporting.ProjectileOverlap(out overlapCount, ref overlaps);
				}
				public void ProjectileMoveCast(out int hitCount, ref RaycastHit[] raycastHits)
				{
					hitReporting.ProjectileMoveCast(out hitCount, ref raycastHits);
				}
				public void SendHitReporting(in int overlapCount, in Collider[] overlaps, in int hitCount, in RaycastHit[] raycastHits)
				{
					hitReporting.SendHitReporting(in overlapCount, in overlaps, in hitCount, in raycastHits);
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

					item.ProjectileOverlap(out int overlapCount, ref overlaps);
					item.ProjectileMoveCast(out int hitCount, ref raycastHits);
					if (overlapCount + hitCount > 0) item.SendHitReporting(in overlapCount, in overlaps, in hitCount, in raycastHits);
				}
			}
		}
	}
}