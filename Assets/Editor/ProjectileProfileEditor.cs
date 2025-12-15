using Sirenix.OdinInspector.Editor;

using UnityEditor;

using UnityEngine;

[CustomEditor(typeof(ProjectileProfileObject))]
public class ProjectileProfileEditor : OdinEditor
{
	protected override void OnEnable()
	{
		base.OnEnable();
		SceneView.duringSceneGui += DrawGizmoInSceneView;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		SceneView.duringSceneGui -= DrawGizmoInSceneView;
	}

	private void DrawGizmoInSceneView(SceneView sceneView)
	{
		ProjectileProfileObject profile = (ProjectileProfileObject)target;
		bool shouldDrawGizmos = Selection.activeObject == profile && profile.showGizmos;
		bool hasSimulationPoints = profile.cepCEPSimulation != null && profile.cepCEPSimulation.Count > 0;
		if (!shouldDrawGizmos && !hasSimulationPoints) return;

		if (shouldDrawGizmos)
		{
			Vector3 targetDirection = Vector3.forward;
			Vector3 directionUp = Vector3.up;
			DrawGizmoCEP(profile, profile.statsData, targetDirection, directionUp);
		}

		DrawSimulationPoints(profile.cepCEPSimulation);
	}

	private void DrawGizmoCEP(ProjectileProfileObject profile, ProjectileStatsData data, Vector3 targetDirection, Vector3 directionUp)
	{
		if (!data.CepEnabled) return;

		float distanceD = data.CepDistance;
		float radiusR = data.CepRadius;
		float percentN = data.CepProbability;
		Vector3 cepScale = data.CepScaleVector3;

		Vector3 V1 = targetDirection.normalized;
		if (V1.sqrMagnitude < 0.001f) V1 = Vector3.forward;
        
		Vector3 V2;
		Vector3 V3;

		V2 = Vector3.Cross(V1, directionUp).normalized;
		if (V2.sqrMagnitude < 0.001f)
		{
			if (Mathf.Abs(V1.y) > 0.99f) V2 = Vector3.right;
			else V2 = Vector3.Cross(V1, Vector3.up).normalized;
		}
		V3 = Vector3.Cross(V2, V1).normalized;

		// 목표 중심점 및 기준선 그리기
		Vector3 center = Vector3.zero + V1 * distanceD; 
		Handles.color = Color.white;
		Handles.DrawLine(Vector3.zero, center);
		Handles.DrawWireDisc(center, V1, 0.1f * HandleUtility.GetHandleSize(center));

        // 2. 표준 편차 및 반경 계산
        float currentFactor = GetSigmaFactor(percentN);
        float n_factor_inv = (currentFactor > float.Epsilon) ? 1.0f / currentFactor : 0f;
        
        float radiusX_Base = radiusR * cepScale.x;
        float radiusY_Base = radiusR * cepScale.y;
        float sigmaZ = radiusR * n_factor_inv;
        
        float nErrorZ = currentFactor * sigmaZ * cepScale.z;
        const float MAX_SIGMA_FACTOR = 4.0f;
        float maxErrorZ = MAX_SIGMA_FACTOR * sigmaZ * cepScale.z;

        // XY 평면 시각화에만 사용되는 오차 포함 중심점
        Vector3 nCenter_CEP = V1 * (distanceD + nErrorZ);
        Vector3 maxCenter_CEP = V1 * (distanceD + maxErrorZ);
        
        // Z축 오차의 절댓값 크기 (타원 반경으로 사용)
        float nRangeZ = Mathf.Abs(nErrorZ); 
        float maxRangeZ = Mathf.Abs(maxErrorZ); 
        
        float maxRadiusX = radiusX_Base * (MAX_SIGMA_FACTOR / currentFactor);
        float maxRadiusY = radiusY_Base * (MAX_SIGMA_FACTOR / currentFactor);


        // 3. 3대 평면 시각화 (중앙 정렬 적용)
        
        Vector3 WORLD_X = Vector3.right;
        Vector3 WORLD_Y = Vector3.up;
        Vector3 WORLD_Z = Vector3.forward;

        // 공통: 오차 미포함 중심점 투영 (D 지점 투영)
        Vector3 center_XZ = center - Vector3.Dot(center, WORLD_Y) * WORLD_Y;
        Vector3 center_YZ = center - Vector3.Dot(center, WORLD_X) * WORLD_X;


        // 3.1. 가로(X) / 길이(Z) 평면 (Normal: WORLD_Y) -> 확률별 분포 포함
        
        // 3.1.1. 90% ~ 10% 공산오차 범위 (붉은 -> 노랑 -> 초록 점선)
        for (int p = 90; p >= 10; p -= 10)
        {
            float probability = (float)p / 100f;
            float factor = GetSigmaFactor(probability);
            
            Color color;
            if (p >= 70) color = Color.Lerp(Color.red, Color.yellow, (90f - p) / 20f);
            else if (p >= 30) color = Color.Lerp(Color.yellow, Color.green, (70f - p) / 40f);
            else color = Color.green;
            
            float pRadiusX = radiusX_Base * (factor / currentFactor); 
            float pRangeZ = nRangeZ * (factor / currentFactor);
            
            Handles.color = color;
            DrawDottedEllipse(center_XZ, pRadiusX, pRangeZ, WORLD_X, V1, WORLD_Y);
        }

        // 3.1.2. 지정된 radiusR에 대한 공산오차 범위 (검은 실선)
        Handles.color = Color.black;
        DrawWireEllipse(center_XZ, radiusX_Base, nRangeZ, WORLD_X, V1, WORLD_Y);

        // 3.1.3. 최대거리(99.99%)에 대한 공산오차 범위 (검은 점선)
        Handles.color = Color.black;
        DrawDottedEllipse(center_XZ, maxRadiusX, maxRangeZ, WORLD_X, V1, WORLD_Y);


        // 3.2. 높이(Y) / 길이(Z) 평면 (Normal: WORLD_X)
        
        // N% (시안 실선)
        Handles.color = Color.cyan;
        DrawWireEllipse(center_XZ, radiusY_Base, nRangeZ, WORLD_Y, V1, WORLD_X);

        // 99.99% (시안 점선)
        Handles.color = Color.cyan;
        DrawDottedEllipse(center_XZ, maxRadiusY, maxRangeZ, WORLD_Y, V1, WORLD_X);


        // 3.3. 가로(X) / 높이(Y) 평면 (Normal: WORLD_Z)
        
        // XY 평면은 Z 오차가 반경이 아닌 중심 이동에 반영됨
        Vector3 nCenter_XY = nCenter_CEP - Vector3.Dot(nCenter_CEP, WORLD_Z) * WORLD_Z;
        Vector3 maxCenter_XY = maxCenter_CEP - Vector3.Dot(maxCenter_CEP, WORLD_Z) * WORLD_Z;
        
        // N% (녹색 실선)
        Handles.color = Color.green;
        DrawWireEllipse(center_XZ, radiusX_Base, radiusY_Base, WORLD_X, WORLD_Y, WORLD_Z);

        // 99.99% (녹색 점선)
        Handles.color = Color.green;
        DrawDottedEllipse(center_XZ, maxRadiusX, maxRadiusY, WORLD_X, WORLD_Y, WORLD_Z);
	}


	// --- 시뮬레이션 포인트 드로잉 ---
	private static void DrawSimulationPoints(System.Collections.Generic.List<Vector3> simulationPoints)
	{
		if (simulationPoints == null || simulationPoints.Count == 0) return;
		Handles.color = Color.magenta;

		foreach (Vector3 point in simulationPoints)
		{
			float size = HandleUtility.GetHandleSize(point) * 0.05f;
			Handles.SphereHandleCap(0, point, Quaternion.identity, size, EventType.Repaint);
		}
	}


	// --- 헬퍼 함수 ---

	private static void DrawWireEllipse(Vector3 center, float radiusA, float radiusB, Vector3 axisA, Vector3 axisB, Vector3 normal, int segments = 32)
	{
		Vector3[] points = new Vector3[segments + 1];
		for (int i = 0 ; i <= segments ; i++)
		{
			float angle = (float)i / segments * 360f * Mathf.Deg2Rad;
			points[i] = center + axisA * (Mathf.Cos(angle) * radiusA) + axisB * (Mathf.Sin(angle) * radiusB);
		}
		Handles.DrawPolyLine(points);
	}

	private static void DrawDottedEllipse(Vector3 center, float radiusA, float radiusB, Vector3 axisA, Vector3 axisB, Vector3 normal, int segments = 64)
	{
		Vector3[] points = new Vector3[segments + 1];
		for (int i = 0 ; i <= segments ; i++)
		{
			float angle = (float)i / segments * 360f * Mathf.Deg2Rad;
			points[i] = center + axisA * (Mathf.Cos(angle) * radiusA) + axisB * (Mathf.Sin(angle) * radiusB);
		}
		for (int i = 0 ; i < segments ; i++)
		{
			Handles.DrawDottedLine(points[i], points[i + 1], 2f);
		}
	}
    
    public static float GetSigmaFactor(float probability)
	{
		if (probability >= 0.9999f) return 4.0f;
		if (probability <= 0.0f) return 0.0f;
		return Mathf.Sqrt(-2f * Mathf.Log(1f - probability));
	}
}