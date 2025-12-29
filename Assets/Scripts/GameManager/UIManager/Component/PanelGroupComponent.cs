using System;
using System.Collections;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;

namespace GameUI
{
	[RequireComponent(typeof(RectTransform))]
	public abstract class PanelGroupComponent<T> : PanelItemComponent, IPanelGroup<T>, IShowHideAsync
		where T : IPanelItem, IDisposable
	{

		#region IPanelGroup<T>

		[HideIf("@true")]
		private List<T> items;
		[ShowInInspector]
		protected List<T> Items
		{
			get { if (items == null) New(); return items; }
			set { items = value; }
		}

		public int Count => items == null ? 0 : items.Count;

		public bool IsReadOnly => ((ICollection<T>)Items).IsReadOnly;

		public virtual T this[int index] { get => Items[index]; set => Items[index] = value; }
		public int IndexOf(T item)
		{
			return Items.IndexOf(item);
		}

		public virtual void Insert(int index, T item)
		{
			Items.Insert(index, item);
		}

		public virtual void RemoveAt(int index)
		{
			if (index >= 0 && index < Count)
			{
				Remove(this[index]);
			}
		}
		public virtual bool Remove(T item)
		{
			if (Items.Remove(item))
			{
				item.Dispose();
				return true;
			}
			return false;
		}
		public virtual void Add(T item)
		{
			Items.Add(item);
		}

		public virtual void New()
		{
			if (items == null) items = new List<T>();
			else items.Clear();
		}
		public virtual void Clear()
		{
			if (items == null) return;
			int length = Count;
			for (int i = 0 ; i < length ; i++)
			{
				this[i].Dispose();
			}
			Items.Clear();
		}
		public virtual bool Contains(T item)
		{
			return Items.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			Items.CopyTo(array, arrayIndex);
		}

		public IEnumerator<T> GetEnumerator()
		{
			return Items.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return Items.GetEnumerator();
		}
		#endregion

		protected virtual void Reset()
		{
			items = new List<T>();
		}
		protected override void Awake()
		{
			items ??= new List<T>();
			base.Awake();
		}
		protected override void OnDestroy()
		{
			Clear();
			base.OnDestroy();
		}
	}
}
