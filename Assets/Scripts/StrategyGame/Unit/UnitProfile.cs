using UnityEngine;

[CreateAssetMenu(fileName = "UnitProfile", menuName = "Scriptable Objects/StrategyGame/UnitProfile")]
public class UnitProfile : ScriptableObject
{
	public GameObject unitPrefab;

	public int manpower;            // 소비 인력
	public int durability;          // 내구도 (보유한 채력량)

	public int attack;              // 공격력		// 기본 데미지 = attack - defense
	public int defense;             // 방어력
	public int heal;                // 치유력

	public int piercingLevel;       // 관통 레벨		// 데미지 계산 = 기본 데미지 * ((piercingLevel/protectingLevel)^2 : 0.1 ~ 1) * 상성 보정
	public int protectingLevel;     // 방호 레벨

	public int EMPProtectionLevel;  // EMP 공격 방호 레벨

	public int attackHitPoint;      // 공격 기회 점수	 // 명중 확률 = 기회/기회+회피
	public int attackMissPoint;     // 공격 회피 점수

	public int criticalHitPoint;    // 치명타 기회 점수 // 치명 확률 = 기회/기회+회피
	public int criticalMissPoint;   // 치명타 회피 점수

	public float attackDelay;       // 공격 딜레이
	public int firingCount;         // 연속 공격 수
	public float firingDelay;       // 연속 공격 딜레이

	public int electricPerAttack;   // 공격시 전력 소모량
	public int supplyPerAttack;    // 공격시 물자 소모량

	public float attackange;        // 공격 범위
	public float actionRange;       // 반응 범위
	public float viewRange;         // 시야 범위

	public float moveSpeed;         // 이동 속도

	[Space]
	public int capturePoint;

	[Space]
	public SkillProfile[] connectSkill;
}
