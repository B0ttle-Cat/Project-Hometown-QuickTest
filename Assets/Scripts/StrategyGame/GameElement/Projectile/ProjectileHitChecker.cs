using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

public class ProjectileHitChecker : MonoBehaviour
{

	public struct HitCheckJobData
	{
		public int ProjectileIndex;

		// 발사체의 궤적
		public float3 PrevPosition;
		public float3 CurrentPosition;

		// 발사체의 충돌 특성
		public float CollisionRadius;
		public int PiercingCount; // 현재 프레임 시작 시의 관통 횟수 (Read Only)

		// 이미 맞춘 개체 Entity ID Set의 키 (선택 사항: 복잡도 증가)
		// 현재는 단순화를 위해 Job 내에서는 이미 맞춘 개체 확인을 생략하고 Mono에서 처리하는 것으로 가정
	}
	[System.Serializable]
	public struct ProjectileHitResult
	{
		// 충돌을 일으킨 발사체의 인덱스 (HitCheckJobData의 ProjectileIndex와 동일)
		public int ProjectileIndex;

		// 충돌한 대상의 Entity ID
		public Entity HitEntity;
	}
	public void HitCheck()
	{
		Physics.SphereCastAll(transform.position, 0.5f, Vector3.forward);	
	}
}
