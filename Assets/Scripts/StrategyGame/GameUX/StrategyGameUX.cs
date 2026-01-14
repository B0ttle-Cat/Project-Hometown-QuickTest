using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace StrategyManagerModule
{
	[DefaultExecutionOrder(-1)]
	public partial class StrategyGameUX : MonoBehaviour, IStrategyStartGame
	{
		HashSetProcess hashSetProcess;
		HashSetProcess ProcessList => hashSetProcess ??= new HashSetProcess();

		private void Awake()
		{
			hashSetProcess ??= new HashSetProcess();
			IStrategyProcess[] strategyProcess =  GetComponentsInChildren<IStrategyProcess>(true);
			int length = strategyProcess.Length;
			for (int i = 0 ; i < length ; i++)
			{
				var process = strategyProcess[i];
				hashSetProcess.Add(process);
				process.OnInit();
			}
		}
		private void OnDestroy()
		{
			hashSetProcess.Clear();
			hashSetProcess = null;
		}
		void IStrategyStartGame.OnStartGame()
		{
			if (hashSetProcess == null) return;
			foreach (var process in hashSetProcess)
			{
				process.OnStart();
			}
		}
		void IStrategyStartGame.OnStopGame()
		{
			if (hashSetProcess == null) return;
			foreach (var process in hashSetProcess)
			{
				process.OnStop();
			}
			hashSetProcess.Clear();
			hashSetProcess = null;
		}
		private class HashSetProcess : ISet<IStrategyProcess>
		{
			readonly HashSet<IStrategyProcess> set = new HashSet<IStrategyProcess>();

			public int Count => set.Count;

			public bool IsReadOnly => ((ICollection<IStrategyProcess>)set).IsReadOnly;

			public bool Add(IStrategyProcess item)
			{
				if (set.Add(item))
				{
					item.OnStart();
					return true;
				}
				return false;
			}

			public void Clear()
			{
				foreach (var item in set)
				{
					item.OnStop();
				}

				set.Clear();
			}

			public bool Contains(IStrategyProcess item)
			{
				return set.Contains(item);
			}

			public void CopyTo(IStrategyProcess[] array, int arrayIndex)
			{
				set.CopyTo(array, arrayIndex);
			}

			public void ExceptWith(IEnumerable<IStrategyProcess> other)
			{
				set.ExceptWith(other);
			}

			public IEnumerator<IStrategyProcess> GetEnumerator()
			{
				return set.GetEnumerator();
			}

			public void IntersectWith(IEnumerable<IStrategyProcess> other)
			{
				set.IntersectWith(other);
			}

			public bool IsProperSubsetOf(IEnumerable<IStrategyProcess> other)
			{
				return set.IsProperSubsetOf(other);
			}

			public bool IsProperSupersetOf(IEnumerable<IStrategyProcess> other)
			{
				return set.IsProperSupersetOf(other);
			}

			public bool IsSubsetOf(IEnumerable<IStrategyProcess> other)
			{
				return set.IsSubsetOf(other);
			}

			public bool IsSupersetOf(IEnumerable<IStrategyProcess> other)
			{
				return set.IsSupersetOf(other);
			}

			public bool Overlaps(IEnumerable<IStrategyProcess> other)
			{
				return set.Overlaps(other);
			}

			public bool Remove(IStrategyProcess item)
			{
				if (set.Remove(item))
				{
					item.OnStop();
					return true;
				}
				return false;
			}

			public bool SetEquals(IEnumerable<IStrategyProcess> other)
			{
				return set.SetEquals(other);
			}

			public void SymmetricExceptWith(IEnumerable<IStrategyProcess> other)
			{
				set.SymmetricExceptWith(other);
			}

			public void UnionWith(IEnumerable<IStrategyProcess> other)
			{
				set.UnionWith(other);
			}

			void ICollection<IStrategyProcess>.Add(IStrategyProcess item)
			{
				Add(item);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return set.GetEnumerator();
			}
		}


		public void UXUpdate()
		{
			if (hashSetProcess == null || hashSetProcess.Count == 0) return;
			foreach (var item in ProcessList)
			{
				item.Update();
			}
		}
	}
}