using UnityEngine;


namespace GameUI
{
	public abstract class CardGroupPanelComponent<T> : PanelGroupComponent<T>, IShowHideAsync
		where T : CardItemPanelComponent
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
		public override void Add(T item)
		{
			if (item.IsNullRef()) return;
			base.Add(item);
			if(item is ISetTargetPanel setTargetPanel)
				setTargetPanel.OnUpdateUI();
		}
		public override void Insert(int index, T item)
		{
			if (item.IsNullRef()) return;
			base.Insert(index, item);
			if (item is ISetTargetPanel setTargetPanel)
				setTargetPanel.OnUpdateUI();
		}
		public override bool Remove(T item)
		{
			if (item.IsNullRef()) return false;
			bool result = base.Remove(item);
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