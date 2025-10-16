using System;
using System.Collections.Generic;

using UnityEngine;

public interface IStrategyElement
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

	void OnStartGame() { }
}

public partial class StrategyElementCollector : MonoBehaviour, IDisposable
{
	[SerializeField]
	private List<ControlBase> controlBases;
	[SerializeField]
	private List<Faction> factions;
	[SerializeField]
	private List<UnitObject> units;
	[SerializeField]
	private List<SkillObject> skills;
	[SerializeField]
	private List<IStrategyElement> others;

	public List<ControlBase> ControlBaseList { get => controlBases; private set => controlBases = value; }
	public List<Faction> FactionList { get => factions; private set => factions = value; }
	public List<UnitObject> UnitList { get => units; private set => units = value; }
	public List<SkillObject> SkillList { get => skills; private set => skills = value; }
	public List<IStrategyElement> OtherList { get => others; private set => others = value; }

	internal void Init()
	{
		InitControlBase();
		InitFaction();
		InitUnit();
		InitSkill();
		InitOther();
	}
	public void InitControlBase()
	{
		if (ControlBaseList == null)
		{
			ControlBaseList = new List<ControlBase>(32);
			return;
		}

		int length = ControlBaseList.Count;
		for (int i = 0 ; i < length ; i++)
		{
			var element = ControlBaseList[i];
			if (element != null && element is IStrategyElement iElement)
				iElement._OutStrategyCollector();
		}
		ControlBaseList.Clear();
	}
	public void InitFaction()
	{
		if (FactionList == null)
		{
			FactionList = new List<Faction>(8);
			return;
		}

		int length = FactionList.Count;
		for (int i = 0 ; i < length ; i++)
		{
			var element = FactionList[i];
			if (element != null && element is IStrategyElement iElement)
				iElement._OutStrategyCollector();
		}
		FactionList.Clear();
	}
	public void InitUnit()
	{
		if (UnitList == null)
		{
			UnitList = new List<UnitObject>(256);
			return;
		}

		int length = UnitList.Count;
		for (int i = 0 ; i < length ; i++)
		{
			var element = UnitList[i];
			if (element != null && element is IStrategyElement iElement)
				iElement._OutStrategyCollector();
		}
		UnitList.Clear();
	}
	public void InitSkill()
	{
		if (SkillList == null)
		{
			SkillList = new List<SkillObject>(1024);
			return;
		}

		int length = SkillList.Count;
		for (int i = 0 ; i < length ; i++)
		{
			var element = SkillList[i];
			if (element != null && element is IStrategyElement iElement)
				iElement._OutStrategyCollector();
		}
		SkillList.Clear();
	}
	public void InitOther()
	{
		if (OtherList == null)
		{
			OtherList = new List<IStrategyElement>();
			return;
		}

		int length = OtherList.Count;
		for (int i = 0 ; i < length ; i++)
		{
			var element = OtherList[i];
			if (element != null && element is IStrategyElement iElement)
				iElement._OutStrategyCollector();
		}
		OtherList.Clear();
	}

	public void AddElement<T>(IEnumerable<T> elements) where T : class, IStrategyElement
	{
		foreach (var element in elements)
		{
			AddElement(element);
		}
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
		switch (element)
		{
			case ControlBase item:
			{
				if (!ControlBaseList.Contains(item)) ControlBaseList.Add(item);
				break;
			}
			case Faction item:
			{
				if (!FactionList.Contains(item))
				{
					FactionList.Add(item);
				}
				break;
			}
			case UnitObject item:
			{
				if (!UnitList.Contains(item)) UnitList.Add(item);
				break;
			}
			case SkillObject item:
			{
				if (!SkillList.Contains(item)) SkillList.Add(item);
				break;
			}
			default:
			{
				if (!OtherList.Contains(element)) OtherList.Add(element);
				break;
			}
		}
		element._InStrategyCollector();
	}
	public void RemoveElement<T>(T element) where T : class, IStrategyElement
	{
		switch (element)
		{
			case ControlBase item:
			{
				ControlBaseList.Remove(item);
				break;
			}
			case Faction item:
			{
				FactionList.Remove(item);
				break;
			}
			case UnitObject item:
			{
				UnitList.Remove(item);
				break;
			}
			case SkillObject item:
			{
				SkillList.Remove(item);
				break;
			}
			default:
			{
				OtherList.Remove(element);
				break;
			}
		}
		element._OutStrategyCollector();
	}

    public void Dispose()
    {
		controlBases.Clear();
		factions.Clear();
		units.Clear();
		skills.Clear();
		others.Clear();
	}
}

public partial class StrategyElementCollector // Finder
{
	#region FindElement
	public bool TryFindElement<T>(Func<T, bool> condition, out T find) where T : class, IStrategyElement
	{
		find = null;
		if (condition == null)
		{
			return false;
		}
		var findType = typeof(T);

		if (findType.Equals(typeof(ControlBase)))
		{
			find = ControlBaseList.Find(i => condition(i as T)) as T;
		}
		else if (findType.Equals(typeof(Faction)))
		{
			find = FactionList.Find(i => condition(i as T)) as T;
		}
		else if (findType.Equals(typeof(UnitObject)))
		{
			find = UnitList.Find(i => condition(i as T)) as T;
		}
		else if (findType.Equals(typeof(SkillObject)))
		{
			find = SkillList.Find(i => condition(i as T)) as T;
		}
		else
		{
			find = OtherList.Find(i => condition(i as T)) as T;
		}

		return find != null;
	}
	public T FindElement<T>(Func<T, bool> condition) where T : class, IStrategyElement
	{
		if (TryFindElement<T>(condition, out T find))
		{
			return find;
		}
		return null;
	}
	#endregion

	#region ControlBase
	public bool TryFindControlBase(string factionName, out ControlBase find)
	{
		return TryFindElement<ControlBase>(f => f.ControlBaseName == factionName, out find);
	}
	public ControlBase FindControlBase(string factionName)
	{
		return FindElement<ControlBase>(f => f.ControlBaseName == factionName);
	}
	#endregion

	#region Faction
	public bool TryFindFaction(string factionName, out Faction find)
	{
		return TryFindElement<Faction>(f => f.FactionName == factionName, out find);
	}
	public Faction FindFaction(string factionName)
	{
		return FindElement<Faction>(f => f.FactionName == factionName);
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
	public int FactionNameToID(string factionName)
	{
		if (!string.IsNullOrWhiteSpace(factionName) && TryFindElement<Faction>(f => f.FactionName == factionName, out var find))
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

public partial class StrategyElementCollector // Foreach
{
	#region Foreach
	public void ForeachAll(Action<IStrategyElement> func)
	{
		ForControlBase(func);
		ForFaction(func);
		ForUnit(func);
		ForSkill(func);
		ForOther(func);
	}

	public void Foreach<T>(Func<T, bool> func) where T : class, IStrategyElement
	{
		if (func == null)
		{
			return;
		}
		var findType = typeof(T);

		int length = 0;
		if (findType.Equals(typeof(ControlBase)))
		{
			length = ControlBaseList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				if (!func(ControlBaseList[i] as T))
				{
					return;
				}
			}
		}
		else if (findType.Equals(typeof(Faction)))
		{
			length = FactionList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				if (!func(FactionList[i] as T))
				{
					return;
				}
			}
		}
		else if (findType.Equals(typeof(UnitObject)))
		{
			length = UnitList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				if (!func(UnitList[i] as T))
				{
					return;
				}
			}
		}
		else if (findType.Equals(typeof(SkillObject)))
		{
			length = SkillList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				if (!func(SkillList[i] as T))
				{
					return;
				}
			}
		}
		else
		{
			length = ControlBaseList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				if (!func(OtherList[i] as T))
				{
					return;
				}
			}
		}
	}
	public void Foreach<T>(Action<T> func) where T : class, IStrategyElement
	{
		if (func == null)
		{
			return;
		}
		var findType = typeof(T);

		int length = 0;
		if (findType.Equals(typeof(ControlBase)))
		{
			length = ControlBaseList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				func(ControlBaseList[i] as T);
			}
		}
		else if (findType.Equals(typeof(Faction)))
		{
			length = FactionList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				func(FactionList[i] as T);
			}
		}
		else if (findType.Equals(typeof(UnitObject)))
		{
			length = UnitList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				func(UnitList[i] as T);
			}
		}
		else if (findType.Equals(typeof(SkillObject)))
		{
			length = SkillList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				func(SkillList[i] as T);
			}
		}
		else
		{
			length = OtherList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				func(OtherList[i] as T);
			}
		}
	}
	public void Foreach<T>(Func<T, int, bool> func) where T : class, IStrategyElement
	{
		if (func == null)
		{
			return;
		}
		var findType = typeof(T);

		int length = 0;
		if (findType.Equals(typeof(ControlBase)))
		{
			length = ControlBaseList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				if (!func(ControlBaseList[i] as T, i))
				{
					return;
				}
			}
		}
		else if (findType.Equals(typeof(Faction)))
		{
			length = FactionList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				if (!func(FactionList[i] as T, i))
				{
					return;
				}
			}
		}
		else if (findType.Equals(typeof(UnitObject)))
		{
			length = UnitList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				if (!func(UnitList[i] as T, i))
				{
					return;
				}
			}
		}
		else if (findType.Equals(typeof(SkillObject)))
		{
			length = SkillList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				if (!func(SkillList[i] as T, i))
				{
					return;
				}
			}
		}
		else
		{
			length = OtherList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				if (!func(OtherList[i] as T, i))
				{
					return;
				}
			}
		}
	}
	public void Foreach<T>(Action<T, int> func) where T : class, IStrategyElement
	{
		if (func == null)
		{
			return;
		}
		var findType = typeof(T);

		int length = 0;
		if (findType.Equals(typeof(ControlBase)))
		{
			length = ControlBaseList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				func(ControlBaseList[i] as T, i);
			}
		}
		else if (findType.Equals(typeof(Faction)))
		{
			length = FactionList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				func(FactionList[i] as T, i);
			}
		}
		else if (findType.Equals(typeof(UnitObject)))
		{
			length = UnitList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				func(UnitList[i] as T, i);
			}
		}
		else if (findType.Equals(typeof(SkillObject)))
		{
			length = SkillList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				func(SkillList[i] as T, i);
			}
		}
		else
		{
			length = OtherList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				func(OtherList[i] as T, i);
			}
		}
	}
	public void Foreach<T>(Func<T, int, int, bool> func) where T : class, IStrategyElement
	{
		if (func == null)
		{
			return;
		}
		var findType = typeof(T);

		int length = 0;
		if (findType.Equals(typeof(ControlBase)))
		{
			length = ControlBaseList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				if (!func(ControlBaseList[i] as T, i, length))
				{
					return;
				}
			}
		}
		else if (findType.Equals(typeof(Faction)))
		{
			length = FactionList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				if (!func(FactionList[i] as T, i, length))
				{
					return;
				}
			}
		}
		else if (findType.Equals(typeof(UnitObject)))
		{
			length = UnitList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				if (!func(UnitList[i] as T, i, length))
				{
					return;
				}
			}
		}
		else if (findType.Equals(typeof(SkillObject)))
		{
			length = SkillList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				if (!func(SkillList[i] as T, i, length))
				{
					return;
				}
			}
		}
		else
		{
			length = OtherList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				if (!func(OtherList[i] as T, i, length))
				{
					return;
				}
			}
		}
	}
	public void Foreach<T>(Action<T, int, int> func) where T : class, IStrategyElement
	{
		if (func == null)
		{
			return;
		}
		var findType = typeof(T);

		int length = 0;
		if (findType.Equals(typeof(ControlBase)))
		{
			length = ControlBaseList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				func(ControlBaseList[i] as T, i, length);
			}
		}
		else if (findType.Equals(typeof(Faction)))
		{
			length = FactionList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				func(FactionList[i] as T, i, length);
			}
		}
		else if (findType.Equals(typeof(UnitObject)))
		{
			length = UnitList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				func(UnitList[i] as T, i, length);
			}
		}
		else if (findType.Equals(typeof(SkillObject)))
		{
			length = SkillList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				func(SkillList[i] as T, i, length);
			}
		}
		else
		{
			length = OtherList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				func(OtherList[i] as T, i, length);
			}
		}
	}
	#endregion

	#region ControlBase
	public void ForControlBase(Func<ControlBase, bool> func) => Foreach(func);
	public void ForControlBase(Action<ControlBase> func) => Foreach(func);
	public void ForControlBase(Func<ControlBase, int, bool> func) => Foreach(func);
	public void ForControlBase(Action<ControlBase, int> func) => Foreach(func);
	public void ForControlBase(Func<ControlBase, int, int, bool> func) => Foreach(func);
	public void ForControlBase(Action<ControlBase, int, int> func) => Foreach(func);
	#endregion

	#region Faction
	public void ForFaction(Func<Faction, bool> func) => Foreach(func);
	public void ForFaction(Action<Faction> func) => Foreach(func);
	public void ForFaction(Func<Faction, int, bool> func) => Foreach(func);
	public void ForFaction(Action<Faction, int> func) => Foreach(func);
	public void ForFaction(Func<Faction, int, int, bool> func) => Foreach(func);
	public void ForFaction(Action<Faction, int, int> func) => Foreach(func);
	#endregion

	#region Unit
	public void ForUnit(Func<UnitObject, bool> func) => Foreach(func);
	public void ForUnit(Action<UnitObject> func) => Foreach(func);
	public void ForUnit(Func<UnitObject, int, bool> func) => Foreach(func);
	public void ForUnit(Action<UnitObject, int> func) => Foreach(func);
	public void ForUnit(Func<UnitObject, int, int, bool> func) => Foreach(func);
	public void ForUnit(Action<UnitObject, int, int> func) => Foreach(func);
	#endregion

	#region Skill
	public void ForSkill(Func<SkillObject, bool> func) => Foreach(func);
	public void ForSkill(Action<SkillObject> func) => Foreach(func);
	public void ForSkill(Func<SkillObject, int, bool> func) => Foreach(func);
	public void ForSkill(Action<SkillObject, int> func) => Foreach(func);
	public void ForSkill(Func<SkillObject, int, int, bool> func) => Foreach(func);
	public void ForSkill(Action<SkillObject, int, int> func) => Foreach(func);
	#endregion

	#region Other
	public void ForOther(Func<IStrategyElement, bool> func) => Foreach(func);
	public void ForOther(Action<IStrategyElement> func) => Foreach(func);
	#endregion
}
