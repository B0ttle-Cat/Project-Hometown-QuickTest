using UnityEngine;

using static StrategyStartSetterData;
public static class UnitInstantiater
{
	public static UnitObject Instantiate(in UnitData unitData)
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

		return unit;
	}
	public static void ResetWithData(UnitObject unit, in UnitData unitData)
	{
		if(unit == null) return;

		GameObject unitObject = unit.gameObject;

		var position = unitData.position;
		var rotation = Quaternion.Euler(unitData.rotation);
		unitObject.transform.SetPositionAndRotation(position, rotation);

		string name = $"{unitData.unitName}_{unitData.unitID:00}";
		unitObject.gameObject.name = name;

		unit.Init(unitData);
	}
}
