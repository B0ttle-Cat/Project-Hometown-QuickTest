using System;

using Sirenix.OdinInspector;

using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class FloatingPanelItemUI : MonoBehaviour
{
	private int updateThisFrame;

    [SerializeField,ReadOnly]
    private Transform mapTarget;
    [SerializeField]
	protected RectTransform rectTransform;

	[SerializeField]
	protected Canvas canvas;
	[SerializeField]
	protected CanvasGroupUI canvasGroupUI;

    protected virtual Transform MapTarget => mapTarget;

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
	public void SetTargetInMap(Component target) => SetTargetInMap(target == null ? null : target.transform);
	public void SetTargetInMap(GameObject target) => SetTargetInMap(target == null ? null : target.transform);
	public virtual void SetTargetInMap(Transform mapTarget = null)
	{
		rectTransform = rectTransform == null ? GetComponent<RectTransform>() : rectTransform;

		if (mapTarget != null) ReleaseTarget(mapTarget);
		this.mapTarget = mapTarget;
		if(mapTarget != null) InitTarget(mapTarget);
	}
	public virtual void RemoveTargetInMap(Component target)	=> RemoveTargetInMap(target == null ? null : target.transform);
	public virtual void RemoveTargetInMap(GameObject target) => RemoveTargetInMap(target == null ? null : target.transform);
	public virtual void RemoveTargetInMap(Transform mapTarget = null)
	{
		if(mapTarget == null)
			SetTargetInMap();
		else if(this.MapTarget == mapTarget)
			SetTargetInMap();
	}
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
	public void Hide(Action onInvisible = null)
	{
		OnHide();
		if (canvasGroupUI != null) canvasGroupUI.OnHide(_Hide);
		else _Hide();

		void _Hide()
		{
			if (canvas != null) canvas.enabled = false;
			OnInvisible();
			onInvisible?.Invoke();
			gameObject.SetActive(false);
		}
	}

	protected void LateUpdate()
	{
		if (rectTransform == null || MapTarget == null) return;
		int fameCount = Time.frameCount;
		if (updateThisFrame == fameCount) return;
		updateThisFrame = fameCount;
		OnUpdate();
	}
	public void ForceUpdateThisFrame()
	{
		LateUpdate();
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
	protected virtual void OnInvisible()
	{

	}
	protected virtual void OnUpdate()
	{
		Camera camera = Camera.main;
		if (camera == null) return;

		Vector2 screenMapTarget = camera.WorldToScreenPoint(MapTarget.transform.position);
		rectTransform.position = screenMapTarget;
	}
}
