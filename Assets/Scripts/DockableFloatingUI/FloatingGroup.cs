using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DockableFloatingUI
{
	[RequireComponent(typeof(RectTransform))]
	public class FloatingGroup : MonoBehaviour, IDragable, IDockable
	{
		public RectTransform ParentDockableRect => DockableData.ParentRect;
		public IDockable ParentDockable => DockableData.ParentDockable;
		public IDockable.Data.DockingDir ParentDockingDir => DockableData.dockingDir;
		public RectTransform Rect { get; private set; }
		public RectTransform HandleBar => handleBar;
		public Selectable HandleSelectable => null;
		public RectTransform MemberContentRoot => contentRoot;
		public IDragable.Data DragableData { get; set; }
		public IDockable.Data DockableData { get; set; }
		public List<IDockable> MemberList { get => members; set => members = value; }

		public string Name
		{
			get
			{
				if (title == null) return "";
				return title.text;
			}
			set {
				if (title == null) return;
				title.text = value;
			}
		}

		public List<IDockable> members = new List<IDockable>();

		[Header("프리팹 구조")]
		public RectTransform handleBar;
		public RectTransform contentRoot;
		public VerticalLayoutGroup layoutGroup;
		public TMP_Text title;

		private void Awake()
		{
			Rect = GetComponent<RectTransform>();
			
			// Manager 등록
			DockableFloatingUIManager.Instance?.RegisterGroup(this);

			// layoutGroup이 null이면 contentRoot에서 가져오기
			if (layoutGroup == null && contentRoot != null)
				layoutGroup = contentRoot.GetComponent<VerticalLayoutGroup>();
		}

		private void OnDestroy()
		{
			DockableFloatingUIManager.Instance?.UnregisterGroup(this);
		}

		#region Drag
		public void OnPointerDown(BaseEventData eventData)
		{
			(this as IDragable).OnPointerDown(eventData as PointerEventData);
		}
		public void OnPointerUp(BaseEventData eventData)
		{
			(this as IDragable).OnPointerUp(eventData as PointerEventData);
		}
		void IDragable.OnStartDrag()
		{
			if (ParentDockableRect == null) return;

			DockableFloatingUIManager.Instance.RemoveMembersInParentGroup(ParentDockable, this);
		}
		void IDragable.OnEndDrag(Vector2 mouseScreenPoint)
		{
			if (ParentDockableRect != null) return;

			(IDockable Dock, IDockable.Data.DockingDir Dir) = DockableFloatingUIManager.Instance.GetNearestDockable(this, mouseScreenPoint);
			if (Dock == null || Dir == IDockable.Data.DockingDir.None)
			{
			}
			else if (Dock is FloatingGroup nearestGroup && Dir != IDockable.Data.DockingDir.Parent)
			{
				DockableFloatingUIManager.Instance.AddMembersInParentGroup(Dock, Dir, this);
			}
		}
		private void Update()
		{
			(this as IDragable).DragUpdate(out var delta);
			if (DragableData.IsDragging)
			{
				(this as IDockable).MoveChildDockingUI(in delta);
			}
		}
		#endregion
	}
}
