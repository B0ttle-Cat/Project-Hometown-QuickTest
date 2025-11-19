using System;
using System.Collections.Generic;

using UnityEngine;

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
		거점_시설버프계산,

		거점_시설_건설,

		세력_자원갱신시작,
		거점_전력_보충,
		거점_재료_보충,
		거점_인력_보충,
		거점_자원갱신시작,
		거점_자원분배,
		거점_자원갱신종료,
		세력_자원갱신종료,
		거점_유닛버프계산,

		유닛_기본변수_갱신,
		유닛_버프_계산,
		유닛_이동,
		유닛_상태_업데이트,     // 유닛의 위치와 스텟을 토대로 어떤 행동을 할지 결정한다.
		유닛_공격_업데이트,     // 공격 딜레이 계산 및 공격 생성
		유닛_데미지_계산,          // 충돌된 데이미 계산을 진행
		유닛_사망_처리,           // HP 없는 유닛을 삭제.

		작전_기본변수_갱신,

		UI,


		End = int.MaxValue,
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
			else dataList[findIndex] = new DataValue(key, value);
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

			(UpdateLogicSort.거점_시설버프계산, null),
			(UpdateLogicSort.거점_유닛버프계산, null),

			(UpdateLogicSort.세력_자원갱신시작,  new StrategyUpdate_StartFactionResourcesSupply(this)),
			(UpdateLogicSort.거점_자원갱신시작,  new StrategyUpdate_StartSectorResourcesSupply(this)),
			(UpdateLogicSort.거점_자원분배,  null),
			(UpdateLogicSort.거점_자원갱신종료,  new StrategyUpdate_EndedSectorResourcesSupply(this)),
			(UpdateLogicSort.세력_자원갱신종료,  new StrategyUpdate_EndedFactionResourcesSupply(this)),

			(UpdateLogicSort.유닛_기본변수_갱신, null),
			(UpdateLogicSort.유닛_버프_계산,  new StrategyUpdate_UnitBuff(this)),

			(UpdateLogicSort.작전_기본변수_갱신, new StrategyUpdate_OperationUpdate(this)),

			(UpdateLogicSort.유닛_이동,  new StrategyUpdate_NodeMovement(this)),
			(UpdateLogicSort.유닛_상태_업데이트,  null),
			(UpdateLogicSort.유닛_공격_업데이트,  null),
			(UpdateLogicSort.유닛_데미지_계산,  null),
			(UpdateLogicSort.유닛_사망_처리,  null),

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
	protected abstract void Start();
	protected abstract void Update(in float deltaTime);
	protected virtual void Dispose()
	{

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
	public abstract partial class UpdateLogic // ResourcesSupply
	{
		protected bool ResourcesUpdate(ref int current, in int max, in int supplyPerTanSec, ref float supplement, ref float currentResupplyTime, in float resetResupplyTime, in float deltaTime)
		{
			if(max <= 0 || max < current) return false;
			CumulativeUpdate(in current, in max, in supplyPerTanSec, ref supplement, in deltaTime);
			if (CheckResupplyTime(ref currentResupplyTime, in resetResupplyTime, in deltaTime))
			{
				return SupplyUpdate(ref current, in max, ref supplement);
			}
			return false;
		}
		protected void CumulativeUpdate(in int current, in int max, in int supplyPerTanSec, ref float supplement, in float deltaTime)
		{
			if (current >= max)
			{
				supplement = 0;
				return;
			}
			float supplyPerDelta  = supplyPerTanSec * 0.1f * deltaTime;
			supplement += supplyPerDelta;
		}
		protected bool CheckResupplyTime(ref float currentResupplyTime, in float resetResupplyTime, in float deltaTime)
		{
			currentResupplyTime -= deltaTime;
			if (currentResupplyTime <= 0)
			{
				currentResupplyTime = resetResupplyTime;
				return true;
			}
			return false;
		}
		protected bool SupplyUpdate(ref int current, in int max, ref float cumulative)
		{
			if (current >= max)
			{
				cumulative = 0;
				return false;
			}
			if (cumulative < 1) return false;
			
			int intCumulative = (int)cumulative;
			cumulative -= intCumulative;

			current = Mathf.Clamp(current + intCumulative, 0, max);
			
			return true;
		}

	}
	public abstract partial class UpdateLogic // Faction
	{
		public string FactionKey(Faction faction) => FactionKey(faction.FactionID);
		public string FactionIsAliveKey(Faction faction) => FactionIsAliveKey(faction);
		public string FactionTempSupplyValueKey(Faction faction) => FactionTempSupplyValueKey(faction);
		public string FactionKey(int faction) => $"FactionKey_{faction}";
		public string FactionIsAliveKey(int faction) => $"FactionIsAliveKey_{faction}";
		public string FactionTempSupplyValueKey(int faction) => $"FactionTempSupplyValueKey_{faction}";
		public record FactionTempSupplyValue
		{
			public readonly  int factionID;

			public int manpower;
			public int manpowerMax;

			public int material;
			public int materialMax;

			public int electric;
			public int electricMax;

            public FactionTempSupplyValue(Faction faction)
            {
				this.factionID = faction == null ? -1 : faction.FactionID;
			}
        }
	}
}
public class StrategyUpdate_OperationUpdate : StrategyUpdateSubClass<StrategyUpdate_OperationUpdate.OperationUpdate>
{
	public StrategyUpdate_OperationUpdate(StrategyUpdate updater) : base(updater)
	{
	}

	protected override void Start()
	{
		UpdateList = new();
		var iList = StrategyManager.Collector.OperationList;
		foreach (var item in iList)
		{
			if (item == null) continue;
			UpdateList.Add(new(item, this));
		}
		StrategyManager.Collector.AddChangeListener<OperationObject>(ChangeList);
	}
	protected override void Dispose()
	{
		StrategyManager.Collector.RemoveChangeListener<OperationObject>(ChangeList);
	}
	private void ChangeList(IStrategyElement element, bool isAdd)
	{
		if (element is not OperationObject op) return;

		if (isAdd)
		{
			UpdateList.Add(new OperationUpdate(op, this));
		}
		else
		{
			int findIndex = UpdateList.FindIndex(l => l.operationObject == op);
			if (findIndex >= 0) return;
			UpdateList.RemoveAt(findIndex);
		}
	}

	protected override void Update(in float deltaTime)
	{
		int length = UpdateList.Count;
		for (int i = 0 ; i < length ; i++)
		{
			var update = updateList[i];
			if (update == null) continue;
			update.Update(deltaTime);
		}
	}
	public class OperationUpdate : UpdateLogic
	{
		public OperationObject operationObject;
		public OperationUpdate(OperationObject operationObject, StrategyUpdateSubClass<OperationUpdate> thisSubClass) : base(thisSubClass)
		{
			this.operationObject = operationObject;
		}

		protected override void OnDispose()
		{
			operationObject = null;
		}

		protected override void OnUpdate(in float deltaTime)
		{
			if (operationObject == null) return;
			operationObject.ComputeOperationValue();
		}
	}
}