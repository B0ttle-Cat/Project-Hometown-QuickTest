using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public partial class StrategyUpdate : MonoBehaviour
{
	private StrategyElementCollector collector;
	private List<ControlBase> controlBases;
	public void OnEnable()
	{
		collector = GetComponent<StrategyElementCollector>();
		controlBases = collector.ControlBaseList;

		//territory = GetComponent<StrategyTerritoryManager>() ?? gameObject.AddComponent<StrategyTerritoryManager>();
	}
	public void OnDisable()
	{

	}
	public interface IUpdate
	{
		void Update(in float deltaTime);
	}

	public enum UpdateLogicSort
	{
		Start = 0,

		거점_점령상태,
		거점_시설,
		거점_자원_보충,
		거점_자원_네트워크_업데이트,
		거점_버프_계산,

		거점_유닛_생성,

		유닛_버프_계산,
		유닛_이동,
		유닛_상태_업데이트,     // 유닛의 위치와 스텟을 토대로 어떤 행동을 할지 결정한다.
		유닛_공격_업데이트,     // 공격 딜레이 계산 및 공격 생성
		유닛_데미지_계산,          // 충돌된 데이미 계산을 진행
		유닛_사망_처리,           // HP 없는 유닛을 삭제.

		UI
	}

	private void Update()
	{

	}

	public class Update_ControlBaseCapture : IUpdate
	{
		Dictionary<ControlBase, CaptureUpdate> captureUpdates;

		void IUpdate.Update(in float deltaTime)
		{
			if (captureUpdates == null)
			{
				captureUpdates = new Dictionary<ControlBase, CaptureUpdate>();
				var list = StrategyManager.Collector.ControlBaseList;
				int length = list.Count;
				for (int i = 0 ; i < length ; i++)
				{
					var cb = list[i];
					var data = cb.CaptureData;

					int captureFactionID = data.captureFactionID;
					float captureProgress = data.captureProgress;
					float captureTime = data.captureTime;

					captureUpdates.Add(cb, new CaptureUpdate(cb, cb.GetComponentInChildren<ControlBaseTrigger>(), captureFactionID, captureProgress, captureTime));
				}
			}

			foreach (var item in captureUpdates)
			{
				var key = item.Key;
				var update = item.Value;

				int oldFaction = update.ownerFactionID;
				float oldProgress = update.factionProgress[oldFaction];

				update.Update(in deltaTime);

				int nextFaction = update.ownerFactionID;
				float nextProgress = update.factionProgress[nextFaction ];

				captureUpdates[key] = update;
				if(oldFaction != nextFaction || !Mathf.Approximately(oldProgress ,nextProgress))
				{
					var data = key.Capture.GetData();
					data.captureFactionID = nextFaction;
					data.captureProgress = nextProgress;
					key.Capture.SetData(data);
				}
			}
		}

		public struct CaptureUpdate
		{
			public ControlBase controlBase;
			public ControlBaseTrigger controlBaseTrigger;

			public int ownerFactionID;
			public float captureTime;
			public Dictionary<int,float> factionProgress;

			public CaptureUpdate(ControlBase controlBase, ControlBaseTrigger controlBaseTrigger, int ownerFaction, float ownerProgress, float captureTime)
			{
				this.controlBase = controlBase;
				this.controlBaseTrigger = controlBaseTrigger;
				this.ownerFactionID = ownerFaction;
				this.captureTime = Mathf.Max(captureTime, 1f);
				factionProgress = new Dictionary<int, float>()
				{
					[ownerFaction] = ownerProgress,
				};
			}

			public void Update(in float deltaTime)
			{
				var totals = ComputeFactionTotals();
				int totalScore = totals.totalPoint;
				if (totalScore <= 0)
				{
					// 총점이 0점 이면, 우위 없음.
					ApplyNoDominant(in deltaTime);
					return;
				}

				var sorted = totals.factionPoint.OrderByDescending(kv => kv.Value).ToList();
				var top = sorted.First();
				float topPercent = (float)top.Value / (float)totalScore;
				int topKey = top.Key;
				if (topPercent > 0.5f)
				{
					ApplyDominant(topKey, in deltaTime);
				}
				else
				{
					ApplyNoDominant(in deltaTime);
				}
			}
			void ApplyDominant(int dominant, in float deltaTime)
			{
				// 점유 세력과 우위 세력이 동일한 경우
				// => 기존 세력 점수를 올리고 나머지는 0.

				// 점유 세력과 우위 세력이 다른 경우
				// => 기존 세력이 점유를 잃을때 까지 점수를 내리고 나머지는 0

				// 점유 세력 없고 우위 세력만 있는 경우
				// => 우의 세력의 점수를 올린다.


				if (!factionProgress.ContainsKey(ownerFactionID))
				{
					factionProgress[ownerFactionID] = 0f;
				}
				if (!factionProgress.ContainsKey(dominant))
				{
					factionProgress[dominant] = 0f;
				}

				Faction ownerFaction = StrategyManager.Collector.FindFaction(ownerFactionID);
				Faction topFaction = StrategyManager.Collector.FindFaction(dominant);

				if (ownerFaction != null)
				{
					float captureSpeed = ownerFaction == null ? 1 : Mathf.Max(ownerFaction.CaptureSpeed, 0.1f);
					float delta = (captureSpeed / captureTime) * deltaTime;

					if (ownerFactionID == dominant)
					{
						foreach (var item in factionProgress)
						{
							int factionID = item.Key;
							if (factionID == ownerFactionID)
							{
								float progress = factionProgress[ownerFactionID];
								progress += delta;
								factionProgress[ownerFactionID] = Mathf.Clamp01(progress);
							}
							else
							{
								factionProgress[factionID] = 0f;
							}
						}
					}
					else
					{
						foreach (var item in factionProgress)
						{
							int factionID = item.Key;
							if (factionID == ownerFactionID)
							{
								float progress = factionProgress[factionID];
								progress -= delta;
								factionProgress[factionID] = Mathf.Clamp01(progress);
								if (progress <= 0)
								{
									// 점유 상실
									ownerFactionID = -1;
								}
							}
							else
							{
								factionProgress[factionID] = 0f;
							}
						}
					}
				}
				else if (topFaction != null)
				{
					float captureSpeed = ownerFaction == null ? 1 : Mathf.Max(topFaction.CaptureSpeed, 0.1f);
					float delta = (captureSpeed / captureTime) * deltaTime;

					foreach (var item in factionProgress)
					{
						int factionID = item.Key;
						if (factionID == dominant)
						{
							float progress = factionProgress[factionID];
							progress += delta;
							factionProgress[factionID] = Mathf.Clamp01(progress);

							if (progress >= 1)
							{
								// 점유 획득
								ownerFactionID = dominant;
							}
						}
						else
						{
							float progress = factionProgress[factionID];
							if (progress > 0)
							{
								progress -= delta;
								factionProgress[factionID] = Mathf.Clamp01(progress);
							}
						}
					}
				}
				else
				{
					ApplyNoDominant(in deltaTime);
				}
			}
			void ApplyNoDominant(in float deltaTime)
			{
				// 우위 세력이 없을경우 기존 세력의 점유를 올린다.
				// 기존 세력이 없을경우 모든 세력의 줌유를 낮춘다.

				Faction topFaction = StrategyManager.Collector.FindFaction(ownerFactionID);
				float captureSpeed = topFaction == null ? 1 : Mathf.Max(topFaction.CaptureSpeed, 0.1f);
				float delta = (captureSpeed / captureTime) * deltaTime;

				foreach (var item in factionProgress)
				{
					int factionID = item.Key;
					float progress = factionProgress[factionID];
					if (factionID == ownerFactionID)
					{
						progress += delta;
					}
					else
					{
						progress -= delta;
					}
					factionProgress[factionID] = Mathf.Clamp01(progress);
				}
			}
			private (Dictionary<int, int> factionPoint, int totalPoint) ComputeFactionTotals()
			{
				var dict = new Dictionary<int,int>();
				int total = 0;
				foreach (var tag in controlBaseTrigger.CaptureTagList)
				{
					if (!dict.ContainsKey(tag.factionID)) dict[tag.factionID] = 0;
					dict[tag.factionID] += Mathf.Max(0, tag.pointValue);
					total += Mathf.Max(0, tag.pointValue);
				}
				return (dict, total);
			}
		}
	}
}

