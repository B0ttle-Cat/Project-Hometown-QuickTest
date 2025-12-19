using System;
using System.Collections.Generic;
using System.Linq;

using Sirenix.OdinInspector;

using UnityEngine;

public partial class StrategyGamePlayData // StatsValue
{
	public static StatsType[] SectorCurrentStats =new StatsType[]
	{
		StatsType.거점_인력_현재,
		StatsType.거점_재료_현재,
		StatsType.거점_전력_현재,
	};

	public static StatsType[] SectorSupplyStats_Max =new StatsType[]
	{
		StatsType.거점_인력_최대,
		StatsType.거점_재료_최대,
		StatsType.거점_전력_최대,
		StatsType.거점_인력_회복,
		StatsType.거점_재료_회복,
		StatsType.거점_전력_회복,
	};
	/// StatsType 을 string 으로 표현할떄 붙어야 하는 접미사.
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

		public readonly StatsType StatsType => statsType;
		public int Value
		{
			readonly get => value;
			set => this.value = value;
		}

		public StatsValue(StatsType statsType)
		{
			this.statsType = statsType;
			this.value = 0;
		}
		public StatsValue((StatsType t, int v) item)
		{
			this.statsType = item.t;
			this.value = item.v;
		}
		public StatsValue(StatsType statsType, int value)
		{
			this.statsType = statsType;
			this.value = value;
		}
		// int로 변환
		public static implicit operator int(StatsValue v) => v.value;
		public static StatsValue operator +(StatsValue p1, StatsValue p2) => new StatsValue(p1.statsType != StatsType.None ? p1.statsType : p2.statsType, p1.value + p2.value);
		public static StatsValue operator -(StatsValue p1, StatsValue p2) => new StatsValue(p1.statsType != StatsType.None ? p1.statsType : p2.statsType, p1.value - p2.value);
		public static StatsValue operator +(StatsValue p1, int p2) => new StatsValue(p1.statsType, p1.value + p2);
		public static StatsValue operator -(StatsValue p1, int p2) => new StatsValue(p1.statsType, p1.value - p2);
		public static int operator +(int p1, StatsValue p2) => p1 + p2.value;
		public static int operator -(int p1, StatsValue p2) => p1 - p2.value;
		public static bool operator >(StatsValue a, StatsValue b) => a.value > b.value;
		public static bool operator <(StatsValue a, StatsValue b) => a.value < b.value;
		public static bool operator >(StatsValue a, int b) => a.value > b;
		public static bool operator <(StatsValue a, int b) => a.value < b;
		public static bool operator >(int a, StatsValue b) => a > b.value;
		public static bool operator <(int a, StatsValue b) => a < b.value;
		public static bool operator >=(StatsValue a, StatsValue b) => a.value >= b.value;
		public static bool operator <=(StatsValue a, StatsValue b) => a.value <= b.value;
		public static bool operator >=(StatsValue a, int b) => a.value >= b;
		public static bool operator <=(StatsValue a, int b) => a.value <= b;
		public static bool operator >=(int a, StatsValue b) => a >= b.value;
		public static bool operator <=(int a, StatsValue b) => a <= b.value;
		public static bool operator ==(StatsValue p1, StatsValue p2) => p1.Equals(p2);
		public static bool operator !=(StatsValue p1, StatsValue p2) => !p1.Equals(p2);
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
		public void Clamp(int min, int max)
		{
			value = Mathf.Clamp(value, max, max);
		}
		public void Negate()
		{
			value = -value;
		}
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

		public StatsList()
		{
			values = new List<StatsValue>();
			onChangeValue = null;
			onLateChangeValue = null;
			sleepOnChange = false;
		}
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
				// 🛡️ 기본 내구도 및 회복 (Durability & Recovery)
				new StatsValue(StatsType.유닛_최대내구도),
				new StatsValue(StatsType.유닛_현재내구도),
				new StatsValue(StatsType.유닛_치유력),
				new StatsValue(StatsType.유닛_회복력),
				// ⚔️ 기본 전투 능력 (Base Combat)
				new StatsValue(StatsType.유닛_공격력),
				new StatsValue(StatsType.유닛_방어력),
				// 🎯 치명타 스탯 (Critical Stats_old) 
				new StatsValue(StatsType.유닛_치명공격력),
				new StatsValue(StatsType.유닛_치명피해율),
				new StatsValue(StatsType.유닛_치명방어력),
				// 🛡️ 관통 및 저항 레벨 (Level Stats_old)
				new StatsValue(StatsType.유닛_관통레벨),
				new StatsValue(StatsType.유닛_장갑레벨),
				new StatsValue(StatsType.유닛_EMP충격레벨), 
				new StatsValue(StatsType.유닛_EMP방호레벨), 
				new StatsValue(StatsType.유닛_상태이상적용레벨),
				new StatsValue(StatsType.유닛_상태이상저항레벨),
				// 🎲 명중 및 회피 확률 (Chance Score)
				new StatsValue(StatsType.유닛_공격명중기회),
				new StatsValue(StatsType.유닛_공격회피기회),
				new StatsValue(StatsType.유닛_치명명중기회),
				new StatsValue(StatsType.유닛_치명회피기회),
				// 탄약 (Ammunition)
				// ⚙️ 공격 시스템 계수 (Cycle Multipliers)
				new StatsValue(StatsType.유닛_탄용량),
				new StatsValue(StatsType.유닛_사용탄수),
				new StatsValue(StatsType.유닛_연속공격횟수),
				new StatsValue(StatsType.유닛_동시공격개수), 
				// ⏱️ 시간 및 딜레이 (Time Delays)
				new StatsValue(StatsType.유닛_조준지연시간_c),
				new StatsValue(StatsType.유닛_재공격지연시간_c),
				new StatsValue(StatsType.유닛_연속공격지연시간_c),
				new StatsValue(StatsType.유닛_재장전시간_c),
				// 💸 공격 소모 자원 (Cycle Cost)
				new StatsValue(StatsType.유닛_공격소모_전력),
				new StatsValue(StatsType.유닛_공격소모_물자),
				// 🔭 범위 및 이동 (Range & Movement)
				new StatsValue(StatsType.유닛_이동속도_c),
				new StatsValue(StatsType.유닛_점령점수),
				new StatsValue(StatsType.유닛_행동범위_c),
				new StatsValue(StatsType.유닛_시야범위_c),
				new StatsValue(StatsType.유닛_공격범위_종료최소_c),
				new StatsValue(StatsType.유닛_공격범위_시작최소_c),
				new StatsValue(StatsType.유닛_공격범위_시작최대_c),
				new StatsValue(StatsType.유닛_공격범위_종료최대_c)
			);
		public static StatsList SectorStatsList => new StatsList(

				new StatsValue(StatsType.거점_인력_최대, 100),
				new StatsValue(StatsType.거점_재료_최대, 1000),
				new StatsValue(StatsType.거점_전력_최대, 1000),

				new StatsValue(StatsType.거점_인력_회복, 5),
				new StatsValue(StatsType.거점_재료_회복, 50),
				new StatsValue(StatsType.거점_전력_회복, 50)
		);
		public static StatsList SectorCurrentStatsList => new StatsList(
				new StatsValue(StatsType.거점_인력_현재, 0),
				new StatsValue(StatsType.거점_재료_현재, 0),
				new StatsValue(StatsType.거점_전력_현재, 0)
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
				values.Add((new StatsValue(value.StatsType)));
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
				values.Add(new StatsValue(value.StatsType));
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
		public int GetValueInt(StatsType statsType)
		{
			int findindex = values.FindIndex(b=>b.StatsType == statsType);
			if (findindex < 0)
			{
				return 0;
			}
			return values[findindex].Value;
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
		public List<StatsValue> GetValueList(bool newCopy = false)
		{
			return newCopy ? new List<StatsValue>(values) : values;
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

		private Action<string> onChangeGroupKey;
		private Action<string> onRemoveGroupKey;
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
		public void SetList(string key, StatsList list)
		{
			if (string.IsNullOrWhiteSpace(key) || list == null) return;
			int findindex = values.FindIndex(b=>b.Key == key);
			if (findindex < 0)
			{
				values.Add(new KeyValue(key, list));
				onChangeGroupKey?.Invoke(key);
				return;
			}
			values[findindex] = new KeyValue(key, list);
			onChangeGroupKey?.Invoke(key);
		}
		public void RemoveList(string key)
		{
			int findindex = values.FindIndex(b=>b.Key == key);
			if (findindex < 0)
			{
				return;
			}
			values.RemoveAt(findindex);
			onRemoveGroupKey?.Invoke(key);
		}
		public bool TryGetList(string key, out StatsList list)
		{
			int findindex = values.FindIndex(b=>b.Key == key);
			list = findindex >= 0 ? values[findindex].List : null;
			return list != null;
		}
		internal static StatsGroup Empty => new StatsGroup();
		public bool HasKey(string key)
		{
			int length = values.Count;
			for (int i = 0 ; i < length ; i++)
			{
				if (values[i].Key == key) return true;
			}
			return false;
		}
		private bool CheckKey(in KeyValue keyValue, string startsWith = "", string endsWith = "")
		{
			string key = keyValue.Key;

			if (!string.IsNullOrWhiteSpace(startsWith) && !key.StartsWith(startsWith))
				return false;

			if (!string.IsNullOrWhiteSpace(endsWith) && !key.EndsWith(endsWith))
				return false;

			return true;
		}
		public List<string> GetkeyList(string startsWith = "", string endsWith = "")
		{
			return values.Where(i => CheckKey(in i, startsWith, endsWith)).Select(i => i.Key).ToList();
		}
		public StatsValue GetValue(StatsType statsType, string startsWith = "", string endsWith = "")
		{
			StatsValue statsValue = new StatsValue(statsType,0);
			int length = values.Count;

			for (int i = 0 ; i < length ; i++)
			{
				var keyValue = values[i];
				if (keyValue.List == null) continue;
				if (!CheckKey(in keyValue, startsWith, endsWith)) continue;
				statsValue += values[i].List.GetValue(statsType);
			}
			return statsValue;
		}
		public int GetValueInt(StatsType statsType, string startsWith = "", string endsWith = "")
		{
			int statsValue = 0;
			int length = values.Count;
			for (int i = 0 ; i < length ; i++)
			{
				var keyValue = values[i];
				if (keyValue.List == null) continue;
				if (!CheckKey(in keyValue, startsWith, endsWith)) continue;
				statsValue += keyValue.List.GetValueInt(statsType);
			}
			return statsValue;
		}
		public List<StatsValue> GetValueList(string startsWith = "", string endsWith = "")
		{
			var merge = StatsList.Empty;
			int length = values.Count;
			merge.ClearValues();
			for (int i = 0 ; i < length ; i++)
			{
				var keyValue = values[i];
				if (keyValue.List == null) continue;
				if (!CheckKey(in keyValue, startsWith, endsWith)) continue;
				merge.MergeList(keyValue.List);
			}
			var result = new List<StatsValue>();
			result.AddRange(merge.GetValueList());
			merge.Dispose();
			return result;
		}
		public List<StatsValue> GetValueList(string startsWith, string endsWith, params StatsType[] types)
		{
			if (values == null || values.Count == 0 || types == null || types.Length == 0)
				return new List<StatsValue>();

			List<StatsValue> newList = new List<StatsValue>(types.Length);
			newList.AddRange(types.Select(t => new StatsValue(t)));

			int length = values.Count;
			for (int i = 0 ; i < length ; i++)
			{
				var keyValue = values[i];
				if (keyValue.List == null) continue;
				if (!CheckKey(in keyValue, startsWith, endsWith)) continue;
				var findDic = keyValue.List.GetValueList(types);
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
		public List<StatsValue> GetValueList(params StatsType[] types)
		{
			return GetValueList("", "", types);
		}
		public StatsGroup Copy()
		{
			return new StatsGroup(values.ToArray());
		}
		public void AddListener(Action<string> onChangeGroupKey, Action<string> onRemoveGroupKey)
		{
			if (onChangeGroupKey != null)
			{
				this.onChangeGroupKey -= onChangeGroupKey;
				this.onChangeGroupKey += onChangeGroupKey;
			}
			if (onRemoveGroupKey != null)
			{
				this.onRemoveGroupKey -= onRemoveGroupKey;
				this.onRemoveGroupKey += onRemoveGroupKey;
			}
		}
		public void RemoveListener(Action<string> onChangeGroupKey, Action<string> onRemoveGroupKey)
		{
			if (onChangeGroupKey != null)
			{
				this.onChangeGroupKey -= onChangeGroupKey;
			}
			if (onRemoveGroupKey != null)
			{
				this.onRemoveGroupKey -= onRemoveGroupKey;
			}
		}
	}
	public interface IStatsValueControl
	{
		// 데이터 저장소 접근을 위한 속성
		public IStatsValueControl StatsValue { get; }
		int GetStatsValue(StatsType type);
		float GetStatsValuePrecent(StatsType type);
		public void SetStatsValue(StatsType type, int value);
		void SetStatsValuePrecent(StatsType type, float valuePercent);
	}

}
