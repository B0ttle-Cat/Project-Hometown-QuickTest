using System.Collections.Generic;

public class Squad
{
	public int squadID;
	public string squadName;
	public List<UnitObject> units;
	public BattleTendency battleTendency;
	public TacticsState tacticsState;

	public List<Command> commands;

	public enum BattleTendency
	{
		Balanced = 0,		// 균형을 중시	| 공 50 : 수 50
		Offensive = 1,		// 공격을 우선시	| 공 75 : 수 25
		Defensive = 2,		// 방어를 우선시	| 공 25 : 수 75
		VeryOffensive = 3,	// 매우 공격적	| 공 100 : 수 0
		VertDefensive = 4	// 매우 방어적	| 공 0 : 수 100
	}

	public enum TacticsState
	{
		Standby = 0,		// 분대 배치 대기 중
		None = 1,			// 전술 없음 / 기본 상태
		Engage = 2,			// 교전 유지 / 표준 전투 모드
		Patrol = 3,			// 순찰, 경계 유지
		Ambush = 4,			// 매복 기습 / 공격 중심
		Infiltration = 5,	// 침투, 적진 잠입
		Charge = 6,			// 강제 돌파 / 공격 
		Hold = 7,			// 위치 고수, 방어 태세
		Harassment = 8,		// 견제, 적과의 거리 유지
		Retreat = 9,		// 전력 보존, 후퇴
		Regroup = 10,		// 분대 재정비, 집결
		Redeployment = 11	// 분대 재배치 중	(집결지 이동)
	}
}
