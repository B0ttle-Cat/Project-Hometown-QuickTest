using System;
using System.Collections;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
public partial class StrategyMouseSelecter : MonoBehaviour
{
	public enum SelecterState
	{
		None,
		Click,          // 일반적인 마우스 클릭
		Drag,           // 마우스 드래그로 범위 선택
		Released,
	}

	[SerializeField, ReadOnly] private Mouse mouse;
	[SerializeField, ReadOnly] private Keyboard keyboard;
	[SerializeField, ReadOnly] private InputData inputData;
	[SerializeField, ReadOnly] private EventSystem eventSystem;
	[SerializeField] private LayerMask layerMask;
	[SerializeField, ReadOnly] private SelecterState selecterState;
	[SerializeField, ReadOnly] private BaseSelecter currentSelecter;
	[ShowInInspector, ReadOnly] private HashSet<ISelectMouse> selectItemList;
	[ShowInInspector, ReadOnly] private ISelectMouse singleSelectItem;
	[ShowInInspector, ReadOnly] private HashSet<ISelectMouse> enterItemList;

	[ShowInInspector, ReadOnly] private ISelectMouse uiSelect;

	public HashSet<ISelectMouse> GetCurrentSelectList => selectItemList;

	private Action<ISelectMouse> onSelected;
	private Action<ISelectMouse> onDeselected;

	private Action<ISelectMouse> onSingleSelected;
	private Action<ISelectMouse> onSingleDeselected;

	private Action<ISelectMouse> onFirstSelected;
	private Action<ISelectMouse> onLastDeselected;

	public InputData GetInputData => inputData;

	[Serializable]
	public struct InputData
	{
		public Vector2 mouseDownPosition;
		public Vector2 mouseCurrPosition;

		public bool isPointerOver;

		public float leftPressedTime;
		public float leftReleasedTime;

		public bool leftPressedThisFrame;
		public bool leftIsPressed;
		public bool leftIsReleased => !leftIsPressed;
		public bool leftReleasedThisFrame;
		public bool shift;
		public bool alt;
		public bool isDown;
		public bool isDrag;

	}

	private void Awake()
	{
		onFirstSelected = null;
		onLastDeselected = null;
	}
	private void OnEnable()
	{
		mouse = Mouse.current;
		keyboard = Keyboard.current;
		eventSystem = EventSystem.current;
		selecterState = SelecterState.None;
		currentSelecter = null;
		selectItemList = new HashSet<ISelectMouse>();
	}
	private void OnDisable()
	{
		mouse = null;
		keyboard = null;
		if (currentSelecter != null)
		{
			currentSelecter.Dispose();
			currentSelecter = null;
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
	private void Update()
	{
		if (eventSystem == null) eventSystem = EventSystem.current;
		if (mouse == null) mouse = Mouse.current;
		if (keyboard == null) keyboard = Keyboard.current;
		if (eventSystem == null || mouse == null || keyboard == null) return;

		Camera mainCamera = StrategyManager.MainCamera;
		if (mainCamera == null || !mainCamera.isActiveAndEnabled) return;

		var nextSelecterState = UpdateSelecterState();
		if (nextSelecterState != selecterState)
		{
			selecterState = nextSelecterState;
			Selecter_Released();
			Selecter_Start();
		}
		Selecter_Update();

		void Selecter_Start()
		{
			currentSelecter?.Dispose();
			currentSelecter = CreateSelecter(selecterState);
			currentSelecter?.Start();
		}
		void Selecter_Update()
		{
			if (currentSelecter == null) return;

			if (currentSelecter.Valid())
			{
				currentSelecter.Pressed();
			}
			else
			{
				selecterState = SelecterState.None;
				currentSelecter.Dispose();
				currentSelecter = null;
			}
		}
		void Selecter_Released()
		{
			if (selecterState == SelecterState.Released)
			{
				currentSelecter?.Released();
				selecterState = SelecterState.None;
			}
		}
	}
	private SelecterState UpdateSelecterState()
	{
		inputData.isPointerOver = eventSystem.IsPointerOverGameObject();
		if (inputData.isPointerOver)
		{
			return selecterState;
		}


		inputData.leftPressedThisFrame = mouse.leftButton.wasPressedThisFrame;
		inputData.leftIsPressed = mouse.leftButton.isPressed;
		inputData.leftReleasedThisFrame = mouse.leftButton.wasReleasedThisFrame;
		inputData.shift = keyboard.shiftKey.isPressed;
		inputData.alt = keyboard.altKey.isPressed;
		inputData.mouseCurrPosition = mouse.position.ReadValue();

		if (inputData.leftReleasedThisFrame && inputData.isDown)
		{
			inputData.isDown = false;
			inputData.leftReleasedTime = Time.unscaledTime;
			return SelecterState.Released;
		}
		else if (inputData.leftIsReleased)
		{
			inputData.isDown = false;
			return SelecterState.None;
		}
		if (inputData.leftPressedThisFrame)
		{
			inputData.leftPressedTime = Time.unscaledTime;
			inputData.mouseDownPosition = inputData.mouseCurrPosition;
			inputData.isDown = true;
			inputData.isDrag = false;
			return SelecterState.Click;
		}
		if (inputData.leftIsPressed && !inputData.isDrag && inputData.isDown)
		{
			float distance = Vector2.Distance(inputData.mouseDownPosition, inputData.mouseCurrPosition);
			float dragThreshold = eventSystem.pixelDragThreshold;

			inputData.isDrag = distance > dragThreshold;

			return inputData.isDrag ? SelecterState.Drag : SelecterState.Click;
		}
		return selecterState;
	}
	public void OnSystemSelectObject(ISelectMouse target, bool beforeClearList = true)
	{
		if (target == null) return;
		if (beforeClearList) ClearInSelectItemList();
		AddInSelectItemList(target);
	}
	public void OnSystemDeselectObject(ISelectMouse target)
	{
		if (target == null) return;
		RemoveInSelectItemList(target);
	}
	public void OnSystemClearSelectList()
	{
		ClearInSelectItemList();
	}

	private BaseSelecter CreateSelecter(SelecterState state) => state switch
	{
		SelecterState.Click => new ClickSelecter(this),
		SelecterState.Drag => new DragSelecter(this),
		_ => null
	};
	public void AddListener_OnSelectedAndDeselected(Action<ISelectMouse> onSelected, Action<ISelectMouse> onDeselected)
	{
		if (onSelected != null)
		{
			this.onSelected -= onSelected;
			this.onSelected += onSelected;
		}
		if (onDeselected != null)
		{
			this.onDeselected -= onDeselected;
			this.onDeselected += onDeselected;
		}
	}
	public void RemoveListener_OnSelectedAndDeselected(Action<ISelectMouse> onSelected, Action<ISelectMouse> onDeselected)
	{
		if (onSelected != null)
		{
			this.onSelected -= onSelected;
		}
		if (onDeselected != null)
		{
			this.onDeselected -= onDeselected;
		}
	}
	public void AddListener_OnSingleAndDeselected(Action<ISelectMouse> onSingleSelected, Action<ISelectMouse> onSingleDeselected)
	{
		if (onSingleSelected != null)
		{
			this.onSingleSelected -= onSingleSelected;
			this.onSingleSelected += onSingleSelected;
		}
		if (onSingleDeselected != null)
		{
			this.onSingleDeselected -= onSingleDeselected;
			this.onSingleDeselected += onSingleDeselected;
		}
	}
	public void AddListener_OnSingleSelectedAndDeselected(Action<ISelectMouse> onSelected, Action<ISelectMouse> onSingleDeselected)
	{
		if (onSelected != null)
		{
			this.onSingleSelected -= onSelected;
		}
		if (onSingleDeselected != null)
		{
			this.onSingleDeselected -= onSingleDeselected;
		}
	}
	public void AddListener_OnFirstAndLast(Action<ISelectMouse> onFirstSelected, Action<ISelectMouse> onLastDeselected)
	{
		if (onFirstSelected != null)
		{
			this.onFirstSelected -= onFirstSelected;
			this.onFirstSelected += onFirstSelected;
		}
		if (onLastDeselected != null)
		{
			this.onLastDeselected -= onLastDeselected;
			this.onLastDeselected += onLastDeselected;
		}
	}
	public void RemoveListener_OnFirsAndLast(Action<ISelectMouse> onFirstSelected, Action<ISelectMouse> onLastDeselected)
	{
		if (onFirstSelected != null)
		{
			this.onFirstSelected -= onFirstSelected;
		}
		if (onLastDeselected != null)
		{
			this.onLastDeselected -= onLastDeselected;
		}
	}

	private bool AddInSelectItemList(ISelectMouse target)
	{
		if (target != null && selectItemList.Add(target))
		{
			target.IsSelectMouse = true;
			if (target.OnSelect())
			{
				onSelected?.Invoke(target);
				if (selectItemList.Count == 1)
				{
					singleSelectItem = target;
					target.OnSingleSelect();
					onSingleSelected?.Invoke(target);

					target.OnFirstSelect();
					onFirstSelected?.Invoke(target);
				}
				else if (singleSelectItem != null)
				{
					singleSelectItem.OnSingleDeselect();
					onSingleDeselected?.Invoke(singleSelectItem);
					singleSelectItem = null;
				}
				return true;
			}
			else
			{
				selectItemList.Remove(target);
			}
		}
		return false;
	}
	private bool RemoveInSelectItemList(ISelectMouse target)
	{
		if (target != null && selectItemList.Remove(target))
		{
			target.IsSelectMouse = false;
			if (target.OnDeselect())
			{
				onDeselected?.Invoke(target);

				if (selectItemList.Count == 1)
				{
					singleSelectItem = target;
					target.OnSingleSelect();
					onSingleSelected?.Invoke(target);
				}
				else if (singleSelectItem != null)
				{
					singleSelectItem.OnSingleDeselect();
					onSingleDeselected?.Invoke(singleSelectItem);
					singleSelectItem = null;
				}
				if (selectItemList.Count == 0)
				{
					target.OnLastDeselect();
					onLastDeselected?.Invoke(target);
				}
				return true;
			}
			else
			{
				selectItemList.Add(target);
			}
		}
		return false;
	}
	private bool ContainsInSelectItemList(ISelectMouse target)
	{
		return target != null && selectItemList.Contains(target);
	}
	private void ClearInSelectItemList()
	{
		ISelectMouse last = null;
		foreach (ISelectMouse target in selectItemList)
		{
			if (target == null) continue;
			last = target;
			target.OnDeselect();
			onDeselected?.Invoke(target);
			if (target == singleSelectItem)
			{
				singleSelectItem.OnSingleDeselect();
				onSingleDeselected?.Invoke(singleSelectItem);
				singleSelectItem = null;
			}
		}
		if (last != null)
		{
			last.OnLastDeselect();
			onLastDeselected?.Invoke(last);
			last = null;
		}

		selectItemList.Clear();
	}


}
public partial class StrategyMouseSelecter
{
	[Serializable]
	public abstract class BaseSelecter : IDisposable
	{
		protected BaseSelecter(StrategyMouseSelecter selecter)
		{
			Selecter = selecter;
		}

		protected StrategyMouseSelecter Selecter { get; private set; }
		protected Mouse MainMouse => Selecter.mouse;
		protected EventSystem MainEventSystem => Selecter.eventSystem;
		protected LayerMask LayerMask => Selecter.layerMask;
		protected InputData InputData => Selecter.inputData;
		public virtual void OnDeinit()
		{
			Selecter = null;
		}
		public abstract void Start();
		public abstract bool Valid();
		public abstract void Pressed();
		public abstract void Released();
		protected ISelectMouse GetTargetUnderMouse(in Vector2 mousePosition)
		{
			if (StrategyManager.MainCamera == null) return null;
			if (MainEventSystem.IsPointerOverGameObject()) return null;

			Ray ray = StrategyManager.MainCamera.ScreenPointToRay(mousePosition);
			RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity, LayerMask != 0 ? LayerMask : -1);
			if (hits.Length == 0) return null;

			if (hits.Length > 1)
				Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

			foreach (var hit in hits)
			{
				var target = hit.collider.GetComponentInParent<ISelectMouse>();
				if (target != null) return target;
			}
			return null;
		}
		protected void SelectNew(ISelectMouse target)
		{
			Selecter.AddInSelectItemList(target);
		}
		protected void Deselect(ISelectMouse target)
		{
			Selecter.RemoveInSelectItemList(target);
		}
		protected void ClearSelect()
		{
			Selecter.ClearInSelectItemList();
		}
		protected void ToggleSelect(ISelectMouse target)
		{
			if (target == null) return;
			if (Selecter.ContainsInSelectItemList(target)) Deselect(target);
			else SelectNew(target);
		}
		public void Dispose()
		{
			OnDeinit();
		}
	}

	[Serializable]
	public class ClickSelecter : BaseSelecter
	{
		protected ISelectMouse mouseDownTarget;
		public ClickSelecter(StrategyMouseSelecter selecter) : base(selecter)
		{
			mouseDownTarget = null;
		}
		public override void Start()
		{
			mouseDownTarget = GetTargetUnderMouse(InputData.mouseDownPosition);
		}
		public override bool Valid()
		{
			return !InputData.isDrag;
		}
		public override void Pressed()
		{

		}
		public override void Released()
		{
			if (!InputData.shift)
			{
				ClearSelect();
			}

			if (mouseDownTarget != GetTargetUnderMouse(InputData.mouseCurrPosition)) return;

			if (InputData.alt)
			{
				Deselect(mouseDownTarget);
			}
			else
			{
				SelectNew(mouseDownTarget);
			}
		}
	}

	[Serializable]
	public class DragSelecter : BaseSelecter
	{
		public DragSelecter(StrategyMouseSelecter selecter) : base(selecter)
		{
		}
		public override void Start()
		{
		}
		public override bool Valid()
		{
			return InputData.isDrag;
		}
		public override void Pressed()
		{

		}
		public override void Released()
		{
			if (!InputData.shift)
			{
				ClearSelect();
			}
			ComputeEnterRect();
		}
		void ComputeEnterRect()
		{
			if (StrategyManager.MainCamera == null) return;

			Vector2 start = InputData.mouseDownPosition;
			Vector2 end = InputData.mouseCurrPosition;

			float xMin = Mathf.Min(start.x, end.x);
			float xMax = Mathf.Max(start.x, end.x);
			float yMin = Mathf.Min(start.y, end.y);
			float yMax = Mathf.Max(start.y, end.y);

			Rect rect = Rect.MinMaxRect(xMin, yMin, xMax, yMax);
			foreach (IList list in StrategyManager.Collector.GetAllLists())
			{
				foreach (IStrategyElement item in list)
				{
					if (item is not ISelectMouse target) continue;

					Vector2 screenPos = StrategyManager.MainCamera.WorldToScreenPoint(target.ClickCenter);
					if (rect.Contains(screenPos))
					{
						if (InputData.alt) Deselect(target);
						else SelectNew(target);
					}
				}
			}
		}
	}
}
