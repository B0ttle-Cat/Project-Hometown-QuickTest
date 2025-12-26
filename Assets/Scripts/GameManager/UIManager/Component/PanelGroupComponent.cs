using System.Collections;
using System.Collections.Generic;
using System.Threading;

using Sirenix.OdinInspector;

using UnityEngine;

namespace GameUI
{
	[RequireComponent(typeof(RectTransform))]
	public abstract class PanelGroupComponent<T> : MonoBehaviour, IPanelGroup<T>, IShowHideAsync where T : IPanelItem
	{
		#region IPanelGroup<T>
		private RectTransform rectTransform;
		public abstract IPanelGroup<T> ThisPanel { get; }
		public virtual RectTransform ThisRect
		{
			get
			{
				if (rectTransform.IsNullRef())
					rectTransform = GetComponent<RectTransform>();
				return rectTransform;
			}
		}
		IPanelItem IPanelItem.ThisPanel => ThisPanel;

		protected List<T> items;
		protected virtual List<T> Items
		{
			get { return items ??= new List<T>(); }
			set { items = value; }
		}
		protected virtual void Reset()
		{
			items = new List<T>();
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
			Items.RemoveAt(index);
		}

		public virtual void Add(T item)
		{
			Items.Add(item);
		}

		public virtual void Clear()
		{
			if (items == null) return;
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

		public virtual bool Remove(T item)
		{
			return Items.Remove(item);
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


		#region IShowHideAsync
		public abstract IShowHideAsync ThisShowHide { get; }
		IShowHide IShowHide.ThisShowHide => ThisShowHide;
		CancellationTokenSource IShowHideAsync.ShowHideCancellationTokenSource { get; set; }
		[SerializeField, PropertyOrder(-10000) , ReadOnly]
		private bool isShow = false;
		bool IShowHide.IsShow { get => isShow; set => isShow = value; }

		async Awaitable IShowHideAsync.Show(CancellationToken cancellationToken) => await Show(cancellationToken);
		async Awaitable IShowHideAsync.Hide(CancellationToken cancellationToken) => await Hide(cancellationToken);
		void IShowHide.Show() => Show();
		void IShowHide.Hide() => Hide();
		protected abstract void Show();
		protected abstract void Hide();
		protected virtual async Awaitable Show(CancellationToken cancellationToken) { return; }
		protected virtual async Awaitable Hide(CancellationToken cancellationToken) { return; }
		#endregion
	}
}
