using System;
using System.Collections.Generic;

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using UnityEngine;

// ProjectileMovement.ProjectileKey와 ProjectileMovement.ProjectileConstantData를 사용
using static ProjectileMovement;
using static StrategyGamePlayData;

namespace StrategyManagerModule
{
	public partial class StrategyUpdate
	{
		public class StrategyUpdate_ProjectileMovement : StrategyUpdateSubClass<StrategyUpdate_ProjectileMovement.Movement>
		{
			// 모든 발사체 키에 대한 상수 데이터를 NativeHashMap으로 관리
			private NativeHashMap<int, MovmentConstantData> constantDataMap;
			// Curve 정보 (시작 인덱스, 해상도)
			private NativeHashMap<int, CurveInfo> curveInfoMap;

			// Curve 데이터 생성에 사용할 해상도 상수를 정의합니다.
			private const int CURVE_RESOLUTION = 128;
			private NativeList<float> curveDataList;

			public struct CurveInfo
			{
				// 전체 Curve NativeArray에서 해당 Curve가 시작하는 인덱스
				public int StartIndex;
				// Curve 데이터의 길이 (해상도)
				public int Resolution;
			}

			[BurstCompile]
			public struct ProjectileMovementJob : IJobParallelFor
			{
				public NativeArray<MovementJobData> Movements;

				// Job에 상수 데이터 HashMap을 읽기 전용으로 전달
				[ReadOnly] public NativeHashMap<int, MovmentConstantData> ConstantDataMap;
				[ReadOnly] public NativeHashMap<int, CurveInfo> CurveInfoMap; 
				[ReadOnly] public NativeArray<float> CurveDataArray;
				public void Execute(int index)
				{
					var m = Movements[index];
					float dt = m.DeltaTime;

					if (dt <= 0f) return;

					// 1. Key를 이용해 HashMap에서 상수 데이터 조회
					MovmentConstantData c;
					if (!ConstantDataMap.TryGetValue(m.ProjectileKey, out c))
					{
						// 키가 Map에 존재하지 않으면 해당 발사체는 처리하지 않고 종료
						return;
					}

					m.UpdateTime -= dt;

					// --- MoveSpeed 계산 ---
					UpdateMoveSpeed(ref m, in c, CurveInfoMap, CurveDataArray);

					// --- Homing 방향 계산 ---
					UpdateHoming(ref m, dt, in c);

					// --- 이동 적용 ---
					ApplyMovement(ref m, dt);

					Movements[index] = m;
				}

				// 2. UpdateMoveSpeed: ConstantData c를 사용하여 속도 계산
				private void UpdateMoveSpeed(ref MovementJobData m, in MovmentConstantData c,
									  in NativeHashMap<int, CurveInfo> curveInfoMap,
									  in NativeArray<float> curveDataArray)
				{
					if (!c.IsShiftSpeed) return;

					if (m.UpdateTime >= c.TimeFromStartToMaxSpeed || c.TimeFromStartToMaxSpeed < 0.01f)
					{
						m.MoveSpeed = c.MoveMaxSpeed;
					}
					else if (m.MoveSpeed <= 0f)
					{
						m.MoveSpeed = c.MoveStartSpeed;
					}
					else
					{
						float timeRate = math.clamp(m.UpdateTime / c.TimeFromStartToMaxSpeed, 0f, 1f);

						// 변경: SampleCurveTable을 사용하여 커브 값 조회 및 적용
						float curveVal = SampleCurveTable(m.ProjectileKey, timeRate, curveInfoMap, curveDataArray);

						m.MoveSpeed = math.lerp(c.MoveStartSpeed, c.MoveMaxSpeed, curveVal);
					}
				}

				// 3. UpdateHoming: ConstantData c를 사용하여 호밍 계산
				private static void UpdateHoming(ref MovementJobData m, float dt, in MovmentConstantData c)
				{
					if (!c.HomingEnabled) return;

					float3 toTarget = m.TargetPosition + m.CepOffset - m.Position;
					float sqrDist = math.lengthsq(toTarget);

					bool withinDistance = float.IsPositiveInfinity(c.HomingLimitSqrDistance) || sqrDist <= c.HomingLimitSqrDistance;

					if (!withinDistance || sqrDist <= 1e-6f) return;

					float3 newDir = math.normalize(toTarget);
					float dot = math.dot(m.MoveDirection, newDir);

					if (dot < c.HomingLimitAngleCosine) return;

					float turnSpeed = c.HomingTurnSpeed;
					if (c.IsShiftSpeed)
					{
						float denom = c.MoveMaxSpeed - c.MoveStartSpeed;
						float rateTurnSpeed = (denom > 0f) ? math.clamp((m.MoveSpeed - c.MoveStartSpeed) / denom, 0f, 1f) : 0f;
						turnSpeed = math.lerp(c.HomingTurnSpeed, c.HomingTurnSpeedWhenMaxSpeed, rateTurnSpeed);
					}

					float maxRadiansDelta = math.radians(turnSpeed) * dt;
					m.MoveDirection = RotateTowards(m.MoveDirection, newDir, maxRadiansDelta);
				}

				// 4. ApplyMovement: 이동 적용
				private static void ApplyMovement(ref MovementJobData m, float dt)
				{
					m.PrevPosition = m.Position;
					m.Position += m.MoveDirection * m.MoveSpeed * dt;
				}

				// 5. RotateTowards: 방향 회전 유틸리티
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

				private float SampleCurveTable(int key, float normalizedT,
									   in NativeHashMap<int, CurveInfo> curveInfoMap,
									   in NativeArray<float> curveDataArray)
				{
					CurveInfo info;
					if (!curveInfoMap.TryGetValue(key, out info))
					{
						return normalizedT; // 정보가 없으면 선형 보간 (Default)
					}

					// T값을 인덱스로 변환
					int index = (int)(normalizedT * (info.Resolution - 1));

					// 인덱스 범위 안전성 보장
					index = math.clamp(index, 0, info.Resolution - 1);

					// 전체 NativeArray에서 오프셋을 적용하여 값을 조회
					return curveDataArray[info.StartIndex + index];
				}
			}


			public StrategyUpdate_ProjectileMovement(StrategyUpdate updater) : base(updater)
			{
			}

			// 6. Dispose: HashMap 해제
			protected override void Dispose()
			{
				StrategyManager.Pooling.RemoveChangeListener<ProjectileObject>(OnChangeValue);

				if (constantDataMap.IsCreated) constantDataMap.Dispose();
				if (curveInfoMap.IsCreated) curveInfoMap.Dispose(); // 신규 해제
				if (curveDataList.IsCreated) curveDataList.Dispose(); // 해제
			}

			// 7. Start: HashMap 초기화 (실제 데이터 로드 로직 필요)
			protected override void Start()
			{
				SetupNativeData();
				StrategyManager.Pooling.AddChangeListener<ProjectileObject>(OnChangeValue, true);

				void SetupNativeData()
				{
					List<float> allCurveData = new List<float>();

					// 1. 모든 ProjectileKey 조회 (가정)
					var allKeys = Enum.GetValues(typeof(ProjectileKey));
					int totalKeys = allKeys.Length;

					// 2. HashMap 초기화 (총 키 개수만큼 용량 확보)
					constantDataMap = new NativeHashMap<int, MovmentConstantData>(totalKeys, Allocator.Persistent);
					curveInfoMap = new NativeHashMap<int, CurveInfo>(totalKeys, Allocator.Persistent);
					
					// Curve Data 누적 및 Map 채우기
					foreach (ProjectileKey key in allKeys)
					{
						if (key == ProjectileKey.None) continue; // Default 키는 제외하거나 별도 처리
						int keyInt = (int)key;
						if (!StrategyManager.Key2Projectile.TryGetAsset(key, out var asset)) continue;
						if (asset.ProjectileProfileObject == null) continue;
						ProjectileStatsData stats = asset.ProjectileProfileObject.statsData;
						if (stats == null) continue;

						MovmentConstantData constantData = stats.GetMovementConstantData();
						constantDataMap.Add(keyInt, constantData);

						// 4. Curve 데이터 생성 및 합치기
						NativeArray<float> curveData = stats.GetMovementCurveData(CURVE_RESOLUTION, Allocator.Temp);

						int startIndex = allCurveData.Count;

						// Curve Info 맵에 등록
						curveInfoMap.Add(keyInt, new CurveInfo
						{
							StartIndex = startIndex,
							Resolution = CURVE_RESOLUTION
						});

						// Curve 데이터를 임시 리스트에 합치기
						for (int i = 0 ; i < curveData.Length ; i++)
						{
							allCurveData.Add(curveData[i]);
						}

						curveData.Dispose(); // Temp NativeArray 해제
					}

					// 5. 최종 Curve Data Array 생성 및 할당
					curveDataList = new NativeList<float>(allCurveData.Count, Allocator.Persistent);
					NativeArray<float> tempArray = allCurveData.ToNativeArray<float>(Allocator.Temp);
					curveDataList.AddRange(tempArray);
					tempArray.Dispose(); // 임시 배열 해제

					// 임시 리스트 해제
					allCurveData.Clear();
				}
			}

			private void OnChangeValue(GameObject element, bool added)
			{
				if (!element.TryGetComponent<ProjectileObject>(out var component)) return;

				if (added)
				{
					updateList.Add(new(component, this));
				}
				else
				{
					updateList.RemoveAll(f => f.thisProjectile == component);
				}
			}
			public void RegisterNewProjectileData(int key)
			{
				// 1. 키 중복 검사
				ProjectileKey enumKey = (ProjectileKey)key;

				if (constantDataMap.ContainsKey(key))
				{
					Debug.LogWarning($"[ProjectileManager] Key {enumKey} already exists. Skipping registration.");
					return;
				}

				if (!StrategyManager.Key2Projectile.TryGetAsset(enumKey, out var asset)) return;
				if (asset.ProjectileProfileObject == null) return;
				ProjectileStatsData stats = asset.ProjectileProfileObject.statsData;
				if (stats == null) return;

				// 2. 상수 데이터 추가
				MovmentConstantData constantData = stats.GetMovementConstantData();
				constantDataMap.Add(key, constantData);

				// 3. Curve Data NativeList에 추가
				NativeArray<float> newCurve = stats.GetMovementCurveData(CURVE_RESOLUTION, Allocator.Temp);

				int startIndex = curveDataList.Length; // 현재 NativeList의 끝 인덱스가 새 데이터의 시작점

				curveDataList.AddRange(newCurve); // NativeList의 동적 확장 기능 사용

				newCurve.Dispose(); // Temp NativeArray 해제

				// 4. CurveInfo 맵에 등록
				curveInfoMap.Add(key, new CurveInfo
				{
					StartIndex = startIndex,
					Resolution = CURVE_RESOLUTION
				});

				Debug.Log($"[ProjectileManager] Successfully registered new projectile key: {key}.");
			}
			public class Movement : UpdateLogic
			{
				public ProjectileObject thisProjectile;
				public IProjectileMovement ThisMovement => thisProjectile == null ? null : thisProjectile.ThisMovement;
				public MovementJobData movementJobData;

				public Movement(ProjectileObject projectile, StrategyUpdateSubClass<Movement> thisSubClass) : base(thisSubClass)
				{
					thisProjectile = projectile;
					movementJobData = default;
				}

				protected override void OnDispose()
				{
					thisProjectile = null;
				}

				protected override void OnUpdate(in float deltaTime)
				{
					if (ThisMovement == null) return;
					if (ThisMovement.ResetJobDataFlag)
					{
						ThisMovement.InitMovementJobData(out movementJobData);
						if(thisSubClass is StrategyUpdate_ProjectileMovement parent)
							parent.RegisterNewProjectileData(movementJobData.ProjectileKey);
					}
					else
					{
						ThisMovement.ApplyJobResult(in movementJobData);
					}
				}

				public void JobDataUpdate()
				{
					if (ThisMovement == null) return;
					// Job 실행 전에 필요한 데이터만 업데이트 (타겟 위치 업데이트 등)
					ThisMovement.UpdateMovementJobData(ref movementJobData);
				}
			}

			// 8. Update: Job에 HashMap 전달
			protected override void Update(in float deltaTime)
			{
				int length = UpdateList.Count;
				if (length == 0) return;

				// Create native array for job (TempJob)
				var movements = new NativeArray<MovementJobData>(length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);


				// Mono -> Native: read once per instance
				for (int i = 0 ; i < length ; i++)
				{
					var entry = UpdateList[i];
					if (entry == null || entry.ThisMovement == null)
					{
						// default-initialize (safe)
						movements[i] = default;
						continue;
					}

					// ensure curve sampling is prepared by the Mono object (we assume Mono did this on Init/SetTarget)
					// but check defensively:
					entry.JobDataUpdate();
					var pd = entry.movementJobData;
					pd.DeltaTime = deltaTime;

					movements[i] = pd;
				}

				// schedule job
				var job = new ProjectileMovementJob
				{
					Movements = movements,
					ConstantDataMap = constantDataMap,
                    CurveInfoMap = curveInfoMap,
					CurveDataArray = curveDataList.AsArray()
				};

				JobHandle handle = job.Schedule(length, 64);
				handle.Complete();

				// Apply results back to Mono (single write per instance)
				for (int i = 0 ; i < length ; i++)
				{
					var entry = UpdateList[i];
					if (entry == null || entry.ThisMovement == null)
					{
						continue;
					}
					entry.movementJobData = movements[i];
					entry.Update(in deltaTime);
				}
				movements.Dispose();
			}
		}
	}
}