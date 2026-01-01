using UnityEngine;


namespace GameUI
{
	public abstract class CardGroupPanelComponent : PanelGroupComponent<CardItemPanelComponent>, IShowHideAsync
	{
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
		public override void Add(CardItemPanelComponent item)
		{
			if (item.IsNullRef()) return;
			base.Add(item);
			item.OnUpdateUI();
		}
		public override void Insert(int index, CardItemPanelComponent item)
		{
			if (item.IsNullRef()) return;
			base.Insert(index, item);
			item.OnUpdateUI();
		}
		public override bool Remove(CardItemPanelComponent item)
		{
			if (item.IsNullRef()) return false;
			bool result = base.Remove(item);
			item.OnUpdateUI();
			return result;
		}
		public override void RemoveAt(int index)
		{
			if (index >= 0 && index < Count)
			{
				Remove(this[index]);
			}
		}
		public Rect GetCardRect()
		{
			if (PanelPrefab.IsNullRef()) return default;
			return PanelPrefab.ThisPanel.ThisRect.rect;
		}
	}
}