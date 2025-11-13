using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class MovementPathRenderer : MonoBehaviour
{
	[SerializeField, ReadOnly] private LineRenderer lineRenderer;
	[SerializeField, ReadOnly] private float offsetX;
	[SerializeField] private float scrollSpeed;

	private MaterialPropertyBlock propertyBlock;
	[SerializeField] private bool showDetail;

	private INodeMovement.MovementPlan[] plans;

	private void Reset()
	{
		lineRenderer = GetComponent<LineRenderer>();
		offsetX = 0f;
		showDetail = false;
		scrollSpeed = 0.5f;
	}

	private void Awake()
	{
		lineRenderer = GetComponent<LineRenderer>();
		offsetX = Random.value;
		propertyBlock = new MaterialPropertyBlock();
		SetMovementPlan();
	}
	public void SetSpeed(float scrollSpeed)
	{
		this.scrollSpeed = scrollSpeed;
	}
	public void ShowDetail()
	{
		showDetail = true;
		SetMovementPlan();
	}
	public void HideDetail()
	{
		showDetail = false;
		SetMovementPlan();
	}
	public void SetMovementPlan(INodeMovement.MovementPlan[] plans)
	{
		this.plans = plans;
		SetMovementPlan();
	}
	private void SetMovementPlan()
	{
		if (lineRenderer == null) return;
		if (plans == null) return;

		int length =  plans.Length;
		List<Vector3> points = new List<Vector3>();
		for (int i = 0 ; i < length ; i++)
		{
			INodeMovement.MovementPlan plan = plans[i];
			if (showDetail)
			{
				points.AddRange(plan.path);
			}
			else
			{
				points.Add(plan.path[0]);
				points.Add(plan.path[^1]);
			}
		}

		lineRenderer.SetPositions(points.ToArray());
	}
	public void ClearMovementPlan()
	{
		if (!didAwake)
		{
			lineRenderer = GetComponent<LineRenderer>();
		}
		if (lineRenderer == null) return;
		lineRenderer.SetPositions(new Vector3[0]);
	}
	public void Update()
	{
		if (lineRenderer == null) return;

		offsetX -= scrollSpeed * Time.deltaTime;
		if (offsetX < -1f) offsetX += 1f; // 루프


		// 머티리얼 속성 읽기 및 수정
		lineRenderer.GetPropertyBlock(propertyBlock);
		propertyBlock.SetVector("_MainTex_ST", new Vector4(1, 1, offsetX, 0));
		lineRenderer.SetPropertyBlock(propertyBlock);

	}


}
