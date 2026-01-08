
using System;
using System.Collections;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;
using UnityEngine.UI;

namespace StrategyManagerModule
{
	[DefaultExecutionOrder(-1)]
	public partial class StrategySelecter : MonoBehaviour
	{
		[Flags]
		public enum SelectType
		{
			None = 0,
			UnitType = 1<<0,
			SectorType = 1<<1,
			Default = UnitType | SectorType,
		}

		public SelectType SelectFlag = SelectType.Default;


		public List<SelectComputer> selectComputers;
		public List<SelectCollector> selectCollectors;

		internal SelectHashSet<ISelectable> selectItemList;

		public event Action<ISelectable, bool> OnSelectChange;
		public event Action<ISelectable> OnPointing;


		[Button("Reset ThisSelecter"), PropertyOrder(-999)]
		public void Awake()
		{
			selectComputers = new List<SelectComputer>(GetComponentsInChildren<SelectComputer>());
			selectCollectors = new List<SelectCollector>(GetComponentsInChildren<SelectCollector>());
		}
		public void OnEnable()
		{
			SelectFlag = SelectType.Default;
			selectItemList = new SelectHashSet<ISelectable>();

			foreach (var item in selectComputers)
			{
				item.Init(this);
			}
			foreach (var item in selectCollectors)
			{
				item.OnInit(this);
			}
		}
		public void OnDisable()
		{
			SelectFlag = SelectType.None;
			foreach (var item in selectComputers)
			{
				item.Deinit();
			}
			foreach (var item in selectCollectors)
			{
				item.OnDeinit();
			}

			if (selectItemList != null)
			{
				foreach (var item in selectItemList)
				{
					if (item == null) continue;
					item.OnDeselect();
				}
				selectItemList.Clear();
				selectItemList = null;
			}
		}
		public void Update()
		{
			foreach (var item in selectComputers)
			{
				if (item.IsVaild())
				{
					item.InputUpdate();
					item.Compute();
				}
			}

			foreach (var item in selectCollectors)
			{

			}
		}

		public void OnSystemSelectObject(ISelectable target, bool clearOldSelect = true)
		{
			if (target == null) return;
			if (clearOldSelect) ClearInSelectItemList();
			AddInSelectItemList(target);
		}
		public void OnSystemDeselectObject(ISelectable target)
		{
			if (target == null) return;
			RemoveInSelectItemList(target);
		}
		public void OnSystemClearSelectList()
		{
			ClearInSelectItemList();
		}
		public void OnSystemPointingTarget(ISelectable target)
		{
			OnPointingTarget(target);
		}
		internal void AddInSelectItemList(ISelectable selectable)
		{
			if (SelectFlag == SelectType.None)
			{
				return;
			}

			if (!SelectFlag.HasFlag(SelectType.SectorType))
			{
				if (selectable.Type == ISelectable.SelectableType.Sector)
				{
					return;
				}
			}
			if (!SelectFlag.HasFlag(SelectType.UnitType))
			{
				if (selectable.Type == ISelectable.SelectableType.Unit)
				{
					return;
				}
			}


			HashSet<ISelectable> passingList = new (){ selectable };
			while (selectable.HasPassthrough(out var passthrough))
			{
				if (passthrough == null || !passingList.Add(passthrough)) break;
				selectable = passthrough;
			}

			if (!selectable.CanSelect())
			{
				return;
			}
			if (!selectItemList.Add(selectable))
			{
				return;
			}

			selectable.OnSelect();
			OnSelectChange?.Invoke(selectable, true);
			for (int i = 0 ; i < selectCollectors.Count ; i++)
			{
				SelectCollector coll = selectCollectors[i];
				coll.OnSelected(selectable);
			}
		}
		internal void RemoveInSelectItemList(ISelectable selectable)
		{
			HashSet<ISelectable> passingList = new (){ selectable };
			while (selectable.HasPassthrough(out var passthrough))
			{
				if (passthrough == null || !passingList.Add(passthrough)) break;
				selectable = passthrough;
			}

			if (!selectItemList.Remove(selectable))
			{
				return;
			}
			selectable.OnDeselect();
			OnSelectChange?.Invoke(selectable, false);
			for (int i = 0 ; i < selectCollectors.Count ; i++)
			{
				SelectCollector coll = selectCollectors[i];
				coll.OnDeselected(selectable);
			}
		}
		internal void ClearInSelectItemList()
		{
			foreach (ISelectable selectable in selectItemList)
			{
				if (selectable.IsNullRef()) continue;

				selectable.OnDeselect();
				OnSelectChange?.Invoke(selectable, false);
				for (int i = 0 ; i < selectCollectors.Count ; i++)
				{
					SelectCollector coll = selectCollectors[i];
					coll.OnDeselected(selectable);
				}
			}
			selectCollectors.Clear();
		}
		internal void OnPointingTarget(ISelectable selectable)
		{
			if (selectable.IsNullRef()) return;

			selectable.OnPointing();
			OnPointing?.Invoke(selectable);

			for (int i = 0 ; i < selectCollectors.Count ; i++)
			{
				SelectCollector coll = selectCollectors[i];
				coll.OnPointing(selectable);
			}
		}
	}



	public class SelectHashSet<T> : IEnumerable<T> where T : class
	{
		private readonly HashSet<T> _hashSet = new HashSet<T>();
		private readonly List<T> _list = new List<T>();

		public int Count => _hashSet.Count;
		public bool Add(T item)
		{
			if (_hashSet.Add(item))
			{
				_list.Add(item);
				return true;
			}
			return false;
		}
		public bool Remove(T item)
		{
			if (_hashSet.Remove(item))
			{
				_list.Remove(item);
				return true;
			}
			return false;
		}
		public void Clear()
		{
			_hashSet.Clear();
			_list.Clear();
		}
		public bool Contains(T item)
		{
			return _hashSet.Contains(item);
		}
		public T First => _list.Count > 0 ? _list[0] : null;
		public T Last => _list.Count > 0 ? _list[_list.Count - 1] : null;
		public T this[int index] => _list[index];
		public IEnumerator<T> GetEnumerator()
		{
			return _list.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}