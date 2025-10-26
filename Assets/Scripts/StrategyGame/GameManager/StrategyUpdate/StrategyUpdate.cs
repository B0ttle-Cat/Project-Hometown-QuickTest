using System;
using System.Collections.Generic;

using UnityEngine;

public interface IStrategyUpdater : IDisposable
{
	void Start();
	void Update(in float deltaTime);
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
	public abstract class UpdateLogic : IDisposable
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
}
public partial class StrategyUpdate : MonoBehaviour
{
	private StrategyUpdateTempData tempData;

	private List<(UpdateLogicSort type, IStrategyUpdater updater)> updateList;
	public StrategyUpdateTempData TempData => tempData;

	public enum UpdateLogicSort
	{
		None = 0,

		거점_점령상태,
		거점_시설버프계산,

		거점_시설_건설,

		거점_전력_보충,
		거점_물자_보충,
		거점_물류_네트워크_업데이트,
		거점_인력_보충,
		거점_자원갱신이벤트,
		거점_유닛버프계산,

		유닛_인스턴스생성,
		유닛_버프_계산,
		유닛_이동,
		유닛_상태_업데이트,     // 유닛의 위치와 스텟을 토대로 어떤 행동을 할지 결정한다.
		유닛_공격_업데이트,     // 공격 딜레이 계산 및 공격 생성
		유닛_데미지_계산,          // 충돌된 데이미 계산을 진행
		유닛_사망_처리,           // HP 없는 유닛을 삭제.

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
		public void SetTrigger(string key, UpdateLogicSort alive)
		{
			int findIndex = dataList.FindIndex(d=>d.key == key);
			if (findIndex < 0) dataList.Add(new DataValue(key, true, alive));
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

	public void OnEnable()
	{
		tempData = new StrategyUpdateTempData();
		updateList = new List<(UpdateLogicSort type, IStrategyUpdater updater)>()
		{
			(UpdateLogicSort.거점_점령상태,  new StrategyUpdate_CaptureUpdate(this)),
			(UpdateLogicSort.거점_시설_건설,  new StrategyUpdate_ConstructUpdate(this)),
			(UpdateLogicSort.거점_시설버프계산,  null),

			(UpdateLogicSort.거점_전력_보충,  new StrategyUpdate_ElectricSupply(this)),
			(UpdateLogicSort.거점_물자_보충,  new StrategyUpdate_MaterialSupply(this)),
			(UpdateLogicSort.거점_인력_보충,  new StrategyUpdate_PersonnelSupply(this)),

			(UpdateLogicSort.거점_물류_네트워크_업데이트,  null),

			(UpdateLogicSort.거점_자원갱신이벤트,  new StrategyUpdate_EndedResourcesSupply(this)),
			(UpdateLogicSort.거점_유닛버프계산, null),

			(UpdateLogicSort.유닛_인스턴스생성, null),
			(UpdateLogicSort.유닛_버프_계산,  new StrategyUpdate_UnitBuff(this)),

			(UpdateLogicSort.유닛_이동,  null),
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

