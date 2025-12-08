using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Sirenix.OdinInspector;

using UnityEngine;
/*
public partial class StrategyElementCollector2 : MonoBehaviour, IDisposable
{
	public abstract class CollectList
	{
		public abstract IList IList { get; }
	}
	[Serializable]
	public class ElementList<T> : CollectList, IEnumerable<T>, IDisposable where T : class, IStrategyElement
	{
		[SerializeField]
		private List<T> list;
		public List<T> List => list ??= new List<T>();
		public override IList IList => List;

		private Action<T, bool> onChange;
		private bool sleepCallback;
		public IEnumerator<T> GetEnumerator()
		{
			return List.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return List.GetEnumerator();
		}

		private int nextUniqueID;
		private HashSet<int> recyclingID;
		private int[] lockingID;
		public ElementList(int capacity = 32)
		{
			onChange = null;

			sleepCallback = false;

			if (list == null)
			{
				list = new List<T>(capacity);
			}
			else
			{
				int length = list.Count;
				for (int i = 0 ; i < length ; i++)
				{
					var element = list[i];
					if (element != null && element is IStrategyElement iElement)
						iElement.OutStrategyCollector();
				}
				list.Clear();
			}
			nextUniqueID = 0;
			recyclingID = new HashSet<int>();
		}
		public void Dispose()
		{
			if (list != null)
			{
				list.Clear();
				list = null;
			}
			nextUniqueID = 0;
			recyclingID = null;
			onChange = null;
			sleepCallback = false;
		}
		public void LockUniqueID(int[] lockingID)
		{
			this.lockingID = lockingID;
		}
		public void UnlockUniqueID()
		{
			lockingID = null;
		}
		private bool IsLockID(int uniqueID)
		{
			int length = lockingID == null ? 0 : lockingID.Length;
			for (int i = 0 ; i < length ; i++)
			{
				if (lockingID[i] == uniqueID) return true;
			}
			return false;
		}
		private int GetNextUniqueID()
		{
			if (recyclingID.Count > 0)
			{
				foreach (var item in recyclingID)
				{
					if (!IsLockID(item))
						return item;
				}
			}
			while (IsLockID(nextUniqueID))
			{
				RemoveUniqueID(nextUniqueID);
				nextUniqueID++;
			}
			return nextUniqueID;
		}
		private void RemoveUniqueID(int uniqueID)
		{
			if (!recyclingID.Add(uniqueID))
			{
				Debug.LogError($"중복된 ID({uniqueID})를 사용중 이었던 것으로 보임");
			}
		}
		private void UsedUniqueID(int uniqueID)
		{
			if (recyclingID.Remove(uniqueID))
			{
				return;
			}
			else if (nextUniqueID == uniqueID)
			{
				nextUniqueID++;
			}
			else
			{
				Debug.LogError($"GetNextUniqueID 으로 얻을수 있는 ID({uniqueID}) 가 아님");
			}
		}
		public bool AddElement(IEnumerable<T> elements)
		{
			Queue<T> changeList = new Queue<T>();
			sleepCallback = true;
			foreach (var element in elements)
			{
				if (AddElement(element))
				{
					changeList.Enqueue(element);
				}
			}
			sleepCallback = false;

			int changeListCount = changeList.Count;
			bool isChange = changeListCount > 0;
			if (isChange)
			{
				while (changeList.TryDequeue(out var dequeue))
				{
					Invoke(dequeue, true);
				}
				changeList = null;
			}
			return isChange;
		}
		public bool RemoveElement(IEnumerable<T> elements)
		{
			Queue<T> changeList = new Queue<T>();
			sleepCallback = true;
			foreach (var element in elements)
			{
				if (RemoveElement(element))
				{
					changeList.Enqueue(element);
				}
			}
			sleepCallback = false;

			int changeListCount = changeList.Count;
			bool isChange = changeListCount > 0;
			if (isChange)
			{
				while (changeList.TryDequeue(out var dequeue))
				{
					Invoke(dequeue, false);
				}
				changeList = null;
			}
			return isChange;
		}
		public bool AddElement(T element)
		{
			if (element == null) return false;

			if (!list.Contains(element))
			{
				list.Add(element);
				element.ThisElement.ID = GetNextUniqueID();
				UsedUniqueID(element.ThisElement.ID);
				element.InStrategyCollector();
				Invoke(element, true);
				return true;
			}
			return false;
		}
		public bool RemoveElement(T element)
		{
			if (element == null) return false;
			if (list.Remove(element))
			{
				RemoveUniqueID(element.ID);
				element.OutStrategyCollector();
				Invoke(element, false);
				return true;
			}
			return false;
		}
		public void Invoke(T element, bool isAdded)
		{
			if (sleepCallback || onChange == null) return;
			try
			{
				onChange.Invoke(element, isAdded);
			}
			catch (Exception ex) { Debug.LogException(ex); }
		}
		public void OnAddListener(Action<IStrategyElement, bool> action)
		{
			if (action == null) return;
			onChange -= action;
			onChange += action;
		}
		public void OnRemoveListener(Action<IStrategyElement, bool> action)
		{
			if (action == null) return;
			onChange -= action;
		}
		public T Find(Func<T, bool> condition)
		{
			if (condition == null) return null;

			int length = list.Count;

			for (int i = 0 ; i < length ; i++)
			{
				var item = list[i];
				if (item == null) continue;
				if (condition.Invoke(item))
				{
					return item;
				}
			}

			return null;
		}
		public List<T> FindList(Func<T, bool> condition)
		{
			List<T> result = new List<T>();

			if (condition == null) return result;

			int length = list.Count;

			for (int i = 0 ; i < length ; i++)
			{
				var item = list[i];
				if (item == null) continue;
				if (condition.Invoke(item))
				{
					result.Add(item);
				}
			}

			return result;
		}
		public void Foreach(Action<T> action)
		{
			if (action == null) return;
			foreach (var item in list)
			{
				if (item == null) continue;

				action(item);
			}
		}
	}
	[Serializable]
	public class OtherTypeList<T> : CollectList, IEnumerable<T>, IDisposable
	{
		[SerializeField]
		private List<T> list;
		public List<T> List => list ??= new List<T>();
		public override IList IList => List;

		private Action<T, bool> onChange;
		private bool sleepCallback;
		public IEnumerator<T> GetEnumerator()
		{
			return List.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return List.GetEnumerator();
		}

		private int nextUniqueID;
		private HashSet<int> recyclingID;
		private int[] lockingID;
		public OtherTypeList(int capacity = 32)
		{
			onChange = null;

			sleepCallback = false;

			if (list == null)
			{
				list = new List<T>(capacity);
			}
			else
			{
				int length = list.Count;
				for (int i = 0 ; i < length ; i++)
				{
					var element = list[i];
					if (element != null && element is IStrategyElement iElement)
						iElement.OutStrategyCollector();
				}
				list.Clear();
			}
			nextUniqueID = 0;
			recyclingID = new HashSet<int>();
		}
		public void Dispose()
		{
			if (list != null)
			{
				list.Clear();
				list = null;
			}
			nextUniqueID = 0;
			recyclingID = null;
			onChange = null;
			sleepCallback = false;
		}
		public bool AddItem(IEnumerable<T> item)
		{
			Queue<T> changeList = new Queue<T>();
			sleepCallback = true;
			foreach (var element in item)
			{
				if (AddItem(element))
				{
					changeList.Enqueue(element);
				}
			}
			sleepCallback = false;

			int changeListCount = changeList.Count;
			bool isChange = changeListCount > 0;
			if (isChange)
			{
				while (changeList.TryDequeue(out var dequeue))
				{
					Invoke(dequeue, true);
				}
				changeList = null;
			}
			return isChange;
		}
		public bool RemoveItem(IEnumerable<T> item)
		{
			Queue<T> changeList = new Queue<T>();
			sleepCallback = true;
			foreach (var element in item)
			{
				if (RemoveItem(element))
				{
					changeList.Enqueue(element);
				}
			}
			sleepCallback = false;

			int changeListCount = changeList.Count;
			bool isChange = changeListCount > 0;
			if (isChange)
			{
				while (changeList.TryDequeue(out var dequeue))
				{
					Invoke(dequeue, false);
				}
				changeList = null;
			}
			return isChange;
		}
		public bool AddItem(T item)
		{
			if (item == null) return false;

			if (!list.Contains(item))
			{
				list.Add(item);
				Invoke(item, true);
				return true;
			}
			return false;
		}
		public bool RemoveItem(T item)
		{
			if (item == null) return false;
			if (list.Remove(item))
			{
				Invoke(item, false);
				return true;
			}
			return false;
		}
		public void Invoke(T item, bool isAdded)
		{
			if (sleepCallback || onChange == null) return;
			try
			{
				onChange.Invoke(item, isAdded);
			}
			catch (Exception ex) { Debug.LogException(ex); }
		}
		public void OnAddListener(Action<T, bool> action)
		{
			if (action == null) return;
			onChange -= action;
			onChange += action;
		}
		public void OnRemoveListener(Action<T, bool> action)
		{
			if (action == null) return;
			onChange -= action;
		}
		public bool TryFind(Func<T, bool> condition, out T t)
		{
			if (condition == null)
			{
				t = default;
				return false;
			}
			int length = list.Count;

			for (int i = 0 ; i < length ; i++)
			{
				var item = list[i];
				if (item == null) continue;
				if (condition.Invoke(item))
				{
					t = item;
					return true;
				}
			}

			t = default;
			return false;
		}
		public List<T> FindList(Func<T, bool> condition)
		{
			List<T> result = new List<T>();

			if (condition == null) return result;

			int length = list.Count;

			for (int i = 0 ; i < length ; i++)
			{
				var item = list[i];
				if (item == null) continue;
				if (condition.Invoke(item))
				{
					result.Add(item);
				}
			}

			return result;
		}
		public void Foreach(Action<T> action)
		{
			if (action == null) return;
			foreach (var item in list)
			{
				if (item == null) continue;

				action(item);
			}
		}
	}
	[SerializeField]
	private ElementList<SectorObject> sectorList;
	[SerializeField]
	private ElementList<Faction> factionList;
	[SerializeField]
	private ElementList<UnitObject> unitList;
	[SerializeField]
	public ElementList<OperationObject> operationList;
	[SerializeField]
	private ElementList<SkillObject> skillList;
	private Dictionary<Type, CollectList> _elementLists;
	public List<SectorObject> SectorList => sectorList?.List ?? new List<SectorObject>();
	public List<Faction> FactionList => factionList?.List ?? new List<Faction>();
	public List<UnitObject> UnitList => unitList?.List ?? new List<UnitObject>();
	public List<OperationObject> OperationList => operationList?.List ?? new List<OperationObject>();
	public List<SkillObject> SkillList => skillList?.List ?? new List<SkillObject>();
	public event Action<IStrategyElement, bool> OnChangeAnyElement;
	private Dictionary<Type, IList> _listCache;

	// Other
	[ShowInInspector]
	private Dictionary<Type, CollectList> otherList;
	public Dictionary<Type, CollectList> OtherList => otherList ?? new Dictionary<Type, CollectList>();


	public IEnumerable<IList> GetAllElementIList()
	{
		yield return SectorList;
		yield return FactionList;
		yield return UnitList;
		yield return OperationList;
		yield return SkillList;
	}
	#region Init
	internal void Init()
	{
		InitSector();
		InitFaction();
		InitUnit();
		InitOperation();
		InitSkill();
		InitOther();

		OnChangeAnyElement = null;
		sectorList.OnAddListener(_OnChangeAnyElement);
		factionList.OnAddListener(_OnChangeAnyElement);
		unitList.OnAddListener(_OnChangeAnyElement);
		operationList.OnAddListener(_OnChangeAnyElement);
		skillList.OnAddListener(_OnChangeAnyElement);
	}
	private void InitListTypeCache()
	{
		if (_listCache != null) return;

		_listCache = new Dictionary<Type, IList>
		{
			[typeof(SectorObject)] = SectorList,
			[typeof(Faction)] = FactionList,
			[typeof(UnitObject)] = UnitList,
			[typeof(OperationObject)] = OperationList,
			[typeof(SkillObject)] = SkillList,
		};
	}
	private void InitElementListCache()
	{
		if (_elementLists != null) return;

		_elementLists = new Dictionary<Type, CollectList>
		{
			[typeof(SectorObject)] = sectorList,
			[typeof(Faction)] = factionList,
			[typeof(UnitObject)] = unitList,
			[typeof(OperationObject)] = operationList,
			[typeof(SkillObject)] = skillList,
		};
	}
	public void InitSector() => sectorList = new ElementList<SectorObject>(32);
	public void InitFaction() => factionList = new ElementList<Faction>(8);
	public void InitUnit() => unitList = new ElementList<UnitObject>(512);
	public void InitOperation() => operationList = new ElementList<OperationObject>(32);
	public void InitSkill() => skillList = new ElementList<SkillObject>(512);
	public void InitOther()
	{
		if (otherList != null)
		{
			foreach (var item in otherList)
			{
				if (item.Value is IDisposable disposable) disposable.Dispose();
			}
			otherList = null;
		}
		otherList = new Dictionary<Type, CollectList>();
	}
	public void Dispose()
	{
		sectorList?.Dispose();
		factionList?.Dispose();
		unitList?.Dispose();
		operationList?.Dispose();
		skillList?.Dispose();

		sectorList = null;
		factionList = null;
		unitList = null;
		operationList = null;
		skillList = null;

		if (otherList != null)
		{
			foreach (var item in otherList)
			{
				if (item.Value is IDisposable disposable) disposable.Dispose();
			}
			otherList = null;
		}
	}
	#endregion
	#region Add/Remove
	public void AddElement<TList, TItem>(TList elements) where TList : IEnumerable<TItem> where TItem : class, IStrategyElement
	{
		_ = elements switch
		{
			IEnumerable<SectorObject> item => sectorList.AddElement(item),
			IEnumerable<Faction> item => factionList.AddElement(item),
			IEnumerable<UnitObject> item => unitList.AddElement(item),
			IEnumerable<OperationObject> item => operationList.AddElement(item),
			IEnumerable<SkillObject> item => skillList.AddElement(item),
			_ => default
		};
	}
	public void AddElement<T>(T element) where T : class, IStrategyElement
	{
		var list = GetElementByType<T>();
		list?.AddElement(element);
	}

	public void RemoveElement<T>(T element) where T : class, IStrategyElement
	{
		_ = element switch
		{
			SectorObject item => sectorList.RemoveElement(item),
			Faction item => factionList.RemoveElement(item),
			UnitObject item => unitList.RemoveElement(item),
			OperationObject item => operationList.RemoveElement(item),
			SkillObject item => skillList.RemoveElement(item),
			_ => default,
		};
	}
	public void RemoveElement<T>(IEnumerable<T> elements) where T : class, IStrategyElement
	{
		foreach (var element in elements)
		{
			RemoveElement(element);
		}
	}

	public void AddOther<T>(T item)
	{
		var dic = OtherList;
		var typeKey = typeof(T);
		if (dic.TryGetValue(typeKey, out var collectList) && collectList is OtherTypeList<T> otherList)
		{
			otherList.AddItem(item);
		}
		else
		{
			otherList = new OtherTypeList<T>(8);
			dic[typeKey] = otherList;
			otherList.AddItem(item);
		}
	}
	public void RemoveOther<T>(T item)
	{
		var dic = OtherList;
		var typeKey = typeof(T);
		OtherTypeList<T> otherList = null;
		if (!dic.TryGetValue(typeKey, out var collectList) || collectList is not OtherTypeList<T>)
		{
			return;
		}
		otherList = dic[typeKey] as OtherTypeList<T>;
		otherList.RemoveItem(item);
	}
	#endregion
	#region GetListByType
	private IList GetListByType<T>()
	{
		InitListTypeCache();
		return _listCache.TryGetValue(typeof(T), out var list) ? list : default;
	}
	private ElementList<T> GetElementByType<T>() where T : class, IStrategyElement
	{
		InitElementListCache();
		return _elementLists.TryGetValue(typeof(T), out var element) ? element as ElementList<T> : default;
	}
	#endregion
	#region ChangeListener
	public void AddChangeListener<T>(Action<IStrategyElement, bool> action, out List<T> getCurrentList) where T : class, IStrategyElement
	{
		var element = GetElementByType<T>();

		element.OnAddListener(action);

		getCurrentList = element.IList as List<T>;
	}
	public void AddChangeListener<T>(Action<IStrategyElement, bool> action, Action<IStrategyElement> allForeach) where T : class, IStrategyElement
	{
		AddChangeListener(action, out List<T> list);
		if (allForeach == null || list == null || list.Count == 0) return;
		foreach (var item in list)
		{
			allForeach.Invoke(item);
		}
	}
	public void AddChangeListener<T>(Action<IStrategyElement, bool> action) where T : class, IStrategyElement
	{
		var element = GetElementByType<T>();

		element.OnAddListener(action);
	}
	public void RemoveChangeListener<T>(Action<IStrategyElement, bool> action) where T : class, IStrategyElement
	{
		if (action == null) return;

		ElementList<T> element = GetElementByType<T>();
		element.OnRemoveListener(action);
	}
	public void AddChangeAnyListener(Action<IStrategyElement, bool> action, Action<IStrategyElement> allForeach = null)
	{
		if (allForeach != null)
		{
			foreach (var list in GetAllElementIList())
			{
				foreach (IStrategyElement element in list)
				{
					allForeach.Invoke(element);
				}
			}
		}
		OnChangeAnyElement -= action;
		OnChangeAnyElement += action;
	}
	public void RemoveChangeAnyListener(Action<IStrategyElement, bool> action)
	{
		OnChangeAnyElement -= action;
	}
	private void _OnChangeAnyElement(IStrategyElement element, bool added)
	{
		OnChangeAnyElement?.Invoke(element, added);
	}
	public void AddOtherChangeListener<T>(Action<T, bool> action, Action<T> allForeach = null)
	{
		var dic = OtherList;
		var typeKey = typeof(T);
		if (dic.TryGetValue(typeKey, out var collectList) && collectList is OtherTypeList<T> otherList)
		{
			otherList.OnAddListener(action);
		}
		else
		{
			otherList = new OtherTypeList<T>();
			dic[typeKey] = otherList;
			otherList.OnAddListener(action);
		}
		if (allForeach != null)
		{
			foreach (var item in otherList.List)
			{
				allForeach?.Invoke(item);
			}
		}
	}
	public void RemoveOtherChangeListener<T>(Action<T, bool> action)
	{
		var dic = OtherList;
		var typeKey = typeof(T);
		if (dic.TryGetValue(typeKey, out var collectList) && collectList is OtherTypeList<T> otherList)
		{
			otherList.OnRemoveListener(action);
		}
	}

	internal void AddChangeListener<T>(object onChangeValue, object foreachAll)
	{
		throw new NotImplementedException();
	}
	#endregion
}
public partial class StrategyElementCollector2 // Finder 
{
	#region FindElement
	public bool TryFindElement<T>(Func<T, bool> condition, out T find) where T : class, IStrategyElement
	{
		find = null;
		if (condition == null) return false;

		var list = GetListByType<T>();
		for (int i = 0 ; i < list.Count ; i++)
		{
			if (list[i] is T t && condition(t))
			{
				find = t;
				return true;
			}
		}
		return false;

	}
	public T FindElement<T>(Func<T, bool> condition) where T : class, IStrategyElement
	{
		if (TryFindElement<T>(condition, out T find))
		{
			return find;
		}
		return null;
	}

	public List<T> FindElementList<T>(Func<T, bool> condition)
	{
		List<T> find = new List<T>();
		if (condition == null) return find;

		var list = GetListByType<T>();
		for (int i = 0 ; i < list.Count ; i++)
		{
			if (list[i] is T t && condition(t))
			{
				find.Add(t);
			}
		}
		return find;
	}
	#endregion

	#region Sector
	public bool TryFindSector(string findName, out SectorObject find)
	{
		if (string.IsNullOrWhiteSpace(findName))
		{
			find = null;
			return false;
		}
		return TryFindElement<SectorObject>(f => f.SectorName == findName, out find);
	}
	public SectorObject FindSector(string findName)
	{
		return FindElement<SectorObject>(f => f.SectorName == findName);
	}
	public bool TryFindSector(int sectorID, out SectorObject find)
	{
		return TryFindElement<SectorObject>(f => f.ThisElement.ID == sectorID, out find);
	}
	public SectorObject FindSector(int sectorID)
	{
		return FindElement<SectorObject>(f => f.ThisElement.ID == sectorID);
	}

	public string SectorIDToName(int sectorID)
	{
		if (sectorID >= 0 && TryFindElement<SectorObject>(f => f.ThisElement.ID == sectorID, out var find))
		{
			return find.SectorName;
		}
		return "";
	}
	public int SectorNameToID(string findName)
	{
		if (!string.IsNullOrWhiteSpace(findName) && TryFindElement<SectorObject>(f => f.SectorName == findName, out var find))
		{
			return find.ThisElement.ID;
		}
		return -1;
	}
	#endregion

	#region Faction
	public bool TryFindFaction(string findName, out Faction find)
	{
		return TryFindElement<Faction>(f => f.FactionName == findName, out find);
	}
	public Faction FindFaction(string findName)
	{
		return FindElement<Faction>(f => f.FactionName == findName);
	}
	public bool TryFindFaction(int factionID, out Faction find)
	{
		return TryFindElement<Faction>(f => f.ThisElement.ID == factionID, out find);
	}
	public Faction FindFaction(int factionID)
	{
		return FindElement<Faction>(f => f.ThisElement.ID == factionID);
	}
	public string FactionIDToName(int factionID)
	{
		if (factionID >= 0 && TryFindElement<Faction>(f => f.ThisElement.ID == factionID, out var find))
		{
			return find.FactionName;
		}
		return "";
	}
	public int FactionNameToID(string findName)
	{
		if (!string.IsNullOrWhiteSpace(findName) && TryFindElement<Faction>(f => f.FactionName == findName, out var find))
		{
			return find.ThisElement.ID;
		}
		return -1;
	}
	#endregion

	#region Unit
	public bool TryFindUnit(int unitID, out UnitObject find)
	{
		return TryFindElement<UnitObject>(f => f.ThisElement.ID == unitID, out find);
	}
	public UnitObject FindUnit(int unitID)
	{
		return FindElement<UnitObject>(f => f.ThisElement.ID == unitID);
	}
	#endregion

	#region Operation
	public bool TryFindOperation(int operationID, out OperationObject find)
	{
		return TryFindElement<OperationObject>(f => f.ThisElement.ID == operationID, out find);
	}
	public OperationObject FindOperation(int operationID)
	{
		return FindElement<OperationObject>(f => f.ThisElement.ID == operationID);
	}
	public bool TryFindOperation(int factionID, string teamName, out OperationObject find)
	{
		return TryFindElement<OperationObject>(f => f.FactionID == factionID && f.TeamName.Equals(teamName), out find);
	}
	public OperationObject FindOperation(int factionID, string teamName)
	{
		return FindElement<OperationObject>(f => f.FactionID == factionID && f.TeamName.Equals(teamName));
	}
	#endregion

	#region Skill
	public bool TryFindSkill(int skillID, out SkillObject find)
	{
		return TryFindElement<SkillObject>(f => f.ThisElement.ID == skillID, out find);
	}
	public SkillObject FindSkill(int skillIS)
	{
		return FindElement<SkillObject>(f => f.SkillID == skillIS);
	}
	#endregion
}
public partial class StrategyElementCollector2 // ForEach 
{
	public struct ForeachIndex
	{
		public int Index;
		public int Count;

		public ForeachIndex(int index, int Count)
		{
			this.Index = index;
			this.Count = Count;
		}
	}
	private bool ForEachInternal<T>(IList list,
		Func<T, ForeachIndex, bool> funcWithIndex = null,
		Action<T, ForeachIndex> actionWithIndex = null,
		Func<T, bool> func = null,
		Action<T> action = null) where T : class, IStrategyElement
	{
		if (list == null) return false;

		int count = list.Count;
		var index = new ForeachIndex(0, count);

		for (int i = 0 ; i < count ; i++)
		{
			index.Index = i;
			if (list[i] is not T t) continue;

			if (funcWithIndex != null && !funcWithIndex(t, index)) return false;
			else if (actionWithIndex != null) actionWithIndex(t, index);
			else if (func != null && !func(t)) return false;
			else if (action != null) action(t);
		}
		return true;
	}

	#region Foreach
	public void ForEachAll(Action<IStrategyElement> func)
	{
		foreach (var list in GetAllElementIList())
		{
			for (int i = 0 ; i < list.Count ; i++)
			{
				if (list[i] is IStrategyElement e)
					func(e);
			}
		}
	}

	public void ForEach<T>(Action<T> func) where T : class, IStrategyElement
		=> ForEachInternal<T>(GetListByType<T>(), action: func);

	public void ForEach<T>(Func<T, bool> func) where T : class, IStrategyElement
		=> ForEachInternal<T>(GetListByType<T>(), func: func);

	public void ForEach<T>(Action<T, ForeachIndex> func) where T : class, IStrategyElement
		=> ForEachInternal<T>(GetListByType<T>(), actionWithIndex: func);

	public void ForEach<T>(Func<T, ForeachIndex, bool> func) where T : class, IStrategyElement
		=> ForEachInternal<T>(GetListByType<T>(), funcWithIndex: func);
	#endregion

	#region Sector
	public void ForEachSector(Action<SectorObject> func) => ForEach(func);
	public void ForEachSector(Func<SectorObject, bool> func) => ForEach(func);
	public void ForEachSector(Action<SectorObject, ForeachIndex> func) => ForEach(func);
	public void ForEachSector(Func<SectorObject, ForeachIndex, bool> func) => ForEach(func);
	#endregion

	#region Faction
	public void ForEachFaction(Action<Faction> func) => ForEach(func);
	public void ForEachFaction(Func<Faction, bool> func) => ForEach(func);
	public void ForEachFaction(Action<Faction, ForeachIndex> func) => ForEach(func);
	public void ForEachFaction(Func<Faction, ForeachIndex, bool> func) => ForEach(func);
	#endregion

	#region Unit
	public void ForEachUnit(Func<UnitObject, bool> func) => ForEach(func);
	public void ForEachUnit(Action<UnitObject> func) => ForEach(func);
	public void ForEachUnit(Func<UnitObject, ForeachIndex, bool> func) => ForEach(func);
	public void ForEachUnit(Action<UnitObject, ForeachIndex> func) => ForEach(func);
	#endregion

	#region Operation
	public void ForEachOperation(Func<OperationObject, bool> func) => ForEach(func);
	public void ForEachOperation(Action<OperationObject> func) => ForEach(func);
	public void ForEachOperation(Func<OperationObject, ForeachIndex, bool> func) => ForEach(func);
	public void ForEachOperation(Action<OperationObject, ForeachIndex> func) => ForEach(func);
	#endregion

	#region Skill
	public void ForEachSkill(Action<SkillObject> func) => ForEach(func);
	public void ForEachSkill(Func<SkillObject, bool> func) => ForEach(func);
	public void ForEachSkill(Action<SkillObject, ForeachIndex> func) => ForEach(func);
	public void ForEachSkill(Func<SkillObject, ForeachIndex, bool> func) => ForEach(func);
	#endregion
}
*/

/// <summary>
/// StrategyElementCollector에 수집될 다음을 포함해야 한다: IStrategyElement
/// </summary>
public interface IStrategyElement : IStrategyStartGame
{
	public IStrategyElement ThisElement { get; }
	public int ID { get; set; }
	void InStrategyCollector();
	void OutStrategyCollector();
}

/// <summary>
/// 공통(비제네릭) 스토어 인터페이스 — Collector는 이 타입들만 보유/관리한다.
/// </summary>
public interface IElementStore
{
	Type ElementType { get; }
	IList GetRawList();
}

/// <summary>
/// 제네릭 스토어 인터페이스: 구체 타입 접근을 허용한다.
/// </summary>
public interface IElementStore<T> : IElementStore where T : class
{
	BaseList<T> List { get; }
}

/// <summary>
/// 기본 리스트: 이벤트, 순회, Find 등 공통 기능을 제공한다.
/// (IStrategyElement 특성이 없는 타입용으로도 사용)
/// </summary>
[Serializable]
public class BaseList<T> : IEnumerable<T>, IDisposable where T : class
{
	[SerializeField]
	private List<T> list;
	public List<T> Items => list ??= new List<T>();

	private event Action<T, bool> onChange;
	private bool sleepCallback;

	public int Count => Items.Count;
	public T this[int index] => Items[index];

	public BaseList(int capacity = 32)
	{
		list = new List<T>(capacity);
		sleepCallback = false;
		onChange = null;
	}

	public virtual void Dispose()
	{
		list?.Clear();
		list = null;
		onChange = null;
	}

	// Add / Remove
	public virtual bool Add(T item)
	{
		if (item == null) return false;
		if (Items.Contains(item)) return false;
		Items.Add(item);
		Invoke(item, true);
		return true;
	}
	public virtual bool Remove(T item)
	{
		if (item == null) return false;
		if (Items.Remove(item))
		{
			Invoke(item, false);
			return true;
		}
		return false;
	}

	// Bulk helpers
	public bool AddRange(IEnumerable<T> items)
	{
		if (items == null) return false;
		sleepCallback = true;
		var changed = false;
		foreach (var i in items)
		{
			if (Add(i)) changed = true;
		}
		sleepCallback = false;
		return changed;
	}
	public bool RemoveRange(IEnumerable<T> items)
	{
		if (items == null) return false;
		sleepCallback = true;
		var changed = false;
		foreach (var i in items)
		{
			if (Remove(i)) changed = true;
		}
		sleepCallback = false;
		return changed;
	}
	internal void AddRaw(T item)
	{
		// ID 유지
		// InStrategyCollector 호출 없음
		// 콜백 없음
		if (Items.Contains(item)) return;
		Items.Add(item);
	}
	public bool RemoveRaw(T item)
	{
		return Items.Remove(item);
	}
	// Event hooks
	protected void Invoke(T item, bool added)
	{
		if (sleepCallback || onChange == null) return;
		try { onChange.Invoke(item, added); }
		catch (Exception ex) { Debug.LogException(ex); }
	}

	public void OnChange(Action<T, bool> handler)
	{
		if (handler == null) return;
		onChange -= handler;
		onChange += handler;
	}
	public void OffChange(Action<T, bool> handler)
	{
		if (handler == null) return;
		onChange -= handler;
	}

	// Query helpers
	public bool TryFind(Func<T, bool> cond, out T t)
	{
		t = default;
		if (cond == null) return false;
		for (int i = 0 ; i < Items.Count ; i++)
		{
			var it = Items[i];
			if (it == null) continue;
			if (cond(it))
			{
				t = it;
				return true;
			}
		}
		return false;
	}
	public T Find(Func<T, bool> cond)
	{
		if (cond == null) return null;
		for (int i = 0 ; i < Items.Count ; i++)
		{
			var it = Items[i];
			if (it == null) continue;
			if (cond(it)) return it;
		}
		return null;
	}
	public List<T> FindAll(Func<T, bool> cond)
	{
		var result = new List<T>();
		if (cond == null) return result;
		for (int i = 0 ; i < Items.Count ; i++)
		{
			var it = Items[i];
			if (it == null) continue;
			if (cond(it)) result.Add(it);
		}
		return result;
	}
	public void ForEach(Action<T> action)
	{
		if (action == null) return;
		for (int i = 0 ; i < Items.Count ; i++)
		{
			var it = Items[i];
			if (it == null) continue;
			action(it);
		}
	}

	// IEnumerable
	public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();

    public bool TryFind(int index, out T t)
    {
		t = Find(index);
		return t != null;
    }
	public virtual T Find(int index)
	{
		if (index < 0 || index >= Items.Count)
			return null;
		return Items[index];
	}

	public IEnumerable<Action<T, bool>> GetOnChangeHandlers()
	{
		if (onChange == null) yield break;

		foreach (var d in onChange.GetInvocationList())
			yield return (Action<T, bool>)d;
	}

}

/// <summary>
/// IStrategyElement 전용 리스트: ID 관리, In/Out 콜백 연결.
/// </summary>
[Serializable]
public class ElementList<T> : BaseList<T> where T : class, IStrategyElement
{
	private int nextUniqueID;
	private readonly HashSet<int> recycled = new HashSet<int>();
	private int[] lockedIDs;

	public ElementList(int capacity = 32) : base(capacity)
	{
		nextUniqueID = 0;
		recycled.Clear();
	}

	public override void Dispose()
	{
		// call OutStrategyCollector for existing items
		foreach (var it in Items.ToList())
		{
			if (it != null)
			{
				it.OutStrategyCollector();
			}
		}
		base.Dispose();
		nextUniqueID = 0;
		recycled.Clear();
		lockedIDs = null;
	}

	// ID lock helpers
	public void LockIDs(int[] lockIds) => lockedIDs = lockIds;
	public void UnlockIDs() => lockedIDs = null;
	private bool IsLocked(int id)
	{
		if (lockedIDs == null) return false;
		for (int i = 0 ; i < lockedIDs.Length ; i++)
			if (lockedIDs[i] == id) return true;
		return false;
	}

	private int AcquireID()
	{
		// try recycled first
		foreach (var id in recycled)
		{
			if (!IsLocked(id))
			{
				// remove from recycled and return
				recycled.Remove(id);
				return id;
			}
		}
		// advance nextUniqueID skipping locked ones
		while (IsLocked(nextUniqueID)) nextUniqueID++;
		return nextUniqueID++;
	}
	private void ReleaseID(int id)
	{
		// recycle
		if (!recycled.Add(id))
		{
			Debug.LogError($"Attempted to recycle duplicate ID {id}");
		}
	}

	public override bool Add(T item)
	{
		if (item == null) return false;
		if (Items.Contains(item)) return false;

		item.ID = AcquireID();
		Items.Add(item);
		item.InStrategyCollector();
		Invoke(item, true);
		return true;
	}
	public override bool Remove(T item)
	{
		if (item == null) return false;
		if (Items.Remove(item))
		{
			ReleaseID(item.ID);
			item.OutStrategyCollector();
			Invoke(item, false);
			return true;
		}
		return false;
	}

	public override T Find(int id)
	{
		for (int i = 0 ; i < Items.Count ; i++)
		{
			var it = Items[i];
			if (it == null) continue;
			if (it.ID == id) return it;
		}
		return null;
	}
}

/// <summary>
/// 기본 ElementStore 구현: ElementList<T> 또는 BaseList<T>를 래핑한다.
/// </summary>
public class ElementStore<T> : IElementStore<T> where T : class
{
	public Type ElementType => typeof(T);
	public BaseList<T> List { get; }

	public ElementStore(BaseList<T> list = null)
	{
		List = list ?? new BaseList<T>();
	}

	public IList GetRawList() => List.Items;
}

/// <summary>
/// StrategyElementCollector: 외부에 노출되는 API. 
/// 내부 구현은 IElementStore 들에 위임한다. 
/// - 타입을 Register 해서 사용한다. (DIP: 등록은 외부 조립 코드에서)
/// - 기본적으로 Register<T>()는 BaseList<T> 또는 ElementList<T>를 자동 선택한다.
/// </summary>
/// 
/// --------------------------
/// 사용 예시(조립 코드 - Composition Root)
/// --------------------------
/// var collector = new StrategyElementCollector();
/// collector.Register<SectorObject>()
/// 		 .Register<Faction>()
/// 		 .Register<UnitObject>()
/// 		 .Register<OperationObject>()
/// 		 .Register<SkillObject>();
/// 
/// // 커스텀 타입 등록
/// collector.Register<MyArbitraryData>();
/// 
/// // 요소 추가
/// collector.Add(unitInstance);
/// 
/// // 변경 리스너
/// collector.AddChangeListener<UnitObject>((u, added) => {
/// 	Debug.Log($"Unit {u?.ID} {(added ? "added" : "removed")}");
/// }, invokeForExisting: true);
/// --------------------------- 

[Serializable]
public class StrategyElementCollector : IDisposable
{
	[ShowInInspector]
	private readonly Dictionary<Type, IElementStore> stores = new Dictionary<Type, IElementStore>();

	// 글로벌 이벤트: 모든 타입 추가/삭제 발생시 호출
	private event Action<object, bool> onAnyElementChanged;

	public StrategyElementCollector Register<T>(int capacity = 32) where T : class => Register(typeof(T), capacity);
	private StrategyElementCollector Register(Type type, int capacity = 32)
	{
		// 이미 있으면 스킵
		if (IsRegistered(type)) return this;
		IElementStore store;
		if (typeof(IStrategyElement).IsAssignableFrom(type))
		{
			var ctorList = Activator.CreateInstance(
				typeof(ElementList<>).MakeGenericType(type), capacity);
			var storeType = typeof(ElementStore<>).MakeGenericType(type);
			store = Activator.CreateInstance(storeType, ctorList) as IElementStore;
		}
		else
		{
			var baseList = Activator.CreateInstance(
				typeof(BaseList<>).MakeGenericType(type), capacity);
			var storeType = typeof(ElementStore<>).MakeGenericType(type);
			store = Activator.CreateInstance(storeType, baseList) as IElementStore;
		}

		stores[type] = store;
		return this;
	}
	/// <summary>명시적 스토어 등록(외부에서 커스텀 리스트/스토어 주입 가능)</summary>
	public StrategyElementCollector Register<T>(IElementStore<T> newStore) where T : class
	{
		if (newStore == null) throw new ArgumentNullException(nameof(newStore));

		var type = typeof(T);

		// IStrategyElement 타입이면 명시적 등록 금지
		if (typeof(IStrategyElement).IsAssignableFrom(type))
		{
			throw new InvalidOperationException(
				$"Explicit registration of IStrategyElement types is not allowed: {type.Name}");
		}


		if (stores.TryGetValue(type, out var oldStoreObj))
		{
			if (oldStoreObj is IElementStore<T> oldStore && oldStore != newStore)
			{
				var oldList = oldStore.List;
				BaseList<T> newList = newStore.List;

				// 기존 요소의 ID 및 상태 그대로 유지하면서 삽입
				foreach (var item in oldList.Items)
				{
					// ID 유지 / InStrategyCollector 호출 없음 / 콜백 없음
					newList.AddRaw(item);
				}

				foreach (var handler in oldList.GetOnChangeHandlers())
				{
					newList.OnChange(handler);
				}

				// 기존 리스트 정리
				oldList.Dispose();

				// 스토어 교환
				stores[type] = newStore;
			}
		}

		return this;
	}

	/// <summary>등록 여부 확인</summary>
	public bool IsRegistered<T>() => IsRegistered(typeof(T));
	private bool IsRegistered(Type type) => stores.ContainsKey(type);

	/// <summary>타입의 BaseList<T> 얻기(등록되어 있어야 함)</summary>
	public BaseList<T> GetList<T>() where T : class
	{
		if (stores.TryGetValue(typeof(T), out var s) && s is IElementStore<T> es)
			return es.List;
		return null;
	}

	/// <summary>원시 IList 접근 — 모든 등록된 리스트를 동일하게 접근 가능</summary>
	public IList GetRawList(Type type)
	{
		if (stores.TryGetValue(type, out var s)) return s.GetRawList();
		return null;
	}

	/// <summary>모든 IList 열거</summary>
	public IEnumerable<IList> GetAllRawLists()
	{
		foreach (var kv in stores)
			yield return kv.Value.GetRawList();
	}

	/// <summary>모든 ElementList 열거 (IStrategyElement 전용)</summary>
	public IEnumerable<IList> GetAllElementLists()
	{
		foreach (var store in stores.Values)
		{
			// IElementStore<T>인지 확인
			if (store is IElementStore es)
			{
				var list = es.GetRawList();

				// list가 ElementList<T>인지 확인
				if (list is ElementList<IStrategyElement> || list.GetType().IsSubclassOf(typeof(ElementList<>)))
				{
					yield return list;
				}
			}
		}

	}

	/// <summary>추가/제거용 API</summary>
	public bool Add<T>(T item) where T : class
	{
		if (item == null) return false;

		Type type = typeof(T);

		// 내부적으로 등록되어 있지 않으면 자동 등록
		Register(type);

		var list = (stores[typeof(T)] as IElementStore<T>)?.List;
		bool result = list.Add(item);
		if (result)
		{
			onAnyElementChanged?.Invoke(item, true);
		}
		return result;
	}
	public bool Remove<T>(T item) where T : class
	{
		var list = GetList<T>();
		if (list == null) throw new InvalidOperationException($"Type {typeof(T).Name} is not registered.");
		bool result = list.Remove(item);
		if (result)
		{
			onAnyElementChanged?.Invoke(item, false);
		}
		return result;
	}
	public bool AddRange<T>(IEnumerable<T> items) where T : class
	{
		var list = GetList<T>();
		if (list == null) throw new InvalidOperationException($"Type {typeof(T).Name} is not registered.");
		return list.AddRange(items);
	}
	public bool RemoveRange<T>(IEnumerable<T> items) where T : class
	{
		var list = GetList<T>();
		if (list == null) throw new InvalidOperationException($"Type {typeof(T).Name} is not registered.");
		return list.RemoveRange(items);
	}

	/// <summary>
	/// 이벤트 연결: IStrategyElement 타입이면 IStrategyElement 형식의 핸들러를, 일반 타입이면 T 형식 핸들러를 사용
	/// invokeForExisting 가 true 이 경우 기존 아이템에 대해 onChange 콜백을 즉시 호출한다.
	/// </summary>
	public void AddChangeListener<T>(Action<T, bool> onChange, bool invokeForExisting = false) where T : class
	{
		var list = GetList<T>();
		if (list == null) throw new InvalidOperationException($"Type {typeof(T).Name} is not registered.");
		list.OnChange(onChange);
		if (invokeForExisting)
		{
			foreach (var item in list.Items.ToList())
			{
				onChange?.Invoke(item, true);
			}
		}
	}
	public void RemoveChangeListener<T>(Action<T, bool> onChange) where T : class
	{
		var list = GetList<T>();
		if (list == null) return;
		list.OffChange(onChange);
	}

	public void AddAnyChangeListener(Action<object, bool> onChange, bool invokeForExisting = false)
	{
		onAnyElementChanged -= onChange;
		onAnyElementChanged += onChange;

		if (invokeForExisting)
		{
			foreach (var kv in stores)
			{
				var list = kv.Value.GetRawList();
				if (list == null) continue;

				foreach (var item in list)
				{
					onChange?.Invoke(item, true);
				}
			}
		}
	}
	public void RemoveAnyChangeListener(Action<object, bool> onChange)
	{
		onAnyElementChanged -= onChange;
	}

	/// <summary>유틸: Find / FindAll</summary>
	public bool TryFind<T>(int id, out T t) where T : class, IStrategyElement
	{
		var list = GetList<T>();
		if (list == null) { t = default; return false; }
		return list.TryFind(id, out t);
	}
	public T Find<T>(int id) where T : class, IStrategyElement
	{
		var list = GetList<T>();
		return list?.Find(id);
	}
	public bool TryFind<T>(Func<T, bool> cond, out T t) where T : class
	{
		var list = GetList<T>();
		if (list == null) { t = default; return false; }
		return list.TryFind(cond, out t);
	}
	public T Find<T>(Func<T, bool> cond) where T : class
	{
		var list = GetList<T>();
		return list?.Find(cond);
	}
	public List<T> FindAll<T>(Func<T, bool> cond) where T : class
	{
		var list = GetList<T>();
		return list?.FindAll(cond) ?? new List<T>();
	}
	public void Dispose()
	{
		foreach (var kv in stores)
		{
			// kv.Value : IElementStore
			var store = kv.Value;

			// store는 ElementStore<T>, store.List가 IDisposable(BaseList<T>)이다.
			var storeType = store.GetType();
			var listProp = storeType.GetProperty("List");
			if (listProp == null) continue;

			var listObj = listProp.GetValue(store);
			if (listObj is IDisposable d)
			{
				d.Dispose();
			}
		}

		stores.Clear();
	}
}


