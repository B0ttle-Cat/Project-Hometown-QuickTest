using System;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;

namespace GameUI
{
	[DefaultExecutionOrder(10)]
	public class LabelOverlapController : MonoBehaviour
	{
		[Flags]
		public enum PushMode
		{
			[HideInInspector]
			None = 0,
			Up = 1 << 0, Down = 1 << 1, Left = 1 << 2, Right = 1 << 3,
			Horizontal = Left | Right, Vertical = Up | Down, All = Horizontal | Vertical
		}

		[EnumToggleButtons]
		public PushMode GlobalPushMode = PushMode.All;
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

			// 1단계: 우선순위 정렬
			_items.Sort((a, b) => a.Priority.CompareTo(b.Priority));

			// 2단계: 겹침 계산
			for (int i = 0 ; i < _items.Count ; i++)
			{
				var itemA = _items[i];
				if (!itemA.gameObject.activeInHierarchy || itemA.Priority < 0) continue;

				for (int j = i + 1 ; j < _items.Count ; j++)
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

			Vector2 diff = rectB.center - rectA.center;

			// 1. 완전히 동일한 위치일 경우, 위쪽 방향으로 아주 미세한 오차를 강제 부여하여 방향성 생성
			if (diff.sqrMagnitude < 0.0001f)
			{
				diff = new Vector2(0f, 0.01f);
			}

			Vector2 pushVec = Vector2.zero;

			bool canPushUp = (GlobalPushMode & PushMode.Up) != 0;
			bool canPushDown = (GlobalPushMode & PushMode.Down) != 0;
			bool canPushLeft = (GlobalPushMode & PushMode.Left) != 0;
			bool canPushRight = (GlobalPushMode & PushMode.Right) != 0;

			// 2. 각 축별 최소 밀어내기 거리 계산
			float overlapX = (diff.x >= 0) ? (rectA.xMax - rectB.xMin) : (rectA.xMin - rectB.xMax);
			float overlapY = (diff.y >= 0) ? (rectA.yMax - rectB.yMin) : (rectA.yMin - rectB.yMax);

			float absOverlapX = Mathf.Abs(overlapX);
			float absOverlapY = Mathf.Abs(overlapY);

			// 3. XY 중 더 적게 이동해도 되는(가까운) 방향을 선택하되, PushMode 권한 확인
			bool tryX = false;
			bool tryY = false;

			// 어느 방향이 물리적으로 더 가까운지 결정
			if (absOverlapX <= absOverlapY)
			{
				// X축이 더 가깝지만, 해당 방향으로 밀 수 있는지 체크
				if ((overlapX > 0 && canPushRight) || (overlapX < 0 && canPushLeft)) tryX = true;
				else if ((overlapY > 0 && canPushUp) || (overlapY < 0 && canPushDown)) tryY = true;
			}
			else
			{
				// Y축이 더 가깝지만, 해당 방향으로 밀 수 있는지 체크
				if ((overlapY > 0 && canPushUp) || (overlapY < 0 && canPushDown)) tryY = true;
				else if ((overlapX > 0 && canPushRight) || (overlapX < 0 && canPushLeft)) tryX = true;
			}

			if (tryX) pushVec.x = overlapX;
			else if (tryY) pushVec.y = overlapY;

			if (pushVec == Vector2.zero) return;

			// 4. 우선순위 분배
			if (itemA.Priority < itemB.Priority)
			{
				itemB.CurrentOffset += pushVec;
			}
			else if (itemA.Priority > itemB.Priority)
			{
				itemA.CurrentOffset -= pushVec;
			}
			else
			{
				// 동일 우선순위 시 반반씩 밀어냄
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