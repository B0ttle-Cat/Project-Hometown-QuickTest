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

		// 글로벌 추가/삭제 이벤트
		private event Action<GameObject, bool> onAnyElementChanged;
		public StrategyPoolingCollector Register<T>(GameObject prefabObject, int capacity = 32) where T : class, IStrategyPoolingElement => Register(typeof(T), prefabObject, capacity);
		private StrategyPoolingCollector Register(Type type, GameObject prefabObject, int capacity = 32)
		{
			// 이미 있으면 스킵
			if (IsRegistered(prefabObject)) return this;
			IElementStore store;
			if (typeof(IStrategyPoolingElement).IsAssignableFrom(type))
			{
				var ctorList = Activator.CreateInstance(
				typeof(PoolElementList<>).MakeGenericType(type), capacity);
				var storeType = typeof(PoolingElementStore<>).MakeGenericType(type);
				store = Activator.CreateInstance(storeType, ctorList) as IElementStore;

				stores[prefabObject] = store;
			}
			else
			{
				throw new NotSupportedException($"{type.FullName} 타입은 IStrategyPoolingElement 인터페이스를 구현하지 않았습니다.");
			}
			return this;
		}
		private bool IsRegistered(GameObject prefabObject) => stores.ContainsKey(prefabObject);
		public PoolElementList<T> GetList<T>(GameObject prefab) where T : class, IStrategyPoolingElement
		{
			if (stores.TryGetValue(prefab, out var s) && s is PoolElementList<T> es)
				return es;
			return null;
		}

		public T Acquire<T>(GameObject prefabObject, Func<T> factory) where T : class, IStrategyPoolingElement
		{
			if (!stores.TryGetValue(prefabObject, out var s))
				Register<T>(prefabObject);

			var store = stores[prefabObject] as PoolingElementStore<T>;
			var item = store.PoolList.Acquire(factory);
			onAnyElementChanged?.Invoke(item.gameObject, true);
			return item;
		}

		public bool Release<T>(T item) where T : class, IStrategyPoolingElement
		{
			if (item == null || item.PrefabReference == null) return false;

			if (stores.TryGetValue(item.PrefabReference, out var store))
			{
				return (store as PoolingElementStore<T>).PoolList.Remove(item);
			}
			else
			{
				Debug.LogWarning($"풀에 등록되지 않은 프리팹 {item.gameObject.name}의 객체를 반환 시도했습니다.");
				return false;
			}
		}

		public IEnumerable<IList> GetAllRawLists()
		{
			foreach (var s in stores.Values)
				yield return s.GetRawList();
		}

		public void AddAnyChangeListener(Action<object, bool> listener)
		{
			onAnyElementChanged -= listener;
			onAnyElementChanged += listener;
		}
		public void RemoveAnyChangeListener(Action<object, bool> listener)
		{
			onAnyElementChanged -= listener;
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