using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Sirenix.OdinInspector;

using UnityEngine;

using Debug = UnityEngine.Debug;

namespace StrategyManagerModule
{

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
				if (factionData.FactionName == data.playerFactionName)
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
		[Conditional("UNITY_EDITOR")]
		internal void OnStartSetter_FactionViewer()
		{
			var list = collector.GetList<Faction>();
			int length = list.Count;
			for (int i = 0 ; i < length ; i++)
			{
				FactionViewer factionViewer =  StrategyManager.Manager.gameObject.AddComponent<FactionViewer>();
				factionViewer.faction = list[i];
			}
		}
		internal async Awaitable OnStartSetter_Sector()
		{
			// 일단 씬에 있는 모든 SectorData 컴퍼넌트를 수집
			var allSector = GameObject.FindObjectsByType<SectorObject>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);

			var data = strategyStartSetterData.ReadonlyData();
			var sectors = data.sectorDatas;

			int sectorLength = allSector.Length;
			int dataLength = sectors.Length;

			var list = collector.GetList<SectorObject>();
			for (int i = 0 ; i < dataLength ; i++)
			{
				var sectorData = sectors[i];
				SectorObject sector = null;
				for (int j = 0 ; j < sectorLength ; j++)
				{
					SectorObject tempSector = allSector[j];
					if (tempSector == null) continue;

					string sectorName =  tempSector.gameObject.name;
					if (sectorName == sectorData.SectorName)
					{
						allSector[j] = null;
						sector = tempSector;
						tempSector.Init(sectorData);
						break;
					}
				}
				if(sector == null)
				{
					Debug.LogError($"현재 씬에서 SectorData({i}: {sectorData.SectorName})와 동일한 이름을 가진 SectorObject 가 없습니다.");
					continue;
				}
				list.Add(sector);
			}


			for (int j = 0 ; j < sectorLength ; j++)
			{
				SectorObject tempSector = allSector[j];
				if (tempSector == null) continue;

				Debug.LogError($"SectorData 가 없는 SectorObject 가 있습니다: {tempSector.gameObject.name}");
			}

#if UNITY_EDITOR
			await Awaitable.NextFrameAsync();
#endif
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
				UnitObject unitObject = StrategyElementFactory.Instantiate(in unitData);
				if (unitObject == null) continue;
				collector.Add(unitObject);
			}

#if UNITY_EDITOR
			await Awaitable.NextFrameAsync();
#endif
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
			await network.Init(sectors, networkDatas);
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
}