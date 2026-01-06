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
		public List<T> SelectedItems { get; private set; }

		sealed public override void OnInit(StrategySelecter selecter)
		{
			ThisSelecter = selecter;
			SelectedItems ??= new List<T>();
			SelectedItems.Clear();
			Init();
		}
		sealed public override void OnDeinit()
		{
			Deinit();
			SelectedItems?.Clear();
			SelectedItems = null;
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
					SelectedItems.Add(selectItem);
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
					SelectedItems.Remove(selectItem);
					OnDeselected(selectItem);
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



		public int Count => SelectedItems.Count;
		public bool IsReadOnly => false;
		public T this[int index]
		{
			get => SelectedItems[index];
			set => SelectedItems[index] = value;
		}
		public void Add(T item) => SelectedItems.Add(item);
		public void Clear() => SelectedItems.Clear();
		public bool Contains(T item) => SelectedItems.Contains(item);
		public void CopyTo(T[] array, int arrayIndex) => SelectedItems.CopyTo(array, arrayIndex);
		public bool Remove(T item) => SelectedItems.Remove(item);
		public int IndexOf(T item) => SelectedItems.IndexOf(item);
		public void Insert(int index, T item) => SelectedItems.Insert(index, item);
		public void RemoveAt(int index) => SelectedItems.RemoveAt(index);
		public IEnumerator<T> GetEnumerator() => SelectedItems.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}