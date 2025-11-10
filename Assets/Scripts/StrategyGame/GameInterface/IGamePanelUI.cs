using System;
using System.Collections.Generic;

using UnityEngine;

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
	void Show();
	void Hide();
}
public interface IViewItemUI : IDisposable
{
	void Visible();
	void Unvisible();
}
public interface IPanelFloating
{
	FloatingPanelItemUI FloatingPanelUI { get; }
	void FloatingUpdate()
	{
		if (FloatingPanelUI == null) return;
		FloatingPanelUI.ForceUpdateThisFrame();
	}
	void AddTarget(IStrategyElement element)
	{
		if (FloatingPanelUI == null) return;
		FloatingPanelUI.SetTargetInMap(element == null ? null : element as Component);
	}
	void RemoveTarget(IStrategyElement element)
	{
		if (FloatingPanelUI == null) return;
		FloatingPanelUI.RemoveTargetInMap(element == null ? null : element as Component);
	}
	void ClearTarget()
	{
		if (FloatingPanelUI == null) return;
		FloatingPanelUI.RemoveTargetInMap();
	}
}
