using System;
using System.Collections.Generic;

using UnityEngine;

public partial class StrategyStatistics : MonoBehaviour
{
    public class StatsItem : IDisposable
	{
		public string catagory;
		public string key;

		private ObserverValue data;

		public bool TryGetValue<T>(out T t) where T : unmanaged
		{
			if (data != null && data is ObserverStruct<T> tData)
			{
				t = tData.Value;
				return true;
			}
			t = default;
			return false;
		}
		public void SetValue<T>(T t) where T : unmanaged
		{
			if (data == null || data is not ObserverStruct<T> tData)
			{
				if (data != null) data.Dispose();

				if (t is bool tb)
				{
					data = new StatsData_Bool(tb);
				}
				else if (t is float tf)
				{
					data = new StatsData_Float(tf);
				}
				else
				{
					data = new ObserverStruct<T>(t);
				}
			}
			else
			{
				tData.Value = t;
			}
		}
		public void Dispose()
		{
			catagory = default;
			key = null;
			if (data != null) data.Dispose();
			data = null;
		}

		public void Invoke()
		{
			if (data == null) return;
			data.Invoke();
		}
		public void AddListener<T>(Action<T> listener) where T : unmanaged
		{
			if (listener == null) return;
			if (data == null || data is not ObserverStruct<T> tData) return;
			tData.AddListener(listener);
		}
		public void RemoveListener<T>(Action<T> listener) where T : unmanaged
		{
			if (listener == null) return;
			if (data == null || data is not ObserverStruct<T> tData) return;
			tData.RemoveListener(listener);
		}
		public void RemoveAllListener()
		{
			if (data == null) return;
			data.RemoveAllListener();
		}

		public void AddListener_ToString(Action<string> listener)
		{
			if (listener == null) return;
			if (data == null) return;
			data.AddListener_ToString(listener);
		}
		public void RemoveListener_ToString(Action<string> listener)
		{
			if (listener == null) return;
			if (data == null) return;
			data.RemoveListener_ToString(listener);
		}

		internal string ToValueString()
		{
			return data.ToString();
		}
	}

	private class StatsData_Bool : ObserverStruct<bool>
	{
		public StatsData_Bool(bool value) : base(value)
		{
		}
		public override string ToString()
		{
			return Value ? "Yes" : "No";
		}
	}
	private class StatsData_Float : ObserverStruct<float>
	{
		public StatsData_Float(float value) : base(value)
		{
		}

		public override bool IsEquals(float value)
		{
			return Mathf.Approximately(Value, value);
		}
		public override string ToString()
		{
			return $"{Value:0.00}";
		}
	}
}
public partial class StrategyStatistics: IDisposable
{
	private Dictionary<string, List<StatsItem>> statsDatas;
	internal void Init()
	{
		statsDatas = new Dictionary<string, List<StatsItem>>();
	}
	public void Dispose()
	{
		if(statsDatas != null)
		{
			foreach (var item in statsDatas)
			{
				if (item.Value == null) continue;
				foreach (var item2 in item.Value)
				{
					if (item2 == null) continue;
					item2.Dispose();
				}
				item.Value.Clear();
			}
			statsDatas.Clear();
			statsDatas = null;
		}
	}
	public void AddItem<T>(string catagory, string key, T value) where T : unmanaged
	{
		List<StatsItem> list = statsDatas[catagory] ??= new List<StatsItem>();

		int findIndex = list.FindIndex(i => i.key.Equals(key));
		if (findIndex < 0)
		{
			var newIteme = new StatsItem()
			{
				catagory = catagory,
				key = key,
			};
			newIteme.SetValue(value);

			list.Add(newIteme);
		}
		else
		{
			list[findIndex].SetValue(value);
		}
	}
	public void RemoveItem(string catagory, string key)
	{
		if (statsDatas.TryGetValue(catagory, out var list))
		{
			int findIndex = list.FindIndex(i => i.key.Equals(key));
			list.RemoveAt(findIndex);
			if (list.Count == 0)
			{
				statsDatas.Remove(catagory);
			}
		}
	}

	private bool TryFindItem(string catagory, string key, out StatsItem statsItem)
	{
		statsItem = null;
		if (statsDatas.TryGetValue(catagory, out var list))
		{
			int findIndex = list.FindIndex(i => i.key.Equals(key));
			if (findIndex > 0)
			{
				statsItem = list[findIndex];
			}
		}
		return statsItem != null;
	}
	public List<(string catagory, string key)> SelectKeyList(Func<(string catagory, string key), bool> condition = null)
	{
		List<(string catagory, string key)> list = new List<(string catagory, string key)>(statsDatas.Count);
		foreach (var item in statsDatas)
		{
			foreach (var _item in item.Value)
			{
				(string catagory, string key) value = (_item.catagory, _item.key);
				if (condition == null || condition(value))
				{
					list.Add(value);
				}
			}
		}
		return list;
	}

	public void AddListener_ToString(string catagory, string key, Action<string> toString, bool callAfterAdd = false)
	{
		if (toString == null) return;
		if (!TryFindItem(catagory, key, out var item)) return;
		item.AddListener_ToString(toString);
		if (callAfterAdd) toString.Invoke(item.ToValueString());
	}
	public void RemoveListener_ToString(string catagory, string key, Action<string> toString)
	{
		if (toString == null) return;
		if (!TryFindItem(catagory, key, out var item)) return;
		item.RemoveListener_ToString(toString);
	}

	public void AddListener<T>(string catagory, string key, Action<T> listener, bool callAfterAdd = false) where T : unmanaged
	{
		if (listener == null) return;
		if (!TryFindItem(catagory, key, out var item)) return;
		item.AddListener(listener);
		if (callAfterAdd && item.TryGetValue<T>(out var value)) listener.Invoke(value);
	}
	public void RemoveListener<T>(string catagory, string key, Action<T> listener) where T : unmanaged
	{
		if (listener == null) return;
		if (!TryFindItem(catagory, key, out var item)) return;
		item.RemoveListener(listener);
	}
	public void RemoveAllListener(string catagory, string key)
	{
		if (!TryFindItem(catagory, key, out var item)) return;
		item.RemoveAllListener();
	}
}

public static class StatsKey
{
	internal static string JoinPath(string[] paths)
	{
		return string.Join("/", paths);
	}
	public static class InGamePlay
	{
		public static string DestroyCount_Unit => $"{nameof(InGamePlay)}_{nameof(DestroyCount_Unit)}";
		public static string DestroyCount_Building => $"{nameof(InGamePlay)}_{nameof(DestroyCount_Building)}";
	}
}
