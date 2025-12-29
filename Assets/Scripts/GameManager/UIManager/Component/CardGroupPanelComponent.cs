using System;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;


namespace GameUI
{


	public abstract class CardGroupPanelComponent : PanelGroupComponent<CardGroupPanelComponent.CardPanel>, IShowHideAsync
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

		protected void AllHideAndClear()
		{
			int length = Count;
			for (int i = 0 ; i < length ; i++)
			{
				this[i].ThisShowHide.OnHide();
			}
			Clear();
		}

		protected void AllShow()
		{
			int length = Count;
			for (int i = 0 ; i < length ; i++)
			{
				this[i].ThisShowHide.OnShow();
			}
		}



		[Serializable]
		public abstract class CardPanel : IPanelItem, IShowHide, IDisposable
		{
			public abstract GameUIController RootUI { get; }
			public abstract IPanelItem ThisPanel { get; }
			public abstract RectTransform ThisRect { get; }
			public abstract IShowHide ThisShowHide { get; }
			public abstract bool IsShow { get; set; }
			public abstract void Dispose();
			public abstract void OnRelease();
			public abstract void OnAttach();
			public abstract void OnUpdateUI();
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
            [ShowInInspector]
            public T Item { get ; private set; }

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
				if (panelItem.IsNullRef()) return;

				ThisShowHide.UnpairingShowHide();
				OnClear();

				if (ThisPanel.ThisRect.IsNotNullRef())
				{
					Destroy(ThisPanel.ThisRect.gameObject);
				}
				panelItem = null;
				Item = null;
			}
			public void OnUpdateUI(T item)
			{
				if (this.Item == item)
				{
					OnUpdateUI();
					return;
				}
				else
				{
					if (this.Item.IsNotNullRef())
					{
						OnRelease();
					}
					this.Item = item;
					if (this.Item.IsNotNullRef())
					{
						OnAttach();
					}
					OnUpdateUI();
				}
			}
			public override void OnRelease()
			{
				if (Item.IsNullRef())
				{
					Item = null;
					return;
				}
				ReleaseUI();
				Item = null;
			}
			public override void OnAttach()
			{
				if (Item.IsNullRef())
				{
					Item = null;
					return;
				}
				AttachUI();
			}
			public override void OnUpdateUI()
			{
				if (Item.IsNullRef())
				{
					OnClear();
					return;
				}
				UpdateUI();
			}
			public override void OnClear()
			{
				if (Item.IsNullRef())
				{
					ClearUI();
				}
			}
			internal bool Contains(T item)
			{
				return this.Item == item;
			}
			protected abstract void ReleaseUI();
			protected abstract void AttachUI();
			protected abstract void UpdateUI();
			protected abstract void ClearUI();
		}
		protected abstract CardPanel CardFactory<T>(GameObject newUIObject, T item) where T : class, ICardUIObject;
		
		protected void InitCardList<T>(IEnumerable<ICardUIObject> cardElements) where T : class, ICardUIObject
		{
			Clear();

			if (PoolingData.IsNotNullRef())
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
			if (PoolingData.IsNotNullRef())
			{
				PoolingData.AddData<T>(item);
				return;
			}
			AddItem<T>(item);
		}
		public virtual void RemovePoolData<T>(T item) where T : class, ICardUIObject
		{
			if (PoolingData.IsNotNullRef())
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

		public override void Add(CardPanel item)
		{
			if (item == null) return;
			Items.Add(item);
			item.OnUpdateUI();
		}
		public override void Insert(int index, CardPanel item)
		{
			if (item == null) return;
			Items.Insert(index, item);
			item.OnUpdateUI();
		}
		public override void Clear()
		{
			int length = Count;
			for (int i = 0 ; i < length ; i++)
			{
				Remove(this[i]);
			}
			if (PoolStack != null) PoolStack.Clear();
			Items.Clear();
		}
		public override bool Remove(CardPanel item)
		{
			if (Items.Remove(item))
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
				var panel = this[i];
				if (panel is not CardPanel<T> tPanel) continue;
				if(tPanel.Item == item)
				{
					return true;
				}
			}
			return false;
		}

		public Rect GetCardRect()
		{
			if (CardPrefab.IsNullRef()) return default;
			return CardPrefab.rect;
		}
	}
}