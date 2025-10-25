using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;

public class StrategyStartSetter : MonoBehaviour
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
		if (StrategyGamePlayData.PreparedData == null)
		{
			var data = strategyStartSetterData.GetData();
			StrategyGamePlayData.PreparedData = new StrategyGamePlayData.GameStartingData(new()
			{
				LanguageType = Language.Type.Korean,
				overview = data.overview,
				mission = data.mission,
			});
		}
	}

	internal void OnStartSetter_Faction()
	{
		var data = strategyStartSetterData.GetData();
		var factions = strategyStartSetterData.GetData().factionDatas;
		int length = factions.Length;
		for (int i = 0 ; i < length ; i++)
		{
			var factionData = factions[i];
			factionData.factionID = i;
			if (factionData.factionName == data.playerFactionName)
			{
				StrategyGamePlayData.PlayerFactionID = i;
			}
			Faction faction = new Faction(factionData);
			collector.AddElement<Faction>(faction);
		}
	}
	internal void OnStartSetter_ControlBase()
	{
		// 일단 씬에 있는 모든 ControlBaseData 컴퍼넌트를 수집
		var allBase = GameObject.FindObjectsByType<ControlBase>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);

		var data = strategyStartSetterData.GetData();
		var controlBases = data.controlBaseDatas;

		int cbLength = allBase.Length;
		int dataLength = controlBases.Length;
		for (int i = 0 ; i < cbLength ; i++)
		{
			ControlBase cb = allBase[i];
			cb.Init();

			string cbName =  cb.gameObject.name;
			for (int j = 0 ; j < dataLength ; j++)
			{
				var cbData = controlBases[j];
				if (cbName == cbData.profileData.controlBaseName)
				{
					cb.Init(cbData);
					break;
				}
			}
			collector.AddElement(cb);
		}
	}
	internal void OnStartSetter_Unit()
	{
		List<UnitObject> includeSceneUnits = new ();
		includeSceneUnits.AddRange(GameObject.FindObjectsByType<UnitObject>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID));

		var data = strategyStartSetterData.GetData();
		var unitDatas = data.unitDatas;

		int dataLength = unitDatas.Length;
		for (int i = 0 ; i < dataLength ; i++)
		{
			var unitData = unitDatas[i];
			unitData.unitID = i; ;
			string unitName = unitData.unitName;
			if (TryFindUnitAlready(unitName, out UnitObject unitObject))
			{
				UnitInstantiater.ResetWithData(unitObject, in unitData);
			}
			else
			{
				unitObject = UnitInstantiater.Instantiate(in unitData);
			}
			collector.AddElement(unitObject);
		}

		bool TryFindUnitAlready(string unitName, out UnitObject unitObject)
		{
			unitObject = null;
			int length = includeSceneUnits.Count;
			for (int i = 0 ; i < length ; i++)
			{
				if (includeSceneUnits[i].ProfileData.unitName == unitName)
				{
					unitObject = includeSceneUnits[i];
					includeSceneUnits.RemoveAt(i);
					break;
				}
			}
			return unitObject != null;
		}

		int includeLength = includeSceneUnits.Count;
		for (int i = 0 ; i < includeLength ; i++)
		{
			var unit = includeSceneUnits[i];
			if (collector.FindUnit(unit.UnitID) == null)
			{
				string name = $"Unit_{dataLength + i:00}";
				unit.gameObject.name = name;
				unit.Init(name, dataLength + i);
				collector.AddElement(unit);
			}
		}
	}
	internal void OnStartSetter_Capture()
	{
		var data = strategyStartSetterData.GetData();
		var occData = data.captureDatas;

		collector.ForEachControlBase(SetCapture);

		void SetCapture(ControlBase cb)
		{
			int length = occData.Length;
			for (int i = 0 ; i < length ; i++)
			{
				var _data = occData[i];
				if (_data.captureControlBase == cb.ControlBaseName)
				{
					cb.Init(_data);
					return;
				}
			}
			cb.Init(new StrategyStartSetterData.CaptureData()
			{
				 captureControlBase = "",
				 captureFaction = "",
				 captureProgress = 0
			});
		}
	}
}