using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using static StrategyManagerModule.StrategyUpdate.StrategyUpdate_CaptureUpdate;

namespace StrategyManagerModule
{
	public partial class StrategyUpdate
	{
		public class StrategyUpdate_CaptureUpdate : StrategyUpdateSubClass<CaptureUpdate>
		{
			private List<CaptureTag> captureTagList;

			public StrategyUpdate_CaptureUpdate(StrategyUpdate updater) : base(updater)
			{
				captureTagList = new List<CaptureTag>();
			}
			protected override void Dispose()
			{
				captureTagList = null;
				StrategyManager.Collector.RemoveChangeListener<SectorObject>(OnChangeSectorObject);
				StrategyManager.Collector.RemoveChangeListener<CaptureTag>(OnChangeCaptureTag);
			}

			protected override void Start()
			{
				StrategyManager.Collector.AddChangeListener<SectorObject>(OnChangeSectorObject, true);
				StrategyManager.Collector.AddChangeListener<CaptureTag>(OnChangeCaptureTag, true);
			}
			private void OnChangeSectorObject(SectorObject item, bool added)
			{
				if (item == null) return;

				if (added)
				{
					this.Add(new CaptureUpdate(item, this));
				}
				else
				{
					int findIndex = this.FindIndex(f=>f.sector == item);
					if (findIndex < 0) return;
					this.RemoveAt(findIndex);
				}
			}
			private void OnChangeCaptureTag(CaptureTag item, bool added)
			{
				if (item == null) return;

				if (added)
				{
					captureTagList.Add(item);
				}
				else
				{
					captureTagList.Remove(item);
				}
			}
			public class CaptureUpdate : UpdateLogic
			{
				public SectorObject sector;
				public CylinderArea sectorArea;
				public SectorColor sectorColor;
				private List<CaptureTag> captureTagList;
				public float captureTime;

				public int ownerFactionID;      // 점령 세력
				public int dominantFactionID;   // 점령중인 세력
				public int topPointFactionID;   // 가장 영향력이 강한 세력
				public float captureProgress;   // 점령 바

				public FactionProgress factionProgress;
				public class FactionProgress : IDisposable
				{
					public List<(int faction, float progress)> factionProgress;
					public int count => factionProgress == null ? 0 : factionProgress.Count;
					public FactionProgress(params (int faction, float progress)[] values)
					{
						factionProgress = new List<(int faction, float progress)>();
						factionProgress.AddRange(values);
					}

					public float this[int faction]
					{
						get
						{
							if (faction < 0) return 0f;

							int index = factionProgress.FindIndex(i=>i.faction == faction);
							if (index < 0)
							{
								index = factionProgress.Count;
								factionProgress.Add((faction, 0f));
							}
							return factionProgress[index].progress;
						}
						set
						{
							if (faction < 0) return;
							int index = factionProgress.FindIndex(i=>i.faction == faction);
							if (index < 0)
							{
								index = factionProgress.Count;
								factionProgress.Add((faction, 0));
							}
							var item = factionProgress[index];
							item.progress = value;
							factionProgress[index] = item;
						}
					}

					public bool ContainsKey(int faction)
					{
						int index = factionProgress.FindIndex(i=>i.faction == faction);
						return index >= 0;
					}

					public void Dispose()
					{
						if (factionProgress != null)
						{
							factionProgress.Clear();
							factionProgress = null;
						}
					}

					public (int faction, float progress) GetIndexValue(int index)
					{
						return factionProgress[index];
					}
				}

				public struct CaptureProgress
				{
					public int faactionID;
					public float progress;
				}
				public CaptureUpdate(SectorObject sector, StrategyUpdate_CaptureUpdate thisSubClass) : base(thisSubClass)
				{
					this.sector = sector;
					this.sectorArea = sector.GetComponentInChildren<CylinderArea>(true);
					this.sectorColor = sector.GetComponentInChildren<SectorColor>(true);
					if (sectorColor == null) sectorColor = sector.gameObject.AddComponent<SectorColor>();
					captureTagList = thisSubClass.captureTagList;

					this.captureTime = Mathf.Max(sector.StatsData.CaptureTimeRequired, 1f);

					this.ownerFactionID = sector.RuntimeData.CaptureFactionID;
					this.dominantFactionID = ownerFactionID;
					this.topPointFactionID = ownerFactionID;
					this.captureProgress = sector.RuntimeData.CaptureProgress;

					factionProgress = new FactionProgress((ownerFactionID, captureProgress));

					ColorUpdate(ownerFactionID, captureProgress);
				}

				protected override void OnDispose()
				{
					sector = null;
					sectorArea = null;
					if (factionProgress != null)
					{
						factionProgress.Dispose();
						factionProgress = null;
					}
					captureTagList = null;
				}
				protected override void OnUpdate(in float deltaTime)
				{
					var totals = ComputeFactionTotals();
					int totalScore = totals.totalPoint;
					if (totalScore <= 0)
					{
						// 총점이 0점 이면, 우위 없음.
						topPointFactionID = -1;
						ApplyDominant(in deltaTime);
						return;
					}

					var sorted = totals.factionPoint.OrderByDescending(kv => kv.Value).ToList();
					var top = sorted.First();
					float topPercent = (float)top.Value / (float)totalScore;
					if (topPercent > 0.5f)
					{
						topPointFactionID = top.Key;
					}
					else
					{
						topPointFactionID = -1;
					}
					ApplyDominant(in deltaTime);
				}
				void ApplyDominant(in float deltaTime)
				{
					// 점유 세력과 우위 세력이 동일한 경우
					// => 기존 세력 점수를 올리고 나머지는 0.

					// 점유 세력과 우위 세력이 다른 경우
					// => 기존 세력이 점유를 잃을때 까지 점수를 내리고 나머지는 0

					// 점유 세력 없고 우위 세력만 있는 경우
					// => 우의 세력의 점수를 올린다.

					var factionList = StrategyManager.Collector.GetList<Faction>();
					Faction ownerFaction = factionList.Find( ownerFactionID);
					Faction dominantFaction = factionList.Find( dominantFactionID);
					Faction topFaction = factionList.Find(topPointFactionID);
					if (ownerFaction == null) ownerFactionID = -1;
					if (dominantFaction == null) dominantFactionID = -1;
					if (topFaction == null) topPointFactionID = -1;

					int controlFaction = -1;
					// 점유자 없음
					if (ownerFactionID < 0)
					{
						// 점령중 없음
						if (dominantFactionID < 0)
						{
							//우세 없음
							if (topPointFactionID < 0)
							{
								// 중립화
								controlFaction = ownerFactionID;
								captureProgress -= deltaTime;
							}
							else
							{
								//우세 세력이 점령 시작
								controlFaction = dominantFactionID = topPointFactionID;
								captureProgress += deltaTime;
							}
						}
						else
						{
							if (dominantFactionID == topPointFactionID)
							{
								// 우세 세력이 점령중
								controlFaction = dominantFactionID;
								captureProgress += deltaTime;
								if (captureProgress >= 1f)
								{
									// 점령중 세력을 점유자로 전환
									controlFaction = ownerFactionID = dominantFactionID;
								}
							}
							else
							{
								// 점령중 우세 상실
								// 점령지 무효와
								controlFaction = dominantFactionID;
								captureProgress -= deltaTime;
								if (captureProgress <= 0f)
								{
									// 우세 세력을 점령중으로 전환
									controlFaction = dominantFactionID = topPointFactionID;
								}
							}
						}
					}
					else
					{
						dominantFactionID = topPointFactionID;
						if (ownerFactionID == dominantFactionID || dominantFactionID < 0)
						{
							// 점령지 회복 & 현싱 유지
							controlFaction = ownerFactionID;
							captureProgress += deltaTime;
						}
						else
						{
							controlFaction = ownerFactionID;
							captureProgress -= deltaTime;
							if (captureProgress <= 0f)
							{
								// 점유자 해제
								controlFaction = ownerFactionID = -1;
							}
						}
					}

					dominantFactionID = controlFaction;
					captureProgress = Mathf.Clamp01(captureProgress);
				}
				private (Dictionary<int, int> factionPoint, int totalPoint) ComputeFactionTotals()
				{
					var dict = new Dictionary<int,int>();
					int total = 0;
					foreach (var tag in captureTagList)
					{
						if (!sectorArea.IsOverlap(tag.transform.position)) continue;

						if (!dict.ContainsKey(tag.FactionID)) dict[tag.FactionID] = 0;
						dict[tag.FactionID] += Mathf.Max(0, tag.PointValue);
						total += Mathf.Max(0, tag.PointValue);
					}
					return (dict, total);
				}
				public void ColorUpdate(int colorFaction, float colorProgress)
				{
					if (StrategyManager.Collector.TryFind<Faction>(colorFaction, out var faction))
					{
						sectorColor.UpdateColor(faction, colorProgress);
					}
					else
					{
						sectorColor.UpdateColor(null, 0);
					}
				}

			}


			protected override void Update(in float deltaTime)
			{
				int length = Count;
				for (int i = 0 ; i < length ; i++)
				{
					CaptureUpdate update = this[i];
					SectorObject sector = update.sector;
					if (sector == null || !sector.isActiveAndEnabled) continue;

					int oldFaction = update.ownerFactionID;
					float oldProgress = update.captureProgress;

					update.Update(in deltaTime);

					int nextFaction = update.ownerFactionID;
					int nextDominantFaction = update.dominantFactionID;
					float nextProgress = update.captureProgress;

					bool changeProgress =  oldProgress != nextProgress;
					this[i] = update;

					if (oldFaction != nextFaction || changeProgress)
					{
						sector.RuntimeData.CaptureFactionID = nextFaction;
						sector.RuntimeData.CaptureProgress = nextProgress;


						if (oldFaction != nextFaction)
						{
							OnTrigger(sector, oldFaction, false);
							OnTrigger(sector, nextFaction, false);
						}
					}
					if (changeProgress)
					{
						update.ColorUpdate(nextDominantFaction, nextProgress);
					}
				}
			}

			private void OnTrigger(SectorObject sector, int factionID, bool state)
			{
				if (factionID < 0) return;
				var faction = FactionAPI.ID2Faction(factionID);
				if (faction.IsNullRef()) return;

				if (state)
				{
					faction.AddCaptured(sector);
				}
				else
				{
					faction.RemoveCaptured(sector);
				}
			}
		}
	}


}