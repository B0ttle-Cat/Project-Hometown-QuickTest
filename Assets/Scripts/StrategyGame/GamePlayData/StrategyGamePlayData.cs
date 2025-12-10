using System;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;


using static StrategyManagerModule.StrategyStartSetterData;

public partial class StrategyGamePlayData
{
	public interface IDataCopy<T>
	{
		public T Copy();
	}
	[Serializable]
	public abstract class GamePlayData<T> where T : struct, IDataCopy<T>
	{
		public GamePlayData(T data)
		{
			_data = data;
		}
		[SerializeField,InlineProperty,HideLabel]
		protected T _data;
		private Action<T> onChangeData;
		private Action<T> onLateChangeData;
		public T GetData() => _data;
		public ref T RefData() => ref _data;
		public ref readonly T ReadonlyData() => ref _data;
		public void SetData(T data, bool ignoreChangeEvent = false)
		{
			_data = data;
			if (ignoreChangeEvent) return;
			Invoke();
		}
		public virtual void ClearData(bool ignoreChangeEvent = false)
		{
			SetData(default, ignoreChangeEvent);
		}
		public void Invoke()
		{
			if (onChangeData == null && onLateChangeData == null)
				return;

			ref readonly T data = ref ReadonlyData();
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

		public static KeyValueData Empty => new KeyValueData(new Data()
		{
			keyValueList = new List<KeyValue>()
		});
	}
}
public partial class StrategyGamePlayData // Prepared Info (준비된 데이터)
{
	[Serializable]
	public class GameStartingData : GamePlayData<GameStartingData.Data>
	{
		public GameStartingData(Data data) : base(data) { }

		[Serializable]
		public struct Data : IDataCopy<Data>
		{
			public Language.Type LanguageType;

			public double unscaleGamePlayTime;
			public double gamePlayTime;

			public Overview overview;
			public Mission mission;

			public Data Copy()
			{
				return new Data()
				{
					LanguageType = LanguageType,
					unscaleGamePlayTime = unscaleGamePlayTime,
					gamePlayTime = gamePlayTime,
					overview = overview.Copy(),
					mission = mission.Copy(),
				};
			}
		}
	}
}
public partial class StrategyGamePlayData // Common Game Play Info
{
	[Serializable]
	public class CommonGamePlayData
	{

	}
}
public partial class StrategyGamePlayData // Mission Info
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
public partial class StrategyGamePlayData // Play Content Info
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
				public StatusEffectsFlag effects;

				[FoldoutGroup("CurrentStats"),InlineProperty,HideLabel]
				public StatsList currentStats;

				public readonly string EffectString()
				{
					return effects.ToString();
				}
				public Data Copy()
				{
					return new Data()
					{
						sectorName = sectorName,
						environmentalKey = environmentalKey,
						effects = effects,
						currentStats = currentStats.Copy(),
					};
				}
				public StatsList GetStatsList()
				{
					return currentStats;
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
				[InlineProperty,HideLabel]
				public StatsList stats;
				public Data(StatsList stats = null) : this()
				{
					this.stats = stats ?? StatsList.SectorStatsList;
				}
				public Data Copy()
				{
					return new Data()
					{
						stats = stats.Copy(),
					};
				}
				public int GetValue(StatsType statsType)
				{
					if (stats == null) return 0;
					return stats.GetValue(statsType).Value;
				}
				public void SetValue(StatsType statsType, int value)
				{
					if (stats == null) return;
					stats.SetValue(statsType, value);
				}
				public StatsList GetStatsList()
				{
					return stats;
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
						slotData = slotData.Clone() as Slot[],
					};
				}
			}
			[Serializable]
			public struct Slot
			{
				public string facilitiesKey;
				public Constructing constructing;
			}
			[Serializable]
			public struct Constructing
			{
				public string facilitiesKey;
				public float constructTime;
				public float duration;
				public void Clear()
				{
					facilitiesKey = string.Empty;
					duration = 0f;
				}
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
			public enum SupportType
			{
				Offensive,
				Defensive,
				Supply,
				Facilities,
			}
		}
	}
	[Serializable]
	public class UnitData
	{
		[Serializable]
		public class Profile : GamePlayData<Profile.Data>
		{
			public Profile(Data data) : base(data) { }
			[Serializable]
			public struct Data : IDataCopy<Data>
			{
				public UnitKey unitKey;     // 원본과 매칭되는 키
				public string displayName;  // 유닛 이름
				public int unitID;          // 씬에 배치된 유닛 고유번호
				public int factionID;       // 유닛이 속한 세력 번호

				// 무기 타입과 방어구 타입
				public ProjectileKey projectileKey;
				public ProtectionType protectType;
				// 적용되어 있는 각종 효과
				public StatusEffectsFlag effects;
				public Data Copy()
				{
					return this;
				}
			}

			public void SetUnitID(int unitID)
			{
				ref var data = ref RefData();
				data.unitID = unitID;
			}
		}
		[Serializable]
		public class Stats : GamePlayData<Stats.Data>
		{
			public Stats(Data data) : base(data) { }
			[Serializable]
			public struct Data : IDataCopy<Data>
			{
				public StatsList stats;
				public Data(StatsList statsList = null)
				{
					this.stats = statsList ?? StatsList.UnitStatsList;
				}
				public readonly Data Copy()
				{
					return this;
				}
				public readonly int GetValue(StatsType statsType)
				{
					if (stats == null) return 0;
					return stats.GetValue(statsType).Value;
				}
				public readonly void SetValue(StatsType statsType, int value)
				{
					if (stats == null) return;
					stats.SetValue(statsType, value);
				}
				public readonly StatsList GetStatsList()
				{
					return stats;
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
				public int skillKey;
				public int skillLevel;

				public SkillData(int skillKey, int skillLevel)
				{
					this.skillKey = skillKey;
					this.skillLevel = skillLevel;
				}
			}
		}
		[Serializable]
		public class StatsBuff
		{

		}

		[Serializable]
		public class ConnectSector : GamePlayData<ConnectSector.Data>
		{
			public ConnectSector(Data data) : base(data) { }
			[Serializable]
			public struct Data : IDataCopy<Data>
			{
				[SerializeField]
				private int lastVisiteSectorID;
				[SerializeField]
				private int currVisiteSectorID;

				public Data(int connectSectorID = -1) : this()
				{
					lastVisiteSectorID = currVisiteSectorID = connectSectorID;
				}

				public int VisiteSectorID
				{
					get
					{
						if (currVisiteSectorID == -1)
						{
							if (lastVisiteSectorID == -1)
							{
								return -1;
							}
							return lastVisiteSectorID;
						}
						return currVisiteSectorID;
					}
					set
					{
						if (currVisiteSectorID == -1)
						{
							lastVisiteSectorID = currVisiteSectorID = value;
						}
						else
						{
							lastVisiteSectorID = currVisiteSectorID;
							currVisiteSectorID = value;
						}
					}
				}

				public Data Copy()
				{
					return this;
				}
			}

		}
	}
	[Serializable]
	public class ProjectileData
	{
		[Serializable]
		public class Profile : GamePlayData<Profile.Data>
		{
			public Profile(Data data) : base(data) { }
			[Serializable]
			public struct Data : IDataCopy<Data>
			{
				public ProjectileKey projectileKey; // 원본과 매칭되는 키

				
				public readonly Data Copy()
				{
					return this;
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
				public ProjectileStatsData stats;
				public Data(ProjectileStatsData stats)
				{
					this.stats = stats;
				}
				public readonly Data Copy()
				{
					return new Data(stats?.Copy());
				}
			}
		}

		[Serializable]
		public class Tracking : GamePlayData<Tracking.Data>
		{
			public Tracking(Data data) : base(data) { }
			[Serializable]
			public struct Data : IDataCopy<Data>
			{
				public int orderElementID;		// 발사 대상
				public int targetElementID;     // 목표 대상

				public Vector3 startPosition;   // 발사 위치
				public Vector3 targetPosition;	// 목표 위치

				public readonly Data Copy()
				{
					return this;
				}
			}
		}
	}
}
