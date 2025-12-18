using System;

using UnityEngine;

public partial class UnitObject : INearbyElement // And INearbySearcher
{
	UnitViewRangeSearcher viewRangeSearcher;
	UnitActionRangeSearcher actionRangeSearcher;
	UnitAttackStartRangeSearcher attackStartRangeSearcher;
	UnitAttackLimitRangeSearcher attackLimitRangeSearcher;

	public INearbySearcher ViewSearcher => viewRangeSearcher;
	public INearbySearcher ActionSearcher => actionRangeSearcher;
	public INearbySearcher AttackStartSearcher => attackStartRangeSearcher;
	public INearbySearcher AttackLimitSearcher => attackLimitRangeSearcher;

	public INearbySearcherAPI ViewSearcherAPI => viewRangeSearcher.SearcherAPI;
	public INearbySearcherAPI ActionSearcherAPI => actionRangeSearcher.SearcherAPI;
	public INearbySearcherAPI AttackStartSearcherAPI => attackStartRangeSearcher.SearcherAPI;
	public INearbySearcherAPI AttackLimitSearcherAPI => attackLimitRangeSearcher.SearcherAPI;
	partial void InitNearby()
	{
		viewRangeSearcher = new UnitViewRangeSearcher(this);
		actionRangeSearcher = new UnitActionRangeSearcher(this);
		attackStartRangeSearcher = new UnitAttackStartRangeSearcher(this);
		attackLimitRangeSearcher = new UnitAttackLimitRangeSearcher(this);

		StrategyManager.Collector.Add<INearbyElement>(this);
	}
	partial void DeinitNearby()
	{
		viewRangeSearcher?.Dispose();
		actionRangeSearcher?.Dispose();
		attackStartRangeSearcher?.Dispose();
		attackLimitRangeSearcher?.Dispose();

		StrategyManager.Collector.Remove<INearbyElement>(this);
	}
	Vector3 INearbyElement.Position => ThisMovement.CurrentPosition;
	float INearbyElement.Radius => ThisMovement.CurrentRadius;

	public abstract class UnitNearbySearcher<T> : IDisposable where T : NearbySearching
	{
		protected readonly UnitObject ThisUnit;
		protected readonly INearbySearcherAPI searcherAPI;
		public UnitNearbySearcher(UnitObject unitObject)
		{
			this.ThisUnit = unitObject;

			if (!unitObject.TryGetComponent<T>(out var nearbySearching))
			{
				nearbySearching = unitObject.gameObject.AddComponent<T>();
			}
			searcherAPI = nearbySearching;
		}
		public void Dispose()
		{
			if (searcherAPI.IsNotNullRef())
			{
				searcherAPI.Deinit();
			}
		}
	}
	public class UnitViewRangeSearcher : UnitNearbySearcher<ViewRangeSearching>, IViewRangeSearcher
	{
		public UnitViewRangeSearcher(UnitObject unitObject) : base(unitObject)
		{
			searcherAPI.Init(this);
		}
		public INearbySearcher ThisSearcher => this;
		public INearbySearcherAPI SearcherAPI => searcherAPI;
		Vector3 INearbySearcher.SearchCenter => ThisUnit.ThisCombatHandler.Position;
		float INearbySearcher.SearchRange => ThisUnit.ThisCombatHandler.VisionRange + ThisUnit.ThisMovement.CurrentRadius;
		int INearbySearcher.FactionID => ThisUnit.FactionID;
	}
	public class UnitActionRangeSearcher : UnitNearbySearcher<ActionRangeSearching>, IActionRangeSearcher
	{
		public UnitActionRangeSearcher(UnitObject unitObject) : base(unitObject)
		{
			searcherAPI.Init(this);
		}
		public INearbySearcher ThisSearcher => this;
		public INearbySearcherAPI SearcherAPI => searcherAPI;
		Vector3 INearbySearcher.SearchCenter => ThisUnit.ThisCombatHandler.Position;
		float INearbySearcher.SearchRange => ThisUnit.ThisCombatHandler.ActionRange + ThisUnit.ThisMovement.CurrentRadius;
		int INearbySearcher.FactionID => ThisUnit.FactionID;
	}
	public class UnitAttackStartRangeSearcher : UnitNearbySearcher<ActionRangeSearching>, IActionRangeSearcher
	{
		public UnitAttackStartRangeSearcher(UnitObject unitObject) : base(unitObject)
		{
			searcherAPI.Init(this);
		}
		public INearbySearcher ThisSearcher => this;
		public INearbySearcherAPI SearcherAPI => searcherAPI;
		Vector3 INearbySearcher.SearchCenter => ThisUnit.ThisCombatHandler.Position;
		float INearbySearcher.SearchRange => ThisUnit.ThisCombatHandler.AttackStartRange.y + ThisUnit.ThisMovement.CurrentRadius;
		float INearbySearcher.SearchMinRange => ThisUnit.ThisCombatHandler.AttackStartRange.x + ThisUnit.ThisMovement.CurrentRadius;
		int INearbySearcher.FactionID => ThisUnit.FactionID;
	}
	public class UnitAttackLimitRangeSearcher : UnitNearbySearcher<ActionRangeSearching>, IActionRangeSearcher
	{
		public UnitAttackLimitRangeSearcher(UnitObject unitObject) : base(unitObject)
		{
			searcherAPI.Init(this);
		}
		public INearbySearcher ThisSearcher => this;
		public INearbySearcherAPI SearcherAPI => searcherAPI;
		Vector3 INearbySearcher.SearchCenter => ThisUnit.ThisCombatHandler.Position;
		float INearbySearcher.SearchRange => ThisUnit.ThisCombatHandler.AttackLimitRange.y + ThisUnit.ThisMovement.CurrentRadius;
		float INearbySearcher.SearchMinRange => ThisUnit.ThisCombatHandler.AttackLimitRange.x + ThisUnit.ThisMovement.CurrentRadius;
		int INearbySearcher.FactionID => ThisUnit.FactionID;
	}
}