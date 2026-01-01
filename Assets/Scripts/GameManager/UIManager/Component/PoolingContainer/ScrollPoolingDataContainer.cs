using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;
using UnityEngine.UI;

namespace GameUI
{
	public class ScrollPoolingDataContainer : PoolingDataContainer
	{
		[SerializeField, Required]
		private RectTransform poolItemRect;


		[SerializeField, ReadOnly, Required]
		private ScrollRect scrollRect;
		[SerializeField, ReadOnly, Required]
		private RectTransform content;
		[SerializeField, ReadOnly, Required]
		private LayoutGroup layoutGroup;

		[Header("Settings")]
		[SerializeField]
		private int viewBufferCount = 2;

		[SerializeField,ReadOnly,HorizontalGroup]
		private int minIndex = -1;
		[SerializeField,ReadOnly,HorizontalGroup]
		private int maxIndex = -1;

		public ScrollRect ScrollRect
		{
			get
			{
				if (scrollRect.IsNullRef())
					scrollRect = GetComponentInChildren<ScrollRect>(true);
				return scrollRect;
			}
		}
		public RectTransform Content
		{
			get
			{
				if (content.IsNullRef())
				{
					scrollRect = ScrollRect;
					if (scrollRect.IsNullRef()) return null;
					content = ScrollRect.content;
				}
				return content;
			}
		}
		public LayoutGroup LayoutGroup
		{
			get
			{
				if (layoutGroup.IsNullRef())
				{
					content = Content;
					if (content.IsNullRef()) return null;
					layoutGroup = content.GetComponentInChildren<LayoutGroup>(true);
				}
				return layoutGroup;
			}
		}

		private void Reset()
		{
			OnValidate();
		}

		protected override void OnValidate()
		{
			scrollRect = ScrollRect;
			content = Content;
			base.OnValidate();
			layoutGroup = LayoutGroup;
		}

		protected override void Awake()
		{
			base.Awake();
			if (scrollRect != null)
			{
				scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
			}
		}

		public override void InitData(IEnumerable<ITargetToPanelAPI> cardElements) 
		{
			dataList.Clear();
			foreach (var item in cardElements)
			{
				dataList.Add(item);
			}
			Refresh();
		}
		public override void ClearData() { 
			dataList.Clear(); 
		}
		public override void AddData(ITargetToPanelAPI item)
		{
			dataList.Add(item);
			Refresh();
		}
		public override void RemoveData(ITargetToPanelAPI item) 
		{
			dataList.Remove(item);
			Refresh();
		}
		public override void AddData(IEnumerable<ITargetToPanelAPI> cardElements) 
		{
			foreach (var item in cardElements)
			{
				dataList.Add(item);
			}
			Refresh();
		}
		public override void RemoveData(IEnumerable<ITargetToPanelAPI> cardElements)
		{
			foreach (var item in cardElements)
			{
				dataList.Remove(item);
			}
			Refresh();
		}
		private void OnScrollValueChanged(Vector2 value)
		{
			UpdateVisibleRange();
		}
		[Button] public void Refresh()
		{
			minIndex = -1;
			maxIndex = -1;
			UpdateVisibleRange();
		}
		private void UpdateVisibleRange()
		{
			if (scrollRect == null || content == null || layoutGroup == null || dataList.Count == 0) return;

			Rect viewportRect = ((RectTransform)scrollRect.transform).rect;
			Vector2 contentPos = content.anchoredPosition;

			int currentMinIndex = 0;
			int currentMaxIndex = 0;

			if (layoutGroup is GridLayoutGroup grid)
			{
				float cellSizeX = grid.cellSize.x + grid.spacing.x;
				float cellSizeY = grid.cellSize.y + grid.spacing.y;
				int constraintCount = grid.constraintCount;

				if (scrollRect.vertical)
				{
					float startY = contentPos.y - grid.padding.top;
					int minRow = Mathf.FloorToInt(startY / cellSizeY);
					int maxRow = Mathf.CeilToInt((startY + viewportRect.height) / cellSizeY);
					currentMinIndex = minRow * constraintCount;
					currentMaxIndex = (maxRow * constraintCount) - 1;
				}
				else
				{
					float startX = -contentPos.x - grid.padding.left;
					int minCol = Mathf.FloorToInt(startX / cellSizeX);
					int maxCol = Mathf.CeilToInt((startX + viewportRect.width) / cellSizeX);
					currentMinIndex = minCol * constraintCount;
					currentMaxIndex = (maxCol * constraintCount) - 1;
				}
			}
			else if (layoutGroup is HorizontalOrVerticalLayoutGroup linear)
			{
				float spacing = linear.spacing;
				float currentOffset = 0;

				if (scrollRect.vertical)
				{
					currentOffset = contentPos.y - linear.padding.top;
					float totalSize = 0;
					bool minFound = false;
					for (int i = 0 ; i < dataList.Count ; i++)
					{
						// 리니어 레이아웃은 모든 요소의 크기가 같다고 가정하거나 별도 측정이 필요하나
						// 일반적으로 동일 프리팹을 사용하므로 첫 번째 자식의 크기를 기준으로 계산
						float itemSize = GetItemSize(linear);
						if (!minFound && totalSize + itemSize >= currentOffset)
						{
							currentMinIndex = i;
							minFound = true;
						}
						if (totalSize >= currentOffset + viewportRect.height)
						{
							currentMaxIndex = i;
							break;
						}
						totalSize += (itemSize + spacing);
						currentMaxIndex = i;
					}
				}
				else
				{
					currentOffset = -contentPos.x - linear.padding.left;
					float totalSize = 0;
					bool minFound = false;
					for (int i = 0 ; i < dataList.Count ; i++)
					{
						float itemSize = GetItemSize(linear);
						if (!minFound && totalSize + itemSize >= currentOffset)
						{
							currentMinIndex = i;
							minFound = true;
						}
						if (totalSize >= currentOffset + viewportRect.width)
						{
							currentMaxIndex = i;
							break;
						}
						totalSize += (itemSize + spacing);
						currentMaxIndex = i;
					}
				}
			}

			// 버퍼 및 범위 보정
			int bufferAmount = (layoutGroup is GridLayoutGroup g) ? g.constraintCount * viewBufferCount : viewBufferCount;
			currentMinIndex = Mathf.Clamp(currentMinIndex - bufferAmount, 0, dataList.Count - 1);
			currentMaxIndex = Mathf.Clamp(currentMaxIndex + bufferAmount, 0, dataList.Count - 1);

			if (minIndex != currentMinIndex || maxIndex != currentMaxIndex)
			{
				ApplyIndexChanges(currentMinIndex, currentMaxIndex);
			}
		}
		private float GetItemSize(HorizontalOrVerticalLayoutGroup layout)
		{
			var cardRect = poolItemRect.IsNullRef()? default : poolItemRect.rect;
			if (cardRect.size == Vector2.zero)
			{
				if (layout.transform.childCount == 0) return 1;
				RectTransform child = layout.transform.GetChild(0) as RectTransform;
				return (layout is VerticalLayoutGroup) ? child.rect.height : child.rect.width;
			}
			else
			{
				return (layout is VerticalLayoutGroup) ? cardRect.height : cardRect.width;
			}
		}
		private void ApplyIndexChanges(int newMin, int newMax)
		{
			if (minIndex != -1 && maxIndex != -1)
			{
				for (int i = minIndex ; i <= maxIndex ; i++)
				{
					if (i < newMin || i > newMax)
					{
						RemoveItem(dataList[i]);
					}
				}
			}

			for (int i = newMin ; i <= newMax ; i++)
			{
				if (i < minIndex || i > maxIndex)
				{
					// 리니어 레이아웃의 경우 순서가 중요하므로 인덱스에 맞춰 삽입 로직 분기 가능
					if (i < minIndex) AddItem(dataList[i], false);
					else AddItem(dataList[i], true);
				}
			}

			minIndex = newMin;
			maxIndex = newMax;
		}

		private void OnDestroy()
		{
			if (scrollRect != null)
			{
				scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
			}
		}
	}
}