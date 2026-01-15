using UnityEngine;
public partial class UnitObject : ITargetBoundary
{
	private ITargetBoundary _boundaryComputer;

	partial void InitBoundary()
	{
		if (!TryGetComponent<BoundaryComputer>(out var boundaryComputer))
		{
			boundaryComputer = gameObject.AddComponent<BoundaryComputer>();
			boundaryComputer.IsEllipticWithOnScreen = true;
		}
		_boundaryComputer = boundaryComputer;
	}
	partial void DeinitBoundary()
	{
		_boundaryComputer = null;
	}
	Bounds ITargetBoundary.GetWorldBounds() => _boundaryComputer.IsNotNullRef() ? _boundaryComputer.GetWorldBounds() : default;
	Rect ITargetBoundary.GetScreenRect(Camera camera) => _boundaryComputer.IsNotNullRef() ? _boundaryComputer.GetScreenRect(camera.IsNullRef() ? StrategyManager.MainCamera : camera) : default;
}