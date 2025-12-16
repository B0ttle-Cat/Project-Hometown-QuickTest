using System;

using UnityEngine;




[RequireComponent(typeof(NearbySearching))]
public partial class OperationObject
{
	private OperationViewRangeSearcher viewRangeSearcher;
	private OperationActionRangeSearcher actionRangeSearcher;
	public INearbySearcher ViewSearcher => viewRangeSearcher;
	public INearbySearcher ActionSearcher => actionRangeSearcher;
	public INearbySearcherAPI ViewSearcherAPI => viewRangeSearcher.SearcherAPI;
	public INearbySearcherAPI ActionSearcherAPI => actionRangeSearcher.SearcherAPI;

	private Vector3 searchCenterPosition;
	private float searchViewRange;


	partial void InitNearby(in float baseRadius)
    {
		viewRangeSearcher = new OperationViewRangeSearcher(this);
	}

    partial void DeInitNearby()
    {
		viewRangeSearcher?.Dispose();
	}

    public class OperationViewRangeSearcher : IViewRangeSearcher , IDisposable
    {
        readonly OperationObject thisOperation;
		readonly INearbySearcherAPI searcherAPI;
		public OperationViewRangeSearcher(OperationObject thisOperation)
        {
            this.thisOperation = thisOperation;

			if (!thisOperation.TryGetComponent<ViewRangeSearching>(out var nearbySearching))
			{
				nearbySearching = thisOperation.gameObject.AddComponent<ViewRangeSearching>();
			}
			searcherAPI = nearbySearching;

			searcherAPI.Init(this);
		}
		public void Dispose()
		{
			if (searcherAPI.IsNotNullRef())
			{
				searcherAPI.Deinit();
			}
		}
		public INearbySearcher  ThisSearcher => this;
		public INearbySearcherAPI SearcherAPI => searcherAPI;
		Vector3 INearbySearcher.SearchCenter => thisOperation.searchCenterPosition;
		float INearbySearcher.SearchRange => thisOperation.searchViewRange + thisOperation.operationRange;
		int INearbySearcher.FactionID => thisOperation.factionID;
	}
	public class OperationActionRangeSearcher : IActionRangeSearcher, IDisposable
	{
		readonly OperationObject thisOperation;
		readonly INearbySearcherAPI searcherAPI;
		public OperationActionRangeSearcher(OperationObject thisOperation)
		{
			this.thisOperation = thisOperation;

			if (!thisOperation.TryGetComponent<ActionRangeSearching>(out var nearbySearching))
			{
				nearbySearching = thisOperation.gameObject.AddComponent<ActionRangeSearching>();
			}
			searcherAPI = nearbySearching;

			searcherAPI.Init(this);
		}
		public void Dispose()
		{
			if (searcherAPI.IsNotNullRef())
			{
				searcherAPI.Deinit();
			}
		}
		public INearbySearcher ThisSearcher => this;
		public INearbySearcherAPI SearcherAPI => searcherAPI;
		Vector3 INearbySearcher.SearchCenter => thisOperation.searchCenterPosition;
		float INearbySearcher.SearchRange => thisOperation.searchViewRange + thisOperation.operationRange;
		int INearbySearcher.FactionID => thisOperation.factionID;
	}
}
