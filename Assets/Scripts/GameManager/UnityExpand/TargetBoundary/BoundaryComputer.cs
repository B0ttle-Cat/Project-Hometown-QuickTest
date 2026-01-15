using UnityEngine;

public class BoundaryComputer : MonoBehaviour, ITargetBoundary
{
	[Header("Boundary Options")]
	public bool IsEllipticWithOnScreen = false;

	private Renderer[] _renderers;
	private Bounds _cachedBounds;
	private int _lastBoundsFrame = -1;
	private Rect _cachedScreenRect;
	private int _lastRectFrame = -1;

	private void Awake()
	{
		_renderers = GetComponentsInChildren<Renderer>(false);
	}

	public Bounds GetWorldBounds()
	{
		if (Time.frameCount == _lastBoundsFrame) return _cachedBounds;

		if (_renderers == null || _renderers.Length == 0)
		{
			_renderers = GetComponentsInChildren<Renderer>(false);
			if (_renderers.Length == 0) return new Bounds(transform.position, Vector3.zero);
		}

		Bounds combinedBounds = _renderers[0].bounds;
		for (int i = 1 ; i < _renderers.Length ; i++)
		{
			combinedBounds.Encapsulate(_renderers[i].bounds);
		}

		_cachedBounds = combinedBounds;
		_lastBoundsFrame = Time.frameCount;
		return _cachedBounds;
	}

	public Rect GetScreenRect(Camera camera)
	{
		if (Time.frameCount == _lastRectFrame) return _cachedScreenRect;

		Bounds b = GetWorldBounds();
		if (!IsEllipticWithOnScreen)
		{
			_cachedScreenRect = GetBoxScreenRect(camera, b);
		}
		else
		{
			_cachedScreenRect = GetEllipticScreenRect(camera, b);
		}

		_lastRectFrame = Time.frameCount;
		return _cachedScreenRect;
	}

	private Rect GetEllipticScreenRect(Camera camera, Bounds b)
	{
		Vector3 c = b.center;
		Vector3 e = b.extents;

		float minX = float.MaxValue, maxX = float.MinValue;
		float minY = float.MaxValue, maxY = float.MinValue;

		// 타원 기둥의 상단(y + extents.y)과 하단(y - extents.y) 두 개의 원판 검사
		float[] yOffsets = { e.y, -e.y };

		foreach (float yOff in yOffsets)
		{
			Vector3 center = c + new Vector3(0, yOff, 0);

			// 카메라 좌표계에서의 타원 축 벡터 추출
			// 월드 축 방향의 반지름 벡터를 화면 공간으로 변환하기 위한 준비
			Vector3 axisX = new Vector3(e.x, 0, 0);
			Vector3 axisZ = new Vector3(0, 0, e.z);

			// 현재 원판의 중심을 화면으로 변환
			Vector3 screenCenter = camera.WorldToScreenPoint(center);

			// 공식을 통한 화면 공간에서의 타원 극점 반경(Radius) 산출
			// screenPos = WorldToScreen(Center + e.x*cos(t)*right + e.z*sin(t)*forward)
			// 이를 t에 대해 미분하여 0이 되는 지점을 찾으면 아래와 같은 벡터 합 형식이 됩니다.
			Vector3 screenEx = camera.WorldToScreenPoint(center + axisX) - screenCenter;
			Vector3 screenEz = camera.WorldToScreenPoint(center + axisZ) - screenCenter;

			// 화면 공간상의 타원 반경 결정 (공식: R = sqrt(A^2 + B^2))
			float rx = Mathf.Sqrt(screenEx.x * screenEx.x + screenEz.x * screenEz.x);
			float ry = Mathf.Sqrt(screenEx.y * screenEx.y + screenEz.y * screenEz.y);

			// 해당 원판의 Rect 확장
			minX = Mathf.Min(minX, screenCenter.x - rx);
			maxX = Mathf.Max(maxX, screenCenter.x + rx);
			minY = Mathf.Min(minY, screenCenter.y - ry);
			maxY = Mathf.Max(maxY, screenCenter.y + ry);
		}

		return new Rect(minX, minY, maxX - minX, maxY - minY);
	}

	private Rect GetBoxScreenRect(Camera camera, Bounds b)
	{
		Vector3 c = b.center;
		Vector3 e = b.extents;
		float minX = float.MaxValue, maxX = float.MinValue;
		float minY = float.MaxValue, maxY = float.MinValue;

		for (int i = 0 ; i < 8 ; i++)
		{
			Vector3 v = c + new Vector3(
				(i & 1) == 0 ? e.x : -e.x,
				(i & 2) == 0 ? e.y : -e.y,
				(i & 4) == 0 ? e.z : -e.z
			);
			Vector3 screenPoint = camera.WorldToScreenPoint(v);
			minX = Mathf.Min(minX, screenPoint.x);
			maxX = Mathf.Max(maxX, screenPoint.x);
			minY = Mathf.Min(minY, screenPoint.y);
			maxY = Mathf.Max(maxY, screenPoint.y);
		}
		return new Rect(minX, minY, maxX - minX, maxY - minY);
	}
}