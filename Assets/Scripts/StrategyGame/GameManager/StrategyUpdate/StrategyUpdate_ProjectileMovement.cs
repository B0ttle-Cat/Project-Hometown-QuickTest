using System;

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using UnityEngine;

namespace StrategyManagerModule
{
	public partial class StrategyUpdate
	{
        public class StrategyUpdate_ProjectileMovement : StrategyUpdateSubClass<StrategyUpdate_ProjectileMovement.Movement>
		{
			public struct PureMovementData : IDisposable
			{
				// transform / motion
				public float3 Position;
				public float3 PrevPosition;
				public float3 TargetPosition;
				public float3 MoveDirection;
				public float MoveSpeed;

				// delta
				public float DeltaTime;
				public float UpdateTime; // original code had updateTime decreased each frame

				// speed shift (curve)
				public bool IsShiftSpeed; // 0/1
				public float MoveStartSpeed;
				public float MoveMaxSpeed;
				public float TimeFromStartToMaxSpeed;

				// homing
				public bool HomingEnabled; // 0/1
				public float HomingTurnSpeed;
				public float HomingTurnSpeedWhenMaxSpeed;
				public float HomingLimitAngleCosine; // precomputed cosine
				public float HomingLimitSqrDistance;

				// Cep Offset
				public float3 CepOffset;

				// Random
				public uint RandomState;

				// Per-instance sampled animation curve table (owned by Mono)
				// IMPORTANT: Mono must Alloc/Dispose this; Job only reads it.
				public NativeArray<float> MoveSpeedCurveTable;

                public void Dispose()
				{
					MoveSpeedCurveTable.Dispose();
				}

                // Helper to sample MoveSpeedCurveTable (normalized t in [0,1])
                public float SampleSpeedCurve(float normalizedT)
				{
					if (!MoveSpeedCurveTable.IsCreated || MoveSpeedCurveTable.Length == 0) return 1f;
					float t = math.clamp(normalizedT, 0f, 1f);
					int len = MoveSpeedCurveTable.Length;
					float idxF = t * (len - 1);
					int idx = (int)math.floor(idxF);
					int idx1 = math.min(idx + 1, len - 1);
					float a = MoveSpeedCurveTable[idx];
					float b = MoveSpeedCurveTable[idx1];
					float frac = idxF - idx;
					return math.lerp(a, b, frac);
				}
			}

			[BurstCompile]
			public struct ProjectileMovementJob : IJobParallelFor
			{
				public NativeArray<PureMovementData> Movements;
				public void Execute(int index)
				{
					var m = Movements[index];
					float dt = m.DeltaTime;

					if (dt <= 0f) return;

					m.UpdateTime -= dt;

					// --- MoveSpeed 계산 ---
					UpdateMoveSpeed(ref m);

					// --- Homing 방향 계산 ---
					UpdateHoming(ref m, dt);

					// --- 이동 적용 ---
					ApplyMovement(ref m, dt);

					Movements[index] = m;
				}
				private static void UpdateMoveSpeed(ref PureMovementData m)
				{
					if (!m.IsShiftSpeed) return;

					if (m.UpdateTime >= m.TimeFromStartToMaxSpeed || m.TimeFromStartToMaxSpeed < 0.01f)
					{
						m.MoveSpeed = m.MoveMaxSpeed;
					}
					else if (m.MoveSpeed <= 0f)
					{
						m.MoveSpeed = m.MoveStartSpeed;
					}
					else
					{
						float timeRate = math.clamp(m.UpdateTime / m.TimeFromStartToMaxSpeed, 0f, 1f);
						float curveVal = m.SampleSpeedCurve(timeRate);
						m.MoveSpeed = math.lerp(m.MoveStartSpeed, m.MoveMaxSpeed, curveVal);
					}
				}

				private static void UpdateHoming(ref PureMovementData m, float dt)
				{
					if (!m.HomingEnabled) return;

					float3 toTarget = m.TargetPosition + m.CepOffset - m.Position;
					float sqrDist = math.lengthsq(toTarget);

					bool withinDistance = float.IsPositiveInfinity(m.HomingLimitSqrDistance) || sqrDist <= m.HomingLimitSqrDistance;

					if (!withinDistance || sqrDist <= 1e-6f) return;

					float3 newDir = math.normalize(toTarget);
					float dot = math.dot(m.MoveDirection, newDir);

					if (dot < m.HomingLimitAngleCosine) return;

					float turnSpeed = m.HomingTurnSpeed;
					if (m.IsShiftSpeed)
					{
						float denom = m.MoveMaxSpeed - m.MoveStartSpeed;
						float rateTurnSpeed = (denom > 0f) ? math.clamp((m.MoveSpeed - m.MoveStartSpeed) / denom, 0f, 1f) : 0f;
						turnSpeed = math.lerp(m.HomingTurnSpeed, m.HomingTurnSpeedWhenMaxSpeed, rateTurnSpeed);
					}

					float maxRadiansDelta = math.radians(turnSpeed) * dt;
					m.MoveDirection = RotateTowards(m.MoveDirection, newDir, maxRadiansDelta);
				}
				private static void ApplyMovement(ref PureMovementData m, float dt)
				{
					m.PrevPosition = m.Position;
					m.Position += m.MoveDirection * m.MoveSpeed * dt;
				}
				private static float3 RotateTowards(float3 current, float3 target, float maxRadiansDelta)
				{
					float3 a = math.normalize(current);
					float3 b = math.normalize(target);
					float cos = math.clamp(math.dot(a, b), -1f, 1f);
					float angle = math.acos(cos);
					if (angle <= 1e-6f) return a;

					float t = math.min(1f, maxRadiansDelta / angle);
					return math.normalize(math.lerp(a, b, t));
				}
			}


			public StrategyUpdate_ProjectileMovement(StrategyUpdate updater) : base(updater)
			{
			}
			protected override void Dispose()
			{
				StrategyManager.Pooling.RemoveChangeListener<ProjectileObject>(OnChangeValue);
			}
			protected override void Start()
			{
				UpdateList = new();

				StrategyManager.Pooling.AddChangeListener<ProjectileObject>(OnChangeValue, true);
			}
			private void OnChangeValue(GameObject element, bool added)
			{
				if (!element.TryGetComponent<ProjectileObject>(out var component)) return;


				if (added)
				{
					updateList.Add(new(component.ThisMovement, this));
				}
				else
				{
					updateList.RemoveAll(f => f.thisMovement == component.ThisMovement);
				}
			}
			public class Movement : UpdateLogic
			{
				public IProjectileMovement thisMovement;
				public PureMovementData pureMovementData;

				public Movement(IProjectileMovement movement, StrategyUpdateSubClass<Movement> thisSubClass) : base(thisSubClass)
				{
					thisMovement = movement;
					thisMovement.InitPureMovementData(out pureMovementData);
				}
				protected override void OnDispose()
				{
					thisMovement = null;
					pureMovementData.Dispose();
				}
				protected override void OnUpdate(in float deltaTime)
				{
					if (thisMovement.PureUpdateFlag)
					{
						pureMovementData.Dispose();
						thisMovement.InitPureMovementData(out pureMovementData);
					}
					else
					{
						thisMovement.ApplyJobResult(in pureMovementData);
					}
				}
				public void PureUpdate()
				{
					thisMovement.UpdatePureMovementData(ref pureMovementData);
				}
			}

			protected override void Update(in float deltaTime)
			{
				int length = UpdateList.Count;
				if (length == 0) return;

				// Create native array for job (TempJob)
				var movements = new NativeArray<PureMovementData>(length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);


				// Mono -> Native: read once per instance
				for (int i = 0 ; i < length ; i++)
				{
					var entry = UpdateList[i];
					if (entry == null || entry.thisMovement == null)
					{
						// default-initialize (safe)
						movements[i] = default;
						continue;
					}

					// ensure curve sampling is prepared by the Mono object (we assume Mono did this on Init/SetTarget)
					// but check defensively:
					entry.PureUpdate();
					var pd = entry.pureMovementData;
					pd.DeltaTime = deltaTime;

					movements[i] = pd;
				}

				// schedule job
				var job = new ProjectileMovementJob
				{
					Movements = movements
				};

				JobHandle handle = job.Schedule(length, 64);
				handle.Complete();

				// Apply results back to Mono (single write per instance)
				for (int i = 0 ; i < length ; i++)
				{
					var entry = UpdateList[i];
					if (entry == null || entry.thisMovement == null)
					{
						continue;
					}
					entry.pureMovementData = movements[i];
					entry.Update(in deltaTime);
				}
				movements.Dispose();
			}
		}

	}
}