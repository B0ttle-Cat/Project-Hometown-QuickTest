using System.Collections.Generic;
using System.Linq;

using Sirenix.OdinInspector;

using UnityEngine;

using static StrategyStartSetterData;

public partial class StrategyStartSetter : MonoBehaviour
{
	private StrategyManager thisManager;
	private StrategyElementCollector collector;

	[SerializeField, InlineEditor, HideLabel, Title("Start Map Info")]
	private StrategyStartSetterData strategyStartSetterData;

	internal bool StartSetterIsValid()
	{
		if (strategyStartSetterData == null)
		{
			Debug.LogError("StrategyStartSetterData Is Null.");
			return false;
		}
		thisManager = StrategyManager.Manager;
		if (thisManager == null)
		{
			Debug.LogError("ThisManager Is Null.");
			return false;
		}
		collector = StrategyManager.Collector;
		if (collector == null)
		{
			Debug.LogError("No StrategyElementCollector ThisComponent found in children of GameManager.");
			return false;
		}
		return true;
	}
	internal void OnSetPreparedData()
	{
		if (StrategyManager.PreparedData == null)
		{
			ref readonly var data = ref strategyStartSetterData.ReadonlyData();
			StrategyManager.PreparedData = new StrategyGamePlayData.GameStartingData(new()
			{
				LanguageType = Language.Type.Korean,
				unscaleGamePlayTime = data.unscaleGamePlayTime,
				gamePlayTime = data.gamePlayTime,
				overview = data.overview,
				mission = data.mission,
			});
		}
	}
	internal void OnStartSetter_Faction()
	{
		ref readonly var data = ref strategyStartSetterData.ReadonlyData();
		var factions = data.factionDatas;
		int length = factions.Length;
		var list = collector.GetList<Faction>();
		for (int i = 0 ; i < length ; i++)
		{
			var factionData = factions[i];
			if (factionData.factionName == data.playerFactionName)
			{
				StrategyManager.PlayerFactionID = i;
			}
			Faction faction = new Faction(factionData);
			list.Add(faction);
		}
	}
	internal void OnStartSetter_FactionRelation(StrategyFactionRelation factionRelation)
	{
		ref readonly var data = ref  strategyStartSetterData.ReadonlyData();
		factionRelation.Init(collector, data.factionRelations);
	}
	internal async Awaitable OnStartSetter_Sector()
	{
		// 일단 씬에 있는 모든 SectorData 컴퍼넌트를 수집
		var allSector = GameObject.FindObjectsByType<SectorObject>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);

		var data = strategyStartSetterData.ReadonlyData();
		var sectors = data.sectorDatas;

		int cbLength = allSector.Length;
		int dataLength = sectors.Length;

		var list = collector.GetList<SectorObject>();
		for (int i = 0 ; i < cbLength ; i++)
		{
			SectorObject sector = allSector[i];
			
			string cbName =  sector.gameObject.name;
			for (int j = 0 ; j < dataLength ; j++)
			{
				var cbData = sectors[j];
				if (cbName == cbData.profileData.sectorName)
				{
					sector.Init(cbData);
					break;
				}
			}
			list.Add(sector);
		}
	}
	internal async Awaitable OnStartSetter_Unit()
	{
		List<UnitObject> includeSceneUnits = new ();
		includeSceneUnits.AddRange(GameObject.FindObjectsByType<UnitObject>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID));

		var data = strategyStartSetterData.ReadonlyData();
		var unitDatas = data.unitDatas;

		int dataLength = unitDatas.Length;
		for (int i = 0 ; i < dataLength ; i++)
		{
			var unitData = unitDatas[i];
			string unitName = unitData.DisplayName();
			UnitObject unitObject = StrategyElementFactory.Instantiate(in unitData);
			if (unitObject == null) continue;
			collector.Add(unitObject);
			SetOperationBelong(unitObject, in unitData);
		}

		void SetOperationBelong(UnitObject unitObject, in UnitData unitData)
		{
			var operation = collector.Find<OperationObject>(unitData.belongedOperation);
			if (operation == null) return;

			operation.AddUnitObject(unitObject);
		}
	}
	internal void OnStartSetter_Capture()
	{
		ref readonly var data = ref strategyStartSetterData.ReadonlyData();
		var occData = data.captureDatas;

		collector.GetList<SectorObject>().ForEach(SetCapture);

		void SetCapture(SectorObject sector)
		{
			int length = occData.Length;
			for (int i = 0 ; i < length ; i++)
			{
				var _data = occData[i];
				if (_data.captureSectorID == sector.SectorID)
				{
					sector.Init(_data);
					return;
				}
			}
			sector.Init(new StrategyStartSetterData.CaptureData()
			{
				 captureSectorID = -1,
				 captureFactionID = -1,
				 captureProgress = 0
			});
		}
	}
    internal async Awaitable OnStartSetter_SectorNetwork(StrategyPathfinding network)
    {
		var data = strategyStartSetterData.ReadonlyData();
		var networkDatas = data.sectorLinkDatas;
		var sectors = collector.GetList<SectorObject>().ToList();
        await network.Init(sectors,networkDatas);
	}
    internal void OnStartSetter_Mission(StrategyMissionTree mission)
    {
		// 메인 미션 세팅
		mission.InitMainMission();

		// 서브 미션 세팅
		mission.InitSubMission();
	}
    internal async Awaitable OnStartSetter_Operation()
	{
		List<OperationObject> includeSceneOperations = new ();
		includeSceneOperations.AddRange(GameObject.FindObjectsByType<OperationObject>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID));

		var data = strategyStartSetterData.ReadonlyData();
		var opDatas = data.operationDatas;
		int length = opDatas.Length;

		var opList = collector.GetList<OperationObject>();
		for (int i = 0 ; i < length ; i++)
        {
			var opData = opDatas[i];
			OperationObject newOp = StrategyElementFactory.Instantiate(opData);
			opList.Add(newOp);
		}
	}
}