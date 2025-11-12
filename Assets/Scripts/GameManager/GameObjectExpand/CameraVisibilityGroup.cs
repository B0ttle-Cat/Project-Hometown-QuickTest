using System;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;

public interface IVisibilityEvent<T> where T : class
{
	IVisibilityEvent<T> ThisVisibility { get; }
	bool IsVisible { get; }
	event Action<T> OnChangeInvisible;
	event Action<T> OnChangeVisible;
}

[DisallowMultipleComponent]
public class CameraVisibilityGroup : MonoBehaviour, IVisibilityEvent<Component>
{
	public IVisibilityEvent<Component> ThisVisibility => this;
	[SerializeField, ReadOnly] protected Camera targetCamera;
	[SerializeField, ReadOnly] protected bool isVisible = false;
	[SerializeField, ReadOnly] protected List<Renderer> renderers = new List<Renderer>();
	[SerializeField, ReadOnly] private Rect visibleScreenRect = Rect.zero;
	[SerializeField, ReadOnly] private Bounds visibleWorldBounds = default;
	public event Action<Component> OnChangeVisible;
	public event Action<Component> OnChangeInvisible;

	private Vector3[] corners = new Vector3[8];

	public bool IsVisible => isVisible;
	public Rect VisibleScreenRect => visibleScreenRect;
	public Bounds VisibleWorldBounds => visibleWorldBounds;
    private void Reset()
    {
        
    }

    void Awake()
	{
		if (targetCamera == null) targetCamera = Camera.main;
		visibleScreenRect = Rect.zero;
		OnRefreshRenderers();
		OnRefreshCamera();
		VisibilityUpdate();
	}

	void OnEnable()
	{
		OnRefreshRenderers();
	}

	void OnTransformChildrenChanged()
	{
		OnRefreshRenderers();
	}

	protected void OnRefreshRenderers()
	{
		if (renderers == null)
			renderers = new List<Renderer>();
		else
			renderers.Clear();
		RefreshRenderers();
	}
	protected void OnRefreshCamera()
	{
		RefreshCamera();
	}
	protected virtual void RefreshRenderers()
	{
		GetComponentsInChildren(true, renderers);
	}
	protected virtual void RefreshCamera()
	{
		targetCamera = Camera.main;
	}
	protected virtual void LateUpdate()
	{
		VisibilityUpdate();
	}
	protected virtual void VisibilityUpdate()
	{
		if (targetCamera == null)
		{
			OnRefreshCamera();
			if (targetCamera == null) return;
		}

		Plane[] planes = GeometryUtility.CalculateFrustumPlanes(targetCamera);
		bool anyVisible = false;

		float minX = float.MaxValue;
		float minY = float.MaxValue;
		float maxX = float.MinValue;
		float maxY = float.MinValue;

		visibleWorldBounds = new Bounds(transform.position, Vector3.zero);
		foreach (Renderer r in renderers)
		{
			if (r == null || !r.enabled) continue;
			if (!r.gameObject.activeInHierarchy) continue;
			if (!GeometryUtility.TestPlanesAABB(planes, r.bounds)) continue;

			anyVisible = true;
			visibleWorldBounds.Encapsulate(r.bounds);
		}

		if (corners == null || corners.Length != 8) corners = new Vector3[8];
		{
			Vector3 min = visibleWorldBounds.min;
			Vector3 max = visibleWorldBounds.max;
			corners[0] = new Vector3(min.x, min.y, min.z);
			corners[1] = new Vector3(min.x, min.y, max.z);
			corners[2] = new Vector3(min.x, max.y, min.z);
			corners[3] = new Vector3(min.x, max.y, max.z);
			corners[4] = new Vector3(max.x, min.y, min.z);
			corners[5] = new Vector3(max.x, min.y, max.z);
			corners[6] = new Vector3(max.x, max.y, min.z);
			corners[7] = new Vector3(max.x, max.y, max.z);
		};

		foreach (var corner in corners)
		{
			Vector3 screenPos = targetCamera.WorldToScreenPoint(corner);
			minX = Mathf.Min(minX, screenPos.x);
			minY = Mathf.Min(minY, screenPos.y);
			maxX = Mathf.Max(maxX, screenPos.x);
			maxY = Mathf.Max(maxY, screenPos.y);
		}

		visibleScreenRect = anyVisible ? new Rect(minX, minY, maxX - minX, maxY - minY) : Rect.zero;

		if (anyVisible && !isVisible)
		{
			isVisible = true;
			var visibleTarget = GetVisibleTarget();
			if(visibleTarget != null)
			{
				OnChangeVisible?.Invoke(visibleTarget);
			}
		}
		else if (!anyVisible && isVisible)
		{
			isVisible = false;
			var visibleTarget = GetVisibleTarget();
			if (visibleTarget != null)
			{
				OnChangeInvisible?.Invoke(visibleTarget);
			}
		}
	}

	protected virtual Component GetVisibleTarget()
	{
		return this;
	}

}
