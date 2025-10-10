using UnityEngine;

[CreateAssetMenu(fileName = "UnitProfile", menuName = "Scriptable Objects/StrategyGame/UnitProfile")]
public class UnitProfile : ScriptableObject
{
	public GameObject unitPrefab;
	[Space]
	public int manpower;
	[Space]
	public int healthPoint;     // 보유한 보호막
	public int suppliePoint;   // 보유한 물자량
	public int electricPoint;   // 보유한 전력량
	[Space]
	public int attack;
	public int defense;
	public int speed;
	public int range;
	public int vision;
	[Space]
	public int occupationPoint;

	[Space]
	public SkillProfile[] connectSkill;
}
