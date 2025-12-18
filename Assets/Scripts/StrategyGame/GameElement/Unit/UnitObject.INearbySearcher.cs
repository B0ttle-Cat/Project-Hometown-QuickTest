using System;

using UnityEngine;

public partial class UnitObject : INearbyElement // And INearbySearcher
{
	UnitVisionRangeSearcher visionRangeSearcher;
	UnitActionRangeSearcher actionRangeSearcher;
	UnitAttackStartRangeSearcher attackStartRangeSearcher;
	UnitAttackLimitRangeSearcher attackLimitRangeSearcher;

	public INearbySearcher VisionSearcher => visionRangeSearcher;
	public INearbySearcher ActionSearcher => actionRangeSearcher;
	public INearbySearcher AttackStartSearcher => attackStartRangeSearcher;
	public INearbySearcher AttackLimitSearcher => attackLimitRangeSearcher;

	public INearbySearcherAPI VisionSearcherAPI => visionRangeSearcher.SearcherAPI;
	public INearbySearcherAPI ActionSearcherAPI => actionRangeSearcher.SearcherAPI;
	public INearbySearcherAPI AttackStartSearcherAPI => attackStartRangeSearcher.SearcherAPI;
	public INearbySearcherAPI AttackLimitSearcherAPI => attackLimitRangeSearcher.SearcherAPI;
	partial void InitNearby()
	{
		visionRangeSearcher = new UnitVisionRangeSearcher(this);
		actionRangeSearcher = new UnitActionRangeSearcher(this);
		attackStartRangeSearcher = new UnitAttackStartRangeSearcher(this);
		attackLimitRangeSearcher = new UnitAttackLimitRangeSearcher(this);

		StrategyManager.Collector.Add<INearbyElement>(this);
	}
	partial void DeinitNearby()
	{
		visionRangeSearcher?.Dispose();
		actionRangeSearcher?.Dispose();
		attackStartRangeSearcher?.Dispose();
		attackLimitRangeSearcher?.Dispose();

		StrategyManager.Collector.Remove<INearbyElement>(this);
	}
	Vector3 INearbyElement.Position => ThisMovement.CurrentPosition;
	float INearbyElement.Radius => ThisMovement.CurrentRadius;

	public abstract class UnitNearbySearcher<T> : INearbySearcher, IDisposable where T : NearbySearching
	{
		protected readonly UnitObject ThisUnit;
		protected readonly T nearbySearching;
		public INearbySearcher ThisSearcher => this;
		public INearbySearcherAPI SearcherAPI => nearbySearching;
		int INearbySearcher.FactionID => ThisUnit.FactionID;
		bool INearbySearcher.IsEnable => ThisUnit.enabled;
		public UnitNearbySearcher(UnitObject unitObject)
		{
			this.ThisUnit = unitObject;

			if (!unitObject.TryGetComponent<T>(out nearbySearching))
			{
				nearbySearching = unitObject.gameObject.AddComponent<T>();
			}
			SearcherAPI.Init(this);

			StrategyManager.Collector.Add<T>(nearbySearching);
		}
		public void Dispose()
		{
			if (nearbySearching.IsNullRef())
			{
				StrategyManager.Collector.Add<T>(nearbySearching);
			}
		}
	}
	public class UnitVisionRangeSearcher : UnitNearbySearcher<VisionRangeSearching>, IVisionRangeSearcher
	{
		public UnitVisionRangeSearcher(UnitObject unitObject) : base(unitObject)
		{
		}
		bool INearbySearcher.IsEnable => !ThisUnit.HasOperation && ThisUnit.enabled;
		Vector3 INearbySearcher.SearchCenter => ThisUnit.ThisCombatHandler.Position;
		float INearbySearcher.SearchRange => ThisUnit.ThisCombatHandler.VisionRange + ThisUnit.ThisMovement.CurrentRadius;
	}
	public class UnitActionRangeSearcher : UnitNearbySearcher<ActionRangeSearching>, IActionRangeSearcher
	{
		public UnitActionRangeSearcher(UnitObject unitObject) : base(unitObject)
		{
		}
		bool INearbySearcher.IsEnable => !ThisUnit.HasOperation && ThisUnit.enabled;
		Vector3 INearbySearcher.SearchCenter => ThisUnit.ThisCombatHandler.Position;
		float INearbySearcher.SearchRange => ThisUnit.ThisCombatHandler.ActionRange + ThisUnit.ThisMovement.CurrentRadius;
	}
	public class UnitAttackStartRangeSearcher : UnitNearbySearcher<AttackStartRangeSearching>, IActionRangeSearcher
	{
		public UnitAttackStartRangeSearcher(UnitObject unitObject) : base(unitObject)
		{
		}
		Vector3 INearbySearcher.SearchCenter => ThisUnit.ThisCombatHandler.Position;
		float INearbySearcher.SearchRange => ThisUnit.ThisCombatStats.AttackStartRangeMax + ThisUnit.ThisMovement.CurrentRadius;
		float INearbySearcher.SearchMinRange => ThisUnit.ThisCombatStats.AttackStartRangeMin + ThisUnit.ThisMovement.CurrentRadius;
	}
	public class UnitAttackLimitRangeSearcher : UnitNearbySearcher<AttackLimitRangeSearching>, IActionRangeSearcher
	{
		public UnitAttackLimitRangeSearcher(UnitObject unitObject) : base(unitObject)
		{
		}
		Vector3 INearbySearcher.SearchCenter => ThisUnit.ThisCombatHandler.Position;
		float INearbySearcher.SearchRange => ThisUnit.ThisCombatStats.AttackLimitRangeMax + ThisUnit.ThisMovement.CurrentRadius;
		float INearbySearcher.SearchMinRange => ThisUnit.ThisCombatStats.AttackLimitRangeMin + ThisUnit.ThisMovement.CurrentRadius;
	}
}