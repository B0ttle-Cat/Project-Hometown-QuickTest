using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class RectUICollisionAvoidance : MonoBehaviour
{
	class RectItem
	{
		public RectTransform rect;
		public int order;
		public Vector2 velocity;
		public Vector2 currentPos;
	}

	List<RectItem> allRects = new List<RectItem>();
	Camera cam;
	public float repulsionStrength = 80f;
	public float smoothTime = 0.1f;

	void Awake()
	{
		cam = Camera.main;
	}

	// 매 프레임 중 여러 번 호출 가능
	public void SetAvoidanceRecttransform(List<RectTransform> lists, int order)
	{
		// 기존 리스트 초기화는 AvoidanceUpdate에서 처리하므로 중복 방지용만 필요
		foreach (var rt in lists)
		{
			if (rt == null) continue;
			var exist = allRects.Find(l => l.rect == rt);
			if (exist == null)
			{
				allRects.Add(new RectItem
				{
					rect = rt,
					order = order,
					currentPos = rt.position
				});
			}
			else
			{
				exist.order = order;
			}
		}

		// 보이지 않는(이번 프레임에 등록되지 않은) 항목은 나중에 제거됨
	}

	// 한 프레임에 한 번 호출됨
	public void AvoidanceUpdate()
	{
		// Step 1. 화면에 남은 RectTransform만 유지
		allRects = allRects
			.Where(l => l.rect != null && l.rect.gameObject.activeInHierarchy)
			.ToList();

		// Step 2. 충돌 감지 및 반발력 적용
		for (int i = 0 ; i < allRects.Count ; i++)
		{
			for (int j = i + 1 ; j < allRects.Count ; j++)
			{
				var a = allRects[i];
				var b = allRects[j];
				Rect rectA = GetRect(a.rect);
				Rect rectB = GetRect(b.rect);

				if (rectA.Overlaps(rectB))
				{
					Vector2 dir = (rectA.center - rectB.center).normalized;
					if (dir == Vector2.zero)
						dir = Random.insideUnitCircle.normalized;

					RectItem mover, fixedOne;
					if (a.order == b.order)
					{
						mover = a;
						fixedOne = b;
					}
					else if (a.order > b.order)
					{
						mover = a;
						fixedOne = b;
					}
					else
					{
						mover = b;
						fixedOne = a;
					}

					float overlap = Mathf.Min(rectA.width, rectB.width) * 0.5f -
									Vector2.Distance(rectA.center, rectB.center);

					if (overlap > 0)
						mover.currentPos += dir * overlap * repulsionStrength * Time.deltaTime;
				}
			}
		}

		// Step 3. 부드럽게 위치 보정
		foreach (var l in allRects)
		{
			Vector2 smooth = Vector2.SmoothDamp(l.rect.position, l.currentPos, ref l.velocity, smoothTime);
			l.rect.position = smooth;
		}

		// Step 4. 다음 프레임 준비
		foreach (var l in allRects)
		{
			l.currentPos = l.rect.position;
		}
	}

	Rect GetRect(RectTransform rt)
	{
		Vector3[] corners = new Vector3[4];
		rt.GetWorldCorners(corners);
		float xMin = corners.Min(c => c.x);
		float xMax = corners.Max(c => c.x);
		float yMin = corners.Min(c => c.y);
		float yMax = corners.Max(c => c.y);
		return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
	}

}
