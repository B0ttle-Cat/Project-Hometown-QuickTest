
using System;
using System.Collections;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;

namespace StrategyManagerModule
{
	public partial class StrategyGameSelecter : MonoBehaviour, IStrategyProcess
	{
		public IStrategyProcess ThisProcess => this;
		public List<ProcessOverrider> OverriderList { get; } = new List<ProcessOverrider>();

		[SerializeField,ReadOnly]
		private SelectComputer selectComputer;
		private HashSet<ISelectable> selectItemList;
		private HashSet<ISelectable> addSelectBuffer;
		private HashSet<ISelectable> removeSelectBuffer;


		public event Action<ISelectable, bool> OnSelectChange;
		public event Action<ISelectable> OnPointing;

		public event Action<Vector3> OnSelectingEmpty;
		public event Action<Vector3> OnPointingEmpty;

		private bool changeThisFrame;
		private bool selectAny;

		private ProcessOverrider pressedEscapeKey;

		void IStrategyProcess.OnInit()
		{
			selectComputer = GetComponentInChildren<SelectComputer>();

			selectItemList = new HashSet<ISelectable>();
			addSelectBuffer = new HashSet<ISelectable>();
			removeSelectBuffer = new HashSet<ISelectable>();
			
			changeThisFrame = false;
			selectAny = false;
		}
		void IStrategyProcess.OnStart()
		{
			selectComputer.Init(this);
		}
		void IStrategyProcess.OnStop()
		{
			selectComputer.Deinit();

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
		void IStrategyProcess.Update()
		{
			if (selectComputer.IsNotNullRef() &&  selectComputer.IsVaild())
			{
				selectComputer.InputUpdate();
				selectComputer.Compute();
			}

			changeThisFrame = false;
			if (removeSelectBuffer.Count > 0)
			{
				changeThisFrame = true;
				foreach (var item in removeSelectBuffer)
				{
					OnDeselectItemInList(item);
				}
				removeSelectBuffer.Clear();
			}
			if (addSelectBuffer.Count > 0)
			{
				changeThisFrame = true;
				foreach (var item in addSelectBuffer)
				{
					OnSelectItemInList(item);
				}
				addSelectBuffer.Clear();
			}
			if (changeThisFrame)
			{
				int selectCount = selectItemList.Count;
				if (selectAny && selectCount == 0)
				{
					selectAny = false;
					OnSelectNothing();
				}
				else if (!selectAny && selectCount >= 1)
				{
					selectAny = true;
					OnSelectAny();
				}
			}
		}

#if UNITY_EDITOR

		[ShowInInspector,LabelWidth(100)]
		[InlineButton("Test_OnSystemClearSelectList", "Clear")]
		[InlineButton("Test_OnSystemSelectObject", "Deelect")]
		[InlineButton("Test_OnSystemDeselectObject", "Select")]
		[InlineButton("Test_OnSystemPointingTarget","Pointing")]
		private ISelectable testTarget { get; set; }
		private void Test_OnSystemSelectObject()
		{
			OnSystemSelectObject(testTarget, true);
		}
		private void Test_OnSystemDeselectObject()
		{
			OnSystemDeselectObject(testTarget);
		}
		private void Test_OnSystemClearSelectList()
		{
			OnSystemClearSelectList();
		}
		private void Test_OnSystemPointingTarget()
		{
			OnSystemPointingTarget(testTarget);
		}
#endif
		public void OnSystemSelectObject(ISelectable target, bool clearOldSelect)
		{
			if (target.IsNullRef()) return;
			if (clearOldSelect) ClearInSelectItemList();
			OnSelectItem(target);
		}
		public void OnSystemSelectObject(ISelectable target)
		{
			OnSystemSelectObject(target, !selectComputer.IsKeyHold_MultiSelect);
		}
		public void OnSystemDeselectObject(ISelectable target)
		{
			if (target.IsNullRef()) return;
			OnDeselectItem(target);
		}
		public void OnSystemClearSelectList()
		{
			ClearInSelectItemList();
		}
		public void OnSystemPointingTarget(ISelectable target)
		{
			if (target.IsNullRef()) return;
			OnPointingTarget(target);
		}
		internal void OnSelectItem(ISelectable selectable)
		{
			if (selectable.IsNullRef())
			{
				return;
			}
			GetPassthrough(ref selectable);

			if (!selectable.CanSelect())
			{
				return;
			}

			removeSelectBuffer.Remove(selectable);
			addSelectBuffer.Add(selectable);
		}
		private void OnSelectItemInList(ISelectable selectable)
		{
			if (ThisProcess.TryGetProcessOverrider<ProcessOverrider_OnSelectTarget>(out var processOverrider))
			{
				processOverrider.InvokeOverrider(selectable);
				return;
			}

			if (!selectItemList.Add(selectable))
			{
				return;
			}

			selectable.OnSelect();
			OnSelectChange?.Invoke(selectable, true);
		}
		internal void OnDeselectItem(ISelectable selectable)
		{
			if (selectable.IsNullRef())
			{
				return;
			}
			GetPassthrough(ref selectable);

			addSelectBuffer.Remove(selectable);
			removeSelectBuffer.Add(selectable);
		}
		private void OnDeselectItemInList(ISelectable selectable)
		{
			if (ThisProcess.TryGetProcessOverrider<ProcessOverrider_OnDeselectTarget>(out var processOverrider))
			{
				processOverrider.InvokeOverrider(selectable);
				return;
			}
			if (!selectItemList.Remove(selectable))
			{
				return;
			}
			selectable.OnDeselect();
			OnSelectChange?.Invoke(selectable, false);
		}
		internal void ClearInSelectItemList()
		{
			int count = selectItemList.Count;
			if (count == 0) return;

			addSelectBuffer.Clear();
			removeSelectBuffer.Clear();
			foreach (ISelectable selectable in selectItemList)
			{
				if (selectable.IsNullRef()) continue;
				removeSelectBuffer.Add(selectable);
			}
		}
		internal void OnPointingTarget(ISelectable selectable)
		{
			if (selectable.IsNullRef())
			{
				return;
			}
			GetPassthrough(ref selectable);
			if (!selectable.CanSelect())
			{
				return;
			}

			if (ThisProcess.TryGetProcessOverrider<ProcessOverrider_OnPointingTarget>(out var processOverrider))
			{
				processOverrider.InvokeOverrider(selectable);
				return;
			}
			selectable.OnPointing();
			OnPointing?.Invoke(selectable);
		}
		internal void OnSelectingEmptyGround(Vector3 mousePoint)
		{
			if (ThisProcess.TryGetProcessOverrider<ProcessOverrider_OnSelectingEmptyGround>(out var processOverrider))
			{
				processOverrider.InvokeOverrider(mousePoint);
				return;
			}
			OnSelectingEmpty?.Invoke(mousePoint);
		}
		internal void OnPointingEmptyGround(Vector3 mousePoint)
		{
			if (ThisProcess.TryGetProcessOverrider<ProcessOverrider_OnPointingEmptyGround>(out var processOverrider))
			{
				processOverrider.InvokeOverrider(mousePoint);
				return;
			}
			OnPointingEmpty?.Invoke(mousePoint);
		}
		private void OnSelectAny()
		{
			if (pressedEscapeKey == null)
				pressedEscapeKey =  new ProcessOverrider_OnPressedEscapeKey(OnSystemClearSelectList);
		}
		private void OnSelectNothing()
		{
			if(pressedEscapeKey != null)
			{
				pressedEscapeKey.Dispose();
				pressedEscapeKey = null;
			}
		}
		private void GetPassthrough(ref ISelectable selectable)
		{
			HashSet<ISelectable> passingList = new (){ selectable };
			while (selectable.HasPassthrough(out var passthrough))
			{
				if (passthrough == null || !passingList.Add(passthrough)) break;
				selectable = passthrough;
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


	public record ProcessOverrider_OnSelectTarget : ProcessOverriderAction<ISelectable>
	{
		public override IStrategyProcess OriginalProcess => StrategyManager.Selecter;
		public ProcessOverrider_OnSelectTarget(ProcessOverriderAction<ISelectable> original) : base(original)
		{
		}
	}
	public record ProcessOverrider_OnDeselectTarget : ProcessOverriderAction<ISelectable>
	{
		public override IStrategyProcess OriginalProcess => StrategyManager.Selecter;
		public ProcessOverrider_OnDeselectTarget(ProcessOverriderAction<ISelectable> original) : base(original)
		{
		}
	}
	public record ProcessOverrider_OnPointingTarget : ProcessOverriderAction<ISelectable>
	{
		public override IStrategyProcess OriginalProcess => StrategyManager.Selecter;
		public ProcessOverrider_OnPointingTarget(Action<ISelectable> action) : base(action)
		{
		}
	}
	public record ProcessOverrider_OnSelectingEmptyGround : ProcessOverriderAction<Vector3>
	{
		public override IStrategyProcess OriginalProcess => StrategyManager.Selecter;
		public ProcessOverrider_OnSelectingEmptyGround(Action<Vector3> action) : base(action)
		{
		}
	}
	public record ProcessOverrider_OnPointingEmptyGround : ProcessOverriderAction<Vector3>
	{
		public override IStrategyProcess OriginalProcess => StrategyManager.Selecter;
		public ProcessOverrider_OnPointingEmptyGround(Action<Vector3> action) : base(action)
		{
		}
	}
}