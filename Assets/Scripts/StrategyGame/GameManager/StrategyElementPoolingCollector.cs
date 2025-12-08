using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public sealed class StrategyElementPoolingCollector 
{
	public abstract class CollectList
	{
		public abstract IList ActiveList { get; }
	}
	[Serializable]
	public class ElementPool<T> : CollectList , IEnumerable<T>, IDisposable where T : MonoBehaviour, IStrategyPoolingElement
	{
		private readonly GameObject prefab;						// 프리팹 기반
		private readonly Transform parent;				// 풀 정리를 위한 부모
		private readonly Stack<T> pool = new();  // 반환된 오브젝트 보관
		private readonly List<T> activeList = new();  // 현재 사용 중인 객체들
		public override IList ActiveList => activeList;

		private Action<T> onSpawn;
		private Action<T> onDespawn;

		public IEnumerator<T> GetEnumerator()
		{
			return activeList.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return activeList.GetEnumerator();
		}

		public ElementPool(GameObject prefab, Transform parent = null)
		{
			this.prefab = prefab;
			this.parent = parent;
		}
		public void Dispose()
		{
			DespawnAll();
			onSpawn = null;
			onDespawn = null;
		}

		public void SetCallBack(Action<T> onSpawn, Action<T> onDespawn)
		{
			this.onSpawn = onSpawn;
			this.onDespawn = onDespawn;
		}

		public bool HasPool(out T inst)
		{
			if (pool.Count > 0)
			{
				inst = pool.Pop();
				return true;
			}

			inst = null;
			return false;
		}
		public void Spawn(T inst)
		{
			if (inst == null) return;

			activeList.Add(inst);
			inst.InStrategyCollector();
			onSpawn?.Invoke(inst);
		}

		public void Despawn(T inst)
		{
			if (inst == null) return;
			if (!activeList.Remove(inst)) return;

			inst.OutStrategyCollector();
			onDespawn?.Invoke(inst);

			inst.gameObject.SetActive(false);
			pool.Push(inst);
		}

		public void DespawnAll()
		{
			for (int i = activeList.Count - 1 ; i >= 0 ; i--)
			{
				Despawn(activeList[i]);
			}
		}

		public IEnumerable<T> Active => activeList;
	}

	private readonly Dictionary<GameObject, CollectList> poolMap = new();

	[SerializeField]
	private Transform defaultPoolingParent;
	public void Init()
	{
		poolMap.Clear();

		if (defaultPoolingParent == null)
		{
			string findName = "StrategyElementPoolingParent";
			GameObject parentObject = GameObject.Find(findName);
			if(parentObject == null) parentObject = new GameObject(findName);

			if(parentObject != null)
			{
				defaultPoolingParent = parentObject.transform;
			}
		}
	}
	public ElementPool<T> GetOrCreatePool<T>(GameObject prefabKey)
		where T : MonoBehaviour, IStrategyPoolingElement
	{
		if (!poolMap.TryGetValue(prefabKey.gameObject, out var list))
		{
			var pool = new ElementPool<T>(prefabKey, NewPoolingParent(prefabKey));
			poolMap[prefabKey.gameObject] = pool;
			return pool;
		}

		return (ElementPool<T>)list;
	}
	private Transform NewPoolingParent(GameObject prefab)
	{
		string findName = $"PoolingParent_{prefab.name}";
		GameObject parentObject = GameObject.Find(findName);
		if (parentObject == null) parentObject = new GameObject(findName);

		if (parentObject != null)
		{
			return parentObject.transform;
		}
		return null;
	}
	public bool HasInPool<T>(GameObject prefabKey, out T item) 
		where T : MonoBehaviour, IStrategyPoolingElement
	{
		return GetOrCreatePool<T>(prefabKey).HasPool(out item);
	}
	public void Spawn<T>(GameObject prefabKey, T inst)
		where T : MonoBehaviour, IStrategyPoolingElement
	{
		if (inst == null) return;

		inst.PrefabReference = prefabKey;

		if (prefabKey == null)
		{
			Debug.LogError($"[PoolingCollector] RegisterActive 실패: PrefabReference 없음. {inst.name}");
			return;
		}

		GetOrCreatePool<T>(prefabKey).Spawn(inst);
	}
	public bool Despawn<T>(T inst)
		where T : MonoBehaviour, IStrategyPoolingElement
	{
		if (inst.PrefabReference == null)
		{
			Debug.LogError($"[PoolingCollector] Despawn 실패: PrefabReference 없음. {inst.name}");
			return false;
		}

		GetOrCreatePool<T>(inst.PrefabReference).Despawn(inst);
		return true;
	}
}
