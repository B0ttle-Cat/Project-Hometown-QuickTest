using Sirenix.OdinInspector;

using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UIElements;

[RequireComponent(typeof(SplineContainer))]
[RequireComponent(typeof(SplineExtrude))]
[RequireComponent(typeof(MeshRenderer))]
public class MovementPathRenderer : MonoBehaviour
{
	[SerializeField, ReadOnly] private SplineContainer splineContainer;
	[SerializeField, ReadOnly] private SplineExtrude splineExtrude;
	[SerializeField, ReadOnly] private MeshRenderer splineRenderMesh;
	[SerializeField] private Vector4 baseMap_ST;

	[SerializeField] private float scrollSpeedX;
	[SerializeField] private float scrollSpeedY;

	private MaterialPropertyBlock propertyBlock;

	private Vector3[] paths;

	private void Reset()
	{
		splineContainer = GetComponent<SplineContainer>();
		splineExtrude = GetComponent<SplineExtrude>();
		splineRenderMesh = GetComponent<MeshRenderer>();
		baseMap_ST = new Vector4(1, 1, 0, 0);
		scrollSpeedX = 0.5f;
		scrollSpeedY = 0.5f;
	}

	private void Awake()
	{
		splineContainer = GetComponent<SplineContainer>();
		splineExtrude = GetComponent<SplineExtrude>();
		splineRenderMesh = GetComponent<MeshRenderer>();
		propertyBlock = new MaterialPropertyBlock();
		SetMovementPlan();
	}

	public void SetSpeed(Vector2 scrollSpeed)
	{
		scrollSpeedX = scrollSpeed.x;
		scrollSpeedY = scrollSpeed.y;
	}

	public void SetMovementPlan(Vector3[] paths)
	{
		this.paths = paths;
		SetMovementPlan();
	}
	public void SetProgress(float progress)
	{
		splineExtrude.Range = new Vector2(progress, 1);
		splineExtrude.Rebuild();
	}

	[Button]
	private void SetMovementPlan()
	{
		if (splineContainer == null) return;
		if (paths == null) return;

		ApplyPointsToSpline(paths);
	}

	private void ApplyPointsToSpline(Vector3[] paths)
	{
		splineContainer.Spline.Clear();

		var spline = new Spline();

		for (int i = 0 ; i < paths.Length ; i++)
		{
			spline.Add(paths[i], TangentMode.Linear);
		}

		splineContainer.Spline.Add(spline);
		splineExtrude.Rebuild(); // Extruded mesh 다시 생성
	}

	public void ClearMovementPlan()
	{
		if (splineContainer == null)
		{
			splineContainer = GetComponent<SplineContainer>();
		}

		if (splineContainer == null) return;

		splineContainer.Spline.Clear();
		splineExtrude.Rebuild();
	}

	public void Update()
	{
		if (splineExtrude == null) return;


		baseMap_ST.z -= scrollSpeedX * Time.deltaTime;
		if (baseMap_ST.z < -1f) baseMap_ST.z += 1f;

		baseMap_ST.w -= scrollSpeedY * Time.deltaTime;
		if (baseMap_ST.w < -1f) baseMap_ST.w += 1f;

		if(splineRenderMesh == null) splineRenderMesh = splineExtrude.GetComponent<MeshRenderer>();
		if (splineRenderMesh == null) return;

		splineRenderMesh.GetPropertyBlock(propertyBlock);
		propertyBlock.SetVector("_BaseMap_ST", baseMap_ST);
		splineRenderMesh.SetPropertyBlock(propertyBlock);
	}
}
