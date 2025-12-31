using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;


namespace GameUI
{
	public abstract class CardGroupPanelComponent : PanelGroupComponent<CardItemPanelComponent>, IShowHideAsync
	{
		[SerializeField, Required]
		protected CardItemPanelComponent CardPrefab;
		[SerializeField]
		protected RectTransform ContentParent;

		public Rect GetCardRect()
		{
			if (CardPrefab.IsNullRef()) return default;
			return CardPrefab.ThisPanel.ThisRect.rect;
		}

		public abstract void AddItem(ICardUIObject item, bool addLast = true);
		public abstract bool RemoveItem(ICardUIObject item);
	}

	public abstract class CardGroupPanelComponent<T> : CardGroupPanelComponent, IShowHideAsync
		where T : class, ICardUIObject
	{

		protected bool PoolingCardObject => PoolingData.IsNotNullRef();
		[SerializeField]
		private PoolingScrollDataContainer poolingScrollDataContainer;
		public PoolingScrollDataContainer PoolingData
		{
			get
			{
				if (poolingScrollDataContainer.IsNullRef())
				{
					TryGetComponent<PoolingScrollDataContainer>(out poolingScrollDataContainer);
				}
				return poolingScrollDataContainer;
			}
		}

		[ShowInInspector, ShowIf("PoolingCardObject")]
		[HideInEditorMode]
		protected Stack<CardItemPanelComponent> PoolStack;

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

		protected virtual bool SetCardTarget(CardItemPanelComponent<T> newCardComponent, T item)
		{
			return newCardComponent.SetTarget(item);
		}

		protected void InitCardList(IEnumerable<ICardUIObject> cardElements)
		{
			Clear();

			if (PoolingCardObject)
			{
				PoolingData.SetData<T>(cardElements);
				return;
			}

			foreach (var card in cardElements)
			{
				if (card is not T item) continue;
				AddItem(item);
			}
		}
		public virtual void AddPoolData(T item)
		{
			if (PoolingCardObject)
			{
				PoolingData.AddData<T>(item);
				return;
			}
			AddItem(item);
		}
		public virtual void RemovePoolData(T item)
		{
			if (PoolingCardObject)
			{
				PoolingData.RemoveData<T>(item);
				return;
			}
			RemoveItem(item);
		}
		sealed public override void AddItem(ICardUIObject item, bool addLast = true)
		{
			if (item.IsNullRef()) return;
			if (item is not T tItem) return;
			if (CardPrefab.IsNullRef()) return;
			if (Contains(tItem)) return;

			if (PoolingCardObject)
			{
				if (PoolStack == null) PoolStack = new Stack<CardItemPanelComponent>();
				while (PoolStack.TryPop(out var pop))
				{
					if (pop.IsNullRef()) continue;
					if (pop.ThisPanel.IsNullRef()) continue;
					var thisRect = pop.ThisPanel.ThisRect;
					if (thisRect.IsNullRef()) continue;
					if (addLast) thisRect.transform.SetAsLastSibling();
					else thisRect.transform.SetAsFirstSibling();
					if (pop.SetTarget(tItem))
					{
						Add(pop);
					}
					else
					{
						Remove(pop);
					}
					return;
				}
			}
			var tComponent = CardPrefab as CardItemPanelComponent<T>;

			var newCardComponent = Instantiate<CardItemPanelComponent<T>>(tComponent, ContentParent.IsNullRef() ? transform : ContentParent);
			if (newCardComponent.IsNullRef()) return;

			var isInit =  SetCardTarget(newCardComponent, tItem);
			if (!isInit)
			{
				GameObject.Destroy(newCardComponent.gameObject);
				return;
			}

			if (addLast)
			{
				newCardComponent.transform.SetAsLastSibling();
				Add(newCardComponent);
			}
			else
			{
				newCardComponent.transform.SetAsFirstSibling();
				Insert(0, newCardComponent);
			}
		}
		sealed public override bool RemoveItem(ICardUIObject item)
		{
			if (item.IsNullRef()) return false;
			if (item is not T tItem) return false;

			int length = Count;
			for (int i = 0 ; i < length ; i++)
			{
				if (Contains(tItem))
				{
					return Remove(this[i]);
				}
			}
			return false;
		}
		public override void Add(CardItemPanelComponent item)
		{
			if (item == null) return;
			Items.Add(item);
			item.OnUpdateUI();
		}
		public override void Insert(int index, CardItemPanelComponent item)
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
			Items.Clear();
		}
		public override bool Remove(CardItemPanelComponent item)
		{
			if (Items.Remove(item))
			{
				if (item.IsNotNullRef())
				{
					if (PoolingCardObject && PoolStack != null)
					{
						PoolStack.Push(item);
					}
					else
					{
						GameObject.Destroy(item.gameObject);
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
		public virtual bool Contains(T item)
		{
			int length = Count;
			for (int i = 0 ; i < length ; i++)
			{
				var panel = this[i];
				if (panel is not CardItemPanelComponent<T> tPanel) continue;
				if (tPanel.Item == item)
				{
					return true;
				}
			}
			return false;
		}
	}
}