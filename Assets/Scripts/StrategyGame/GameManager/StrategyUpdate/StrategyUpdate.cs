using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public interface IStrategyUpdater : IDisposable
{
	void Start();
	void Update(in float deltaTime);
}
namespace StrategyManagerModule
{
	public partial class StrategyUpdate : MonoBehaviour
	{
		private StrategyUpdateTempData tempData;
		private List<(UpdateLogicSort type, IStrategyUpdater updater)> updateList;
		public StrategyUpdateTempData TempData => tempData;

		public enum UpdateLogicSort
		{
			None = 0,
			Start = 1,

			거점_점령상태,

			거점_시설_건설,

			거점_자원갱신시작,
			거점_자원갱신종료,

			거점_시설보급,
			거점_버프계산,

			세력_자원갱신시작,
			세력_자원갱신종료,

			VisionNearbyUpdate,
			ActionNearbyUpdate,
			AttackNearbyUpdate,
			각종_상태_업데이트,         // 전체 FSM Update 진행

			유닛_CombatTarget_업데이트,
			유닛_보급충전,
			유닛_기본변수갱신,
			유닛_버프계산,
			유닛_노드_이동,
			유닛_추격_이동,
			유닛_공격_업데이트,         // 공격 딜레이 계산 및 공격 생성
			데미지_계산,          // 충돌된 데이미 계산을 진행
			사망_파괴_처리,               // HP 없는 유닛을 삭제.

			투사체_위치이동,
			투사체_충돌확인,


			작전_기본변수_갱신,

			UI,

			End = int.MaxValue,
			//거점_자원분배,			// 자원 분배대신 건물을 건설하여 일정 범위 내에 보급 보너스를 주는식으로...
		}
		public class StrategyUpdateTempData : IDisposable
		{
			private List<DataValue> dataList;
			private struct DataValue
			{
				public string key;
				public object value;
				public UpdateLogicSort alive;
				public DataValue(string key, object value)
				{
					this.key = key;
					this.value = value;
					this.alive = UpdateLogicSort.None;
				}
				public DataValue(string key, object value, UpdateLogicSort alive)
				{
					this.key = key;
					this.value = value;
					this.alive = alive;
				}
			}
			public StrategyUpdateTempData()
			{
				dataList = new List<DataValue>();
			}
			public bool HasKey(string key)
			{
				int findIndex = dataList.FindIndex(d=>d.key == key);
				return findIndex >= 0;
			}

			public bool TryGetValue<T>(string key, out T value)
			{
				value = default;
				int findIndex = dataList.FindIndex(d=>d.key == key);
				if (findIndex < 0) return false;
				if (dataList[findIndex].value is not T t) return false;
				value = t;
				return true;
			}
			public void SetValue(string key, object value)
			{
				int findIndex = dataList.FindIndex(d=>d.key == key);
				if (findIndex < 0) dataList.Add(new DataValue(key, value));
				else dataList[findIndex] = new DataValue(key, value, dataList[findIndex].alive);
			}
			public void SetValue(string key, object value, UpdateLogicSort alive)
			{
				int findIndex = dataList.FindIndex(d=>d.key == key);
				if (findIndex < 0) dataList.Add(new DataValue(key, value, alive));
				else dataList[findIndex] = new DataValue(key, value, alive);
			}

			public bool GetTrigger(string key)
			{
				return HasKey(key);
			}
			public void SetTrigger(string key)
			{
				int findIndex = dataList.FindIndex(d=>d.key == key);
				if (findIndex < 0) dataList.Add(new DataValue(key, true));
			}
			public void SetTrigger(string key, UpdateLogicSort aliveLimit)
			{
				int findIndex = dataList.FindIndex(d=>d.key == key);
				if (findIndex < 0) dataList.Add(new DataValue(key, true, aliveLimit));
			}

			public bool RemoveValue(string key)
			{
				int findIndex = dataList.FindIndex(d=>d.key == key);
				if (findIndex < 0) return false;
				dataList.RemoveAt(findIndex);
				return true;
			}
			public void AfterRemove(UpdateLogicSort type)
			{
				int length = dataList.Count;
				for (int i = 0 ; i < length ; i++)
				{
					if (dataList[i].alive == type)
					{
						dataList.RemoveAt(i--);
						length--;
					}
				}
			}

			public void Dispose()
			{
				if (dataList != null)
				{
					dataList.Clear();
					dataList = null;
				}
			}
		}
		private StrategyTime ThisTime { get; set; }
		internal void SetTime(StrategyTime time)
		{
			ThisTime = time;
		}

		public void OnEnable()
		{
			tempData = new StrategyUpdateTempData();
			updateList = new List<(UpdateLogicSort type, IStrategyUpdater updater)>()
			{
				(UpdateLogicSort.Start,  null),

				(UpdateLogicSort.거점_점령상태,  new StrategyUpdate_CaptureUpdate(this)),
				(UpdateLogicSort.거점_시설_건설,  new StrategyUpdate_ConstructUpdate(this)),

				(UpdateLogicSort.세력_자원갱신시작,  new StrategyUpdate_StartFactionResourcesSupply(this)),
				(UpdateLogicSort.거점_자원갱신시작,  new StrategyUpdate_StartSectorResourcesSupply(this)),

				(UpdateLogicSort.거점_자원갱신종료,  new StrategyUpdate_EndedSectorResourcesSupply(this)),
				(UpdateLogicSort.세력_자원갱신종료,  new StrategyUpdate_EndedFactionResourcesSupply(this)),

				(UpdateLogicSort.거점_시설보급, null),
				(UpdateLogicSort.거점_버프계산, null),
				(UpdateLogicSort.유닛_보급충전, null),
				(UpdateLogicSort.유닛_기본변수갱신, null),
				(UpdateLogicSort.유닛_버프계산,  new StrategyUpdate_UnitBuff(this)),
				(UpdateLogicSort.작전_기본변수_갱신, new StrategyUpdate_OperationUpdate(this)),

				(UpdateLogicSort.VisionNearbyUpdate, new StrategyUpdate_VisionNearbyUpdate(this)),
				(UpdateLogicSort.ActionNearbyUpdate, new StrategyUpdate_ActionNearbyUpdate(this)),
				(UpdateLogicSort.AttackNearbyUpdate, new StrategyUpdate_AttackNearbyUpdate(this)),
				(UpdateLogicSort.각종_상태_업데이트,  new StrategyUpdate_FSMUpdater(this)),
				(UpdateLogicSort.유닛_CombatTarget_업데이트, new StrategyUpdate_UnitCombatTargetUpdate(this)),


				(UpdateLogicSort.유닛_노드_이동,  new StrategyUpdate_NodeMovement(this)),
				(UpdateLogicSort.유닛_추격_이동,  new StrategyUpdate_NavMovement(this)),

				(UpdateLogicSort.투사체_위치이동,  new StrategyUpdate_ProjectileMovement(this)),
				(UpdateLogicSort.투사체_충돌확인,  new StrategyUpdate_ProjectileHitCheck(this)),

				(UpdateLogicSort.데미지_계산,  new StrategyUpdate_ComputeDamage(this)),
				(UpdateLogicSort.사망_파괴_처리,  new StrategyUpdate_ElementDestroyer(this)),


				(UpdateLogicSort.End, null)
			};

			foreach ((UpdateLogicSort type, IStrategyUpdater updater) in updateList)
			{
				updater?.Start();
			}
		}
		public void OnDisable()
		{
			if (updateList != null)
			{
				foreach ((_, IStrategyUpdater updater) in updateList)
				{
					updater?.Dispose();
				}
				updateList.Clear();
				updateList = null;
			}

			if (tempData != null)
			{
				tempData.Dispose();
				tempData = null;
			}
		}
		private void Update()
		{
			if (ThisTime != null) ThisTime.TimeUpdate();
			float deltaTime = Time.deltaTime;
			foreach ((UpdateLogicSort type, IStrategyUpdater updater) in updateList)
			{
				try
				{
					updater?.Update(in deltaTime);
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
				finally
				{
					try
					{
						tempData.AfterRemove(type);
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						tempData = new StrategyUpdateTempData();
					}
				}
			}
		}
	}



	public abstract class StrategyUpdateSubClass<T> : IStrategyUpdater, IList<T> where T : StrategyUpdateSubClass<T>.UpdateLogic
	{
		protected StrategyUpdate thisUpdater;
		protected StrategyUpdate.StrategyUpdateTempData TempData => thisUpdater == null ? null : thisUpdater.TempData;

		private readonly List<T> updateList;
		public int Count => updateList.Count;

		public bool IsReadOnly => ((ICollection<T>)updateList).IsReadOnly;

		public T this[int index] { get => updateList[index]; set => updateList[index] = value; }

		public StrategyUpdateSubClass(StrategyUpdate updater)
		{
			thisUpdater = updater;
			updateList = new List<T>();
		}
		void IStrategyUpdater.Start() => Start();
		void IStrategyUpdater.Update(in float deltaTime) => Update(in deltaTime);
		void IDisposable.Dispose()
		{
			thisUpdater = null;

			if (updateList != null)
			{
				int length = updateList.Count;
				for (int i = 0 ; i < length ; i++)
				{
					updateList[i].Dispose();
				}
				updateList.Clear();
			}
			Dispose();
		}
		protected abstract void Dispose();
		protected abstract void Start();
		protected virtual void Update(in float deltaTime)
		{
			int length = updateList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				var item = updateList[i];
				if (item == null) continue;
				item.Update(in deltaTime);
			}
		}
		public int IndexOf(T item)
		{
			return updateList.IndexOf(item);
		}
		public void Insert(int index, T item)
		{
			updateList.Insert(index, item);
		}
		public void Add(T item)
		{
			updateList.Add(item);
		}
		public bool Remove(T item)
		{
			if (item == null) return false;
			item.Dispose();
			return updateList.Remove(item);
		}
		public void RemoveAt(int index)
		{
			if (index < 0 || index >= updateList.Count) return;
			updateList[index].Dispose();
			updateList.RemoveAt(index);
		}
		public void Clear()
		{
			int length = this == null ? 0 : updateList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				updateList[i].Dispose();
			}
			updateList.Clear();
		}
		public int RemoveAll(Predicate<T> match)
		{
			if (match == null) return 0;

			int removed = 0;

			for (int i = updateList.Count - 1 ; i >= 0 ; --i)
			{
				var item = updateList[i];
				if (!match(item))
					continue;

				item.Dispose();
				updateList.RemoveAt(i);
				++removed;
			}

			return removed;

		}
		public bool Contains(T item)
		{
			return updateList.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			updateList.CopyTo(array, arrayIndex);
		}
		public IEnumerator<T> GetEnumerator()
		{
			return updateList.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return updateList.GetEnumerator();
		}

		public int FindIndex(Predicate<T> match)
		{
			return updateList.FindIndex(match);
		}
		public abstract partial class UpdateLogic : IDisposable
		{
			protected StrategyUpdateSubClass<T> thisSubClass;
			protected StrategyUpdate Updater => thisSubClass == null ? null : thisSubClass.thisUpdater;
			protected StrategyUpdate.StrategyUpdateTempData TempData => thisSubClass == null ? null : thisSubClass.TempData;
			protected UpdateLogic(StrategyUpdateSubClass<T> thisSubClass)
			{
				this.thisSubClass = thisSubClass;
			}

			public void Update(in float deltaTime)
			{
				if (thisSubClass == null || Updater == null || TempData == null) return;
				OnUpdate(deltaTime);
			}
			public void Dispose()
			{
				OnDispose();
				thisSubClass = null;
			}

			protected abstract void OnUpdate(in float deltaTime);
			protected abstract void OnDispose();
		}
		public abstract partial class UpdateLogic // Sector
		{
			public string SectorTempSupplyValueKey(SectorObject sector) => SectorTempSupplyValueKey(sector.SectorID);
			public string SectorTempSupplyValueKey(int sector) => $"SectorTempSupplyValueKey_{sector}";
		}
		public abstract partial class UpdateLogic // Faction
		{
			public string FactionKey(Faction faction) => FactionKey(faction.FactionID);
			public string FactionIsAliveKey(Faction faction) => FactionIsAliveKey(faction.FactionID);
			public string FactionTempSupplyValueKey(Faction faction) => FactionTempSupplyValueKey(faction.FactionID);
			public string FactionKey(int faction) => $"FactionKey_{faction}";
			public string FactionIsAliveKey(int faction) => $"FactionIsAliveKey_{faction}";
			public string FactionTempSupplyValueKey(int faction) => $"FactionTempSupplyValueKey_{faction}";
			public struct TempSupplyValue
			{
				public readonly int elementID;

				public int manpower;
				public int manpowerMax;
				public int manpowerSupply;
				public bool manpowerIsUpdate;

				public int material;
				public int materialMax;
				public int materialSupply;
				public bool materialIsUpdate;

				public int electric;
				public int electricMax;
				public int electricSupply;
				public bool electricIsUpdate;
				public TempSupplyValue(int elementID)
				{
					this.elementID = elementID;
					manpower = 0; manpowerMax = 0; manpowerSupply = 0;
					material = 0; materialMax = 0; materialSupply = 0;
					electric = 0; electricMax = 0; electricSupply = 0;
					manpowerIsUpdate = materialIsUpdate = electricIsUpdate = false;

				}
				public TempSupplyValue(IStrategyElement element)
				{
					elementID = element == null ? -1 : element.ID;
					manpower = 0; manpowerMax = 0; manpowerSupply = 0;
					material = 0; materialMax = 0; materialSupply = 0;
					electric = 0; electricMax = 0; electricSupply = 0;
					manpowerIsUpdate = materialIsUpdate = electricIsUpdate = false;
				}
				public static TempSupplyValue operator +(TempSupplyValue a, TempSupplyValue b)
				{
					return new TempSupplyValue(a.elementID)
					{
						manpower = a.manpower + b.manpower,
						material = a.material + b.material,
						electric = a.electric + b.electric,
						manpowerMax = a.manpowerMax + b.manpowerMax,
						materialMax = a.materialMax + b.materialMax,
						electricMax = a.electricMax + b.electricMax,
						manpowerSupply = a.manpowerSupply + b.manpowerSupply,
						materialSupply = a.materialSupply + b.materialSupply,
						electricSupply = a.electricSupply + b.electricSupply,
						manpowerIsUpdate = a.manpowerIsUpdate || b.manpowerIsUpdate,
						materialIsUpdate = a.materialIsUpdate || b.materialIsUpdate,
						electricIsUpdate = a.electricIsUpdate || b.electricIsUpdate
					};
				}
				public static TempSupplyValue operator -(TempSupplyValue a, TempSupplyValue b)
				{
					return new TempSupplyValue(a.elementID)
					{
						manpower = a.manpower - b.manpower,
						material = a.material - b.material,
						electric = a.electric - b.electric,
						manpowerMax = a.manpowerMax - b.manpowerMax,
						materialMax = a.materialMax - b.materialMax,
						electricMax = a.electricMax - b.electricMax,
						manpowerSupply = a.manpowerSupply - b.manpowerSupply,
						materialSupply = a.materialSupply - b.materialSupply,
						electricSupply = a.electricSupply - b.electricSupply,
						manpowerIsUpdate = a.manpowerIsUpdate || b.manpowerIsUpdate,
						materialIsUpdate = a.materialIsUpdate || b.materialIsUpdate,
						electricIsUpdate = a.electricIsUpdate || b.electricIsUpdate
					};
				}
			}
		}
	}
	public partial class StrategyUpdate
	{
		[Serializable]
		public record SupplyRequest
		{
			private bool updateFlag;
			private float reservationPersonnel;
			private float reservationMaterial;
			private float reservationElectric;

			public SupplyRequest()
			{
				updateFlag = false;
				reservationPersonnel = 0;
				reservationMaterial = 0;
				reservationElectric = 0;
			}

			public float ReservationPersonnel
			{
				get => reservationPersonnel; set
				{
					if (!updateFlag && Mathf.Approximately(reservationPersonnel, value)) return;
					reservationPersonnel = value;
					updateFlag = true;
				}
			}
			public float ReservationMaterial
			{
				get => reservationMaterial; set
				{
					if (!updateFlag && Mathf.Approximately(reservationMaterial, value)) return;
					reservationMaterial = value;
					updateFlag = true;
				}
			}
			public float ReservationElectric
			{
				get => reservationElectric; set
				{
					if (!updateFlag && Mathf.Approximately(reservationElectric, value)) return;
					reservationElectric = value;
					updateFlag = true;
				}
			}

			public bool IsUpdateFlag() => updateFlag;
			public void ResetAndLeaveDecimal(out int integerPersonnel, out int integerMaterial, out int integerElectric)
			{
				integerPersonnel = (int)reservationPersonnel;
				reservationPersonnel -= integerPersonnel;

				integerMaterial = (int)reservationMaterial;
				reservationMaterial -= integerMaterial;

				integerElectric = (int)reservationElectric;
				reservationElectric -= integerElectric;

				updateFlag = false;
			}
		}
		public abstract class StrategyUpdate_ResourcesSupply : StrategyUpdateSubClass<StrategyUpdate_ResourcesSupply.ResourcesSupply>
		{
			private BaseList<SectorObject> sectorList;
			private BaseList<Faction> factionList;

			private Dictionary<SectorObject, SupplyRequest> sectorSupplyRequest;
			private Dictionary<Faction, SupplyRequest> factionSupplyRequest;
	
			protected StrategyUpdate_ResourcesSupply(StrategyUpdate updater) : base(updater)
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

					int recoveryPersonnel = sector.StatsValue.GetStatsValue(StrategyGamePlayData.StatsType.거점_인력_회복);
					int recoveryMaterial = sector.StatsValue.GetStatsValue(StrategyGamePlayData.StatsType.거점_재료_회복);
					int recoveryElectric = sector.StatsValue.GetStatsValue(StrategyGamePlayData.StatsType.거점_전력_회복);

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
						float factor = depthfactor * deltaTime;

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

					int capacityPersonnel = sector.StatsValue.GetStatsValue(StrategyGamePlayData.StatsType.거점_인력_최대);
					int localPersonnel = sector.StatsValue.GetStatsValue(StrategyGamePlayData.StatsType.거점_인력_현재);
					float sectorPersonnel = supplyRequest.ReservationPersonnel;
					float factionPersonnel = 0f;
					SectorToFaction(in capacityPersonnel, in localPersonnel,
						ref sectorPersonnel, ref factionPersonnel);
					supplyRequest.ReservationPersonnel = sectorPersonnel;
					factionSupply.ReservationPersonnel += factionPersonnel;


					int capacityMaterial = sector.StatsValue.GetStatsValue(StrategyGamePlayData.StatsType.거점_재료_최대);
					int localMaterial = sector.StatsValue.GetStatsValue(StrategyGamePlayData.StatsType.거점_재료_현재);
					float sectorMaterial = supplyRequest.ReservationMaterial;
					float factionMaterial = 0f;
					SectorToFaction(in capacityMaterial, in localMaterial,
						ref sectorMaterial, ref factionMaterial);
					supplyRequest.ReservationPersonnel = sectorMaterial;
					factionSupply.ReservationPersonnel += factionMaterial;

					int capacityElectric = sector.StatsValue.GetStatsValue(StrategyGamePlayData.StatsType.거점_전력_최대);
					int localElectric = sector.StatsValue.GetStatsValue(StrategyGamePlayData.StatsType.거점_전력_현재);
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
						float factionValue = sectorRecovery * ratio;
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
				int factionCount = factionList.Count;

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
