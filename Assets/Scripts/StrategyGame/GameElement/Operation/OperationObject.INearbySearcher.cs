using System;

using UnityEngine;




[RequireComponent(typeof(NearbySearching))]
public partial class OperationObject // INearbySearcher
{
	private OperationVisionRangeSearcher visionRangeSearcher;
	private OperationActionRangeSearcher actionRangeSearcher;
	public INearbySearcher VisionSearcher => visionRangeSearcher;
	public INearbySearcher ActionSearcher => actionRangeSearcher;
	public INearbySearcherAPI ViewSearcherAPI => visionRangeSearcher.SearcherAPI;
	public INearbySearcherAPI ActionSearcherAPI => actionRangeSearcher.SearcherAPI;

	private Vector3 searchGroupCenter;
	private float searchGroupRadius;
	private float searchVisionRange;
	private float searchActionRange;


	partial void InitNearby()
    {
		visionRangeSearcher = new OperationVisionRangeSearcher(this);
		actionRangeSearcher = new OperationActionRangeSearcher(this);
	}

    partial void DeinitNearby()
    {
		visionRangeSearcher?.Dispose();
		actionRangeSearcher?.Dispose();
	}

	public class OperationNearbySearcher<T> : INearbySearcher, IDisposable where T : NearbySearching
	{
		protected readonly OperationObject thisOperation;
		protected readonly T nearbySearching;
		public INearbySearcher ThisSearcher => this;
		public INearbySearcherAPI SearcherAPI => nearbySearching;
		int INearbySearcher.FactionID => thisOperation.factionID;
		bool INearbySearcher.IsEnable => thisOperation.enabled;

		public OperationNearbySearcher(OperationObject thisOperation)
		{
			this.thisOperation = thisOperation;

			if (!thisOperation.TryGetComponent<T>(out nearbySearching))
			{
				nearbySearching = thisOperation.gameObject.AddComponent<T>();
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
	public class OperationVisionRangeSearcher : OperationNearbySearcher<VisionRangeSearching>, IVisionRangeSearcher
    {
        public OperationVisionRangeSearcher(OperationObject thisOperation) : base(thisOperation)
        {
#if UNITY_EDITOR
			(SearcherAPI as NearbySearching).debugThickness = 5;
#endif
		}
		Vector3 INearbySearcher.SearchCenter => thisOperation.searchGroupCenter;
		float INearbySearcher.SearchRange => thisOperation.searchVisionRange + thisOperation.searchGroupRadius;
	}
	public class OperationActionRangeSearcher : OperationNearbySearcher<ActionRangeSearching>, IActionRangeSearcher
	{
		public OperationActionRangeSearcher(OperationObject thisOperation) : base(thisOperation)
		{
#if UNITY_EDITOR
			(SearcherAPI as NearbySearching).debugThickness = 5;
#endif
		}
		Vector3 INearbySearcher.SearchCenter => thisOperation.searchGroupCenter;
		float INearbySearcher.SearchRange => thisOperation.searchActionRange + thisOperation.searchGroupRadius;
	}
}
