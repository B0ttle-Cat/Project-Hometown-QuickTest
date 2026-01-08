using System;
using System.Collections.Generic;

using UnityEngine;

using static StrategyGamePlayData;

public partial class FacilityObject : MonoBehaviour
{
    [Serializable]
    public class FacilityStaticData
    {
		public List<UnitKey> AvailableUnitKeyList;
	}
	[Serializable]
	public class FacilityRuntimeData
	{

	}


    private FacilityStaticData staticData;
    private FacilityRuntimeData runtimeData;

    public FacilityStaticData StaticData => staticData;
    public FacilityRuntimeData RuntimeData => runtimeData;
}



public partial class FacilityObject : IStrategyElement
{
    IStrategyElement IStrategyElement.ThisElement { get; }
    int IStrategyElement.ID { get; set; }

    void IStrategyElement.InStrategyCollector()
    {
    }

    void IStrategyStartGame.OnStartGame()
    {
    }

    void IStrategyStartGame.OnStopGame()
    {
    }

    void IStrategyElement.OutStrategyCollector()
    {
    }
}
