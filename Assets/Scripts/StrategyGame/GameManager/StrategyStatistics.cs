using System;
using System.Collections.Generic;

using UnityEngine;

public partial class StrategyStatistics : MonoBehaviour
{
  

    public class StatsItem : IDisposable
	{
		public string catagory;
		public string itemID;

		private StatsData data;

		public bool TryGetValue<T>(out T t) where T : unmanaged
		{
			if (data != null && data is StatsData<T> tData)
			{
				t = tData.Value;
				return true;
			}
			t = default;
			return false;
		}
		public void SetValue<T>(T t) where T : unmanaged
		{
			if (data == null || data is not StatsData<T> tData)
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
					data = new StatsData<T>(t);
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
			itemID = null;
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
			if (data == null || data is not StatsData<T> tData) return;
			tData.AddListener(listener);
		}
		public void RemoveListener<T>(Action<T> listener) where T : unmanaged
		{
			if (listener == null) return;
			if (data == null || data is not StatsData<T> tData) return;
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
	private abstract class StatsData : IDisposable
	{
		public abstract void Invoke();
		public abstract void AddListener_ToString(Action<string> listener);
		public abstract void RemoveListener_ToString(Action<string> listener);
		public abstract void RemoveAllListener();
		public abstract void Dispose();
	}
	private class StatsData<T> : StatsData where T : unmanaged
	{
		protected T value;

		public StatsData(T value)
		{
			this.value = value;
		}

		public virtual T Value
		{
			get => value; set
			{
				if (IsEquals(value)) return;
				this.value = value;
				Invoke();
			}
		}
		private event Action<T> onValueChanged;
		private event Action<string> toStringChanged;

		public override void Invoke()
		{
			if (onValueChanged != null) onValueChanged.Invoke(value);
			if (toStringChanged != null) toStringChanged.Invoke(ToString());
		}
		public virtual bool IsEquals(T value)
		{
			return this.value.Equals(value);
		}

		public void AddListener(Action<T> listener)
		{
			if (listener == null) return;
			onValueChanged -= listener;
			onValueChanged += listener;
		}
		public void RemoveListener(Action<T> listener)
		{
			if (listener == null) return;
			onValueChanged -= listener;
		}
		public override void AddListener_ToString(Action<string> listener)
		{
			if (listener == null) return;
			toStringChanged -= listener;
			toStringChanged += listener;
		}
		public override void RemoveListener_ToString(Action<string> listener)
		{
			if (listener == null) return;
			toStringChanged -= listener;
		}
		public override void RemoveAllListener()
		{
			onValueChanged = null;
			toStringChanged = null;
		}
		public override void Dispose()
		{
			onValueChanged = null;
			toStringChanged = null;
			try
			{
				((IDisposable)value).Dispose();
			}
			catch { }
		}

		public override string ToString()
		{
			return value.ToString();
		}
	}

	private class StatsData_Bool : StatsData<bool>
	{
		public StatsData_Bool(bool value) : base(value)
		{
		}
		public override string ToString()
		{
			return Value ? "Yes" : "No";
		}
	}
	private class StatsData_Float : StatsData<float>
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
	public void AddItem<T>(string catagory, string itemID, T value) where T : unmanaged
	{
		List<StatsItem> list = statsDatas[catagory] ??= new List<StatsItem>();

		int findIndex = list.FindIndex(i => i.itemID.Equals(itemID));
		if (findIndex < 0)
		{
			var newIteme = new StatsItem()
			{
				catagory = catagory,
				itemID = itemID,
			};
			newIteme.SetValue(value);

			list.Add(newIteme);
		}
		else
		{
			list[findIndex].SetValue(value);
		}
	}
	public void RemoveItem(string catagory, string itemID)
	{
		if (statsDatas.TryGetValue(catagory, out var list))
		{
			int findIndex = list.FindIndex(i => i.itemID.Equals(itemID));
			list.RemoveAt(findIndex);
			if (list.Count == 0)
			{
				statsDatas.Remove(catagory);
			}
		}
	}

	private bool TryFindItem(string catagory, string itmeID, out StatsItem statsItem)
	{
		statsItem = null;
		if (statsDatas.TryGetValue(catagory, out var list))
		{
			int findIndex = list.FindIndex(i => i.itemID.Equals(itmeID));
			if (findIndex > 0)
			{
				statsItem = list[findIndex];
			}
		}
		return statsItem != null;
	}
	public List<(string catagory, string itemID)> SelectKeyList(Func<(string catagory, string itemID), bool> condition = null)
	{
		List<(string catagory, string itemID)> list = new List<(string catagory, string itemID)>(statsDatas.Count);
		foreach (var item in statsDatas)
		{
			foreach (var _item in item.Value)
			{
				(string catagory, string itemID) value = (_item.catagory, _item.itemID);
				if (condition == null || condition(value))
				{
					list.Add(value);
				}
			}
		}
		return list;
	}

	public void AddListener_ToString(string catagory, string itmeID, Action<string> toString, bool callAfterAdd = false)
	{
		if (toString == null) return;
		if (!TryFindItem(catagory, itmeID, out var item)) return;
		item.AddListener_ToString(toString);
		if (callAfterAdd) toString.Invoke(item.ToValueString());
	}
	public void RemoveListener_ToString(string catagory, string itmeID, Action<string> toString)
	{
		if (toString == null) return;
		if (!TryFindItem(catagory, itmeID, out var item)) return;
		item.RemoveListener_ToString(toString);
	}

	public void AddListener<T>(string catagory, string itmeID, Action<T> listener, bool callAfterAdd = false) where T : unmanaged
	{
		if (listener == null) return;
		if (!TryFindItem(catagory, itmeID, out var item)) return;
		item.AddListener(listener);
		if (callAfterAdd && item.TryGetValue<T>(out var value)) listener.Invoke(value);
	}
	public void RemoveListener<T>(string catagory, string itmeID, Action<T> listener) where T : unmanaged
	{
		if (listener == null) return;
		if (!TryFindItem(catagory, itmeID, out var item)) return;
		item.RemoveListener(listener);
	}
	public void RemoveAllListener(string catagory, string itmeID)
	{
		if (!TryFindItem(catagory, itmeID, out var item)) return;
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
