using System;

using Sirenix.OdinInspector;

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

		public int maxPersonnel;
		public int maxMaterialPoint;
		public int maxElectricPoint;

		public GameObject defaultUnitPrefab;
	}
	[Serializable]
	public struct ControlBaseData
	{
		[InlineProperty, HideLabel, Title("Profile")]
		public StrategyGamePlayData.ControlBaseData.Profile.Data profileData;
		[InlineProperty, HideLabel, FoldoutGroup("MainStats")]
		public StrategyGamePlayData.ControlBaseData.MainStats.Data mainStatsData;
		[FoldoutGroup("MainStats")]
		public float captureTime;
		[InlineProperty, HideLabel, FoldoutGroup("Facilities")]
		public StrategyGamePlayData.ControlBaseData.Facilities.Data facilitiesStatsData;
		[InlineProperty, HideLabel, FoldoutGroup("Support")]
		public StrategyGamePlayData.ControlBaseData.Support.Data supportStatsData;
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
	}

	[SerializeField]
	private Data data;
    protected override Data _data { get => data; set => data = value; }
}
