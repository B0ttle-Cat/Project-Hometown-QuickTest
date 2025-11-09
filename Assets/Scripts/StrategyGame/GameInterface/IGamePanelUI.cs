using System;
using System.Collections.Generic;

public interface IGamePanelUI
{
	public void OpenUI();
	public void CloseUI();

}
public interface IViewStack
{
	IViewStack ViewStack { get; }
	Stack<IViewPanelUI> ViewPanelUIStack { get; set; }
	void Push(IViewPanelUI push)
	{
		if (ViewPanelUIStack == null)
			ViewPanelUIStack = new Stack<IViewPanelUI>();

		if (push == null) return;

		if (ViewPanelUIStack.TryPeek(out var _push))
		{
			_push?.Hide();
		}
		ViewPanelUIStack.Push(push);
		push.Show();
	}
	void Pop(IViewPanelUI pop = null)
	{
		if (ViewPanelUIStack == null) return;

		while (ViewPanelUIStack.TryPop(out var _pop) && _pop != null)
		{
			_pop.Hide();
			_pop.Dispose();
			if (pop != null && _pop != pop)
			{
				continue;
			}
			else if (ViewPanelUIStack.TryPeek(out var peek) && peek != null)
			{
				peek.Show();
				return;
			}
		}
	}
	void ClearViewStack()
	{
		if (ViewPanelUIStack == null) return;

		foreach (var item in ViewPanelUIStack)
		{
			if (item == null) continue;
			item.Hide();
			item.Dispose();
		}
		ViewPanelUIStack.Clear();
	}
}
public interface IViewPanelUI : IDisposable
{
	public void Show();
	public void Hide();
}
public interface IViewItemUI : IDisposable
{
	public void Visible();
	public void Unvisible();
}
public interface IPanelFloating
{
	public void AddTarget(IStrategyElement element);
	public void RemoveTarget(IStrategyElement element);
	public void ClearTarget();
}
