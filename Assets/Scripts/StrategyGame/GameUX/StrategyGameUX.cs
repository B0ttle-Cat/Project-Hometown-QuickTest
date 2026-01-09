using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace StrategyManagerModule
{
	public partial class StrategyGameUX : MonoBehaviour, IStrategyStartGame
	{
		HashSetProcess hashSetProcess;
        HashSetProcess ProcessList => hashSetProcess ??= new HashSetProcess();

		void IStrategyStartGame.OnStartGame()
		{
            hashSetProcess ??= new HashSetProcess();
		}
		void IStrategyStartGame.OnStopGame()
		{
            if (hashSetProcess == null) return;

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
                if(set.Remove(item))
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


        private void Update()
        {
            if (hashSetProcess == null || hashSetProcess.Count == 0) return;
            foreach (var item in ProcessList)
            {
                item.Update();
			}
        }
    }
}