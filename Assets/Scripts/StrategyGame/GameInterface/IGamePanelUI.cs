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
	Stack<IPanelItemUI> ViewPanelUIStack { get; set; }
	void Push(IPanelItemUI push)
	{
		if (ViewPanelUIStack == null)
			ViewPanelUIStack = new Stack<IPanelItemUI>();

		if (push == null) return;

		if (ViewPanelUIStack.TryPeek(out var _push))
		{
			_push?.Hide();
		}
		ViewPanelUIStack.Push(push);
		push.Show();
	}
	void Pop(IPanelItemUI pop = null)
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
public interface IPanelItemUI : IDisposable
{
	void Show();
	void Hide();
}
public interface IViewItemUI : IDisposable
{
	void Visible();
	void Invisible();
}
public interface IPanelTarget : IDisposable
{
	void AddTarget(IStrategyElement element);
	void RemoveTarget(IStrategyElement element);
	void ClearTarget();
}
public interface IPanelFloating
{
	FloatingPanelItemUI FloatingPanelUI { get; set; }
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
