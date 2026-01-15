using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;

namespace GameUI
{
	[DefaultExecutionOrder(10)]
	public class LabelOverlapController : MonoBehaviour
	{
		public enum PushMode
		{
			None = 0,
			Up,          // 전역 위로
			Down,        // 전역 아래로
			Left,        // 전역 왼쪽으로
			Right,        // 전역 오른쪽으로
			Directional, // 충돌 발생 방향으로 밀어내기
		}

		[EnumToggleButtons]
		public PushMode GlobalPushMode = PushMode.Directional;

		private List<LabelPositionItem> _items = new List<LabelPositionItem>();

		public void AddItem(LabelPositionItem item)
		{
			if (item == null) return;
			if (!_items.Contains(item)) _items.Add(item);
		}

		public void RemoveItem(LabelPositionItem item)
		{
			if (item == null) return;
			_items.Remove(item);
		}

		private void LateUpdate()
		{
			_items.RemoveAll(item => item == null);

			if (GlobalPushMode == PushMode.None || _items.Count <= 1)
			{
				foreach (var item in _items) item.ApplySmoothPosition();
				return;
			}

			// 1단계: 우선순위 정렬 (값이 작을수록 절대적인 우선순위 높음)
			_items.Sort((a, b) => a.Priority.CompareTo(b.Priority));

			// 2단계: 겹침 계산 (여러 번 반복하여 수렴시키거나 단일 패스로 처리)
			for (int i = 0 ; i < _items.Count ; i++)
			{
				var itemA = _items[i];
				if (!itemA.gameObject.activeInHierarchy || itemA.Priority < 0) continue;

				for (int j = i + 1 ; j < _items.Count ; j++) // i+1부터 시작하여 중복 체크 방지
				{
					var itemB = _items[j];
					if (!itemB.gameObject.activeInHierarchy || itemB.Priority < 0) continue;

					ResolveOverlap(itemA, itemB);
				}
			}

			// 3단계: 최종 위치 적용
			foreach (var item in _items)
			{
				item.ApplySmoothPosition();
			}
		}

		private void ResolveOverlap(LabelPositionItem itemA, LabelPositionItem itemB)
		{
			Rect rectA = GetWorldRectWithMargin(itemA);
			Rect rectB = GetWorldRectWithMargin(itemB);

			if (!rectA.Overlaps(rectB)) return;

			Vector2 pushVec = Vector2.zero;

			// 밀어낼 거리 계산
			switch (GlobalPushMode)
			{
				case PushMode.Directional:
				Vector2 diff = rectB.center - rectA.center;
				if (Mathf.Abs(diff.x) / rectA.width > Mathf.Abs(diff.y) / rectA.height)
					pushVec = new Vector2(diff.x > 0 ? (rectA.xMax - rectB.xMin) : (rectA.xMin - rectB.xMax), 0);
				else
					pushVec = new Vector2(0, diff.y > 0 ? (rectA.yMax - rectB.yMin) : (rectA.yMin - rectB.yMax));
				break;

				case PushMode.Up:
				pushVec = new Vector2(0, rectA.yMax - rectB.yMin);
				break;

				case PushMode.Down:
				pushVec = new Vector2(0, rectA.yMin - rectB.yMax);
				break;

				case PushMode.Left:
				pushVec = new Vector2(rectA.xMin - rectB.xMax, 0);
				break;

				case PushMode.Right:
				pushVec = new Vector2(rectA.xMax - rectB.xMin, 0);
				break;
			}

			// 우선순위에 따른 분배 로직
			if (itemA.Priority < itemB.Priority)
			{
				// A가 확실히 우선순위가 높음: B만 밀어냄
				itemB.CurrentOffset += pushVec;
			}
			else if (itemA.Priority > itemB.Priority)
			{
				// B가 확실히 우선순위가 높음: A만 반대로 밀어냄
				itemA.CurrentOffset -= pushVec;
			}
			else
			{
				// 우선순위가 동등함: 서로 절반씩 반대 방향으로 밀어냄 (동등한 관계)
				itemA.CurrentOffset -= pushVec * 0.5f;
				itemB.CurrentOffset += pushVec * 0.5f;
			}
		}

		private Rect GetWorldRectWithMargin(LabelPositionItem item)
		{
			RectTransform rt = item.RectTransform;
			Vector2 size = Vector2.Scale(rt.rect.size, rt.lossyScale);
			Vector2 pos = item.OriginalScreenPos + item.CurrentOffset;

			float left = item.MarginOffset.x * rt.lossyScale.x;
			float right = item.MarginOffset.y * rt.lossyScale.x;
			float top = item.MarginOffset.z * rt.lossyScale.y;
			float bottom = item.MarginOffset.w * rt.lossyScale.y;

			float xMin = pos.x - (size.x * rt.pivot.x) - left;
			float yMin = pos.y - (size.y * rt.pivot.y) - bottom;
			float width = size.x + left + right;
			float height = size.y + top + bottom;

			return new Rect(xMin, yMin, width, height);
		}
	}
}