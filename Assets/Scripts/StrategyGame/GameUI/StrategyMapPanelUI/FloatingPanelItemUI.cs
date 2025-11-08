using System;

using Sirenix.OdinInspector;

using UnityEngine;

public class FloatingPanelItemUI : MonoBehaviour
{
	[SerializeField,ReadOnly]
	protected Transform mapTarget;
	[SerializeField]
	protected RectTransform rectTransform;

	[SerializeField]
	protected Canvas canvas;
	[SerializeField]
	protected CanvasGroupUI canvasGroupUI;
	protected virtual void Reset()
	{
		canvas = GetComponent<Canvas>();
		canvasGroupUI = GetComponent<CanvasGroupUI>();
		rectTransform = GetComponent<RectTransform>();
	}
	protected virtual void Awake()
	{
		canvas = canvas == null ? GetComponent<Canvas>() : canvas;
		canvasGroupUI = canvasGroupUI == null ? GetComponent<CanvasGroupUI>() : canvasGroupUI;
		rectTransform = rectTransform == null ? GetComponent<RectTransform>() : rectTransform;
	}
	public void SetTargetInMap(Component target) => SetTargetInMap(target.transform);
	public void SetTargetInMap(GameObject target) => SetTargetInMap(target.transform);
	public virtual void SetTargetInMap(Transform mapTarget = null)
	{
		rectTransform = rectTransform == null ? GetComponent<RectTransform>() : rectTransform;

		if (mapTarget != null) ReleaseTarget(mapTarget);
		this.mapTarget = mapTarget;
		if(mapTarget != null) InitTarget(mapTarget);
	}
	public void RemoveTargetInMap() => SetTargetInMap();

	protected virtual void ReleaseTarget(Transform mapTarget)
	{
	
	}
	protected virtual void InitTarget(Transform mapTarget)
	{
		
	}
	public void Show(Action onVisible = null)
	{
		gameObject.SetActive(true);
		if (canvas != null) canvas.enabled = true;
		OnShow();
		if (canvasGroupUI != null) canvasGroupUI.OnShow(_Show);

		void _Show()
		{
			OnVisible();
			onVisible?.Invoke();
		}
	}
	public void Hide(Action onUnvisible = null)
	{
		OnHide();
		if (canvasGroupUI != null) canvasGroupUI.OnHide(_Hide);
		else _Hide();

		void _Hide()
		{
			if (canvas != null) canvas.enabled = false;
			OnUnvisible();
			onUnvisible?.Invoke();
			gameObject.SetActive(false);
		}
	}

	protected void LateUpdate()
	{
		if (rectTransform == null || mapTarget == null) return;
		OnUpdate();
	}
	protected virtual void OnShow()
	{

	}
	protected virtual void OnHide()
	{

	}
	protected virtual void OnVisible()
	{

	}
	protected virtual void OnUnvisible()
	{

	}
	protected virtual void OnUpdate()
	{
		Camera camera = Camera.main;
		if (camera == null) return;

		Vector3 screenMapTarget = camera.WorldToScreenPoint(mapTarget.transform.position);
		screenMapTarget.z = 0;
		rectTransform.position = screenMapTarget;
	}
}
