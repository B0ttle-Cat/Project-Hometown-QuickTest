using UnityEngine;

using static StrategyStartSetterData;
public static class UnitInstantiater
{
	public static UnitObject Instantiate(UnitData unitData)
	{
		var original = unitData.unitProfile.unitPrefab;
		var position = unitData.position;
		var rotation = Quaternion.Euler(unitData.rotation);
		GameObject unitObject = GameObject.Instantiate( original, position, rotation);

		string name = $"{unitData.unitName}_{unitData.unitID:00}";
		unitObject.gameObject.name = name;

		UnitObject unit = unitObject.GetComponent<UnitObject>();
		if (unit == null) unit = unitObject.AddComponent<UnitObject>();

		unit.Init(unitData);
		unit.Init(unitData.unitProfile);

		return unit;
	}
}
