using System.Collections.Generic;
using System.Linq;

using Sirenix.OdinInspector;

using UnityEngine;

namespace GameUI
{
	[DefaultExecutionOrder(10)]
	public class LabelOverlapController : MonoBehaviour
	{
		[Title("Physics Simulation Settings")]
		[SerializeField] private int stepsPerPriority = 5;  // 우선순위 그룹당 시뮬레이션 스텝 수
		[SerializeField] private float timeStep = 0.02f;    // 시뮬레이션 델타 타임

		[Title("Mass & Force Settings")]
		[SerializeField] private float baseMass = 1.0f;     // 기본 질량
		[SerializeField] private float massScale = 0.05f;   // 거리당 질량 증가 계수
		[SerializeField] private float springK = 150f;      // 복원력 계수 (강도)

		private List<LabelPositionItem> _items = new List<LabelPositionItem>();

		public void AddItem(LabelPositionItem item)
		{
			if (item == null) return;
			if (!_items.Contains(item))
			{
				_items.Add(item);
				PrepareRigidbody(item);
			}
		}

		private void PrepareRigidbody(LabelPositionItem item)
		{
			var rb = item.labelRigidbody2D;
			if (rb != null)
			{
				rb.bodyType = RigidbodyType2D.Dynamic;
				rb.gravityScale = 0;
				rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
				rb.constraints = RigidbodyConstraints2D.FreezeRotation;
				rb.mass = baseMass;
			}
		}

		public void RemoveItem(LabelPositionItem item)
			=> _items.Remove(item);

		private void LateUpdate()
		{
			_items.RemoveAll(item => item == null || !item.gameObject.activeInHierarchy);

			if (_items.Count <= 0) return;

			// 1. 초기 상태 세팅
			foreach (LabelPositionItem item in _items)
			{
				var rb = item.labelRigidbody2D;
				rb.simulated = false;
				rb.bodyType = RigidbodyType2D.Dynamic; // 매 프레임 다이내믹으로 초기화 후 단계별 키네마틱 전환
				rb.linearVelocity = Vector2.zero;
				rb.position = item.OriginalScreenPos;
				rb.mass = baseMass;
			}

			// 2. 우선순위별 그룹화 (내림차순)
			var priorityGroups = _items.GroupBy(x => x.Priority)
									   .OrderByDescending(g => g.Key)
									   .ToList();

			// 3. 단계적 시뮬레이션 수행
			foreach (var group in priorityGroups)
			{
				// 현재 그룹 활성화
				foreach (var item in group)
				{
					item.labelRigidbody2D.simulated = true;
					item.labelRigidbody2D.bodyType = RigidbodyType2D.Dynamic;
				}

				for (int i = 0 ; i < stepsPerPriority ; i++)
				{
					foreach (var item in _items)
					{
						// 시뮬레이션 중인 아이템(현재 그룹 + 상위 그룹)만 힘 계산
						if (!item.labelRigidbody2D.simulated) continue;

						// 키네마틱으로 변한 상위 그룹은 힘을 계산할 필요 없음
						if (item.labelRigidbody2D.bodyType == RigidbodyType2D.Kinematic) continue;

						ApplyDynamicMassAndForce(item);
					}

					// 수동 물리 연산 수행
					Physics2D.Simulate(timeStep);
					Physics2D.SyncTransforms();
				}

				// 현재 우선순위 그룹 배치 완료 -> 키네마틱 전환 (하위 그룹이 밀지 못하게 고정)
				foreach (var item in group)
				{
					var rb = item.labelRigidbody2D;
					rb.linearVelocity = Vector2.zero;
				//	rb.bodyType = RigidbodyType2D.Kinematic;
				}
			}

			// 4. 시뮬레이션 결과를 좌표에 적용
			foreach (var item in _items)
			{
				var rb = item.labelRigidbody2D;
				item.CurrentOffset = rb.position - item.OriginalScreenPos;
				item.ApplySmoothPosition();
			}
		}

		private void ApplyDynamicMassAndForce(LabelPositionItem item)
		{
			var rb = item.labelRigidbody2D;
			Vector2 direction = item.OriginalScreenPos - rb.position;
			float distance = direction.magnitude;

			if (distance > 0.1f)
			{
				// 거리가 멀수록 무게를 늘림
				// 질량이 커지면 외부 충격(다른 UI가 밀치는 힘)에 더 강하게 저항함
				rb.mass = baseMass + (distance * massScale);

				// 복원력 가함
				rb.linearVelocity = direction * springK;

				// 댐핑: 속도가 너무 빠르면 제어력 상실하므로 감쇠 적용
				//rb.linearVelocity *= 0.92f;
			}
			else
			{
				rb.mass = baseMass;
				rb.linearVelocity = Vector2.zero;
			}
		}
	}
}