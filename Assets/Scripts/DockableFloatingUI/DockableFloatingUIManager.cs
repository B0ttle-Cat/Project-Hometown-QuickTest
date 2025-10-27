using System.Collections.Generic;
using System.Linq;

using Sirenix.OdinInspector;

using UnityEngine;

namespace DockableFloatingUI
{
	public class DockableFloatingUIManager : MonoBehaviour
	{
		public static DockableFloatingUIManager Instance { get; private set; }

		[Header("프리팹 참조")]
		public FloatingUI floatingUIPrefab;
		public FloatingGroup floatingGroupPrefab;

		[Header("관리 리스트")]
		public List<FloatingUI> floatingUIs = new List<FloatingUI>();
		public List<FloatingGroup> floatingGroups = new List<FloatingGroup>();

		public float snapDistance = 50f;


#if UNITY_EDITOR
		[ButtonGroup]
		public void CreateTestFloatingUICenter()
		{
			var ui = CreateFloatingUI();
			if (ui != null)
			{
				// 부모 Canvas 또는 DockableFloatingUIManager 기준 중앙
				RectTransform parentRect = ui.Rect.parent as RectTransform;
				if (parentRect != null)
					ui.Rect.anchoredPosition = Vector2.zero; // 중앙
			}
		}
		[ButtonGroup]
		public void DeleteAllFloatingUIAndGroups()
		{
			// FloatingUI 삭제
			foreach (var ui in floatingUIs.ToArray())
			{
				if (ui != null)
					DestroyImmediate(ui.gameObject);
			}
			floatingUIs.Clear();

			// FloatingGroup 삭제
			foreach (var group in floatingGroups.ToArray())
			{
				if (group != null)
					DestroyImmediate(group.gameObject);
			}
			floatingGroups.Clear();
		}
#endif
		private void Awake()
		{
			if (Instance != null && Instance != this)
			{
				Destroy(gameObject);
				return;
			}
			Instance = this;
		}

		#region 등록 / 해제
		public void RegisterFloatingUI(FloatingUI ui)
		{
			if (ui == null || floatingUIs.Contains(ui)) return;
			ui.Name = $"Item {floatingUIs.Count:00}";
			floatingUIs.Add(ui);
		}

		public void UnregisterFloatingUI(FloatingUI ui)
		{
			if (ui == null) return;
			floatingUIs.Remove(ui);
		}

		public void RegisterGroup(FloatingGroup group)
		{
			if (group == null || floatingGroups.Contains(group)) return;
			group.Name = $"Group {floatingGroups.Count:00}";
			floatingGroups.Add(group);
		}

		public void UnregisterGroup(FloatingGroup group)
		{
			if (group == null) return;
			floatingGroups.Remove(group);
		}
		#endregion

		#region 생성
		public FloatingUI CreateFloatingUI()
		{
			if (floatingUIPrefab == null)
			{
				Debug.LogError("FloatingUI Prefab이 설정되지 않았습니다.");
				return null;
			}

			var uiObject = Instantiate(floatingUIPrefab.gameObject, transform);
			var ui = uiObject.GetComponent<FloatingUI>();
			RegisterFloatingUI(ui);
			return ui;
		}

		public FloatingGroup CreateFloatingGroup(FloatingUI target, FloatingUI dragged)
		{
			if (target == null || dragged == null) return null;

			// 그룹 생성
			var groupObject = Instantiate(floatingGroupPrefab.gameObject, transform);
			var group = groupObject.GetComponent<FloatingGroup>();

			// 위치: 대상 UI(target) 기준
			group.Rect.position = target.Rect.position;

			// 두 UI를 그룹에 추가
			AddMembersInParentGroup(group, IDockable.Data.DockingDir.Parent, target);
			AddMembersInParentGroup(group, IDockable.Data.DockingDir.Parent, dragged);

			// 그룹 리스트 등록
			RegisterGroup(group);

			return group;
		}
		#endregion

		#region 탐색 / 근접 판정
		public (IDockable Dock, IDockable.Data.DockingDir Dir) GetNearestDockable(IDockable ui, Vector2 mouseScreenPoint)
		{
			(IDockable nearest, IDockable.Data.DockingDir parentDir) = (null, IDockable.Data.DockingDir.None);
			if (ui == null) return (nearest, parentDir);

			// 1. 검색 대상: 모든 IDockable
			List<IDockable> allDockables = new List<IDockable>();
			allDockables.AddRange(DockableFloatingUIManager.Instance.floatingUIs.Where(x => x != null && x.ParentDockable == null));
			allDockables.AddRange(DockableFloatingUIManager.Instance.floatingGroups.Where(x => x != null));

			float nearestDist = float.MaxValue;

			Rect nearRect = default;

			foreach (var item in allDockables)
			{
				if (item == null || item == ui) continue;

				// 2. 겹치는 지 검사
				Rect itemRect = GetWorldCornersRect(item.Rect, 0f);
				if (itemRect.Contains(mouseScreenPoint))
				{
					// 겹치는게 1개 라도 있으면 Parent 로 고정. 
					if (parentDir != IDockable.Data.DockingDir.Parent)
					{
						nearest = null;
						parentDir = IDockable.Data.DockingDir.Parent;
						nearestDist = float.MaxValue;
					}
				}

				if (parentDir != IDockable.Data.DockingDir.Parent)
				{
					// 3. 외곽 + snapDistance 영역과 겹치는지
					Rect expandedRect = GetWorldCornersRect(item.Rect, snapDistance);
					if (!expandedRect.Contains(mouseScreenPoint)) continue;
				}

				// 4. Center 거리 계산
				Vector2 itemCenter = itemRect.center;
				float dist = Vector2.Distance(itemCenter, mouseScreenPoint);

				// 5. 연결 방향 계산 (상/하/좌/우)
				if (parentDir != IDockable.Data.DockingDir.Parent)
				{
					float leftDist   = IsEmpty(IDockable.Data.DockingDir.Left) ? Mathf.Abs(mouseScreenPoint.x - nearRect.xMin) : float.MaxValue;    // ui 오른쪽 → item 왼쪽
					float rightDist  = IsEmpty(IDockable.Data.DockingDir.Right) ?  Mathf.Abs(nearRect.xMax - mouseScreenPoint.x) : float.MaxValue;    // ui 왼쪽 → item 오른쪽
					float topDist    = IsEmpty(IDockable.Data.DockingDir.Top) ? Mathf.Abs(nearRect.yMax - mouseScreenPoint.y) : float.MaxValue;    // ui 아래 → item 위
					float bottomDist = IsEmpty(IDockable.Data.DockingDir.Bottom) ? Mathf.Abs(mouseScreenPoint.y - nearRect.yMin) : float.MaxValue;    // ui 위 → item 아래

					dist = Mathf.Min(dist, leftDist, rightDist, topDist, bottomDist);
					IDockable.Data.DockingDir minDir = IDockable.Data.DockingDir.None;
					if (dist == leftDist) minDir = IDockable.Data.DockingDir.Left;
					else if (dist == rightDist) minDir = IDockable.Data.DockingDir.Right;
					else if (dist == topDist) minDir = IDockable.Data.DockingDir.Top;
					else minDir = IDockable.Data.DockingDir.Bottom;
					if (IsEmpty(minDir) && dist < nearestDist)
					{
						nearestDist = dist;
						nearest = item;
						nearRect = itemRect;
						parentDir = minDir;
					}
					bool IsEmpty(IDockable.Data.DockingDir dir) => !nearest.DockableData.FindDock(dir, out _);
				}
				else if (dist < nearestDist)
				{
					nearestDist = dist;
					nearest = item;
					nearRect = itemRect;
				}
			}


			return (nearest, parentDir);

			Rect GetWorldCornersRect(RectTransform rect, float snap = 0f)
			{
				Vector3[] corners = new Vector3[4];
				rect.GetWorldCorners(corners);

				return new Rect(
					corners[0].x - snap,
					corners[0].y - snap,
					corners[2].x - corners[0].x + snap * 2f,
					corners[2].y - corners[0].y + snap * 2f);
			}
		}


		public (IDockable Dock, IDockable.Data.DockingDir Dir) GetNearestDockable2(IDockable ui, Vector2 mouseScreenPoint)
		{
			if (ui == null)
				return (null, IDockable.Data.DockingDir.None);

			var allDockables = new List<IDockable>();
			allDockables.AddRange(floatingUIs.Where(x => x != null && x.ParentDockable == null));
			allDockables.AddRange(floatingGroups.Where(x => x != null));

			float nearestDist = float.MaxValue;
			IDockable nearest = null;
			IDockable.Data.DockingDir nearestDir = IDockable.Data.DockingDir.None;

			foreach (var target in allDockables)
			{
				if (target == null || target == ui)
					continue;

				Rect targetRect = GetWorldRect(target.Rect);

				// 1. 겹침 검사 (Parent Dock)
				if (targetRect.Contains(mouseScreenPoint))
				{
					if (nearestDir != IDockable.Data.DockingDir.Parent)
					{
						nearest = null;
						nearestDist = float.MaxValue;
						nearestDir = IDockable.Data.DockingDir.Parent;
					}

					Vector2 targetCenter = targetRect.center;
					float _dist = Vector2.Distance(targetCenter, mouseScreenPoint);
					if (_dist < nearestDist)
					{
						nearest = target;
						nearestDist = _dist;
					}
				}

				if (nearestDir == IDockable.Data.DockingDir.Parent) continue;

				// 2. 스냅 거리 영역 검사
				Rect snapRect = ExpandRect(targetRect, snapDistance);
				if (!snapRect.Contains(mouseScreenPoint))
					continue;

				// 3. 가장 가까운 Dock 방향 계산
				var (dir, dist) = GetNearestDockDirection(targetRect, mouseScreenPoint, target);

				if (dir != IDockable.Data.DockingDir.None && dist < nearestDist)
				{
					nearest = target;
					nearestDir = dir;
					nearestDist = dist;
				}
			}

			return (nearest, nearestDir);
		}
		private Rect GetWorldRect(RectTransform rect)
		{
			Vector3[] corners = new Vector3[4];
			rect.GetWorldCorners(corners);
			return new Rect(
				corners[0].x,
				corners[0].y,
				corners[2].x - corners[0].x,
				corners[2].y - corners[0].y
			);
		}

		private Rect ExpandRect(Rect rect, float margin)
		{
			rect.xMin -= margin;
			rect.yMin -= margin;
			rect.xMax += margin;
			rect.yMax += margin;
			return rect;
		}

		private (IDockable.Data.DockingDir dir, float dist) GetNearestDockDirection(Rect targetRect, Vector2 mouse, IDockable target)
		{
			// 각 방향별 거리 계산
			float leftDist   = Mathf.Abs(mouse.x - targetRect.xMin);
			float rightDist  = Mathf.Abs(mouse.x - targetRect.xMax);
			float topDist    = Mathf.Abs(mouse.y - targetRect.yMax);
			float bottomDist = Mathf.Abs(mouse.y - targetRect.yMin);

			var distances = new (IDockable.Data.DockingDir dir, float dist)[]
			{
				(IDockable.Data.DockingDir.Left,   leftDist),
				(IDockable.Data.DockingDir.Right,  rightDist),
				(IDockable.Data.DockingDir.Top,    topDist),
				(IDockable.Data.DockingDir.Bottom, bottomDist)
			};

			// Dock 가능한 방향만 필터
			var validDirs = distances
				.Where(d => !target.DockableData.FindDock(d.dir, out _))
				.OrderBy(d => d.dist)
				.ToArray();

			return validDirs.Length > 0 ? validDirs[0] : (IDockable.Data.DockingDir.None, float.MaxValue);
		}
		#endregion

		public void AddMembersInParentGroup(IDockable group, IDockable.Data.DockingDir thiToMemberDir, IDockable member)
		{
			if (group == null || group is not FloatingGroup fGroup) return;
			if (member == null) return;

			if (thiToMemberDir == IDockable.Data.DockingDir.Parent && member is FloatingUI)
			{
				group.AddMembers(thiToMemberDir, member);
			}
			else
			{
				group.AddMembers(thiToMemberDir, member);
			}
		}
		public void RemoveMembersInParentGroup(IDockable group, IDockable member)
		{
			if (group == null || group is not FloatingGroup fGroup) return;
			if (member == null || member is not IDockable) return;

			if (group.RemoveMembers(member))
			{
				if (group.MemberList.Count == 0)
				{
					group.RelaseGroup();
					Destroy(fGroup.gameObject);
				}
			}
		}
	}
}
