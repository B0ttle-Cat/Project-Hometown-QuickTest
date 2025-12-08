using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Sirenix.OdinInspector;

using UnityEngine;


/// <summary>
/// StrategyElementCollector에 수집될 다음을 포함해야 한다: IStrategyElement
/// </summary>
public interface IStrategyElement : IStrategyStartGame
{
	public IStrategyElement ThisElement { get; }
	public int ID { get; set; }
	void InStrategyCollector();
	void OutStrategyCollector();
}

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

		private event Action<T, bool> onChange;
		private bool sleepCallback;

		public int Count => Items.Count;
		public T this[int index] => Items[index];

		public BaseList(int capacity = 32)
		{
			list = new List<T>(capacity);
			sleepCallback = false;
			onChange = null;
		}

		public virtual void Dispose()
		{
			list?.Clear();
			list = null;
			onChange = null;
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
			if (sleepCallback || onChange == null) return;
			try { onChange.Invoke(item, added); }
			catch (Exception ex) { Debug.LogException(ex); }
		}

		public void OnChange(Action<T, bool> handler)
		{
			if (handler == null) return;
			onChange -= handler;
			onChange += handler;
		}
		public void OffChange(Action<T, bool> handler)
		{
			if (handler == null) return;
			onChange -= handler;
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
			if (onChange == null) yield break;

			foreach (var d in onChange.GetInvocationList())
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
	/// StrategyElementCollector: 외부에 노출되는 API. 
	/// 내부 구현은 IElementStore 들에 위임한다. 
	/// - 타입을 Register 해서 사용한다. (DIP: 등록은 외부 조립 코드에서)
	/// - 기본적으로 Register<T>()는 BaseList<T> 또는 ElementList<T>를 자동 선택한다.
	/// </summary>
	/// 
	/// --------------------------
	/// 사용 예시(조립 코드 - Composition Root)
	/// --------------------------
	/// var collector = new StrategyElementCollector();
	/// collector.Register<SectorObject>()
	/// 		 .Register<Faction>()
	/// 		 .Register<UnitObject>()
	/// 		 .Register<OperationObject>()
	/// 		 .Register<SkillObject>();
	/// 
	/// // 커스텀 타입 등록
	/// collector.Register<MyArbitraryData>();
	/// 
	/// // 요소 추가
	/// collector.Add(unitInstance);
	/// 
	/// // 변경 리스너
	/// collector.AddChangeListener<UnitObject>((u, added) => {
	/// 	Debug.Log($"Unit {u?.ID} {(added ? "added" : "removed")}");
	/// }, invokeForExisting: true);
	/// --------------------------- 

	[Serializable]
	public class StrategyElementCollector : IDisposable
	{
		[ShowInInspector]
		private readonly Dictionary<Type, IElementStore> stores = new Dictionary<Type, IElementStore>();

		// 글로벌 이벤트: 모든 타입 추가/삭제 발생시 호출
		private event Action<object, bool> onAnyElementChanged;

		public StrategyElementCollector Register<T>(int capacity = 32) where T : class => Register(typeof(T), capacity);
		private StrategyElementCollector Register(Type type, int capacity = 32)
		{
			// 이미 있으면 스킵
			if (IsRegistered(type)) return this;
			IElementStore store;
			if (typeof(IStrategyElement).IsAssignableFrom(type))
			{
				var ctorList = Activator.CreateInstance(
				typeof(ElementList<>).MakeGenericType(type), capacity);
				var storeType = typeof(ElementStore<>).MakeGenericType(type);
				store = Activator.CreateInstance(storeType, ctorList) as IElementStore;
			}
			else
			{
				var baseList = Activator.CreateInstance(
				typeof(BaseList<>).MakeGenericType(type), capacity);
				var storeType = typeof(ElementStore<>).MakeGenericType(type);
				store = Activator.CreateInstance(storeType, baseList) as IElementStore;
			}

			stores[type] = store;
			return this;
		}
		/// <summary>명시적 스토어 등록(외부에서 커스텀 리스트/스토어 주입 가능)</summary>
		public StrategyElementCollector Register<T>(IElementStore<T> newStore) where T : class
		{
			if (newStore == null) throw new ArgumentNullException(nameof(newStore));

			var type = typeof(T);

			// IStrategyElement 타입이면 명시적 등록 금지
			if (typeof(IStrategyElement).IsAssignableFrom(type))
			{
				throw new InvalidOperationException(
					$"Explicit registration of IStrategyElement types is not allowed: {type.Name}");
			}


			if (stores.TryGetValue(type, out var oldStoreObj))
			{
				if (oldStoreObj is IElementStore<T> oldStore && oldStore != newStore)
				{
					var oldList = oldStore.List;
					BaseList<T> newList = newStore.List;

					// 기존 요소의 ID 및 상태 그대로 유지하면서 삽입
					foreach (var item in oldList.Items)
					{
						// ID 유지 / InStrategyCollector 호출 없음 / 콜백 없음
						newList.AddRaw(item);
					}

					foreach (var handler in oldList.GetOnChangeHandlers())
					{
						newList.OnChange(handler);
					}

					// 기존 리스트 정리
					oldList.Dispose();

					// 스토어 교환
					stores[type] = newStore;
				}
			}

			return this;
		}

		/// <summary>등록 여부 확인</summary>
		public bool IsRegistered<T>() => IsRegistered(typeof(T));
		private bool IsRegistered(Type type) => stores.ContainsKey(type);

		/// <summary>타입의 BaseList<T> 얻기(등록되어 있어야 함)</summary>
		public BaseList<T> GetList<T>() where T : class
		{
			if (stores.TryGetValue(typeof(T), out var s) && s is IElementStore<T> es)
				return es.List;
			return null;
		}

		/// <summary>원시 IList 접근 — 모든 등록된 리스트를 동일하게 접근 가능</summary>
		public IList GetRawList(Type type)
		{
			if (stores.TryGetValue(type, out var s)) return s.GetRawList();
			return null;
		}

		/// <summary>모든 IList 열거</summary>
		public IEnumerable<IList> GetAllRawLists()
		{
			foreach (var kv in stores)
				yield return kv.Value.GetRawList();
		}

		/// <summary>모든 ElementList 열거 (IStrategyElement 전용)</summary>
		public IEnumerable<IList> GetAllElementLists()
		{
			foreach (var store in stores.Values)
			{
				// IElementStore<T>인지 확인
				if (store is IElementStore es)
				{
					var list = es.GetRawList();

					// list가 ElementList<T>인지 확인
					if (list is ElementList<IStrategyElement> || list.GetType().IsSubclassOf(typeof(ElementList<>)))
					{
						yield return list;
					}
				}
			}

		}

		/// <summary>추가/제거용 API</summary>
		public bool Add<T>(T item) where T : class
		{
			if (item == null) return false;

			Type type = typeof(T);

			// 내부적으로 등록되어 있지 않으면 자동 등록
			Register(type);

			var list = (stores[typeof(T)] as IElementStore<T>)?.List;
			bool result = list.Add(item);
			if (result)
			{
				onAnyElementChanged?.Invoke(item, true);
			}
			return result;
		}
		public bool Remove<T>(T item) where T : class
		{
			var list = GetList<T>();
			if (list == null) throw new InvalidOperationException($"Type {typeof(T).Name} is not registered.");
			bool result = list.Remove(item);
			if (result)
			{
				onAnyElementChanged?.Invoke(item, false);
			}
			return result;
		}
		public bool AddRange<T>(IEnumerable<T> items) where T : class
		{
			var list = GetList<T>();
			if (list == null) throw new InvalidOperationException($"Type {typeof(T).Name} is not registered.");
			return list.AddRange(items);
		}
		public bool RemoveRange<T>(IEnumerable<T> items) where T : class
		{
			var list = GetList<T>();
			if (list == null) throw new InvalidOperationException($"Type {typeof(T).Name} is not registered.");
			return list.RemoveRange(items);
		}

		/// <summary>
		/// 이벤트 연결: IStrategyElement 타입이면 IStrategyElement 형식의 핸들러를, 일반 타입이면 T 형식 핸들러를 사용
		/// invokeForExisting 가 true 이 경우 기존 아이템에 대해 onChange 콜백을 즉시 호출한다.
		/// </summary>
		public void AddChangeListener<T>(Action<T, bool> onChange, bool invokeForExisting = false) where T : class
		{
			var list = GetList<T>();
			if (list == null) throw new InvalidOperationException($"Type {typeof(T).Name} is not registered.");
			list.OnChange(onChange);
			if (invokeForExisting)
			{
				foreach (var item in list.Items.ToList())
				{
					onChange?.Invoke(item, true);
				}
			}
		}
		public void RemoveChangeListener<T>(Action<T, bool> onChange) where T : class
		{
			var list = GetList<T>();
			if (list == null) return;
			list.OffChange(onChange);
		}

		public void AddAnyChangeListener(Action<object, bool> onChange, bool invokeForExisting = false)
		{
			onAnyElementChanged -= onChange;
			onAnyElementChanged += onChange;

			if (invokeForExisting)
			{
				foreach (var kv in stores)
				{
					var list = kv.Value.GetRawList();
					if (list == null) continue;

					foreach (var item in list)
					{
						onChange?.Invoke(item, true);
					}
				}
			}
		}
		public void RemoveAnyChangeListener(Action<object, bool> onChange)
		{
			onAnyElementChanged -= onChange;
		}

		/// <summary>유틸: Find / FindAll</summary>
		public bool TryFind<T>(int id, out T t) where T : class, IStrategyElement
		{
			var list = GetList<T>();
			if (list == null) { t = default; return false; }
			return list.TryFind(id, out t);
		}
		public T Find<T>(int id) where T : class, IStrategyElement
		{
			var list = GetList<T>();
			return list?.Find(id);
		}
		public bool TryFind<T>(Func<T, bool> cond, out T t) where T : class
		{
			var list = GetList<T>();
			if (list == null) { t = default; return false; }
			return list.TryFind(cond, out t);
		}
		public T Find<T>(Func<T, bool> cond) where T : class
		{
			var list = GetList<T>();
			return list?.Find(cond);
		}
		public List<T> FindAll<T>(Func<T, bool> cond) where T : class
		{
			var list = GetList<T>();
			return list?.FindAll(cond) ?? new List<T>();
		}
		public void Dispose()
		{
			foreach (var kv in stores)
			{
				// kv.Value : IElementStore
				var store = kv.Value;

				// store는 ElementStore<T>, store.List가 IDisposable(BaseList<T>)이다.
				var storeType = store.GetType();
				var listProp = storeType.GetProperty("List");
				if (listProp == null) continue;

				var listObj = listProp.GetValue(store);
				if (listObj is IDisposable d)
				{
					d.Dispose();
				}
			}

			stores.Clear();
		}
	}
}