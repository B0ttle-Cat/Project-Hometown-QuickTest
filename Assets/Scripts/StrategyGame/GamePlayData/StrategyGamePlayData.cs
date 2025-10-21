using System;
using System.Collections.Generic;
using System.Linq;

using Sirenix.OdinInspector;

using UnityEngine;

using static StrategyStartSetterData;
using static UnityEngine.Rendering.DebugUI;

public partial class StrategyGamePlayData
{
	[Serializable]
	public abstract class GamePlayData<T>
	{
		public GamePlayData(T data)
		{
			_data = data;
		}
		[SerializeField,InlineProperty,HideLabel]
		protected T _data;
		private Action<T> onChangeData;
		private Action<T> onLateChangeData;
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
			if (onChangeData == null && onLateChangeData == null)
				return;

			T data = _GetData();
			if (onChangeData != null)
			{
				foreach (var handler in onChangeData.GetInvocationList())
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
			if (onLateChangeData != null)
			{
				foreach (var handler in onLateChangeData.GetInvocationList())
				{
					try
					{
						((Action<T>)handler).Invoke(data);
					}
					catch (Exception ex)
					{
						UnityEngine.Debug.LogError($"GamePlayData<{typeof(T).Name}> Late Listener error: {ex.Message}");
					}
				}
			}
		}
		public void OnChangeData(Action<T> action)
		{
			if (action == null) return;
			onChangeData -= action;
			onChangeData += action;
		}
		public virtual void AddListener(Action<T> listener)
		{
			if (listener == null) return;
			onChangeData -= listener;
			onChangeData += listener;
		}
		public virtual void RemoveListener(Action<T> listener)
		{
			if (listener == null) return;
			onChangeData -= listener;
			onLateChangeData -= listener;
		}
		public virtual void AddLateListener(Action<T> listener)
		{
			if (listener == null) return;
			onLateChangeData -= listener;
			onLateChangeData += listener;
		}
		public void RemoveAllListener()
		{
			onChangeData = null;
			onLateChangeData = null;
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
	}
}
public partial class StrategyGamePlayData // Common Game Play Data
{
	[Serializable]
	public class CommonGamePlayData
	{
		public StrategyDetailsPanelUI.StrategyDetailsPanelType openStartType;

		public ObserverString selectControlBase;
		public ObserverInt selectUnitID;
	}
}
public partial class StrategyGamePlayData // Mission Data
{
	[Serializable]
	public class MissionTreeData
	{
		[Serializable]
		public struct ItemStruct : IDisposable
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
	public class ControlBaseData
	{
		[Serializable]
		public class Profile : GamePlayData<Profile.Data>
		{
			public Profile(Data data) : base(data) { }
			[Serializable]
			public struct Data
			{
				public string controlBaseName;
				// 환경 요소
				public string environmentalKey;
				// 적용되어 있는 각종 효과
				public Effect effect;

				public string EffectString()
				{
					return effect.ToString();
				}
			}
		}
		[Serializable]
		public class Capture : GamePlayData<Capture.Data>
		{
			public Capture(Data data) : base(data) { }
			[Serializable]
			public struct Data
			{
				public int captureFactionID;
				public float captureProgress;

				public float captureTime;
			}
		}
		[Serializable]
		public class MainStats : GamePlayData<MainStats.Data>
		{
			public MainStats(Data data) : base(data) { }
			[Serializable]
			public struct Data
			{
				// 현재 비축량
				public StatsList stats;
				public StatsList buff;
				public Data(StatsList stats = null, StatsList buff = null) : this()
				{
					this.stats = stats ?? StatsList.SupplyStatsList;
					this.buff = buff ?? StatsList.Empty;
				}
				public int GetValue(StatsType statsType, SymbolType symbol)
				{
					if (stats == null) return 0;
					return stats.GetValue(statsType, symbol).Value;
				}
				internal StatsList GetStatsList()
				{
					return stats;
				}
				internal StatsList GetBuffList()
				{
					return buff;
				}
			}
		}
		[Serializable]
		public class Facilities : GamePlayData<Facilities.Data>
		{
			public Facilities(Data data) : base(data) { }
			[Serializable]
			public struct Data
			{
				public Slot[] slotData;
			}
			[Serializable]
			public struct Slot
			{
				public string facilitiesKey;
				public Installing installing;
			}
			[Serializable]
			public struct Installing
			{
				public string facilitiesKey;
				public float  installingTime;
				public float  timeRemaining;
			}
		}
		[Serializable]
		public class Support : GamePlayData<Support.Data>
		{
			public Support(Data data) : base(data)
			{
			}
			[Serializable]
			public struct Data
			{
				public int supportPoint;

				public int offensivePoint;
				public int defensivePoint;
				public int supplyPoint;
				public int facilitiesPoint;
			}
		}
		[Flags]
		public enum Effect
		{
			None = 0,

			정전 = 1 << 0,
			재밍 = 1 << 1,
			화재 = 1 << 2,

			포위 = 1 << 10,

			반파 = 1 << 30,
			완파 = 1 << 31,
		}
	}
	public class UnitData
	{
		[Serializable]
		public class Profile : GamePlayData<Profile.Data>
		{
			public Profile(Data data) : base(data) { }
			[Serializable]
			public struct Data
			{
				public string unitKey;      // 원본과 매칭되는 키
				public string unitName;     // 유닛 이름
				public int unitID;          // 씬에 배치된 유닛 고유번호
				public int factionID;       // 유닛이 속한 세력 번호

				public WeaponType weaponType;
				public ProtectionType protectType;
			}
		}
		[Serializable]
		public class Stats : GamePlayData<Stats.Data>
		{
			public Stats(Data data) : base(data) { }
			[Serializable]
			public struct Data
			{
				public StatsList statsList;

				public Data(StatsList statsList = null)
				{
					this.statsList = statsList ?? StatsList.UnitStatsList;
				}
				public int GetValue(StatsType statsType, SymbolType symbol)
				{
					if (statsList == null) return 0;
					return statsList.GetValue(statsType, symbol).Value;
				}
				public void SetValue(StatsType statsType, int value, SymbolType symbol)
				{
					if (statsList == null) return;
					statsList.SetValue(statsType, value, symbol);
				}
			}
		}
		[Serializable]
		public class Skill : GamePlayData<Skill.Data>
		{
			public Skill(Data data) : base(data) { }
			[Serializable]
			public struct Data
			{
				public SkillData[] skillDatas;
			}
			[Serializable]
			public struct SkillData
			{
				public int skillID;
				public int skillLevel;
			}
		}
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


	// 유닛 피해량 (명중)
	// 기본 피해량 = (유닛_공격력 - 유닛_방어력)
	// 상성 보정량 = ((유닛_관통레벨/유닛_장갑레벨)^2 : > 0.1 and < 1) * 상성 보정; 
	// 최종 피해량 = 기본 피해량 * 상성 보정량 * 난수(0.8~1.2) 
	// 피해 계산 반복 => 유닛_공격당피격수 만큼
	// 
	// 유닛 피해량 (치명)
	// 기본 피해량 = (유닛_공격력 - 유닛_방어력)
	// 치명 피해량 = ((기본 피해량 + 유닛_치명공격력) * 적용 피해량 - 유닛_치명방어력)
	// 상성 보정량 = ((유닛_관통레벨 / 유닛_장갑레벨)^2 : > 0.3 and < 1) * 상성 보정; 
	// 최종 피해량 = (기본 피해량 + 치명 피해량) * 상성 보정량 * 난수(0.8~1.2)
	// 피해 계산 반복 => 유닛_공격당피격수 만큼
	//
	// 확률 
	// 명중 확률 = 유닛_공격명중기회 / (유닛_공격명중기회 + 유닛_공격회피기회)
	// 치명 확률 = 기회/(기회+회피)
	//
	// 공격 딜레이 (전력 부족 또는 EMP로 인한 방전 상태에서 %50 속도)
	// 타겟 변경 => 유닛_조준지연시간 => (유닛_연속공격횟수 * 유닛_연속공격지연시간)(공격이벤트 발생) => 유닛_재공격지연시간 => 재공격
	// 평균 DPS
	// 시간 = (유닛_연속공격횟수 * 유닛_연속공격지연시간) + 유닛_재공격지연시간 
	// 공격 = 유닛_공격력 * (유닛_공격당피격수 * 유닛_연속공격횟수)
	// DPS = 공격/시간
	//
	// 거점 피해량 (필중)
	// 기본 피해량 = 유닛_공격력
	// 상성 보정량 = 상성 보정
	// 최종 피해량 = 기본 피해량 * 상성 보정량 * 난수(0.8~1.2) 
	// 피해 계산 반복 => 유닛_공격당피격수 만큼
	public enum StatsType
	{
		None = -1,

		유닛_인력               = 1000, // 사용하기 위해 필요한 인력
		유닛_물자               = 1001, // 사용하기 위해 필요한 물자
		유닛_전력               = 1002, // 사용하기 위해 필요한 전력

		유닛_최대내구도        = 1100, //보유한 최대 채력량
		유닛_현재내구도        = 1101, //보유한 현재 채력량
		유닛_공격력          = 1102, // 기본 피해량 = 공격력 - 방어력
		유닛_방어력          = 1103, //
		유닛_치유력          = 1104, // 기본 회복량 = 치유력 + 회복력
		유닛_회복력          = 1105, //
		유닛_이동속도         = 1106, //이동 속도
		유닛_점령점수         = 1107, //점령 점수
		유닛_치명공격력        = 1108, // 치명 피해량 = (적용 피해량 + 유닛_치명공격력) *  유닛_치명공격배율 - 유닛_치명방어력 
		유닛_치명공격배율       = 1108, // 최종 피해량 = 적용 피해량 + 치명 피해량
		유닛_치명방어력        = 1109, //

		유닛_관통레벨         = 1200, // 적용 피해량 = 기본 피해량 * ((유닛_관통레벨/유닛_장갑레벨)^2 : 0.1 ~ 1) * 상성 보정
		유닛_장갑레벨         = 1201, //
		유닛_EMP저항레벨      = 1202, //
		유닛_상태이상적용레벨  = 1203, //
		유닛_상태이상저항레벨  = 1204, //

		유닛_공격명중기회       = 1300, //공격 기회 점수	// 명중 확률 = 기회/(기회+회피)
		유닛_공격회피기회       = 1301, //공격 회피 점수
		유닛_치명명중기회       = 1302, //치명타 기회 점수 // 치명 확률 = 기회/(기회+회피)
		유닛_치명회피기회       = 1303, //치명타 회피 점수

		유닛_명중피격수         = 1400, // 1회의 공격 명중시, 몇번의 피격을 발생시키는지 (0 이면 피해 계산 없음)
		유닛_연속공격횟수       = 1401, // 공격 시작시, 연속적으로 공격하는 횟수 (0 이면 공격안함)
		유닛_조준지연시간       = 1402, //공격 대상 변경 후 // 유닛_조준지연시간 => 공격시작 => (유닛_연속공격횟수 * 유닛_연속공격지연시간) => 유닛_재공격지연시간 => 재공격
		유닛_연속공격지연시간    = 1403, //연속 공격 딜레이
		유닛_재공격지연시간     = 1404, //공격 후 딜레이

		유닛_공격소모_물자      = 1500, //공격시 물자 소모량
		유닛_공격소모_전력      = 1501, //공격시 전력 소모량

		유닛_공격범위         = 1600, //공격 범위
		유닛_행동범위         = 1601, //반응 범위
		유닛_시야범위         = 1602, //시야 범위

		거점_인력_최대보유량 = 2000,
		거점_물자_최대보유량 = 2001,
		거점_전력_최대보유량 = 2002,
		거점_인력_분당회복량 = 2010,
		거점_물자_분당회복량 = 2011,
		거점_전력_분당회복량 = 2012,
		거점_인력_현재보유량 = 2020,
		거점_물자_현재보유량 = 2021,
		거점_전력_현재보유량 = 2022,

		거점_최대내구도     = 2100,
		거점_현재내구도     = 2101,
		거점_적정병력수용량  = 2102,
		거점_현재병력수용량  = 2103,
	}
	public enum SymbolType
	{
		Number = 0,
		Percent,
	}

	[Serializable]
	public struct StatsValue
	{
		[HorizontalGroup(width:0.3f), HideLabel]
		private readonly StatsType statsType;
		[HorizontalGroup, HideLabel]
		private int value;
		[HorizontalGroup, HideLabel]
		private SymbolType symbol;
		public StatsType StatsType => statsType;
		public int Value
		{
			get => value; set
			{
				if (this.value == value) return;
				this.value = value;
			}
		}
		public SymbolType Symbol => symbol;
		public StatsValue(StatsType statsType)
		{
			this.statsType = statsType;
			this.value = 0;
			symbol = SymbolType.Number;
		}
		public StatsValue(StatsType statsType, int value, SymbolType symbol)
		{
			this.statsType = statsType;
			this.value = value;
			this.symbol = symbol;
		}

		public static StatsValue operator +(StatsValue p1, StatsValue p2)
		{
			if (p1.statsType != StatsType.None)
			{
				return new StatsValue(p1.statsType, p1.value + p2.value, p1.symbol);
			}
			else
			{
				return new StatsValue(p2.statsType, p1.value + p2.value, p2.symbol);
			}
		}
		public static StatsValue operator -(StatsValue p1, StatsValue p2)
		{
			if (p1.statsType != StatsType.None)
			{
				return new StatsValue(p1.statsType, p1.value - p2.value, p1.symbol);
			}
			else
			{
				return new StatsValue(p2.statsType, p1.value - p2.value, p2.symbol);
			}
		}
		public static StatsValue operator +(StatsValue p1, int p2)
		{
			return new StatsValue(p1.statsType, p1.value + p2, p1.symbol);
		}
		public static StatsValue operator -(StatsValue p1, int p2)
		{
			return new StatsValue(p1.statsType, p1.value - p2, p1.symbol);
		}
		public static StatsValue operator +(int p1, StatsValue p2)
		{
			return new StatsValue(p2.statsType, p1 + p2.value, p2.symbol);
		}
		public static StatsValue operator -(int p1, StatsValue p2)
		{
			return new StatsValue(p2.statsType, p1 - p2.value, p2.symbol);
		}
		public static bool operator ==(StatsValue p1, StatsValue p2)
		{
			return p1.Equals(p2);
		}
		public static bool operator !=(StatsValue p1, StatsValue p2)
		{
			return !p1.Equals(p2);
		}
		public override bool Equals(object obj)
		{
			return obj is StatsValue value &&
				   statsType == value.statsType &&
				   symbol == value.symbol &&
				   this.value == value.value;
		}
		public override int GetHashCode()
		{
			return HashCode.Combine(statsType, value);
		}
		public static StatsValue None => new StatsValue(StatsType.None);
	}

	[Serializable]
	public class StatsList : IDisposable
	{
		[SerializeField]
		private List<StatsValue> values;
		[SerializeField]
		private Action<StatsValue> onChangeValue;
		private Action<StatsValue> onLateChangeValue;
		private bool sleepOnChange;

		public StatsList(params (StatsType type, int value, SymbolType symbol)[] values)
		{
			var list = values == null  ? new StatsValue[0] :  values.Select(i => new StatsValue(i.type, i.value, i.symbol));
			this.values = new List<StatsValue>(values.Length);
			this.values.AddRange(list);
			sleepOnChange = false;
		}
		public StatsList(params StatsValue[] values)
		{
			values ??= new StatsValue[0];
			this.values = new List<StatsValue>(values.Length);
			this.values.AddRange(values);
			sleepOnChange = false;
		}
		public static StatsList Empty => new StatsList(new StatsValue(StatsType.None));
		public static StatsList UnitStatsList => new StatsList(
		new StatsValue(StatsType.유닛_인력),
			new StatsValue(StatsType.유닛_최대내구도),
			new StatsValue(StatsType.유닛_현재내구도),
			new StatsValue(StatsType.유닛_공격력),
			new StatsValue(StatsType.유닛_방어력),
			new StatsValue(StatsType.유닛_치유력),
			new StatsValue(StatsType.유닛_회복력),
			new StatsValue(StatsType.유닛_관통레벨),
			new StatsValue(StatsType.유닛_장갑레벨),
			new StatsValue(StatsType.유닛_EMP저항레벨),
			new StatsValue(StatsType.유닛_공격명중기회),
			new StatsValue(StatsType.유닛_공격회피기회),
			new StatsValue(StatsType.유닛_치명명중기회),
			new StatsValue(StatsType.유닛_치명회피기회),
			new StatsValue(StatsType.유닛_재공격지연시간),
			new StatsValue(StatsType.유닛_연속공격횟수),
			new StatsValue(StatsType.유닛_연속공격지연시간),
			new StatsValue(StatsType.유닛_공격소모_전력),
			new StatsValue(StatsType.유닛_공격소모_물자),
			new StatsValue(StatsType.유닛_공격범위),
			new StatsValue(StatsType.유닛_행동범위),
			new StatsValue(StatsType.유닛_시야범위),
			new StatsValue(StatsType.유닛_이동속도),
			new StatsValue(StatsType.유닛_점령점수),
			new StatsValue(StatsType.None)
			);
		public static StatsList SupplyStatsList => new StatsList(
				new StatsValue(StatsType.거점_인력_현재보유량),
				new StatsValue(StatsType.거점_물자_현재보유량),
				new StatsValue(StatsType.거점_전력_현재보유량),
				new StatsValue(StatsType.거점_인력_최대보유량),
				new StatsValue(StatsType.거점_물자_최대보유량),
				new StatsValue(StatsType.거점_전력_최대보유량),
				new StatsValue(StatsType.거점_인력_분당회복량),
				new StatsValue(StatsType.거점_물자_분당회복량),
				new StatsValue(StatsType.거점_전력_분당회복량),
				new StatsValue(StatsType.거점_최대내구도),
				new StatsValue(StatsType.거점_현재내구도),
				new StatsValue(StatsType.거점_적정병력수용량),
				new StatsValue(StatsType.거점_현재병력수용량),
				new StatsValue(StatsType.None)
		);
		public void Invoke(in StatsValue statsValue)
		{
			if (sleepOnChange) return;
			if (onChangeValue == null && onLateChangeValue == null)
				return;

			if (onChangeValue != null)
			{
				foreach (var handler in onChangeValue.GetInvocationList())
				{
					try
					{
						((Action<StatsValue>)handler).Invoke(statsValue);
					}
					catch (Exception ex)
					{
						UnityEngine.Debug.LogError($"StatsList Listener error: {ex.Message}");
					}
				}
			}
			if (onLateChangeValue != null)
			{
				foreach (var handler in onLateChangeValue.GetInvocationList())
				{
					try
					{
						((Action<StatsValue>)handler).Invoke(statsValue);
					}
					catch (Exception ex)
					{
						UnityEngine.Debug.LogError($"StatsList Late Listener error: {ex.Message}");
					}
				}
			}
		}

		public void SumStats(StatsValue value)
		{
			int findindex = values.FindIndex(b=>b.StatsType == value.StatsType && b.Symbol == value.Symbol);
			if (findindex < 0)
			{
				findindex = values.Count;
				values.Add(value);
			}
			var nextValue = values[findindex] + value;
			if (values[findindex] != nextValue)
			{
				values[findindex] = nextValue;
				Invoke(in nextValue);
			}
		}
		public void SubStats(StatsValue value)
		{
			int findindex = values.FindIndex(b=>b.StatsType == value.StatsType && b.Symbol == value.Symbol);
			if (findindex < 0)
			{
				findindex = values.Count;
				values.Add(0 - value);
			}
			var nextValue = values[findindex] - value;
			if (values[findindex] != nextValue)
			{
				values[findindex] = nextValue;
				Invoke(in nextValue);
			}
		}
		public void AddListener(Action<StatsValue> onChangeValue)
		{
			if (onChangeValue == null) return;
			this.onChangeValue -= onChangeValue;
			this.onChangeValue += onChangeValue;
		}
		public void RemoveListener(Action<StatsValue> onChangeValue)
		{
			if (onChangeValue == null) return;
			this.onChangeValue -= onChangeValue;
			this.onLateChangeValue -= onChangeValue;
		}
		public void AddLateListener(Action<StatsValue> onChangeValue)
		{
			if (onChangeValue == null) return;
			onLateChangeValue -= onChangeValue;
			onLateChangeValue += onChangeValue;
		}
		public void RemoveAllListener()
		{
			onChangeValue = null;
			onLateChangeValue = null;
		}
		public StatsValue GetValue(StatsType statsType, SymbolType symbol)
		{
			int findindex = values.FindIndex(b=>b.StatsType == statsType && b.Symbol == symbol);
			if (findindex < 0)
			{
				return new StatsValue(statsType);
			}
			return values[findindex];
		}
		public void SetValue(StatsType statsType, int value, SymbolType symbol)
		{
			int findindex = values.FindIndex(b=>b.StatsType == statsType && b.Symbol == symbol);
			if (findindex < 0)
			{
				findindex = values.Count;
				values.Add(new StatsValue(statsType));
			}
			var find = values[findindex];
			if (find.Value == value) return;

			find.Value = value;
			values[findindex] = find;
			Invoke(find);
		}
		public void SetValue(StatsValue value)
		{
			int findindex = values.FindIndex(b=>b.StatsType == value.StatsType && b.Symbol == value.Symbol);
			if (findindex < 0)
			{
				findindex = values.Count;
				values.Add(new StatsValue(value.StatsType));
			}
			var find = values[findindex];
			if (find == value) return;
			values[findindex] = value;
			Invoke(find);
		}
		public List<StatsValue> GetValueList()
		{
			return values;
		}
		public void MergeList(params StatsList[] others)
		{
			if (others == null || others.Length == 0) return;
			sleepOnChange = true;
			HashSet<(StatsType statsType, SymbolType symbol)> changed = new HashSet<(StatsType statsType, SymbolType symbol)>();
			foreach (var other in others)
			{
				var list = other.GetValueList();
				int length = list.Count;
				for (int i = 0 ; i < length ; i++)
				{
					var item = list[i];
					changed.Add((item.StatsType, item.Symbol));
					SumStats(list[i]);
				}
			}
			sleepOnChange = false;
			foreach (var item in changed)
			{
				Invoke(GetValue(item.statsType, item.symbol));
			}
		}
		public void ClearValues()
		{
			if (values != null) values.Clear();
		}

        public void Dispose()
        {
			if(values != null) values.Clear();
			values = null;
			onChangeValue = null;
			onLateChangeValue = null;
			sleepOnChange = false;
		}
    }


	[Serializable]
	public class StatsGroup
	{
		[Serializable]
		public struct KeyValue
		{
			public readonly string Key;
			public readonly StatsList List;
			public KeyValue(string key, StatsList list)
			{
				Key = key;
				List = list;
			}
		}
		[SerializeField]
		private List<KeyValue> values;

		public StatsGroup(params (string key, StatsList list)[] values)
		{
			var list = values == null  ? new KeyValue[0] :  values.Select(i => new KeyValue(i.key, i.list));
			this.values = new List<KeyValue>(values.Length);
			this.values.AddRange(list);
		}
		public void AddList(string key, StatsList list)
		{
			int findindex = values.FindIndex(b=>b.Key == key);
			if (findindex < 0)
			{
				values.Add(new KeyValue(key, list));
				return;
			}
			values[findindex].List.MergeList(list);
		}
		public void SetList(string key, StatsList list)
		{
			int findindex = values.FindIndex(b=>b.Key == key);
			if (findindex < 0)
			{
				values.Add(new KeyValue(key, list));
				return;
			}
			values[findindex] = new KeyValue(key, list);
		}
		public void RemoveList(string key)
		{
			int findindex = values.FindIndex(b=>b.Key == key);
			if (findindex < 0)
			{
				return;
			}
			values.RemoveAt(findindex);
		}
		public bool TryGetList(string key, out StatsList list)
		{
			int findindex = values.FindIndex(b=>b.Key == key);
			list = findindex >= 0 ? values[findindex].List : null;
			return list != null;
		}
		internal static StatsGroup Empty => new StatsGroup();

		public List<StatsValue> MergedStatsValueList()
		{
			var merge = StatsList.Empty;
			int length = values.Count;
			merge.ClearValues();
			for (int i = 0 ; i < length ; i++)
			{
				merge.MergeList(values[i].List);
			}
			var result = new List<StatsValue>();
			result.AddRange(merge.GetValueList());
			merge.Dispose();
			return result;
		}
	}
}
