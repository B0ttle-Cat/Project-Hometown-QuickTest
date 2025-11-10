#if UNITY_EDITOR
using System.Linq;

using Sirenix.OdinInspector.Editor;

using UnityEditor;

using UnityEngine;

[CustomEditor(typeof(StrategyStartSetterData))]
public class StrategyStartSetterDataEditor : OdinEditor
{
	protected override void OnEnable()
	{
		base.OnEnable();
		SceneView.duringSceneGui += EditorOnSceneGUI;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		SceneView.duringSceneGui -= EditorOnSceneGUI;
	}
	private void EditorOnSceneGUI(SceneView sceneView)
	{
		if (EditorApplication.isPlaying) return;

		if (target is not StrategyStartSetterData _target) return;

		if (_target == null || !_target.onShowGizmo) return;

		var data = _target.GetData();
		var sectorDatas = data.sectorDatas;

		if (sectorDatas != null)
		{
			foreach (var sector in sectorDatas)
			{
				DrawSectorLabel(sector);
			}
		}
		var captureDatas = data.captureDatas;
		var factionDatas = data.factionDatas;
		if (captureDatas != null)
		{
			foreach (var capture in captureDatas)
			{
				DrawCapture(capture, factionDatas);
			}
		}

		var networkDatas = data.sectorLinkDatas;
		if (networkDatas != null)
		{
			foreach (var net in networkDatas)
			{
				DrawNetworkLink(_target, net, data);
			}
		}
	}
	private void DrawSectorLabel(StrategyStartSetterData.SectorData sector)
	{
		// 실제 씬 오브젝트 찾기 (씬에 존재한다고 가정)
		var obj = GameObject.Find(sector.profileData.sectorName);
		if (obj == null)
			return;
		DrawLabel(obj.transform.position, sector.profileData.sectorName, Color.black);
	}
	private void DrawCapture(StrategyStartSetterData.CaptureData captureData, StrategyStartSetterData.FactionData[] factionDatas)
	{
		// 실제 씬 오브젝트 찾기 (씬에 존재한다고 가정)
		var obj = GameObject.Find(captureData.captureSector);
		if (obj == null) return;
		// 점령 세력 색상 추출
		var faction = factionDatas.FirstOrDefault(f => f.factionName == captureData.captureFaction);
		Color color = faction.factionColor;
		if (color == default) return;

		DrawLabel(obj.transform.position, Vector3.down, $"Capture: {captureData.captureFaction} {(int)(captureData.captureProgress * 100)}%", color);
	}

	private void DrawNetworkLink(StrategyStartSetterData target, StrategyStartSetterData.SectorLinkData net, StrategyStartSetterData.Data data)
	{
		// A와 B 섹터 위치 추출
		var sectorA = data.sectorDatas.FirstOrDefault(s => s.profileData.sectorName == net.sectorA);
		var sectorB = data.sectorDatas.FirstOrDefault(s => s.profileData.sectorName == net.sectorB);

		// 실제 씬 오브젝트 찾기 (씬에 존재한다고 가정)
		var objA = GameObject.Find(sectorA.profileData.sectorName);
		var objB = GameObject.Find(sectorB.profileData.sectorName);

		if (objA == null || objB == null)
			return;

		Vector3 posA = objA.transform.position;
		Vector3 posB = objB.transform.position;

		// 연결선 색상 설정
		Color color = net.connectDir switch
		{
			NetworkLink.ConnectDirType.Both => Color.green,
			NetworkLink.ConnectDirType.Forward => Color.cyan,
			NetworkLink.ConnectDirType.Backward => Color.magenta,
			_ => Color.gray
		};

		Handles.color = color;

		// 라인 + 화살표 표시
		Handles.DrawAAPolyLine(3f, posA, posB);
		DrawArrow(posA, posB, net.connectDir);

		// WayPoints 존재 시
		if (target.onShowWayPointsGizmo && net.waypoint != null && net.waypoint.Length > 0)
		{
			Handles.color = Color.yellow;
			var linePoints = WaypointUtility.GetLineWithWaypoints(posA, posB, net.waypoint);
			if (linePoints != null && linePoints.Length > 1)
			{
				Handles.DrawAAPolyLine(2f, linePoints);
			}

			if (net.onShowEditPoint)
			{
				for (int i = 0 ; i < net.waypoint.Length ; i++)
				{
					EditorGUI.BeginChangeCheck();
					var newPos = Handles.PositionHandle(net.waypoint[i].point, Quaternion.identity);
					if (EditorGUI.EndChangeCheck())
					{
						Undo.RecordObject(target, "Move Waypoint");
						net.waypoint[i].point = newPos;
						EditorUtility.SetDirty(target);
					}

					Handles.SphereHandleCap(0, newPos, Quaternion.identity, 0.3f, EventType.Repaint);
				}
			}
		}

		string arrowText = net.connectDir switch
		{
			NetworkLink.ConnectDirType.Both => "↔",
			NetworkLink.ConnectDirType.Forward => "→",
			NetworkLink.ConnectDirType.Backward => "←",
			_ => "|"
		};

		// 이름 시각화
		DrawLabel((posA + posB) * 0.5f, $"{net.sectorA} {arrowText} {net.sectorB}", color);
	}

	private void DrawArrow(Vector3 from, Vector3 to, NetworkLink.ConnectDirType type)
	{
		Vector3 dir = (to - from).normalized;
		Vector3 mid = Vector3.Lerp(from, to, 0.5f);
		float size = Vector3.Distance(from, to) * 0.05f;

		switch (type)
		{
			case NetworkLink.ConnectDirType.Forward:
			Handles.ConeHandleCap(0, mid - dir * size * 0.5f, Quaternion.LookRotation(dir), size, EventType.Repaint);
			break;
			case NetworkLink.ConnectDirType.Backward:
			Handles.ConeHandleCap(0, mid + dir * size * 0.5f, Quaternion.LookRotation(-dir), size, EventType.Repaint);
			break;
			case NetworkLink.ConnectDirType.Both:
			Handles.ConeHandleCap(0, mid - dir * size * 0.5f, Quaternion.LookRotation(dir), size, EventType.Repaint);
			Handles.ConeHandleCap(0, mid + dir * size * 0.5f, Quaternion.LookRotation(-dir), size, EventType.Repaint);
			break;
		}
	}
	public void DrawLabel(Vector3 worldPos, Vector3 offset, string text, Color? color = null, GUIStyle style = null)
	{
		if (SceneView.currentDrawingSceneView == null)
			return;

		Camera cam = SceneView.currentDrawingSceneView.camera;
		if (cam == null)
			return;

		worldPos += cam.transform.TransformDirection(offset); // 약간 위로 띄우기

		// 3D -> 2D 화면 좌표 변환
		Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

		// 카메라 뒤쪽에 있으면 표시 안 함
		if (screenPos.z < 0)
			return;

		// GUI 좌표계는 Y가 반대라서 변환 필요
		screenPos.y = SceneView.currentDrawingSceneView.position.height - screenPos.y;

		Handles.BeginGUI();

		Color prev = GUI.color;
		GUI.color = color ?? Color.white;

		style ??= EditorStyles.boldLabel;
		Vector2 size = style.CalcSize(new GUIContent(text));

		// 중심 정렬된 라벨
		Rect rect = new Rect(screenPos.x - size.x / 2f, screenPos.y - size.y / 2f, size.x, size.y);
		GUI.Label(rect, text, style);

		GUI.color = prev;
		Handles.EndGUI();
	}
	public void DrawLabel(Vector3 worldPos, string text, Color? color = null, GUIStyle style = null)
	{
		DrawLabel(worldPos, Vector3.up, text, color, style);
	}

}
#endif
