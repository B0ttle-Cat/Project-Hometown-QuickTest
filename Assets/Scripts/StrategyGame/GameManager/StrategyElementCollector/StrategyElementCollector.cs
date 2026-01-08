using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Sirenix.OdinInspector;

namespace StrategyManagerModule
{
	/// <summary>
	/// StrategyElementCollector: 외부에 노출되는 API. 
	/// 내부 구현은 IElementStore 들에 위임한다. 
	/// - 타입을 Register 해서 사용한다. (DIP: 등록은 외부 조립 코드에서)
	/// - 기본적으로 Register<TResult>()는 BaseList<TResult> 또는 ElementList<TResult>를 자동 선택한다.
	/// </summary>
	/// 
	/// --------------------------
	/// 사용 예시(조립 코드 - Composition FindRoot)
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
	/// collector.AddItem(unitInstance);
	/// 
	/// // 변경 리스너
	/// collector.AddChangeListener<UnitObject>((u, added) => {
	/// 	Debug.Log($"CardTarget {u?.ID} {(added ? "added" : "removed")}");
	/// }, invokeForExisting: true);
	/// --------------------------- 

	[Serializable]
	public class StrategyElementCollector : IDisposable
	{
		[ShowInInspector]
		private readonly Dictionary<Type, IElementStore> stores = new Dictionary<Type, IElementStore>();
		public StrategyElementCollector Register<T>(int capacity = 32) where T : class => Register(typeof(T), capacity);
		private StrategyElementCollector Register(Type type, int capacity = 32)
		{
			// 이미 등록된 타입이면 스킵
			if (IsRegistered(type))
				return this;


			// ----------------------------------------------------------
			// 1) 타입별로 사용할 리스트 타입을 결정한다.
			//    IStrategyElement 를 구현했다면 ElementList<TResult> 를,
			//    아니라면 BaseList<TResult> 를 사용한다.
			// ----------------------------------------------------------
			Type listType = typeof(IStrategyElement).IsAssignableFrom(type) 
				? listType = typeof(ElementList<>).MakeGenericType(type)
				: listType = typeof(BaseList<>).MakeGenericType(type);

			// 리스트 인스턴스 생성 (capacity 전달)
			object listInstance = Activator.CreateInstance(listType, capacity);

			// ----------------------------------------------------------
			// 2) ElementStore<TResult> 생성
			//    ElementStore<TResult> 생성자의 시그니처는
			//        (IList<TResult> list)
			//    형태라고 가정하고 리스트 인스턴스를 넘긴다.
			// ----------------------------------------------------------
			Type storeType = typeof(ElementStore<>).MakeGenericType(type);
			IElementStore store = Activator.CreateInstance(storeType, listInstance) as IElementStore;

			// 스토어 등록
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
						newList.AddListener(handler);
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

		/// <summary>타입의 BaseList<TResult> 얻기(등록되어 있어야 함)</summary>
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
				var interfaces = store.GetType().GetInterfaces();

				foreach (var itf in interfaces)
				{
					if (itf.IsGenericType &&
						itf.GetGenericTypeDefinition() == typeof(IElementStore<>))
					{
						var es = (IElementStore)store;
						yield return es.GetRawList();
						break;
					}
				}
			}
		}

		/// <summary>추가/제거용 API</summary>
		public bool Add<T>(T item, Action beforeCallback = null) where T : class
		{
			if (item == null) return false;

			Type type = typeof(T);

			// 내부적으로 등록되어 있지 않으면 자동 등록
			Register(type);

			var list = (stores[typeof(T)] as IElementStore<T>)?.List;
			bool result = list.Add(item , beforeCallback);
			return result;
		}
		public bool Remove<T>(T item) where T : class
		{
			var list = GetList<T>();
			if (list == null) throw new InvalidOperationException($"Type {typeof(T).Name} is not registered.");
			bool result = list.Remove(item);
			return result;
		}
		public bool Clear<T>(bool callback = true) where T : class
		{
			var list = GetList<T>();
			if (list == null) throw new InvalidOperationException($"Type {typeof(T).Name} is not registered.");
			bool result = list.Clear(callback);
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
		/// 이벤트 연결: IStrategyElement 타입이면 IStrategyElement 형식의 핸들러를, 일반 타입이면 TResult 형식 핸들러를 사용
		/// invokeForExisting 가 true 이 경우 기존 아이템에 대해 onChangeListener 콜백을 즉시 호출한다.
		/// </summary>
		public void AddChangeListener<T>(Action<T, bool> onChange, bool invokeForExisting = true) where T : class
		{
			var list = GetList<T>();
			if (list == null)
			{
				Register<T>();
				list = GetList<T>();
			}
			list.AddListener(onChange);
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
			list.RemoveListener(onChange);
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

				// store는 ElementStore<TResult>, store.List가 IDisposable(BaseList<TResult>)이다.
				var storeType = store.GetType();
				var listProp = storeType.GetProperty("PoolList");
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