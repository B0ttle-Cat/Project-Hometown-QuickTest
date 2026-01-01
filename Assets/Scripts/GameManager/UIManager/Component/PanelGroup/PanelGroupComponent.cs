using System;
using System.Collections;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;

namespace GameUI
{
	[RequireComponent(typeof(RectTransform))]
	public abstract class PanelGroupComponent : PanelItemComponent, IShowHideAsync
	{
		public abstract void Add(IPanelItem item);
		public abstract void Insert(int index, IPanelItem item);
		public abstract bool Remove(IPanelItem item);
		public abstract bool Contains(IPanelItem item);
		public abstract void AddItem(IObjectForPanel item, bool addLast = true);
		public abstract bool RemoveItem(IObjectForPanel item);
	}
	public abstract class PanelGroupComponent<T> : PanelGroupComponent, IPanelGroup<T>, IShowHideAsync
		where T : class , IPanelItem
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
		sealed public override void Insert(int index, IPanelItem item) => Insert(index, item as T);
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
		sealed public override void Add(IPanelItem item) => Add(item as T);
		sealed public override bool Remove(IPanelItem item) => Remove(item as T);
		public virtual bool Remove(T item)
		{
			if (Items.Remove(item))
			{
				if (!PoolingCardObject)
				{
					GameObject.Destroy(item.ThisRect.gameObject);
				}
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
				var item = this[i];
				var itemRect = item.ThisRect;

				if (item is IDisposable disposable) disposable.Dispose();

				if (itemRect.IsNotNullRef())
				{
					GameObject.Destroy(itemRect.gameObject);
				}
			}
			if (PoolingCardObject)
			{
				ClearPoolStack();
			}
			Items.Clear();
		}
		sealed public override bool Contains(IPanelItem item) => Contains(item as T);
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

		#region PoolingDataContainer

		protected bool PoolingCardObject
			=> PoolingData.IsNotNullRef()
			&& PanelPrefab.IsNotNullRef()
			&& PanelPrefab is IPanelSetObject and T;

		[SerializeField]
		private PoolingDataContainer poolingDataContainer;
		[SerializeField]
		protected PanelItemComponent PanelPrefab;
		[SerializeField]
		protected RectTransform PanelParent;
		public PoolingDataContainer PoolingData
		{
			get
			{
				if (poolingDataContainer.IsNullRef())
				{
					TryGetComponent<PoolingDataContainer>(out poolingDataContainer);
				}
				return poolingDataContainer;
			}
		}

		[ShowInInspector, ShowIf("PoolingCardObject")]
		[HideInEditorMode]
		protected Stack<PanelItemComponent> PoolStack;

		protected virtual void InitObjects(IEnumerable<IObjectForPanel> cardElements)
		{
			Clear();

			if (PoolingCardObject)
			{
				PoolingData.InitData(cardElements);
				return;
			}

			foreach (var item in cardElements)
			{
				if (item.IsNullRef()) continue;
				AddItem(item);
			}
		}
		public void AddObject(IObjectForPanel item)
		{
			if (PoolingCardObject)
			{
				PoolingData.AddData(item);
				return;
			}
		}
		public void RemoveObject(IObjectForPanel item)
		{
			if (PoolingCardObject)
			{
				PoolingData.RemoveData(item);
				return;
			}
		}
		public override void AddItem(IObjectForPanel item, bool addLast = true)
		{
			if (!PoolingCardObject) return;
			if (item.IsNullRef() || Contains(item)) return;

			while (PoolStack != null && PoolStack.Count > 0 && PoolStack.TryPop(out PanelItemComponent pop))
			{

				if (pop.IsNullRef()) continue;
				if (pop is not T tPanel) return;
				if (pop.ThisPanel.IsNullRef()) continue;
				var thisRect = pop.ThisPanel.ThisRect;
				if (thisRect.IsNullRef()) continue;
				if (addLast) thisRect.transform.SetAsLastSibling();
				else thisRect.transform.SetAsFirstSibling();
				if (pop is IPanelSetObject setPop && setPop.SetTarget(item))
				{
					Add(tPanel);
				}
				else
				{
					Remove(tPanel);
				}
				return;
			}

			var newPanel = Instantiate(PanelPrefab, PanelParent.IsNullRef() ? transform : PanelParent);
			if (newPanel.IsNullRef()) return;

			if (newPanel is not T tNewPanel)
			{
				GameObject.Destroy(newPanel.gameObject);
				return;
			}

			var isInit =  SetPanelObject(tNewPanel, item);
			if (!isInit)
			{
				GameObject.Destroy(newPanel.gameObject);
				return;
			}

			if (addLast)
			{
				newPanel.transform.SetAsLastSibling();
				Add(tNewPanel);
			}
			else
			{
				newPanel.transform.SetAsFirstSibling();
				Insert(0, tNewPanel);
			}
		}
		public override bool RemoveItem(IObjectForPanel item)
		{
			if (!PoolingCardObject) return false;
			if (item.IsNullRef()) return false;

			int length = Count;
			for (int i = 0 ; i < length ; i++)
			{
				if (Contains(item))
				{
					var panel = this[i];
					if(panel is PanelItemComponent panelComponent)
					{
						if (Remove(panel))
						{
							PoolStack ??= new Stack<PanelItemComponent>();
							PoolStack.Push(panelComponent);
						}
						else
						{
							GameObject.Destroy(panelComponent.gameObject);	
						}
					}
					else 
					{
						Remove(panel);
					}
				}
			}
			return false;
		}
		protected virtual bool SetPanelObject(T newPanel, IObjectForPanel item)
		{
			if (newPanel is IPanelSetObject setPanel)
			{
				return setPanel.SetTarget(item);
			}
			return false;
		}
		public virtual bool Contains(IObjectForPanel item)
		{
			int length = Count;
			for (int i = 0 ; i < length ; i++)
			{
				var panel = this[i];
				if (panel.IsNullRef()) continue;
				if (panel is not IPanelSetObject setPanel) continue;
				if (setPanel.Target == item)
				{
					return true;
				}
			}
			return false;
		}

		private void ClearPoolStack()
		{
			if (PoolStack == null) return;
			foreach (var item in PoolStack)
			{
				if (item.IsNullRef()) continue;
				GameObject.Destroy(item.gameObject);
			}
			PoolStack.Clear();
			if(poolingDataContainer != null)
			{
				poolingDataContainer.ClearData();
			}
		}
		#endregion
	}
}
