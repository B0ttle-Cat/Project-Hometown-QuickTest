using UnityEngine;

public class StrategyStartSetter : MonoBehaviour
{
	private StrategyManager thisManager;
	private StrategyElementCollector collector;
	[SerializeField]
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
			Debug.LogError("No StrategyElementCollector component found in children of GameManager.");
			return false;
		}
		return true;
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
			if(factionData.factionName == data.playerFactionName)
			{
				StrategyGamePlayData.PlayerFactionID = i;
			}
			Faction faction = new Faction(factionData);
			collector.AddElement<Faction>(faction);
		}
	}

	internal void OnStartSetter_ControlBase()
	{
		// 일단 씬에 있는 모든 ControlBase 컴퍼넌트를 수집
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
				if (cbName == cbData.controlBaseName)
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
		var includeSceneUnits = GameObject.FindObjectsByType<UnitObject>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);

		var data = strategyStartSetterData.GetData();
		var unitDatas = data.unitDatas;

		int dataLength = unitDatas.Length;
		for (int i = 0 ; i < dataLength ; i++)
		{
			var unitData = unitDatas[i];

			UnitObject unitObject = UnitInstantiater.Instantiate(unitData);
			collector.AddElement(unitObject);
		}

		int includeLength = includeSceneUnits.Length;
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
		int length = occData.Length;
		collector.ForControlBase(SetCapture);

		void SetCapture(ControlBase cb)
		{
			for (int i = 0 ; i < length ; i++)
			{
				var data = occData[i];
				if (data.captureControlBase == cb.ControlBaseName)
				{
					cb.Init(data);
				}
			}
		}
	}
}