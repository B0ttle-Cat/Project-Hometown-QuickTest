using System;

using UnityEngine;

[CreateAssetMenu(fileName = "StrategyStartSetterData", menuName = "Scriptable Objects/StrategyGame/StrategyStartSetterData")]
public class StrategyStartSetterData : DataGetterSetter<StrategyStartSetterData.Data>
{
	[Serializable]
	public struct Data
	{
		public string playerFactionName;
		[Space]
		public FactionData[] factionDatas;
		public ControlBaseData[] controlBaseDatas;
		public UnitData[] unitDatas;
		[Space]
		public CaptureData[] captureDatas;
	}
	[Serializable]
	public struct FactionData
	{
		public string factionName;
		[NonSerialized]
		public int factionID;
		public Color factionColor;
		public Sprite factionIcon;

		public int maxManpower;
		public int maxSupplyPoint;
		public int maxElectricPoint;

		public GameObject defaultUnitPrefab;
	}
	[Serializable]
	public struct ControlBaseData
	{
		public string controlBaseName;
		[Header("CB Status")]
		public int maxManpower;
		public int maxSupplyPoint;
		public int maxElectricPoint;
		[Space]
		public float defenseAddition;
		public float defenseMultiplication;
		[Space]
		public float attackAddition;
		public float attackMultiplication;
		[Space]
		public float hpRecoveryAddition;
		public float hpRecoveryMultiplication;
		[Space]
		public float moraleRecoveryAddition;
		public float moraleRecoveryMultiplication;
	}
	[Serializable]
	public struct UnitData
	{
		public string unitName;
		public int unitID;
		public string factionName;
		public UnitProfile unitProfile;

		public int weaponType;
		public int protectType;

		public Vector3 position;
		public Vector3 rotation;
	}
	[Serializable]
	public struct CaptureData
	{
		public string captureControlBase;
		public string captureFaction;
		public float captureProgress;

		public int supplysQuantity;
	}

	[SerializeField]
	private Data data;
    protected override Data _data { get => data; set => data = value; }
}
