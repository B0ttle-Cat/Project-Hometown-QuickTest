using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace StrategyManagerModule
{
	[Serializable]
	public class StrategyPoolingCollector : IDisposable
	{
		private readonly Dictionary<GameObject, IElementStore> stores = new();

		private readonly Dictionary<Type, Action<GameObject, bool>> onChangeEventWithType = new Dictionary<Type, Action<GameObject, bool>>();
		// 글로벌 추가/삭제 이벤트
		private event Action<GameObject, bool> onAnyElementChanged;
		public StrategyPoolingCollector Register<T>(GameObject prefabObject, int capacity = 32) where T : class, IStrategyPoolingElement => Register(typeof(T), prefabObject, capacity);
		private StrategyPoolingCollector Register(Type type, GameObject prefabObject, int capacity = 32)
		{
			// 이미 있으면 스킵
			if (IsRegistered(prefabObject)) return this;

			if (!typeof(IStrategyPoolingElement).IsAssignableFrom(type))
				throw new NotSupportedException($"{type.FullName} 타입은 IStrategyPoolingElement 인터페이스를 구현하지 않았습니다.");

			// PoolElementList<T> 생성
			Type listType = typeof(PoolElementList<>).MakeGenericType(type);
			object list = Activator.CreateInstance(listType, capacity);

			// PoolingElementStore<T> 생성
			Type storeType = typeof(PoolingElementStore<>).MakeGenericType(type);
			IElementStore store = Activator.CreateInstance(storeType, prefabObject, list) as IElementStore;

			stores[prefabObject] = store;

			return this;
		}
		private bool IsRegistered(GameObject prefabObject) => stores.ContainsKey(prefabObject);
		public PoolElementList<T> GetList<T>(GameObject prefab) where T : class, IStrategyPoolingElement
		{
			if (stores.TryGetValue(prefab, out var s) && s is PoolElementList<T> es)
				return es;
			return null;
		}

		public void ReadyPoolCount<T>(GameObject prefabObject, int count, Func<int, Awaitable<T[]>> factory) where T : class, IStrategyPoolingElement
		{
			if (!stores.TryGetValue(prefabObject, out var s))
				Register<T>(prefabObject);

			var store = stores[prefabObject] as PoolingElementStore<T>;
			 store.PoolList.ReadyPoolCount(count, factory);
		}
		public async Awaitable<T[]> Acquires<T>(GameObject prefabObject, int count, Func<int, Awaitable<T[]>> factory) where T : class, IStrategyPoolingElement
		{
			if (!stores.TryGetValue(prefabObject, out var s))
				Register<T>(prefabObject);

			var store = stores[prefabObject] as PoolingElementStore<T>;
			T[] items = await store.PoolList.Acquires(count, factory);

            for (int i = 0 ; i < count ; i++)
            {
                var item = items[i];
				item.PrefabReference = prefabObject;

				onAnyElementChanged?.Invoke(item.gameObject, true);
				if (onChangeEventWithType.TryGetValue(typeof(T), out var eventWithType))
				{
					eventWithType?.Invoke(item.gameObject, true);
				}
            }

			return items;
		}
		public T Acquire<T>(GameObject prefabObject, Func<T> factory) where T : class, IStrategyPoolingElement
		{
			if (!stores.TryGetValue(prefabObject, out var s))
				Register<T>(prefabObject);

			var store = stores[prefabObject] as PoolingElementStore<T>;
			var item = store.PoolList.Acquire(factory);
			item.PrefabReference = prefabObject;
			
			onAnyElementChanged?.Invoke(item.gameObject, true);
			if (onChangeEventWithType.TryGetValue(typeof(T), out var eventWithType))
			{
				eventWithType?.Invoke(item.gameObject, true);
			}

			return item;
		}
		public bool Release<T>(T item) where T : class, IStrategyPoolingElement
		{
			if (item == null || item.PrefabReference == null) return false;

			if (stores.TryGetValue(item.PrefabReference, out var store))
			{
				if((store as PoolingElementStore<T>).PoolList.Remove(item))
				{
					onAnyElementChanged?.Invoke(item.gameObject, false);
					if (onChangeEventWithType.TryGetValue(typeof(T), out var eventWithType))
					{
						eventWithType?.Invoke(item.gameObject, false);
					}
					return true;
				}
			}
			else
			{
				Debug.LogWarning($"풀에 등록되지 않은 프리팹 {item.gameObject.name}의 객체를 반환 시도했습니다.");
			}
			return false;
		}

		public IEnumerable<IList> GetAllRawLists()
		{
			foreach (var s in stores.Values)
				yield return s.GetRawList();
		}
		public void AddChangeListener<T>(GameObject prefab, Action<T, bool> onChange, bool invokeForExisting = false) where T : class, IStrategyPoolingElement
		{
			if (onChange == null) return;
			if (prefab != null && stores.TryGetValue(prefab, out var findStore))
			{
				if (findStore is PoolElementList<T> findList && findList != null)
				{
					AddListener(findList);
				}
			}
			else
			{
				foreach (var pair in stores)
				{
					var store = pair.Value;

					if (store is not PoolElementList<T> es) continue;
					AddListener(es);
				}
			}

			void AddListener(PoolElementList<T> list)
			{
				list.AddListener(onChange);
				if (!invokeForExisting) return;

				int count = list.Count;
				for (int i = 0 ; i < count ; i++)
				{
					onChange(list[i], true);
				}
			}
		}
		public void RemoveChangeListener<T>(GameObject prefab, Action<T, bool> onChange) where T : class, IStrategyPoolingElement
		{
			if (onChange == null) return;
			if (prefab != null && stores.TryGetValue(prefab, out var findStore))
			{
				if (findStore is PoolElementList<T> findList && findList != null)
				{
					RemoveListener(findList);
				}
			}
			else
			{
				foreach (var pair in stores)
				{
					var store = pair.Value;

					if (store is not PoolElementList<T> es) continue;
					RemoveListener(es);
				}
			}

			void RemoveListener(PoolElementList<T> list)
			{
				list.RemoveListener(onChange);
			}
		}
		public void AddAnyChangeListener(Action<GameObject, bool> listener)
		{
			onAnyElementChanged -= listener;
			onAnyElementChanged += listener;
		}
		public void RemoveAnyChangeListener(Action<GameObject, bool> listener)
		{
			onAnyElementChanged -= listener;
		}
		public void AddChangeListener<T>(Action<GameObject, bool> listener, bool invokeForExisting = false) where T : class, IStrategyPoolingElement
		{
			if (listener == null) return;

			if(onChangeEventWithType.TryGetValue(typeof(T), out var existingEvent))
			{
				existingEvent -= listener;
				existingEvent += listener;
				onChangeEventWithType[typeof(T)] = existingEvent;
			}
			else
			{
				onChangeEventWithType[typeof(T)] = listener;
			}

			if (invokeForExisting)
			{
				foreach (var pair in stores)
				{
					var store = pair.Value;
					if (store is not PoolElementList<T> es) continue;
					int count = es.Count;
					for (int i = 0 ; i < count ; i++)
					{
						listener(es[i].gameObject, true);
					}
				}
			}

		}
		public void RemoveChangeListener<T>(Action<GameObject, bool> listener)
		{
			if (onChangeEventWithType.TryGetValue(typeof(T), out var existingEvent))
			{
				existingEvent -= listener;
				onChangeEventWithType[typeof(T)] = existingEvent;
			}
		}

		public void Dispose()
		{
			foreach (var store in stores.Values)
			{
				if (store is IElementStore<PoolElementList<IStrategyPoolingElement>> es)
				{
					es.List?.Dispose();
				}
			}
			stores.Clear();
		}
    }
}