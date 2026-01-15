using UnityEngine;

public interface ITargetBoundary
{
	// 월드 공간의 3D Bounds
	Bounds GetWorldBounds();
	// 화면 공간의 2D Rect
	Rect GetScreenRect(Camera camera = null);
}