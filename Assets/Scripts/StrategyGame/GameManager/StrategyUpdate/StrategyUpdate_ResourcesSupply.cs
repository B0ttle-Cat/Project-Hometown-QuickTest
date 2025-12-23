using System;
using System.Collections.Generic;

using UnityEngine;
namespace StrategyManagerModule
{
	public partial class StrategyUpdate
	{
		[Serializable]
		public record SupplyRequest
		{

			private float reservationPersonnel;
			private float reservationMaterial;
			private float reservationElectric;

			public SupplyRequest()
			{
				reservationPersonnel = 0;
				reservationMaterial = 0;
				reservationElectric = 0;
			}

			public float ReservationPersonnel
			{
				get => reservationPersonnel; set => reservationPersonnel = value;
			}
			public float ReservationMaterial
			{
				get => reservationMaterial; set => reservationMaterial = value;
			}
			public float ReservationElectric
			{
				get => reservationElectric; set => reservationElectric = value;
			}

			public bool IsUpdateFlag() => 
				reservationPersonnel >= 1
				|| reservationMaterial >= 1
				|| reservationElectric >= 1;
			public void ResetAndLeaveDecimal(out int integerPersonnel, out int integerMaterial, out int integerElectric)
			{
				integerPersonnel = (int)reservationPersonnel;
				reservationPersonnel -= integerPersonnel;

				integerMaterial = (int)reservationMaterial;
				reservationMaterial -= integerMaterial;

				integerElectric = (int)reservationElectric;
				reservationElectric -= integerElectric;
			}
		}
		public class StrategyUpdate_ResourcesSupply : StrategyUpdateSubClass<StrategyUpdate_ResourcesSupply.ResourcesSupply>
		{
			private BaseList<SectorObject> sectorList;
			private BaseList<Faction> factionList;

			private Dictionary<SectorObject, SupplyRequest> sectorSupplyRequest;
			private Dictionary<Faction, SupplyRequest> factionSupplyRequest;
	
			public StrategyUpdate_ResourcesSupply(StrategyUpdate updater) : base(updater)
			{
			}
			protected override void Dispose()
			{
				sectorList = null;
				factionList = null;

				sectorSupplyRequest?.Clear();
				sectorSupplyRequest = null;

				factionSupplyRequest?.Clear();
				factionSupplyRequest = null;
			}
			protected override void Start()
			{
				// 둘다 게임시작 이후 변하지 않는 배열.
				sectorList = StrategyManager.Collector.GetList<SectorObject>();
				factionList = StrategyManager.Collector.GetList<Faction>();

				sectorSupplyRequest = new Dictionary<SectorObject, SupplyRequest>();
				factionSupplyRequest = new Dictionary<Faction, SupplyRequest>();

				foreach (var item in sectorList)
				{
					this.Add(new SectorResourcesSupply(item, this, sectorSupplyRequest, factionSupplyRequest));
				}
				foreach (var item in factionList)
				{
					this.Add(new FactionResourcesSupply(item, this, factionSupplyRequest));
				}
			}

			public abstract class ResourcesSupply : UpdateLogic
			{
				protected ResourcesSupply(StrategyUpdate_ResourcesSupply thisSubClass) : base(thisSubClass)
				{
				}
				public abstract bool IsValid();
				public bool IsInvalid() => !IsValid();
			}
			public class SectorResourcesSupply : ResourcesSupply
			{
				private readonly SectorObject sector;
				private readonly SupplyRequest supplyRequest;
				private float watingTime;

				private readonly Dictionary<SectorObject, SupplyRequest> sectorSupplyRequest;
				private readonly Dictionary<Faction, SupplyRequest> factionSupplyRequest;
				public SectorResourcesSupply(SectorObject sector, StrategyUpdate_ResourcesSupply thisSubClass,
					Dictionary<SectorObject, SupplyRequest> sectorSupplyRequest,
					Dictionary<Faction, SupplyRequest> factionSupplyRequest) : base(thisSubClass)
				{
					this.sector = sector;
					watingTime = 0;
					supplyRequest = new SupplyRequest();

					this.sectorSupplyRequest = sectorSupplyRequest;
					this.sectorSupplyRequest.Add(sector, supplyRequest);
					this.factionSupplyRequest = factionSupplyRequest;
				}
				protected override void OnDispose()
				{

				}


				public override bool IsValid()
				{
					if (sector.IsNullRef()) return false;
					if (sector.CaptureFactionID < 0) return false;

					Faction faction = sector.CaptureFaction;
					if (faction.IsNullRef()) return false;
					if (!faction.IsEnableResourcesSupply) return false;


					return true;
				}
				public bool CheclSupplyTimeUpdate(in float deltaTime)
				{
					float cycleTime = sector.StatsData.CycleTime;
					if (cycleTime < 1f) cycleTime = 1;

					if (watingTime >= cycleTime)
					{
						watingTime -= cycleTime;
						return true;
					}
					watingTime += deltaTime;
					return false;
				}
				public void UpdateCumulativePoint(in float deltaTime)
				{
					int depthCount = sector.StatsData.DistributionDepth + sector.RuntimeData.DistributionDepth;

					int recoveryPersonnel = sector.StatsValue.GetStatsValue(StrategyGamePlayData.StatsType.자원_인력_회복);
					int recoveryMaterial = sector.StatsValue.GetStatsValue(StrategyGamePlayData.StatsType.자원_재료_회복);
					int recoveryElectric = sector.StatsValue.GetStatsValue(StrategyGamePlayData.StatsType.자원_전력_회복);

					if (depthCount <= 0)
					{
						SectorUpdateSupplyRequest(sector, 1, in deltaTime);
					}
					else
					{
						StrategyManager.Pathfinding.FindSectorNeighbors(sector, depthCount, out var neighbors);
						SectorUpdateSupplyRequest(sector, 1, in deltaTime);
						for (int i = 1 ; i <= depthCount ; i++)
						{
							if (neighbors.TryGetValue(i, out var list))
							{
								float depthFactor = 1f - (float)i/(float)(depthCount+1);
								foreach (var item in list)
								{
									SectorUpdateSupplyRequest(item, in depthFactor, in deltaTime);
								}
							}
						}
					}

					void SectorUpdateSupplyRequest(SectorObject target, in float depthfactor, in float deltaTime)
					{
						float factor = depthfactor * deltaTime * (1/60f);

						if (sectorSupplyRequest.TryGetValue(target, out var request))
						{
							request.ReservationPersonnel += recoveryPersonnel * factor;
							request.ReservationMaterial += recoveryMaterial * factor;
							request.ReservationElectric += recoveryElectric * factor;
						}
					}
				}
				public void UpdateSectorToFaction()
				{
					if (!supplyRequest.IsUpdateFlag()) return;
					var factionSupply = factionSupplyRequest[sector.CaptureFaction];

					int capacityPersonnel = sector.StatsValue.GetStatsValue(StrategyGamePlayData.StatsType.자원_인력_최대);
					int localPersonnel = sector.StatsValue.GetStatsValue(StrategyGamePlayData.StatsType.자원_인력_현재);
					float sectorPersonnel = supplyRequest.ReservationPersonnel;
					float factionPersonnel = 0f;
					SectorToFaction(in capacityPersonnel, in localPersonnel,
						ref sectorPersonnel, ref factionPersonnel);
					supplyRequest.ReservationPersonnel = sectorPersonnel;
					factionSupply.ReservationPersonnel += factionPersonnel;


					int capacityMaterial = sector.StatsValue.GetStatsValue(StrategyGamePlayData.StatsType.자원_재료_최대);
					int localMaterial = sector.StatsValue.GetStatsValue(StrategyGamePlayData.StatsType.자원_재료_현재);
					float sectorMaterial = supplyRequest.ReservationMaterial;
					float factionMaterial = 0f;
					SectorToFaction(in capacityMaterial, in localMaterial,
						ref sectorMaterial, ref factionMaterial);
					supplyRequest.ReservationPersonnel = sectorMaterial;
					factionSupply.ReservationPersonnel += factionMaterial;

					int capacityElectric = sector.StatsValue.GetStatsValue(StrategyGamePlayData.StatsType.자원_전력_최대);
					int localElectric = sector.StatsValue.GetStatsValue(StrategyGamePlayData.StatsType.자원_전력_현재);
					float sectorElectric = supplyRequest.ReservationElectric;
					float factionElectric = 0f;
					SectorToFaction(in capacityElectric, in localElectric,
						ref sectorElectric, ref factionElectric);
					supplyRequest.ReservationPersonnel = sectorElectric;
					factionSupply.ReservationPersonnel += factionElectric;

					static void SectorToFaction(in int capacityValue, in int localValue, ref float sectorRecovery, ref float factionRecovery)
					{
						if (capacityValue == 0) { return; }
						float ratio = (float)localValue / (float)capacityValue;
						ratio = Mathf.Clamp(ratio, 0f, 0.9f);
						if (ratio < 0.5f)
						{
							return;
						}
						factionRecovery = sectorRecovery * ratio;
						sectorRecovery -= factionRecovery;
					}
				}
				protected override void OnUpdate(in float deltaTime)
				{
					if (!supplyRequest.IsUpdateFlag()) return;

					sector.OnSupplyUpdate(supplyRequest);
				}
			}
			public class FactionResourcesSupply : ResourcesSupply
			{
				private readonly Faction faction;
				private readonly SupplyRequest supplyRequest;
				private readonly Dictionary<Faction, SupplyRequest> factionSupplyRequest;
				public FactionResourcesSupply(Faction faction, StrategyUpdate_ResourcesSupply thisSubClass,
					Dictionary<Faction, SupplyRequest> factionSupplyRequest) : base(thisSubClass)
				{
					this.faction = faction;
					this.factionSupplyRequest = factionSupplyRequest;
					supplyRequest = new SupplyRequest();

					factionSupplyRequest.Add(faction, supplyRequest);
				}
				public override bool IsValid()
				{
					if (faction.IsNullRef()) return false;
					if (!faction.IsEnableResourcesSupply) return false;

					return true;
				}
				protected override void OnDispose()
				{
				}

				protected override void OnUpdate(in float deltaTime)
				{
					if (!supplyRequest.IsUpdateFlag()) return;

					faction.OnSupplyUpdate(supplyRequest);
				}
			}
			protected override void Update(in float deltaTime)
			{
				int sectorCount = sectorList.Count;
				int factionCount = sectorCount + factionList.Count;

				for (int i = 0 ; i < sectorCount ; i++)
				{
					var item = this[i];
					if (item == null) continue;
					if (item.IsInvalid()) continue;
					if (item is SectorResourcesSupply itemSector)
					{
						itemSector.UpdateCumulativePoint(in deltaTime);
					}
				}
				for (int i = 0 ; i < sectorCount ; i++)
				{
					var item = this[i];
					if (item == null) continue;
					if (item.IsInvalid()) continue;
					if (item is SectorResourcesSupply itemSector)
					{
						if (itemSector.CheclSupplyTimeUpdate(in deltaTime))
						{
							itemSector.UpdateSectorToFaction();
							itemSector.Update(in deltaTime);
						}
					}
				}
				for (int i = sectorCount ; i < factionCount ; i++)
				{
					var item = this[i];
					if (item == null) continue;
					if (item.IsInvalid()) continue;
					if (item is FactionResourcesSupply itemSector)
					{
						item.Update(in deltaTime);
					}
				}
			}
		}
	}
}
