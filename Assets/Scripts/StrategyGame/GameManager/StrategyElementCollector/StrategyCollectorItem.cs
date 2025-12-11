using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;




namespace StrategyManagerModule
{
	/// <summary>
	/// 공통(비제네릭) 스토어 인터페이스 — Collector는 이 타입들만 보유/관리한다.
	/// </summary>
	public interface IElementStore
	{
		Type ElementType { get; }
		IList GetRawList();
	}

	/// <summary>
	/// 제네릭 스토어 인터페이스: 구체 타입 접근을 허용한다.
	/// </summary>
	public interface IElementStore<T> : IElementStore where T : class
	{
		BaseList<T> List { get; }
	}

	/// <summary>
	/// 기본 리스트: 이벤트, 순회, Find 등 공통 기능을 제공한다.
	/// (IStrategyElement 특성이 없는 타입용으로도 사용)
	/// </summary>
	[Serializable]
	public class BaseList<T> : IEnumerable<T>, IDisposable where T : class
	{
		[SerializeField]
		private List<T> list;
		public List<T> Items => list ??= new List<T>();

		private event Action<T, bool> onChangeListener;
		private bool sleepCallback;

		public int Count => Items.Count;
		public T this[int index] => Items[index];

		public BaseList(int capacity = 32)
		{
			list = new List<T>(capacity);
			sleepCallback = false;
			onChangeListener = null;
		}

		public virtual void Dispose()
		{
			list?.Clear();
			list = null;
			onChangeListener = null;
		}

		// Add / Remove
		public virtual bool Add(T item)
		{
			if (item == null) return false;
			if (Items.Contains(item)) return false;
			Items.Add(item);
			Invoke(item, true);
			return true;
		}
		public virtual bool Remove(T item)
		{
			if (item == null) return false;
			if (Items.Remove(item))
			{
				Invoke(item, false);
				return true;
			}
			return false;
		}

		// Bulk helpers
		public bool AddRange(IEnumerable<T> items)
		{
			if (items == null) return false;
			sleepCallback = true;
			var changed = false;
			foreach (var i in items)
			{
				if (Add(i)) changed = true;
			}
			sleepCallback = false;
			return changed;
		}
		public bool RemoveRange(IEnumerable<T> items)
		{
			if (items == null) return false;
			sleepCallback = true;
			var changed = false;
			foreach (var i in items)
			{
				if (Remove(i)) changed = true;
			}
			sleepCallback = false;
			return changed;
		}
		internal void AddRaw(T item)
		{
			// ID 유지
			// InStrategyCollector 호출 없음
			// 콜백 없음
			if (Items.Contains(item)) return;
			Items.Add(item);
		}
		public bool RemoveRaw(T item)
		{
			return Items.Remove(item);
		}
		// Event hooks
		protected void Invoke(T item, bool added)
		{
			if (sleepCallback || onChangeListener == null) return;
			try { onChangeListener.Invoke(item, added); }
			catch (Exception ex) { Debug.LogException(ex); }
		}

		public void AddListener(Action<T, bool> listener)
		{
			if (listener == null) return;
			onChangeListener -= listener;
			onChangeListener += listener;
		}
		public void RemoveListener(Action<T, bool> listener)
		{
			if (listener == null) return;
			onChangeListener -= listener;
		}

		// Query helpers
		public bool TryFind(Func<T, bool> cond, out T t)
		{
			t = default;
			if (cond == null) return false;
			for (int i = 0 ; i < Items.Count ; i++)
			{
				var it = Items[i];
				if (it == null) continue;
				if (cond(it))
				{
					t = it;
					return true;
				}
			}
			return false;
		}
		public T Find(Func<T, bool> cond)
		{
			if (cond == null) return null;
			for (int i = 0 ; i < Items.Count ; i++)
			{
				var it = Items[i];
				if (it == null) continue;
				if (cond(it)) return it;
			}
			return null;
		}
		public List<T> FindAll(Func<T, bool> cond)
		{
			var result = new List<T>();
			if (cond == null) return result;
			for (int i = 0 ; i < Items.Count ; i++)
			{
				var it = Items[i];
				if (it == null) continue;
				if (cond(it)) result.Add(it);
			}
			return result;
		}
		public void ForEach(Action<T> action)
		{
			if (action == null) return;
			for (int i = 0 ; i < Items.Count ; i++)
			{
				var it = Items[i];
				if (it == null) continue;
				action(it);
			}
		}

		// IEnumerable
		public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();

		public bool TryFind(int index, out T t)
		{
			t = Find(index);
			return t != null;
		}
		public virtual T Find(int index)
		{
			if (index < 0 || index >= Items.Count)
				return null;
			return Items[index];
		}

		public IEnumerable<Action<T, bool>> GetOnChangeHandlers()
		{
			if (onChangeListener == null) yield break;

			foreach (var d in onChangeListener.GetInvocationList())
				yield return (Action<T, bool>)d;
		}

	}

	/// <summary>
	/// IStrategyElement 전용 리스트: ID 관리, In/Out 콜백 연결.
	/// </summary>
	[Serializable]
	public class ElementList<T> : BaseList<T> where T : class, IStrategyElement
	{
		private int nextUniqueID;
		private readonly HashSet<int> recycled = new HashSet<int>();
		private int[] lockedIDs;

		public ElementList(int capacity = 32) : base(capacity)
		{
			nextUniqueID = 0;
			recycled.Clear();
		}

		public override void Dispose()
		{
			// call OutStrategyCollector for existing items
			foreach (var it in Items.ToList())
			{
				if (it != null)
				{
					it.OutStrategyCollector();
				}
			}
			base.Dispose();
			nextUniqueID = 0;
			recycled.Clear();
			lockedIDs = null;
		}

		// ID lock helpers
		public void LockIDs(int[] lockIds) => lockedIDs = lockIds;
		public void UnlockIDs() => lockedIDs = null;
		private bool IsLocked(int id)
		{
			if (lockedIDs == null) return false;
			for (int i = 0 ; i < lockedIDs.Length ; i++)
				if (lockedIDs[i] == id) return true;
			return false;
		}

		private int AcquireID()
		{
			// try recycled first
			foreach (var id in recycled)
			{
				if (!IsLocked(id))
				{
					// remove from recycled and return
					recycled.Remove(id);
					return id;
				}
			}
			// advance nextUniqueID skipping locked ones
			while (IsLocked(nextUniqueID)) nextUniqueID++;
			return nextUniqueID++;
		}
		private void ReleaseID(int id)
		{
			// recycle
			if (!recycled.Add(id))
			{
				Debug.LogError($"Attempted to recycle duplicate ID {id}");
			}
		}

		public override bool Add(T item)
		{
			if (item == null) return false;
			if (Items.Contains(item)) return false;

			item.ID = AcquireID();
			Items.Add(item);
			item.InStrategyCollector();
			Invoke(item, true);
			return true;
		}
		public override bool Remove(T item)
		{
			if (item == null) return false;
			if (Items.Remove(item))
			{
				ReleaseID(item.ID);
				item.OutStrategyCollector();
				Invoke(item, false);
				return true;
			}
			return false;
		}

		public override T Find(int id)
		{
			for (int i = 0 ; i < Items.Count ; i++)
			{
				var it = Items[i];
				if (it == null) continue;
				if (it.ID == id) return it;
			}
			return null;
		}
	}

	/// <summary>
	/// 기본 ElementStore 구현: ElementList<T> 또는 BaseList<T>를 래핑한다.
	/// </summary>
	public class ElementStore<T> : IElementStore<T> where T : class
	{
		public Type ElementType => typeof(T);
		public BaseList<T> List { get; }

		public ElementStore(BaseList<T> list = null)
		{
			List = list ?? new BaseList<T>();
		}

		public IList GetRawList() => List.Items;
	}

	/// <summary>
	/// PoolElementList: BaseList 기반, 풀링 전용 기능 포함
	/// </summary>
	[Serializable]
	public class PoolElementList<T> : BaseList<T> where T : class, IStrategyPoolingElement
	{
		private Stack<T> recycled;

		public PoolElementList(int capacity = 32) : base(capacity)
		{
			recycled = new Stack<T>(capacity);
		}
		public override void Dispose()
		{
			base.Dispose();
			if (recycled != null)
			{
				recycled.Clear();
				recycled = null;
			}
		}
		/// <summary>
		/// 새 객체를 생성하지 않고, 재활용 가능 객체 제공
		/// </summary>
		public T Acquire(Func<T> factory)
		{
			if (recycled.Count > 0)
			{
				var item = recycled.Pop();
				item.gameObject.SetActive(true);
				Add(item);
				return item;
			}
			else
			{
				var item = factory();
				item.gameObject.SetActive(true);
				Add(item);
				return item;
			}
		}
		public async Awaitable<T[]> Acquires(int count, Func<int, Awaitable<T[]>> factory)
		{
			T[] result = new T[count];
			while (count > 0)
			{
				if (recycled.Count > 0)
				{
					var item = recycled.Pop();
					result[count - 1] = item;
					count--;
				}
				else
				{
					var newArray = await factory(count);
					int length = newArray.Length;
                    for (int i = 0 ; i < length ; i++)
                    {
						result[i] = newArray[i];
					}
                    count = 0;
				}

			}
			count = result.Length;
            for (int i = 0 ; i < count ; i++)
            {
				var item = result[i];
				item.gameObject.SetActive(true);
				Add(item);
			}


            return result;
		}
		public async void ReadyPoolCount(int count, Func<int, Awaitable<T[]>> factory)
		{
			if (recycled.Count >= count) return;
			count -= recycled.Count;
			var newArray = await factory(count);

            for (int i = 0 ; i < count ; i++)
            {
				var item = newArray[i];
				item.gameObject.SetActive(false);
				recycled.Push(item); // 풀에 반환
			}
		}

		public override bool Add(T item)
		{
			if (item == null) return false;
			if (Items.Contains(item)) return false;

			Items.Add(item);
			item.InStrategyCollector();
			Invoke(item, true);
			return true;
		}

		/// <summary>
		/// 제거 시, ID 재활용 및 OutStrategyCollector 호출, 풀에 반환
		/// </summary>
		public override bool Remove(T item)
		{
			if (item == null) return false;
			if (base.Remove(item))
			{
				item.gameObject.SetActive(false);
				recycled.Push(item); // 풀에 반환
				item.OutStrategyCollector();
				Invoke(item, false);
				return true;
			}
			return false;
		}

		public void ClearPool()
		{
			recycled.Clear();
		}

        public int RecycledCount => recycled.Count;
	}

	/// <summary>
	/// 풀링 전용 스토어
	/// </summary>
	public class PoolingElementStore<T> : IElementStore<T> where T : class, IStrategyPoolingElement
	{
		public GameObject Prefab { get; }
		public Type ElementType => typeof(T);
		BaseList<T> IElementStore<T>.List => PoolList;
		public PoolElementList<T> PoolList { get; }

		public PoolingElementStore(GameObject prefab, PoolElementList<T> list = null)
		{
			Prefab = prefab;
			PoolList = list ?? new PoolElementList<T>();
		}

		public IList GetRawList() => PoolList.Items;
	}

}