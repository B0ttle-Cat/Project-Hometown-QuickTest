using System;
using System.Collections.Generic;

using UnityEngine;

using static StrategyUpdate.StrategyUpdate_UnitCombatTargetUpdate;

public interface IStrategyUpdater : IDisposable
{
	void Start();
	void Update(in float deltaTime);
}

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

		NearbyUpdate,
		세력_감지목록_업데이트,
		각종_상태_업데이트,         // 전체 FSM Update 진행

		유닛_CombatTarget_업데이트,
		유닛_보급충전,
		유닛_기본변수갱신,
		유닛_버프계산,
		유닛_이동,
		유닛_공격_업데이트,         // 공격 딜레이 계산 및 공격 생성
		유닛_데미지_계산,          // 충돌된 데이미 계산을 진행
		유닛_사망_처리,               // HP 없는 유닛을 삭제.

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

			(UpdateLogicSort.거점_시설보급, null),
			(UpdateLogicSort.거점_버프계산, null),
			(UpdateLogicSort.유닛_보급충전, null),

			(UpdateLogicSort.NearbyUpdate, new StrategyUpdate_NearbyUpdate(this)),
			(UpdateLogicSort.세력_감지목록_업데이트, new StrategyUpdate_FactionDetectListUpdate(this)),
			(UpdateLogicSort.유닛_CombatTarget_업데이트, new StrategyUpdate_UnitCombatTargetUpdate(this)),
			(UpdateLogicSort.각종_상태_업데이트,  new StrategyUpdate_FSMUpdater(this)),

			(UpdateLogicSort.유닛_기본변수갱신, null),
			(UpdateLogicSort.유닛_버프계산,  new StrategyUpdate_UnitBuff(this)),

			(UpdateLogicSort.작전_기본변수_갱신, new StrategyUpdate_OperationUpdate(this)),

			(UpdateLogicSort.유닛_이동,  new StrategyUpdate_NodeMovement(this)),
			(UpdateLogicSort.유닛_공격_업데이트,  null),
			(UpdateLogicSort.유닛_데미지_계산,  null),
			(UpdateLogicSort.유닛_사망_처리,  null),

			(UpdateLogicSort.거점_자원갱신종료,  new StrategyUpdate_EndedSectorResourcesSupply(this)),
			(UpdateLogicSort.세력_자원갱신종료,  new StrategyUpdate_EndedFactionResourcesSupply(this)),

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
public abstract class StrategyUpdateSubClass<T> : IStrategyUpdater where T : StrategyUpdateSubClass<T>.UpdateLogic
{
	protected StrategyUpdate thisUpdater;
	protected StrategyUpdate.StrategyUpdateTempData TempData => thisUpdater == null ? null : thisUpdater.TempData;
	public StrategyUpdateSubClass(StrategyUpdate updater)
	{
		thisUpdater = updater;
		updateList = new List<T>();
	}
	protected List<T> updateList;
	public virtual List<T> UpdateList { get => updateList; protected set => updateList = value; }

	void IStrategyUpdater.Start() => Start();
	void IStrategyUpdater.Update(in float deltaTime) => Update(in deltaTime);
	void IDisposable.Dispose()
	{
		thisUpdater = null;

		if (UpdateList != null)
		{
			int length = UpdateList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				UpdateList[i].Dispose();
			}
			UpdateList.Clear();
		}
		UpdateList = null;

		Dispose();
	}
	protected abstract void Dispose();
	protected abstract void Start();
	protected abstract void Update(in float deltaTime);
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
	public class StrategyUpdate_UnitCombatTargetUpdate : StrategyUpdateSubClass<CombatTarget>
	{
		public StrategyUpdate_UnitCombatTargetUpdate(StrategyUpdate updater) : base(updater)
		{
		}

		protected override void Dispose()
		{
		}

		protected override void Start()
		{
			StrategyManager.Collector.AddChangeListener<UnitObject>(OnChangeValue, ForeachAll);
			void ForeachAll(IStrategyElement element)
			{
				OnChangeValue(element, true);
			}
		}

		private void OnChangeValue(IStrategyElement element, bool added)
		{
			if (element == null || element is not UnitObject unitObject) return;

			if (added)
			{
				UpdateList.Add(new CombatTarget(unitObject, this));
			}
			else
			{
				int findIndex = UpdateList.FindIndex(i=>i.unitObject == unitObject);
				if (findIndex < 0) return;
				UpdateList.RemoveAt(findIndex);
			}
		}



		protected override void Update(in float deltaTime)
		{
		}

		public class CombatTarget : UpdateLogic
		{
			public readonly UnitObject unitObject;
			public readonly IUnitCombatController combatController;
			public CombatTarget(UnitObject unitObject, StrategyUpdate_UnitCombatTargetUpdate thisSubClass) : base(thisSubClass)
			{
				this.unitObject = unitObject;
				combatController = unitObject;
			}

			protected override void OnDispose()
			{
			}

			protected override void OnUpdate(in float deltaTime)
			{
				if (unitObject == null) return;
				if (!combatController.IsCombatState) return;

				combatController.UpdateParameters();

				if (combatController.IsKeepingTargetAllowed()) return;

				if (combatController.SearchingNewTarget(out var newTarget))
				{
					combatController.SetCombatTarget(newTarget);
				}
				else
				{
					combatController.ClearCombatTarget();
				}
			}
		}
	}
}