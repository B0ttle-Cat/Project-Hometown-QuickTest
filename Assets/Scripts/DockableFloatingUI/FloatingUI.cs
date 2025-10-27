using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DockableFloatingUI
{
	[RequireComponent(typeof(RectTransform))]
	public class FloatingUI : MonoBehaviour, IDragable, IDockable
	{
		public RectTransform ParentDockableRect => DockableData.ParentRect;
		public IDockable ParentDockable => DockableData.ParentDockable;
		public IDockable.Data.DockingDir ParentDockingDir => DockableData.dockingDir;
		public RectTransform Rect { get; private set; }
		public RectTransform HandleBar => handleBar;
		public Selectable HandleSelectable => foldToggle;
		public RectTransform MemberContentRoot => contentRoot;
		public IDragable.Data DragableData { get; set; }
		public IDockable.Data DockableData { get; set; }
		public List<IDockable> MemberList { get; set; }

		public string Name
		{
			get
			{
				if (title == null) return "";
				return title.text;
			}
			set
			{
				if (title == null) return;
				title.text = value;
			}
		}

		[Header("프리팹 구조")]
		public RectTransform handleBar;
		public Toggle foldToggle;
		public RectTransform contentRoot;
		public TMP_Text title;

		private void Awake()
		{
			Rect = GetComponent<RectTransform>();
			DockableFloatingUIManager.Instance?.RegisterFloatingUI(this);
			// 토글 이벤트 연결은 인스펙터에서 설정했으므로 코드에서는 생략
		}

		private void OnDestroy()
		{
			DockableFloatingUIManager.Instance?.UnregisterFloatingUI(this);
		}

		public void OnPointerDown(BaseEventData eventData)
		{
			(this as IDragable).OnPointerDown(eventData as PointerEventData);
		}
		public void OnPointerUp(BaseEventData eventData)
		{
			(this as IDragable).OnPointerUp(eventData as PointerEventData);
		}

		private void Update()
		{
			(this as IDragable).DragUpdate(out var _);
		}

		void IDragable.OnStartDrag()
		{
			if (ParentDockableRect == null) return;

			DockableFloatingUIManager.Instance.RemoveMembersInParentGroup(ParentDockable, this);
		}
		void IDragable.OnEndDrag(Vector2 mouseScreenPoint)
		{
			if (ParentDockableRect != null) return;

			//IDockable dockable = DockableFloatingUIManager.Instance.GetOverlapsDockable(this, mouseScreenPoint);
			(IDockable Dock, IDockable.Data.DockingDir Dir) = DockableFloatingUIManager.Instance.GetNearestDockable(this, mouseScreenPoint);
			if (Dock == null || Dir == IDockable.Data.DockingDir.None)
			{
			}
			else if (Dock is FloatingGroup nearestGroup)
			{
				DockableFloatingUIManager.Instance.AddMembersInParentGroup(Dock, Dir, this);
			}
			else if(Dock is FloatingUI floatingUI && Dir == IDockable.Data.DockingDir.Parent)
			{
				DockableFloatingUIManager.Instance.CreateFloatingGroup(floatingUI, this);
			}
		}

		#region 접기/펴기
		public void Fold()
		{
			if (contentRoot != null) contentRoot.gameObject.SetActive(false);
		}

		public void Unfold()
		{
			if (contentRoot != null) contentRoot.gameObject.SetActive(true);
		}

		public void ToggleFold()
		{
			if (contentRoot != null)
				contentRoot.gameObject.SetActive(!contentRoot.gameObject.activeSelf);
		}
		#endregion
	}
}
