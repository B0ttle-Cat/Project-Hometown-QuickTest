public partial class StrategyGamePlayData // WeaponType & ProtectionType
{
	public enum WeaponType : byte
	{
		None = 1,     // 무기 없음
		일반 = 0,     // 상성표: 무효 |<< (--) (- ) (  ) (+ ) (++) >>|유효
					  // 대미지%	  |<< (25) (50) (100) (200) (300) >>|
		관통,         // 경장갑(  ) | 중장갑(+ ) | 역장(- ) | 건물(  )
		폭발,         // 경장갑(+ ) | 중장갑(  ) | 역장(- ) | 건물(  )

		관통특화,     // 경장갑(- ) | 중장갑(++) | 역장(- ) | 건물(+ )
		폭발특화,     // 경장갑(++) | 중장갑(--) | 역장(  ) | 건물(+ )

		에너지,       // 경장갑(- ) | 중장갑(--) | 역장(++) | 건물(--)
	}
	public enum ProtectionType : byte
	{
		일반 = 0,

		경장갑,
		중장갑,
		강화장갑,

		역장,
		건물,
	}
}
