using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DockableFloatingUI
{
	public interface IDragable : IPointerDownHandler, IPointerUpHandler
	{
		public struct Data
		{
			public Vector2 PointerDownPos;
			public bool IsPointerDown;
			public bool IsDragging;
			public Vector2 LastPosition;
		}
		RectTransform Rect { get; }
		RectTransform HandleBar { get; }
		Selectable HandleSelectable { get; }
		Data DragableData { get; set; }

		void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
		{
			if (!IsPointerOnHandle(eventData)) return;

			var data = DragableData;
			data.IsPointerDown = true;
			data.IsDragging = false;
			data.PointerDownPos = Mouse.current.position.ReadValue();
			data.LastPosition = data.PointerDownPos;
			DragableData = data;
		}
		void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
		{
			var data = DragableData;
			if (data.IsDragging)
			{
				data.IsDragging = false;
				WaitNextFreame();
				OnEndDrag(Mouse.current.position.ReadValue());
			}
			data.IsPointerDown = false;
			data.IsDragging = false;
			DragableData = data;
			async void WaitNextFreame()
			{
				await Awaitable.NextFrameAsync();
				if (HandleSelectable != null)
				{
					HandleSelectable.interactable = true;
				}
			}
		}
		void DragUpdate(out Vector2 delta)
		{
			delta = Vector2.zero;

			var data = DragableData;
			if (!data.IsPointerDown) return;
			if (Mouse.current == null || EventSystem.current == null) return;

			// 마우스 현재 위치
			Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

			if (!data.IsDragging)
			{
				// threshold 이상 이동 시 드래그 시작
				if (Vector2.Distance(mouseScreenPos, data.PointerDownPos) > EventSystem.current.pixelDragThreshold)
				{
					data.IsDragging = true;
					if (HandleSelectable != null)
					{
						HandleSelectable.interactable = false;
					}
					OnStartDrag();
				}
			}

			if (data.IsDragging)
			{
				delta = mouseScreenPos - data.LastPosition;
				Rect.anchoredPosition += delta;

				//LockInsideUpdate();
				data.LastPosition = mouseScreenPos;
			}
			DragableData = data;
		}
		void OnStartDrag() { }
		void OnEndDrag(Vector2 mouseScreenPoint) { }
		private bool IsPointerOnHandle(PointerEventData eventData)
		{
			if (HandleBar == null) return false;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(HandleBar, eventData.position, eventData.pressEventCamera, out var localPoint);
			return HandleBar.rect.Contains(localPoint);
		}
		private void LockInsideUpdate()
		{
			if (Rect == null) return;

			var parent = Rect.parent as RectTransform;
			if (parent == null) return;

			Vector3 pos = Rect.localPosition;
			Vector2 size = Rect.rect.size;
			Vector2 parentSize = parent.rect.size;

			// 좌우
			float xMin = -parentSize.x / 2 + size.x / 2;
			float xMax = parentSize.x / 2 - size.x / 2;
			pos.x = Mathf.Clamp(pos.x, xMin, xMax);

			// 상하
			float yMin = -parentSize.y / 2 + size.y / 2;
			float yMax = parentSize.y / 2 - size.y / 2;
			pos.y = Mathf.Clamp(pos.y, yMin, yMax);

			Rect.localPosition = pos;
		}
	}
}
