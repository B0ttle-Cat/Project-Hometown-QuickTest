using System;
using System.Collections.Generic;
using System.Linq;

using Sirenix.OdinInspector;

using UnityEngine;


using static StrategyStartSetterData;
using static UnityEngine.Rendering.DebugUI;

public partial class StrategyGamePlayData
{
	public interface IDataCopy<T>
	{

		public T Copy();
	}
	[Serializable]
	public abstract class GamePlayData<T> where T : IDataCopy<T>
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
		public struct Data : IDataCopy<Data>
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

			public void Paste(Data copy)
			{
			}
			public Data Copy()
			{
				return new Data()
				{
					keyValueList = new List<KeyValue>(keyValueList.ToArray())
				};
			}
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
		public struct Data : IDataCopy<Data>
		{
			public Language.Type LanguageType;

			public Overview overview;
			public Mission mission;

			public Data Copy()
			{
				return new Data()
				{
					LanguageType = LanguageType,
					overview = overview.Copy(),
					mission = mission.Copy()
				};
			}
		}
	}
}
public partial class StrategyGamePlayData // Common Game Play Data
{
	[Serializable]
	public class CommonGamePlayData
	{
		public StrategyDetailsPanelUI.StrategyDetailsPanelType openStartType;

		public ObserverString selectSector;
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
			Sector_Count,
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
	public class SectorData
	{
		[Serializable]
		public class Profile : GamePlayData<Profile.Data>
		{
			public Profile(Data data) : base(data) { }
			[Serializable]
			public struct Data : IDataCopy<Data>
			{
				public string sectorName;
				// 환경 요소
				public string environmentalKey;
				// 적용되어 있는 각종 효과
				public Effect effect;

				public string EffectString()
				{
					return effect.ToString();
				}

				public Data Copy()
				{
					return new Data()
					{
						sectorName = sectorName,
						environmentalKey = environmentalKey,
						effect = effect
					};
				}
			}
		}
		[Serializable]
		public class Capture : GamePlayData<Capture.Data>
		{
			public Capture(Data data) : base(data) { }
			[Serializable]
			public struct Data : IDataCopy<Data>
			{
				public int captureFactionID;
				public float captureProgress;

				public float captureTime;

				public Data Copy()
				{
					return new Data()
					{
						captureFactionID = captureFactionID,
						captureProgress = captureProgress,
						captureTime = captureTime
					};
				}
			}
		}
		[Serializable]
		public class MainStats : GamePlayData<MainStats.Data>
		{
			public MainStats(Data data) : base(data) { }
			[Serializable]
			public struct Data : IDataCopy<Data>
			{
				// 현재 비축량
				public StatsList stats;
				public Data(StatsList stats = null) : this()
				{
					this.stats = stats ?? StatsList.SectorStatsList;
				}
				public int GetValue(StatsType statsType)
				{
					if (stats == null) return 0;
					return stats.GetValue(statsType).Value;
				}
				internal StatsList GetStatsList()
				{
					return stats;
				}
				public Data Copy()
				{
					return new Data()
					{
						stats = stats.Copy(),
					};
				}
			}
		}
		[Serializable]
		public class Facilities : GamePlayData<Facilities.Data>
		{
			public Facilities(Data data) : base(data) { }
			[Serializable]
			public struct Data : IDataCopy<Data>
			{
				public Slot[] slotData;

				public Data Copy()
				{
					return new Data()
					{
						slotData = slotData.Clone() as Slot[]
					};
				}
			}
			[Serializable]
			public struct Slot
			{
				public string facilitiesKey;
				public Construct constructing;
			}
			[Serializable]
			public struct Construct
			{
				public string facilitiesKey;
				public float  constructTime;
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
			public struct Data : IDataCopy<Data>
			{
				public int supportPoint;

				public int offensivePoint;
				public int defensivePoint;
				public int supplyPoint;
				public int facilitiesPoint;

				public Data Copy()
				{
					return new Data()
					{
						supportPoint = supportPoint,
						offensivePoint = offensivePoint,
						defensivePoint = defensivePoint,
						supplyPoint = supplyPoint,
						facilitiesPoint = facilitiesPoint
					};
				}
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
			public struct Data : IDataCopy<Data>
			{
				public string unitKey;      // 원본과 매칭되는 키
				public string unitName;     // 유닛 이름
				public int unitID;          // 씬에 배치된 유닛 고유번호
				public int factionID;       // 유닛이 속한 세력 번호

				public WeaponType weaponType;
				public ProtectionType protectType;


				public Data Copy()
				{
					return default;
				}
			}
		}
		[Serializable]
		public class Stats : GamePlayData<Stats.Data>
		{
			public Stats(Data data) : base(data) { }
			[Serializable]
			public struct Data : IDataCopy<Data>
			{
				public StatsList statsList;

				public Data(StatsList statsList = null)
				{
					this.statsList = statsList ?? StatsList.UnitStatsList;
				}

				public Data Copy()
				{
					return default;
				}

				public int GetValue(StatsType statsType)
				{
					if (statsList == null) return 0;
					return statsList.GetValue(statsType).Value;
				}
				public void SetValue(StatsType statsType, int value)
				{
					if (statsList == null) return;
					statsList.SetValue(statsType, value);
				}
			}
		}
		[Serializable]
		public class Skill : GamePlayData<Skill.Data>
		{
			public Skill(Data data) : base(data) { }
			[Serializable]
			public struct Data : IDataCopy<Data>
			{
				public SkillData[] skillDatas;


				public Data Copy()
				{
					return new Data()
					{
						skillDatas = skillDatas.Clone() as SkillData[],
					};
				}
			}
			[Serializable]
			public struct SkillData
			{
				public int skillID;
				public int skillLevel;
			}
		}
		public class ConnectedSector
		{
			[Header("State")]
			[SerializeField]
			private string lastEnterSectorName;
			[SerializeField]
			private string currEnterSectorName;
			public string ConnectSectorName
			{
				get
				{
					if (string.IsNullOrWhiteSpace(currEnterSectorName))
					{
						if (string.IsNullOrWhiteSpace(lastEnterSectorName))
						{
							return null;
						}
						return lastEnterSectorName;
					}
					return currEnterSectorName;
				}
				set
				{
					if (string.IsNullOrWhiteSpace(currEnterSectorName))
					{
						lastEnterSectorName = currEnterSectorName = value;
					}
					else
					{
						lastEnterSectorName = currEnterSectorName;
						currEnterSectorName = value;
					}
				}
			}
			public SectorObject ConnectSector
			{
				get
				{
					if (StrategyManager.Collector.TryFindSector(ConnectSectorName, out var cb))
					{
						return cb;
					}
					return null;
				}
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
		유닛_공격력            = 1102, // 기본 피해량 = 공격력 - 방어력
		유닛_방어력            = 1103, //
		유닛_치유력            = 1104, // 기본 회복량 = 치유력 + 회복력
		유닛_회복력            = 1105, //
		유닛_이동속도          = 1106, //이동 속도
		유닛_점령점수          = 1107, //점령 점수
		유닛_치명공격력        = 1108, // 치명 피해량 = (적용 피해량 + 유닛_치명공격력) *  유닛_치명공격배율 - 유닛_치명방어력 
		유닛_치명공격배율      = 1108, // 최종 피해량 = 적용 피해량 + 치명 피해량
		유닛_치명방어력        = 1109, //

		유닛_관통레벨          = 1200, // 적용 피해량 = 기본 피해량 * ((유닛_관통레벨/유닛_장갑레벨)^2 : 0.1 ~ 1) * 상성 보정
		유닛_장갑레벨          = 1201, //
		유닛_EMP저항레벨       = 1202, //
		유닛_상태이상적용레벨  = 1203, //
		유닛_상태이상저항레벨  = 1204, //

		유닛_공격명중기회      = 1300, //공격 기회 점수	// 명중 확률 = 기회/(기회+회피)
		유닛_공격회피기회      = 1301, //공격 회피 점수
		유닛_치명명중기회      = 1302, //치명타 기회 점수 // 치명 확률 = 기회/(기회+회피)
		유닛_치명회피기회      = 1303, //치명타 회피 점수

		유닛_명중피격수        = 1400, // 1회의 공격 명중시, 몇번의 피격을 발생시키는지 (0 이면 피해 계산 없음)
		유닛_연속공격횟수      = 1401, // 공격 시작시, 연속적으로 공격하는 횟수 (0 이면 공격안함)
		유닛_조준지연시간      = 1402, //공격 대상 변경 후 // 유닛_조준지연시간 => 공격시작 => (유닛_연속공격횟수 * 유닛_연속공격지연시간) => 유닛_재공격지연시간 => 재공격
		유닛_연속공격지연시간  = 1403, //연속 공격 딜레이
		유닛_재공격지연시간    = 1404, //공격 후 딜레이

		유닛_공격소모_물자     = 1500, //공격시 물자 소모량
		유닛_공격소모_전력     = 1501, //공격시 전력 소모량

		유닛_공격범위          = 1600, //공격 범위
		유닛_행동범위          = 1601, //반응 범위
		유닛_시야범위          = 1602, //시야 범위

		거점_인력_최대보유량   = 2000,
		거점_물자_최대보유량   = 2001,
		거점_전력_최대보유량   = 2002,
		거점_인력_분당회복량   = 2010,
		거점_물자_분당회복량   = 2011,
		거점_전력_분당회복량   = 2012,
		거점_인력_현재보유량   = 2020,
		거점_물자_현재보유량   = 2021,
		거점_전력_현재보유량   = 2022,

		거점_최대내구도        = 2100,
		거점_현재내구도        = 2101,
		거점_적정병력수용량    = 2102,
		거점_현재병력수용량    = 2103,
	}
	public static string SuffixStatsType(StatsType type) => type switch
	{
		_ => ""
	};

	[Serializable]
	public struct StatsValue
	{
		[HorizontalGroup(width:0.5f), HideLabel, SerializeField]
		private StatsType statsType;
		[HorizontalGroup, HideLabel, SerializeField]
		private int value;

		public StatsType StatsType => statsType;
		public int Value
		{
			get => value; set
			{
				if (this.value == value) return;
				this.value = value;
			}
		}

		public StatsValue(StatsType statsType)
		{
			this.statsType = statsType;
			this.value = 0;
		}
		public StatsValue(StatsType statsType, int value)
		{
			this.statsType = statsType;
			this.value = value;
		}

		public static StatsValue operator +(StatsValue p1, StatsValue p2)
		{
			if (p1.statsType != StatsType.None)
			{
				return new StatsValue(p1.statsType, p1.value + p2.value);
			}
			else
			{
				return new StatsValue(p2.statsType, p1.value + p2.value);
			}
		}
		public static StatsValue operator -(StatsValue p1, StatsValue p2)
		{
			if (p1.statsType != StatsType.None)
			{
				return new StatsValue(p1.statsType, p1.value - p2.value);
			}
			else
			{
				return new StatsValue(p2.statsType, p1.value - p2.value);
			}
		}
		public static StatsValue operator +(StatsValue p1, int p2)
		{
			return new StatsValue(p1.statsType, p1.value + p2);
		}
		public static StatsValue operator -(StatsValue p1, int p2)
		{
			return new StatsValue(p1.statsType, p1.value - p2);
		}
		public static StatsValue operator +(int p1, StatsValue p2)
		{
			return new StatsValue(p2.statsType, p1 + p2.value);
		}
		public static StatsValue operator -(int p1, StatsValue p2)
		{
			return new StatsValue(p2.statsType, p1 - p2.value);
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
				   this.value == value.value;
		}
		public override int GetHashCode()
		{
			return HashCode.Combine(statsType, value);
		}
		public static StatsValue None => new StatsValue(StatsType.None);
	}

	[Serializable]
	public class StatsList : IDisposable, IDataCopy<StatsList>
	{
		[SerializeField, ListDrawerSettings(ShowFoldout = false, ShowPaging = false)]
		private List<StatsValue> values;
		[SerializeField]
		private Action<StatsValue> onChangeValue;
		private Action<StatsValue> onLateChangeValue;
		private bool sleepOnChange;

		public StatsList(params (StatsType type, int value)[] values)
		{
			var list = values == null  ? new StatsValue[0] :  values.Select(i => new StatsValue(i.type, i.value));
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
		public static StatsList SectorStatsList => new StatsList(
				new StatsValue(StatsType.거점_인력_현재보유량, 50),
				new StatsValue(StatsType.거점_물자_현재보유량, 50),
				new StatsValue(StatsType.거점_전력_현재보유량, 50),
				new StatsValue(StatsType.거점_인력_최대보유량, 100),
				new StatsValue(StatsType.거점_물자_최대보유량, 1000),
				new StatsValue(StatsType.거점_전력_최대보유량, 1000),
				new StatsValue(StatsType.거점_인력_분당회복량, 5),
				new StatsValue(StatsType.거점_물자_분당회복량, 50),
				new StatsValue(StatsType.거점_전력_분당회복량, 50),
				new StatsValue(StatsType.거점_최대내구도, 500),
				new StatsValue(StatsType.거점_현재내구도, 500),
				new StatsValue(StatsType.거점_적정병력수용량, 100),
				new StatsValue(StatsType.거점_현재병력수용량, 10)
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
			int findindex = values.FindIndex(b=>b.StatsType == value.StatsType);
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
		public void SumStats(List<StatsValue> values)
		{
			int length = values.Count;
			for (int i = 0 ; i < length ; i++)
			{
				SumStats(values[i]);
			}
		}
		public void SubStats(StatsValue value)
		{
			int findindex = values.FindIndex(b=>b.StatsType == value.StatsType);
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
		public void SubStats(List<StatsValue> values)
		{

			int length = values.Count;
			for (int i = 0 ; i < length ; i++)
			{
				SubStats(values[i]);
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
		public StatsValue GetValue(StatsType statsType)
		{
			int findindex = values.FindIndex(b=>b.StatsType == statsType);
			if (findindex < 0)
			{
				return new StatsValue(statsType);
			}
			return values[findindex];
		}
		public void SetValue(StatsType statsType, int value)
		{
			int findindex = values.FindIndex(b=>b.StatsType == statsType);
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
			int findindex = values.FindIndex(b=>b.StatsType == value.StatsType);
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
		public List<StatsValue> GetValueList(params StatsType[] types)
		{
			if (values == null || values.Count == 0 || types == null || types.Length == 0)
				return new List<StatsValue>();

			HashSet<StatsType> findSet = new HashSet<StatsType>(types);
			List<StatsValue> newList = new List<StatsValue>(types.Length);
			newList.AddRange(types.Select(t => new StatsValue(t)));

			foreach (var value in values)
			{
				var statsType = value.StatsType;
				int findIndex = newList.FindIndex(f => f.StatsType == statsType);
				if (findIndex < 0) continue;
				newList[findIndex] += value;
			}

			return newList;
		}
		public void MergeList(params StatsList[] others)
		{
			if (others == null || others.Length == 0) return;
			sleepOnChange = true;
			HashSet<StatsType> changed = new HashSet<StatsType>();
			foreach (var other in others)
			{
				var list = other.GetValueList();
				int length = list.Count;
				for (int i = 0 ; i < length ; i++)
				{
					var item = list[i];
					changed.Add(item.StatsType);
					SumStats(list[i]);
				}
			}
			sleepOnChange = false;
			foreach (var item in changed)
			{
				Invoke(GetValue(item));
			}
		}
		public void ClearValues()
		{
			if (values != null) values.Clear();
		}
		public void Dispose()
		{
			if (values != null) values.Clear();
			values = null;
			onChangeValue = null;
			onLateChangeValue = null;
			sleepOnChange = false;
		}

		public StatsList Copy()
		{
			return new StatsList(values.ToArray());
		}
	}


	[Serializable]
	public class StatsGroup : IDisposable, IDataCopy<StatsGroup>
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
		public StatsGroup()
		{
			this.values = new List<KeyValue>();
		}
		public StatsGroup(params KeyValue[] values)
		{
			var list = values == null ? new KeyValue[0] :  values;
			this.values = new List<KeyValue>(values.Length);
			this.values.AddRange(list);
		}
		public void Dispose()
		{
			if (values == null)
			{
				values.Clear();
				values = null;
			}
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

		public StatsValue GetValue(StatsType statsType)
		{
			StatsValue statsValue = new StatsValue(statsType,0);
			int length = values.Count;
			
			for (int i = 0 ; i < length ; i++)
			{
				if (values[i].List == null) continue;
				statsValue += values[i].List.GetValue(statsType);
			}
			return statsValue;
		}
		public List<StatsValue> GetValueList()
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
		public List<StatsValue> GetValueList(params StatsType[] types)
		{
			if (values == null || values.Count == 0 || types == null || types.Length == 0)
				return new List<StatsValue>();

			List<StatsValue> newList = new List<StatsValue>(types.Length);
			newList.AddRange(types.Select(t => new StatsValue(t)));

			int length = values.Count;
			for (int i = 0 ; i < length ; i++)
			{
				var list = values[i].List;
				var findDic = list.GetValueList(types);
				foreach (var findItem in findDic)
				{
					var statsType = findItem.StatsType;
					int findIndex = newList.FindIndex(f => f.StatsType == statsType);
					if (findIndex < 0) continue;
					newList[findIndex] += findItem.Value;
				}
			}

			return newList;
		}

		public StatsGroup Copy()
		{
			return new StatsGroup(values.ToArray());
		}

	}
}
