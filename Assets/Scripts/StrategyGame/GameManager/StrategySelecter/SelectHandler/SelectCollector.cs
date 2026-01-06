using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace StrategyManagerModule
{
	public abstract class SelectCollector : MonoBehaviour
	{
		public abstract void OnInit(StrategySelecter selecter);
		public abstract void OnDeinit();
		public abstract bool OnSelected(ISelectable selectable);
		public abstract bool OnDeselected(ISelectable selectable);
		public abstract bool OnPointing(ISelectable selectable);
	}



	public abstract class SelectCollector<T> : SelectCollector , IList<T> where T : class , ISelectable
	{
		protected StrategySelecter ThisSelecter { get; private set; }
		public List<T> Items { get; private set; }

		sealed public override void OnInit(StrategySelecter selecter)
		{
			ThisSelecter = selecter;
			Items ??= new List<T>();
			Items.Clear();
			Init();
		}
		sealed public override void OnDeinit()
		{
			Deinit();
			Items?.Clear();
			Items = null;
			ThisSelecter = null;
		}
		protected abstract void Init();
		protected abstract void Deinit();
		sealed public override bool OnSelected(ISelectable selectable)
		{
			if (selectable is T selectItem)
			{
				try
				{
					Items.Add(selectItem);
					OnSelected(selectItem);
				}
				catch
				{
					return false;

				}
				return true;
			}
			return false;
		}
		sealed public override bool OnDeselected(ISelectable selectable)
		{
			if (selectable is T selectItem)
			{
				try
				{
					if (Items.Remove(selectItem))
					{
						OnDeselected(selectItem);
					}
				}
				catch
				{
					return false;

				}
				return true;
			}
			return false;
		}
		sealed public override bool OnPointing(ISelectable selectable)
		{
			if (selectable is T selectItem)
			{
				try
				{
					OnPointing(selectItem);
				}
				catch
				{
					return false;

				}
				return true;
			}
			return false;
		}
		protected abstract void OnSelected(T selectItem);
		protected abstract void OnDeselected(T selectItem);
		protected abstract void OnPointing(T selectItem);



		public int Count => Items == null ? 0 : Items.Count;
		public bool IsReadOnly => false;
		public T this[int index]
		{
			get => Items[index];
			set => Items[index] = value;
		}
		public void Add(T item) => Items.Add(item);
		public void Clear() => Items.Clear();
		public bool Contains(T item) => Items.Contains(item);
		public void CopyTo(T[] array, int arrayIndex) => Items.CopyTo(array, arrayIndex);
		public bool Remove(T item) => Items.Remove(item);
		public int IndexOf(T item) => Items.IndexOf(item);
		public void Insert(int index, T item) => Items.Insert(index, item);
		public void RemoveAt(int index) => Items.RemoveAt(index);
		public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}