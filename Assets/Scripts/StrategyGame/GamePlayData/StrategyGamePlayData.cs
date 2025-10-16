using System;
using System.Collections.Generic;

using UnityEngine;

public partial class StrategyGamePlayData
{
	public abstract class GamePlayData<T>
	{
		public GamePlayData(T data)
		{
			_data = data;
		}
		protected T _data;

		private Action<T> onChangeValue;
		public T GetData() => _GetData();
		public void SetData(T data, bool ignoreChangeEvent = false)
		{
			_SetData(data);
			if (ignoreChangeEvent) return;
			Invoke();
		}
		protected virtual T _GetData() => _data;
		protected virtual void _SetData(T data) => _data = data;
		public virtual void ClearData(bool ignoreChangeEvent = false)
		{
			SetData(default, ignoreChangeEvent);
		}

		public void Invoke()
		{
			if (onChangeValue == null)
				return;

			T data = _GetData();
			foreach (var handler in onChangeValue.GetInvocationList())
			{
				try
				{
					((Action<T>)handler).Invoke(data);
				}
				catch (Exception ex)
				{
					UnityEngine.Debug.LogError($"GamePlayData<{typeof(T).Name}> Listener error: {ex.Message}");
				}
			}

		}
	}

	public class KeyValueData : GamePlayData<KeyValueData.Data>
	{
		public KeyValueData(Data data) : base(data) { }
		public struct Data
		{
			public List<KeyValue> KeyValueList
			{
				get
				{
					if (keyValueList == null)
						keyValueList = new List<KeyValue>();
					return keyValueList;
				}
			}
			public List<KeyValue> keyValueList;
		}
		public struct KeyValue
		{
			public string Key;
			public object Value;
		}
		private Data data;
		public override void ClearData(bool ignoreChangeEvent = false)
		{
			SetData(default, ignoreChangeEvent);
		}
		public void SetKeyValue(string key, object value, bool ignoreChangeEvent = false)
		{
			var list = data.KeyValueList;
			int listCount = list == null ? 0 :list.Count;
			bool isNew = true;
			for (int i = 0 ; i < listCount ; i++)
			{
				var keyValue = list[i];
				if (keyValue.Key.Equals(key))
				{
					keyValue.Value = value;
					list[i] = keyValue;
					isNew = false;
					break;
				}
			}
			if (isNew)
			{
				list.Add(new KeyValue { Key = key, Value = value });
			}

			if (ignoreChangeEvent)
			{
				Invoke();
			}
		}
		public bool TryGetValue<T>(string key, out T t)
		{
			t = default;
			var list = data.KeyValueList;

			if (list == null) return false;
			int listCount = list.Count;
			if (listCount == 0) return false;

			for (int i = 0 ; i < listCount ; i++)
			{
				var keyValue = list[i];
				if (keyValue.Key.Equals(key))
				{
					if (Convert(keyValue.Value, out T tValue))
					{
						return true;
					}
				}
			}
			return false;

			bool Convert(object value, out T tValue)
			{
				try
				{
					tValue = (T)value;
					return true;
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
					tValue = default;
					return false;
				}
			}
		}
	}

	public static KeyValueData KeyValue;
}
public partial class StrategyGamePlayData // Prepared Data (준비된 데이터)
{
	public static int PlayerFactionID;
	public static GameStartingData PreparedData;
	[Serializable]
	public class GameStartingData : GamePlayData<GameStartingData.Data>
	{
		public GameStartingData(Data data) : base(data) { }

		[Serializable]
		public struct Data
		{
			public Language.Type LanguageType;
			public Overview overview;
			public Mission mission;
		}
		[Serializable]
		public struct Overview
		{
			public string title;
			public string description;
		}
		[Serializable]
		public struct Mission
		{
			public string id;
			public string title;
			public string description;

			public string victoryScript;
			public string defeatScript;

			public SubMission[] enableSubMissions;
		}
		[Serializable]
		public struct SubMission
		{
			public string id;
			public string missionScript;
		}
	}
}
public partial class StrategyGamePlayData // Mission Data
{
	[Serializable]
	public class MissionTreeData
	{
		[Serializable]
		public struct ItemStruct :IDisposable
		{
			public string[] targets;
			public MissionType missionType;

			public ComparisonType comparisonType;
			public int count;
  
            public void Dispose()
            {
				targets = null;
			}
        }
		[Serializable]
		public struct GroupStruct : IDisposable
		{
			public LogicType logicType;
			public ComparisonType anyComparisonType;
			public int anyCount;

			public void Dispose()
			{
			}
		}
		
		public enum MissionType
		{
			Kill,
			Protect,
			ControlBase_Count,
			CaptureAndSecureBase,
		}
		public enum ResultTyoe
		{
			Wait = 0,
			Succeed,
			Failed,
		}
		public enum ComparisonType
		{
			동등, 이하, 이상,
		}
		public enum LogicType
		{
			All,
			Any,
		}
	}
}
public partial class StrategyGamePlayData // Play Content Data
{
	[Serializable]
	public class ControlBaseData : GamePlayData<ControlBaseData.Data>
	{
		public ControlBaseData(Data data) : base(data) { }
		public struct Data
		{
			public string controlBaseName;
			public Sprite iconImage;

			public string captureFaction;

			// 최대 보유량을 해당 수치만큼 늘린다.
			public int maxManpower;
			public int maxSupplyPoint;
			public int maxElectricPoint;

			// 분당 충전량을 해당 수치만큼 늘린다.
			public int manpowerPerMinute;
			public int supplyPerMinute;
			public int electricPerMinute;

			// 현재 비축량
			public int reservesManpower;
			public int reservesSupply;
			public int reservesElectric;

			public int currentSupplyPoint;
		}
	}
	[Serializable]
	public class ControlBaseBattleVariable : GamePlayData<ControlBaseBattleVariable.Data>
	{
		public ControlBaseBattleVariable(Data data) : base(data) { }
		public struct Data
		{
			public float defenseAddition;
			public float defenseMultiplication;

			public float attackAddition;
			public float attackMultiplication;

			public float hpRecoveryAddition;
			public float hpRecoveryMultiplication;

			public float moraleRecoveryAddition;
			public float moraleRecoveryMultiplication;
		}
	}
	[Serializable]
	public class ControlBaseBuildingData : GamePlayData<ControlBaseBuildingData.Data>
	{
		public ControlBaseBuildingData(Data data) : base(data) { }
		public struct Data
		{
			public int buildingID;
			public int buildingLevel;

			internal Sprite buildingImage;

			// 최대 보유량을 해당 수치만큼 늘린다.
			public int maxManpower;
			public int maxSupplyPoint;
			public int maxElectricPoint;

			// 분당 충전량을 해당 수치만큼 늘린다.
			public int manpowerPerMinute;
			public int supplyPerMinute;
			public int electricPerMinute;
		}
		private Data data;
	}

	[Serializable]
	public class UnitBaseData : GamePlayData<UnitBaseData.Data>
	{
		public UnitBaseData(Data data) : base(data) { }
		public struct Data
		{
			public string unitKey;
			public string unitName;
			public int unitID;
			public int factionID;

			public WeaponType weaponType;
			public ProtectionType protectType;
		}
		public enum WeaponType
		{
			None = 0, //	상성표: 무효 |<< (--) (- ) (  ) (+ ) (++) >>|유효
					  //	대미지%	  |<< (20) (50) (100) (200) (300) >>|
			관통, //			경장갑(  ) | 중장갑(+ ) | 역장(- ) | 건물(  )
			폭발, //			경장갑(+ ) | 중장갑(  ) | 역장(- ) | 건물(  )

			관통특화, //		경장갑(- ) | 중장갑(++) | 역장(- ) | 건물(+ )
			폭발특화, //		경장갑(++) | 중장갑(--) | 역장(  ) | 건물(+ )

			에너지, //		경장갑(- ) | 중장갑(--) | 역장(++) | 건물(--)
		}
		public enum ProtectionType
		{
			None = 0,
			경장갑,
			중장갑,
			역장,
			건물,
		}
	}
	[Serializable]
	public class UnitProfileData : GamePlayData<UnitProfileData.Data>
	{
		public UnitProfileData(Data data) : base(data) { }
		public struct Data
		{
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

			public int capturePoint;
		}
	}
	[Serializable]
	public class UnitSkillData : GamePlayData<UnitSkillData.Data>
	{
		public UnitSkillData(Data data) : base(data) { }
		public struct Data
		{
			public SkillData[] skillDatas;
		}
		public struct SkillData
		{
			public int skillID;
			public int skillLevel;
		}
	}
}
