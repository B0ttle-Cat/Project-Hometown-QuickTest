using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public interface IStrategyElement : IStartGame
{
	public bool IsInCollector { get; set; }
	void _InStrategyCollector()
	{
		if (IsInCollector) return;
		IsInCollector = true;
		InStrategyCollector();
	}
	void _OutStrategyCollector()
	{
		if (!IsInCollector) return;
		IsInCollector = false;
		OutStrategyCollector();
	}
	void InStrategyCollector();
	void OutStrategyCollector();
}
public interface IStartGame
{
	public static int Default = 0;
	int StartEventOrder() => Default;
	int StopEventOrder() => Default;
	void OnStartGame();
	void OnStopGame();
}
public interface ISelectMouse
{
	Vector3 ClickCenter { get; }
	bool IsPointEnter { get; set; }
	bool IsSelectMouse { get; set; }
	void OnPointEnter() { }
	void OnPointExit() { }
	bool OnSelect();
	bool OnDeselect();
	void OnSingleSelect();
	void OnSingleDeselect();
	void OnFirstSelect();
	void OnLastDeselect();
}
public partial class StrategyElementCollector : MonoBehaviour, IDisposable
{
	public abstract class ElementList
	{
		public abstract IList IList { get; }

		public abstract void OnAddListener(Action<IList> action);
		public abstract void OnRemoveListener(Action<IList> action);
		public abstract void OnAddListener(Action<IStrategyElement, bool> action);
		public abstract void OnRemoveListener(Action<IStrategyElement, bool> action);
	}

	[Serializable]
	public class ElementList<T> : ElementList, IEnumerable<T>, IDisposable where T : class, IStrategyElement
	{
		[SerializeField]
		private List<T> list;
		public List<T> List => list ??= new List<T>();
		public override IList IList => List;

		private Action<List<T>> onChangeList;
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

		public void Init(int capacity = 32)
		{
			onChangeList = null;
			onChange = null;

			sleepCallback = false;

			if (list == null)
			{
				list = new List<T>(capacity);
				return;
			}

			int length = list.Count;
			for (int i = 0 ; i < length ; i++)
			{
				var element = list[i];
				if (element != null && element is IStrategyElement iElement)
					iElement._OutStrategyCollector();
			}
			list.Clear();
		}
		public void Dispose()
		{
			if (list != null)
			{
				list.Clear();
				list = null;
			}
			onChangeList = null;
			onChange = null;
			sleepCallback = false;
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
				Invoke();
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
				Invoke();
			}
			return isChange;
		}
		public bool AddElement(T element)
		{
			if (element == null) return false;

			if (!list.Contains(element))
			{
				list.Add(element);
				element._InStrategyCollector();
				Invoke();
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
				element._OutStrategyCollector();
				Invoke();
				Invoke(element, false);
				return true;
			}
			return false;
		}

		public void Invoke()
		{
			if (sleepCallback || onChangeList == null) return;
			try
			{
				onChangeList.Invoke(List);
			}
			catch (Exception ex) { Debug.LogException(ex); }
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
		public override void OnAddListener(Action<IList> action)
		{
			if (action == null) return;
			onChangeList -= action;
			onChangeList += action;
		}
		public override void OnRemoveListener(Action<IList> action)
		{
			if (action == null) return;
			onChangeList -= action;
		}
		public override void OnAddListener(Action<IStrategyElement, bool> action)
		{
			if (action == null) return;
			onChange -= action;
			onChange += action;
		}
		public override void OnRemoveListener(Action<IStrategyElement, bool> action)
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

	[SerializeField]
	private ElementList<SectorObject> sectors;
	[SerializeField]
	private ElementList<Faction> factions;
	[SerializeField]
	private ElementList<UnitObject> units;
	[SerializeField]
	private ElementList<SkillObject> skills;
	[SerializeField]
	private ElementList<IStrategyElement> others;
	private Dictionary<Type, ElementList> _elementLists;
	public List<SectorObject> SectorList => sectors?.List ?? new List<SectorObject>();
	public List<Faction> FactionList => factions?.List ?? new List<Faction>();
	public List<UnitObject> UnitList => units?.List ?? new List<UnitObject>();
	public List<SkillObject> SkillList => skills?.List ?? new List<SkillObject>();
	public List<IStrategyElement> OtherList => others?.List ?? new List<IStrategyElement>();
	private Dictionary<Type, IList> _listCache;

	//private Dictionary<Delegate, Action<IList>> _listenerMap = new();



	public IEnumerable<IList> GetAllLists()
	{
		yield return SectorList;
		yield return FactionList;
		yield return UnitList;
		yield return SkillList;
		yield return OtherList;
	}
	internal void Init()
	{
		//	_listenerMap = new Dictionary<Delegate, Action<IList>>();
		InitSector();
		InitFaction();
		InitUnit();
		InitSkill();
		InitOther();
	}
	private void InitListTypeCache()
	{
		_listCache ??= new Dictionary<Type, IList>
		{
			[typeof(SectorObject)] = SectorList,
			[typeof(Faction)] = FactionList,
			[typeof(UnitObject)] = UnitList,
			[typeof(SkillObject)] = SkillList,
			[typeof(IStrategyElement)] = OtherList
		};
	}
	private void InitElementListCache()
	{
		_elementLists = new Dictionary<Type, ElementList>
		{
			[typeof(SectorObject)] = sectors,
			[typeof(Faction)] = factions,
			[typeof(UnitObject)] = units,
			[typeof(SkillObject)] = skills,
			[typeof(IStrategyElement)] = others,
		};
	}

	public void InitSector() => (sectors ??= new ElementList<SectorObject>()).Init(32);
	public void InitFaction() => (factions ??= new ElementList<Faction>()).Init(8);
	public void InitUnit() => (units ??= new ElementList<UnitObject>()).Init(512);
	public void InitSkill() => (skills ??= new ElementList<SkillObject>()).Init(512);
	public void InitOther() => (others ??= new ElementList<IStrategyElement>()).Init(64);
	public void Dispose()
	{
		sectors?.Dispose();
		factions?.Dispose();
		units?.Dispose();
		skills?.Dispose();
		others?.Dispose();
	}

	private IList GetListByType<T>()
	{
		InitListTypeCache();
		return _listCache.TryGetValue(typeof(T), out var list) ? list : OtherList;
	}
	private ElementList GetElementByType<T>()
	{
		InitElementListCache();
		return _elementLists.TryGetValue(typeof(T), out var element) ? element : others;
	}
	public void AddElement<TList, TItem>(TList elements) where TList : IEnumerable<TItem> where TItem : class, IStrategyElement
	{
		_ = elements switch
		{
			IEnumerable<SectorObject> item => sectors.AddElement(item),
			IEnumerable<Faction> item => factions.AddElement(item),
			IEnumerable<UnitObject> item => units.AddElement(item),
			IEnumerable<SkillObject> item => skills.AddElement(item),
			_ => others.AddElement(elements),
		};
	}
	public void RemoveElement<T>(IEnumerable<T> elements) where T : class, IStrategyElement
	{
		foreach (var element in elements)
		{
			RemoveElement(element);
		}
	}
	public void AddElement<T>(T element) where T : class, IStrategyElement
	{
		_ = element switch
		{
			SectorObject item => sectors.AddElement(item),
			Faction item => factions.AddElement(item),
			UnitObject item => units.AddElement(item),
			SkillObject item => skills.AddElement(item),
			_ => others.AddElement(element),
		};
	}
	public void RemoveElement<T>(T element) where T : class, IStrategyElement
	{
		_ = element switch
		{
			SectorObject item => sectors.RemoveElement(item),
			Faction item => factions.RemoveElement(item),
			UnitObject item => units.RemoveElement(item),
			SkillObject item => skills.RemoveElement(item),
			_ => others.RemoveElement(element),
		};
	}

	public void AddChangeListListener<T>(Action<IList> action, bool callAtAfter = false) where T : class, IStrategyElement
	{
		if (action == null) return;

		var element = GetElementByType<T>();
		element.OnAddListener(action);
		if (callAtAfter)
		{
			action.Invoke(element.IList);
		}
	}
	public void RemoveChangeListListener<T>(Action<IList> action) where T : class, IStrategyElement
	{
		if (action == null) return;

		var element = GetElementByType<T>();
		element.OnRemoveListener(action);
	}
	public void AddChangeListener<T>(Action<IStrategyElement, bool> action, out IList getCurrentList) where T : class, IStrategyElement
	{
		var element = GetElementByType<T>();

		element.OnAddListener(action);

		getCurrentList = element.IList is List<T> ? element.IList : null;
	}
	public void RemoveChangeListener<T>(Action<IStrategyElement, bool> action) where T : class, IStrategyElement
	{
		if (action == null) return;

		ElementList element = GetElementByType<T>();
		element.OnRemoveListener(action);
	}
}
public partial class StrategyElementCollector // Finder 
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
		return TryFindElement<Faction>(f => f.FactionID == factionID, out find);
	}
	public Faction FindFaction(int factionID)
	{
		return FindElement<Faction>(f => f.FactionID == factionID);
	}
	public string FactionIDToName(int factionID)
	{
		if (factionID >= 0 && TryFindElement<Faction>(f => f.FactionID == factionID, out var find))
		{
			return find.FactionName;
		}
		return "";
	}
	public int FactionNameToID(string findName)
	{
		if (!string.IsNullOrWhiteSpace(findName) && TryFindElement<Faction>(f => f.FactionName == findName, out var find))
		{
			return find.FactionID;
		}
		return -1;
	}
	#endregion

	#region Unit
	public bool TryFindUnit(int unitID, out UnitObject find)
	{
		return TryFindElement<UnitObject>(f => f.UnitID == unitID, out find);
	}
	public UnitObject FindUnit(int unitID)
	{
		return FindElement<UnitObject>(f => f.UnitID == unitID);
	}
	#endregion

	#region Skill
	public bool TryFindSkill(int skillID, out SkillObject find)
	{
		return TryFindElement<SkillObject>(f => f.SkillID == skillID, out find);
	}
	public SkillObject FindSkill(int skillIS)
	{
		return FindElement<SkillObject>(f => f.SkillID == skillIS);
	}
	#endregion
}
public partial class StrategyElementCollector // ForEach 
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
		foreach (var list in GetAllLists())
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

	#region Skill
	public void ForEachSkill(Action<SkillObject> func) => ForEach(func);
	public void ForEachSkill(Func<SkillObject, bool> func) => ForEach(func);
	public void ForEachSkill(Action<SkillObject, ForeachIndex> func) => ForEach(func);
	public void ForEachSkill(Func<SkillObject, ForeachIndex, bool> func) => ForEach(func);
	#endregion

	#region Other
	public void ForEachOther(Action<IStrategyElement> func) => ForEach(func);
	public void ForEachOther(Func<IStrategyElement, bool> func) => ForEach(func);
	#endregion
}
