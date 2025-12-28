using System;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;


namespace GameUI
{


	public abstract class CardGroupPanelComponent : PanelGroupComponent<CardGroupPanelComponent.CardPanel>
	{
		[SerializeField, Required]
		protected RectTransform CardPrefab;
		[SerializeField]
		protected RectTransform ContentParent;

		[SerializeField, ToggleGroup("poolingCardObject")]
		private bool poolingCardObject;
		[SerializeField, ToggleGroup("poolingCardObject")]
		private PoolingScrollDataContainer poolingScrollDataContainer;
		public PoolingScrollDataContainer PoolingData
		{
			get
			{
				if (!poolingCardObject) return null;
				if(poolingScrollDataContainer.IsNullRef())
				{
					TryGetComponent<PoolingScrollDataContainer>(out poolingScrollDataContainer);
				}
				return poolingScrollDataContainer;
			}
		}

		[ShowInInspector, ToggleGroup("poolingCardObject")]
		[HideInEditorMode]
		protected Stack<CardPanel> PoolStack;

		[Serializable]
		public abstract class CardPanel : IPanelItem, IShowHide, IDisposable
		{
			public abstract GameUIController RootUI { get; }
			public abstract IPanelItem ThisPanel { get; }
			public abstract RectTransform ThisRect { get; }
			public abstract IShowHide ThisShowHide { get; }
			public abstract bool IsShow { get; set; }

			public abstract void Hide();
			public abstract void Show();
			public abstract void Dispose();
			public abstract void OnRelease();
			public abstract void OnAttach();
			public abstract void OnChange();
			public abstract void OnClear();
		}
		public abstract class CardPanel<T> : CardPanel, IPanelItem, IShowHide, IDisposable
			where T : class, ICardUIObject
		{
			protected IPanelItem panelItem;
			public override IPanelItem ThisPanel => panelItem;
			public override RectTransform ThisRect => panelItem.ThisRect;
			public override GameUIController RootUI => panelItem.RootUI;
			public override IShowHide ThisShowHide => this;
			public override bool IsShow { get; set; }
			[SerializeField, ReadOnly]
			protected T item;
			public CardPanel(GameObject thisObject, T item = null)
			{
				panelItem = thisObject.GetComponent<IPanelItem>();
				ThisShowHide.PairingShowHide();
				OnUpdateUI(item);
			}
			public CardPanel(RectTransform thisRect, T item = null)
			{
				panelItem = thisRect.gameObject.GetComponent<IPanelItem>();
				ThisShowHide.PairingShowHide();
				OnUpdateUI(item);
			}
			public CardPanel(IShowHide showHide, T item = null)
			{
				this.panelItem = showHide;
				ThisShowHide.PairingShowHide();
				OnUpdateUI(item);
			}
			public override void Dispose()
			{
				ThisShowHide.UnpairingShowHide();
				OnClear();

				if (ThisPanel.ThisRect.IsNotNullRef())
				{
					Destroy(ThisPanel.ThisRect.gameObject);
				}
				panelItem = null;
				item = null;
			}
			public override void Hide()
			{
			}
			public override void Show()
			{
			}
			public void OnUpdateUI(T item)
			{
				if (this.item == item)
				{
					OnChange();
					return;
				}
				else
				{
					if (this.item.IsNotNullRef())
					{
						OnRelease();
					}
					this.item = item;
					if (this.item.IsNotNullRef())
					{
						OnAttach();
					}
					OnChange();
				}
			}
			public override void OnRelease()
			{
				if (item.IsNullRef())
				{
					item = null;
					return;
				}
				ReleaseUI();
				item = null;
			}
			public override void OnAttach()
			{
				if (item.IsNullRef())
				{
					item = null;
					return;
				}
				AttachUI();
			}
			public override void OnChange()
			{
				if (item.IsNullRef())
				{
					OnClear();
					return;
				}
				ChangeUI();
			}
			public override void OnClear()
			{
				if (item.IsNullRef())
				{
					ClearUI();
				}
			}
			internal bool Contains(T item)
			{
				return this.item == item;
			}
			protected abstract void ReleaseUI();
			protected abstract void AttachUI();
			protected abstract void ChangeUI();
			protected abstract void ClearUI();
		}
		protected abstract CardPanel CardFactory<T>(GameObject newUIObject, T item) where T : class, ICardUIObject;
		protected void InitCardList<T>(IEnumerable<ICardUIObject> cardElements) where T : class, ICardUIObject
		{
			Clear();

			if (PoolingData.IsNullRef())
			{
				PoolingData.SetData<T>(cardElements);
				return;
			}

			foreach (var card in cardElements)
			{
				if (card is not T item) continue;
				AddItem<T>(item);
			}
		}
		public virtual void AddPoolData<T>(T item) where T : class, ICardUIObject
		{
			if (PoolingData.IsNullRef())
			{
				PoolingData.AddData<T>(item);
				return;
			}
			AddItem<T>(item);
		}
		public virtual void RemovePoolData<T>(T item) where T : class, ICardUIObject
		{
			if (PoolingData.IsNullRef())
			{
				PoolingData.RemoveData<T>(item);
				return;
			}
			RemoveItem<T>(item);
		}

		public virtual void AddItem<T>(T item, bool addLast = true) where T : class, ICardUIObject
		{
			if (item.IsNullRef()) return;
			if (CardPrefab.IsNullRef()) return;
			if (Contains(item)) return;

			if (poolingCardObject)
			{
				if (PoolStack == null) PoolStack = new Stack<CardPanel>();
				while (PoolStack.TryPop(out var pop))
				{
					if (pop == null) continue;
					if (pop.ThisPanel.IsNullRef()) continue;
					var thisRect = pop.ThisPanel.ThisRect;
					if (thisRect.IsNullRef()) continue;
					if (addLast) thisRect.transform.SetAsLastSibling();
					else thisRect.transform.SetAsFirstSibling();
					Add(pop);
					return;
				}
			}
			var newUIObject = Instantiate(CardPrefab.gameObject, ContentParent.IsNullRef() ? transform : ContentParent);
			if (newUIObject.IsNullRef()) return;

			var newItem = CardFactory(newUIObject, item);
			if (newItem.IsNullRef())
			{
				GameObject.Destroy(newUIObject);
				return;
			}

			if (addLast)
			{
				newUIObject.transform.SetAsLastSibling();
				Add(CardFactory(newUIObject, item));
			}
			else
			{
				newUIObject.transform.SetAsFirstSibling();
				Insert(0, CardFactory(newUIObject, item));
			}

		}
		public override void Add(CardPanel item)
		{
			if (item == null) return;
			base.Add(item);
			item.OnChange();
		}
		public override void Insert(int index, CardPanel item)
		{
			if (item == null) return;
			base.Insert(index, item);
			item.OnChange();
		}
		public override void Clear()
		{
			int length = Count;
			for (int i = 0 ; i < length ; i++)
			{
				Remove(this[i]);
			}
			if (PoolStack != null) PoolStack.Clear();
			base.Clear();
		}
		public virtual bool RemoveItem<T>(T item) where T : class, ICardUIObject
		{
			int length = Count;
			for (int i = 0 ; i < length ; i++)
			{
				if (Contains<T>(item))
				{
					return Remove(this[i]);
				}
			}
			return false;
		}
		public override bool Remove(CardPanel item)
		{
			if (base.Remove(item))
			{
				if (item.IsNotNullRef())
				{
					if (poolingCardObject && PoolStack != null)
					{
						item.OnClear();
						PoolStack.Push(item);
					}
					else
					{
						item.Dispose();
					}
				}
				return true;
			}
			return false;
		}
		public override void RemoveAt(int index)
		{
			if (index >= 0 && index < Count)
			{
				Remove(this[index]);
			}
		}
		public virtual bool Contains<T>(T item) where T : class, ICardUIObject
		{
			int length = Count;
			for (int i = 0 ; i < length ; i++)
			{
				if (Contains<T>(item))
				{
					return true;
				}
			}
			return false;
		}
		public override bool Contains(CardPanel item)
		{
			return base.Contains(item);
		}

		public Rect GetCardRect()
		{
			if (CardPrefab.IsNullRef()) return default;
			return CardPrefab.rect;
		}
	}
}