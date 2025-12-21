using System;
using System.Collections.Generic;

namespace GameUI
{
	public interface IShowStackController : IDisposable
	{
		IShowStackController StackController { get; }
		GroupShowStack ShowStack { get; }

		public IShowStackController Clear()
		{
			if (StackController.IsNullRef() || ShowStack == null) return StackController;
			ShowStack.Clear();
			return StackController;
		}
		public IShowStackController Push<T>(T item, bool newGroup = false) where T : IPanelItem
		{
			if (StackController.IsNullRef() || ShowStack == null) return StackController;
			ShowStack.Push(item, newGroup);
			return StackController;
		}
		public IShowStackController PopGroup()
		{
			if (StackController.IsNullRef() || ShowStack == null) return StackController;
			ShowStack.PopGroup();
			return StackController;
		}
		public IShowStackController Pop<T>(Func<T, bool> condition, out T item, bool canPopGroup = true) where T : class, IPanelItem
		{
			if (StackController.IsNullRef() || ShowStack == null)
			{
				item = null;
				return StackController;
			}
			if (!ShowStack.TryPop(condition, out item, canPopGroup))
			{
				item = null;
			}
			return StackController;
		}
		public IShowStackController Pop<T>(out T item) where T : class, IPanelItem => Pop<T>(null, out item);
		public IShowStackController Pop(out IPanelItem item) => Pop<IPanelItem>(out item);

		public class GroupShowStack
		{
			private readonly Stack<ItemShowStack> showStack;
			public GroupShowStack()
			{
				showStack = new Stack<ItemShowStack>();
			}
			public void Clear()
			{
				if (showStack == null || showStack.Count == 0) return;
				foreach (var stack in showStack)
				{
					stack.Clear();
				}
				showStack.Clear();
			}
			public void Push<T>(T item, bool newGroup = false) where T : IPanelItem
			{
				if (!showStack.TryPeek(out var itemStack))
				{
					newGroup = true;
				}

				if (newGroup)
				{
					itemStack.Push(item);
				}
				else
				{
					itemStack?.AllHide();

					itemStack = new ItemShowStack();
					itemStack.Push(item);
					showStack.Push(itemStack);
				}
			}
			public void PopGroup()
			{
				if (showStack.TryPop(out var group))
				{
					group.Clear();
				}
				if (showStack.TryPeek(out group))
				{
					group.AllShow();
				}
				return;
			}
			public bool TryPop<T>(Func<T, bool> condition, out T item, bool canPopGroup = true) where T : class, IPanelItem
			{
				while (showStack.TryPeek(out var group))
				{
					if (group.TryPop<T>(condition, out item))
					{
						return true;
					}

					if (!canPopGroup) break;
					if (showStack.TryPop(out group))
					{
						group.Clear();
					}
					if (showStack.TryPeek(out group))
					{
						group.AllShow();
					}
				}
				item = null;
				return false;
			}
		}
		public class ItemShowStack
		{
			private readonly Stack<IPanelItem> showStack;

			public ItemShowStack()
			{
				showStack = new Stack<IPanelItem>();
			}

			public void Push<T>(T item) where T : IPanelItem
			{
				showStack.Push(item);
				ItemShow(item);
			}
			public bool TryPop<T>(Func<T, bool> condition, out T item) where T : IPanelItem
			{
				while (showStack.TryPop(out var pop))
				{
					if (pop.IsNullRef()) continue;
					if (pop is T t && (condition == null || condition.Invoke(t)))
					{
						item = t;
						ItemHide(item);
						return true;
					}
				}
				item = default;
				return false;
			}
			public bool TryPeek(out IPanelItem item)
			{
				while (showStack.TryPeek(out item))
				{
					if (item.IsNullRef())
					{
						showStack.Pop();
						continue;
					}
					return true;
				}

				item = default;
				return false;
			}
			public void Clear()
			{
				if (showStack == null || showStack.Count == 0) return;

				AllHide();
				showStack.Clear();
			}
			public void ItemShow(IPanelItem item)
			{
				if (item.IsNullRef()) return;
				if (item is IShowHide showHide)
				{
					if (item is IAsyncShowHide asyncShowHide)
					{
						asyncShowHide.OnShowAsync();
					}
					else
					{
						showHide.OnShow();
					}
				}
			}
			public void ItemHide(IPanelItem item)
			{
				if (item.IsNullRef()) return;
				if (item is IShowHide showHide)
				{
					if (item is IAsyncShowHide asyncShowHide)
					{
						asyncShowHide.OnHideAsync();
					}
					else
					{
						showHide.OnHide();
					}
				}
			}
			public void AllShow()
			{
				if (showStack.TryPop(out var item))
				{
					ItemShow(item);
				}
			}
			public void AllHide()
			{
				if (showStack.TryPop(out var item))
				{
					ItemHide(item);
				}
			}
		}
	}
}
