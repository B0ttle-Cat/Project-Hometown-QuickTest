using System;

using Shapes;

using Sirenix.OdinInspector;

using UnityEngine;

public abstract class SelectVisualizer : ImmediateModeShapeDrawer, ISelectVisualizer
{
	private ISelectable thisSelectable;
	public ISelectable ThisSelectable => thisSelectable;
	public ISelectVisualizer ThisSelectVisualizer => this;

	[Flags]
	protected enum VisualizerState
	{
		None = 0,
		PointUp = 1 << 0,
		Select = 1 << 1,
		Blink = 1 << 2,
	}
	[ShowInInspector, ReadOnly, EnumToggleButtons]
	protected VisualizerState visualizerState { get; private set; }

	public virtual void OnInit(ISelectable target)
	{
		thisSelectable = target;
		visualizerState = VisualizerState.None;
	}
	public virtual void Deinit()
	{
		thisSelectable = null;
		visualizerState = VisualizerState.None;
	}
	[ButtonGroup("Visualizer"), Button("OnPointEnter")]
	void ISelectVisualizer.OnPointEnter()
	{
		visualizerState |= VisualizerState.PointUp;
	}
	[ButtonGroup("Visualizer"), Button("OnPointExit")]

	void ISelectVisualizer.OnPointExit()
	{
		visualizerState &= ~VisualizerState.PointUp;
	}
	[ButtonGroup("Visualizer"), Button("OnSelect")]
	void ISelectVisualizer.OnSelect()
	{
		visualizerState |= VisualizerState.Select;
	}
	[ButtonGroup("Visualizer"), Button("OnDeselect")]
	void ISelectVisualizer.OnDeselect()
	{
		visualizerState &= ~VisualizerState.Select;
	}
	[ButtonGroup("OtherButton"), Button("Blink")]
	async void ISelectVisualizer.OnPointing()
	{
		visualizerState |= VisualizerState.Blink;
		await OnBlinkShapes();
		visualizerState &= ~VisualizerState.Blink;
	}
	public void LateUpdate()
	{
		if(thisSelectable.IsNotNullRef()) OnComputeShapes();
	}


	public sealed override void DrawShapes(Camera cam)
	{
		if (cam != StrategyManager.MainCamera) return;
		if (visualizerState != VisualizerState.None)
		{
			if (thisSelectable.IsNotNullRef()) OnDrawShapes(cam);
		}
	}
	protected abstract void EnterShowState();
	protected abstract void OnComputeShapes();
	protected abstract Awaitable OnBlinkShapes();
	protected abstract void OnDrawShapes(Camera cam);
}
