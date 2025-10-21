using UnityEngine;

[CreateAssetMenu(fileName = "UnitProfile", menuName = "Scriptable Objects/StrategyGame/UnitProfile")]
public class UnitProfile : ScriptableObject
{
	public GameObject unitPrefab;

	[Header("Stats")]
	public int 유닛_인력;
	public int 유닛_물자;
	public int 유닛_전력;
	[Space]
	public int 유닛_최대내구도;
	public int 유닛_현재내구도;

	public int 유닛_공격력;
	public int 유닛_방어력;
	public int 유닛_치유력;
	public int 유닛_회복력;
	public int 유닛_이동속도;
	public int 유닛_점령점수;

	public int 유닛_치명공격력;
	public int 유닛_치명공격배율;
	public int 유닛_치명방어력;

	[Space]
	public int 유닛_관통레벨;
	public int 유닛_장갑레벨;
	public int 유닛_EMP저항레벨;
	[Space]
	public int 유닛_공격명중기회;
	public int 유닛_공격회피기회;
	public int 유닛_치명명중기회;
	public int 유닛_치명회피기회;
	[Space]
	public int 유닛_명중피격수;
	public int 유닛_연속공격횟수;
	public int 유닛_조준지연시간;
	public int 유닛_연속공격지연시간;
	public int 유닛_재공격지연시간;
	[Space]
	public int 유닛_공격소모_전력;
	public int 유닛_공격소모_물자;
	[Space]
	public int 유닛_공격범위;
	public int 유닛_행동범위;
	public int 유닛_시야범위;


	[Space]
	public int capturePoint;

	[Space]
	public SkillProfile[] connectSkill;
}
