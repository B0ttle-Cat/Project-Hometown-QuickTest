using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;
using UnityEngine.UI;

namespace GameUI
{
	public class ScrollPoolingDataContainer : PoolingDataContainer
	{
		[Title("References")]
		[SerializeField, Required]
		private RectTransform poolItemRect; // 슬롯의 크기 및 프리팹 참조용

		[SerializeField, ReadOnly]
		private ScrollRect scrollRect;
		[SerializeField, ReadOnly]
		private RectTransform viewport;
		[SerializeField, ReadOnly]
		private RectTransform content;

		[Title("Settings")]
		[SerializeField, Min(0)]
		private int viewBufferCount = 1;

		// 실제 데이터를 가진 아이템 관리 (Index -> Data)
		private Dictionary<int, ITargetToPanelAPI> activeItems = new Dictionary<int, ITargetToPanelAPI>();

		// 모든 데이터에 대응하는 빈 슬롯 리스트
		private List<RectTransform> proxySlots = new List<RectTransform>();

		protected override void OnValidate()
		{
			base.OnValidate();
			InitializeReferences();
		}

		protected override void Awake()
		{
			base.Awake();
			InitializeReferences();
			if (scrollRect != null)
			{
				scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
			}
		}

		private void InitializeReferences()
		{
			if (scrollRect == null) scrollRect = GetComponentInChildren<ScrollRect>(true);
			if (scrollRect != null)
			{
				viewport = scrollRect.viewport;
				content = scrollRect.content;
			}
		}

		public override void InitData(IEnumerable<ITargetToPanelAPI> elements)
		{
			// 1. 기존 데이터 및 슬롯 제거
			ClearData();

			// 2. 데이터 리스트 확보
			dataList.AddRange(elements);

			// 3. 데이터 개수만큼 빈 슬롯(Proxy Slot) 생성
			CreateProxySlots();

			// 4. 초기 화면 갱신
			Refresh();
		}

		private void CreateProxySlots()
		{
			if (content == null || poolItemRect == null) return;

			for (int i = 0 ; i < dataList.Count ; i++)
			{
				GameObject slotGo = new GameObject($"Slot_{i}", typeof(RectTransform));
				RectTransform slotRect = slotGo.GetComponent<RectTransform>();

				slotRect.SetParent(content, false);
				slotRect.localScale = Vector3.one;
				// 참조용 프리팹과 동일한 크기 설정
				slotRect.sizeDelta = poolItemRect.sizeDelta;

				proxySlots.Add(slotRect);
			}

			// LayoutGroup이 새 자식들을 인식하도록 강제 갱신
			LayoutRebuilder.ForceRebuildLayoutImmediate(content);
		}

		public override void ClearData()
		{
			// 실제 아이템 제거
			foreach (var kvp in activeItems)
			{
				RemoveItem(kvp.Value, proxySlots[kvp.Key]);
			}
			activeItems.Clear();

			// 슬롯 오브젝트 파괴
			foreach (var slot in proxySlots)
			{
				if (slot != null) Destroy(slot.gameObject);
			}
			proxySlots.Clear();
			dataList.Clear();
		}

		private void OnScrollValueChanged(Vector2 pos)
		{
			UpdateVisibleRange();
		}

		[Button]
		public void Refresh()
		{
			UpdateVisibleRange();
		}

		private void UpdateVisibleRange()
		{
			if (scrollRect == null || viewport == null || proxySlots.Count == 0) return;

			// 뷰포트의 세계 좌표 범위를 가져옴
			Vector3[] viewWorldCorners = new Vector3[4];
			viewport.GetWorldCorners(viewWorldCorners);
			Rect viewWorldRect = new Rect(viewWorldCorners[0], viewWorldCorners[2] - viewWorldCorners[0]);

			// 버퍼 영역 확장을 위한 마진 계산 (상하좌우 버퍼 추가)
			if (viewBufferCount < 0) viewBufferCount = 0;
			float bufferMargin = (scrollRect.vertical ? poolItemRect.rect.height : poolItemRect.rect.width) * viewBufferCount;

			for (int i = 0 ; i < proxySlots.Count ; i++)
			{
				RectTransform slot = proxySlots[i];
				bool isVisible = IsRectVisibleInViewport(slot, viewWorldRect, bufferMargin);

				if (isVisible)
				{
					// 보여야 하는데 아직 없는 경우 생성
					if (!activeItems.ContainsKey(i))
					{
						ITargetToPanelAPI data = dataList[i];
						activeItems.Add(i, data);
						AddItem(data, slot); // 슬롯을 매개변수로 전달
					}
				}
				else
				{
					// 안 보여야 하는데 있는 경우 제거
					if (activeItems.ContainsKey(i))
					{
						RemoveItem(activeItems[i], slot); // 슬롯을 매개변수로 전달
						activeItems.Remove(i);
					}
				}
			}
		}

		private bool IsRectVisibleInViewport(RectTransform target, Rect viewWorldRect, float margin)
		{
			Vector3[] corners = new Vector3[4];
			target.GetWorldCorners(corners);
			Rect targetRect = new Rect(corners[0], corners[2] - corners[0]);

			// 마진 적용
			Rect expandedViewRect = new Rect(
				viewWorldRect.x - margin,
				viewWorldRect.y - margin,
				viewWorldRect.width + margin * 2,
				viewWorldRect.height + margin * 2);

			return expandedViewRect.Overlaps(targetRect);
		}


		private void OnDestroy()
		{
			if (scrollRect != null)
				scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
		}
	}
}