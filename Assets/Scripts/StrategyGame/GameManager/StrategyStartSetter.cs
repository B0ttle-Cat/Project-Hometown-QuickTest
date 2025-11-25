using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;

using static StrategyStartSetterData;

public partial class StrategyStartSetter : MonoBehaviour
{
	private StrategyManager thisManager;
	private StrategyElementCollector collector;

	[SerializeField, InlineEditor, HideLabel, Title("Start Map Data")]
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
		for (int i = 0 ; i < length ; i++)
		{
			var factionData = factions[i];
			if (factionData.factionName == data.playerFactionName)
			{
				StrategyManager.PlayerFactionID = i;
			}
			Faction faction = new Faction(factionData);
			collector.AddElement<Faction>(faction);
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
			collector.AddElement(sector);
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
			UnitObject unitObject = StrategyElementUtility.Instantiate(in unitData);
			if (unitObject == null) continue;
			collector.AddElement(unitObject);
			SetOperationBelong(unitObject, in unitData);
		}

		int includeLength = includeSceneUnits.Count;
		for (int i = 0 ; i < includeLength ; i++)
		{
			var unit = includeSceneUnits[i];
			if (collector.FindUnit(unit.UnitID) == null)
			{
				collector.AddElement(unit);
				unit.Init(name);
			}
		}
		void SetOperationBelong(UnitObject unitObject, in UnitData unitData)
		{
			string operationName = unitData.belongedOperation;
			if (string.IsNullOrWhiteSpace(operationName)) return;
			int factionID = collector.FactionNameToID(unitData.factionName);
			if (factionID < 0) return;
			var operation = collector.FindOperation(factionID, operationName);
			if (operation == null) return;

			operation.AddUnitObject(unitObject);
		}
	}
	internal void OnStartSetter_Capture()
	{
		ref readonly var data = ref strategyStartSetterData.ReadonlyData();
		var occData = data.captureDatas;

		collector.ForEachSector(SetCapture);

		void SetCapture(SectorObject sector)
		{
			int length = occData.Length;
			for (int i = 0 ; i < length ; i++)
			{
				var _data = occData[i];
				if (_data.captureSector == sector.SectorName)
				{
					sector.Init(_data);
					return;
				}
			}
			sector.Init(new StrategyStartSetterData.CaptureData()
			{
				 captureSector = "",
				 captureFaction = "",
				 captureProgress = 0
			});
		}
	}
    internal async Awaitable OnStartSetter_SectorNetwork(StrategyNodeNetwork network)
    {
		var data = strategyStartSetterData.ReadonlyData();
		var networkDatas = data.sectorLinkDatas;
		var sectors = collector.SectorList;
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
        for (int i = 0 ; i < length ; i++)
        {
			var opData = opDatas[i];
			OperationObject newOp = StrategyElementUtility.Instantiate(opData);
			collector.AddElement(newOp);
		}

		int includeLength = includeSceneOperations.Count;
		for (int i = 0 ; i < includeLength ; i++)
		{
			var op = includeSceneOperations[i];
			if (collector.FindUnit(op.OperationID) == null)
			{
				collector.AddElement(op);
			}
		}
	}
}
public partial class StrategyStartSetter // Instantiate
{
	private void ResetWithData(UnitObject unit, in UnitData unitData)
	{
		if (unit == null) return;

		GameObject unitObject = unit.gameObject;

		var position = unitData.position;
		var rotation = Quaternion.Euler(unitData.rotation);
		unitObject.transform.SetPositionAndRotation(position, rotation);
		unitObject.gameObject.name = name;
		unit.Init(unitData);
	}
}