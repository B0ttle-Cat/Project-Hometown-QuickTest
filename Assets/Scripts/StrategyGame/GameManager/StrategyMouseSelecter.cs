using System;
using System.Collections;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
[DefaultExecutionOrder(-1)]
public partial class StrategyMouseSelecter : MonoBehaviour
{
	public enum ClickSelectMode
	{
		NoSelect,
		SaveSelect,
		TempSelect,
	}
	public enum SelecterState
	{
		None,
		Click,          // 일반적인 마우스 클릭
		Drag,           // 마우스 드래그로 범위 선택
		Released,
	}

	[SerializeField, ReadOnly] private Mouse mouse;
	[SerializeField, ReadOnly] private Keyboard keyboard;
	[SerializeField, ReadOnly] private EventSystem eventSystem;
	[SerializeField] private LayerMask layerMask;

	[SerializeField, ReadOnly] private InputData inputData;
	[SerializeField, ReadOnly] private SelecterState leftSelecterState;
	[SerializeField, ReadOnly] private BaseSelecter leftCurrentSelecter;

	[SerializeField, ReadOnly] private SelecterState rightSelecterState;
	[SerializeField, ReadOnly] private BaseSelecter rightCurrentSelecter;

	[ShowInInspector, ReadOnly] private HashSet<ISelectable> selectItemList;
	[ShowInInspector, ReadOnly] private ISelectable singleSelectItem;

	public HashSet<ISelectable> GetCurrentSelectList => selectItemList;

	private event Action<ISelectable> onSelected;
	private event Action<ISelectable> onDeselected;

	private event Action<ISelectable> onSingleSelected;
	private event Action<ISelectable> onSingleDeselected;

	private event Action<ISelectable> onFirstSelected;
	private event Action<ISelectable> onLastDeselected;

	private event Action<ISelectable> onPointingTarget;
	public InputData GetInputData => inputData;

	[Serializable]
	public struct InputData
	{
		public Vector2 mouseCurrPosition;
		public Vector2 mouseCurrDelta;
		public bool shift;
		public bool alt;
		public bool isPointerOver;

		public Vector2 leftMouseDownPosition;
		public Vector2 rightMouseDownPosition;
		public float leftPressedTime;
		public float leftReleasedTime;
		public bool leftPressedThisFrame;
		public bool leftIsPressed;
		public bool leftIsReleased => !leftIsPressed;
		public bool leftReleasedThisFrame;
		public bool leftIsDown;
		public bool leftIsDrag;

		public float rightPressedTime;
		public float rightReleasedTime;
		public bool rightPressedThisFrame;
		public bool rightIsPressed;
		public bool rightIsReleased => !rightIsPressed;
		public bool rightReleasedThisFrame;
		public bool rightIsDown;
		public bool rightIsDrag;
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
		leftSelecterState = SelecterState.None;
		leftCurrentSelecter = null;
		selectItemList = new HashSet<ISelectable>();
	}
	private void OnDisable()
	{
		mouse = null;
		keyboard = null;
		if (leftCurrentSelecter != null)
		{
			leftCurrentSelecter.Dispose();
			leftCurrentSelecter = null;
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

		CommonInputUpdate();
		SelecterState leftNextSelecterState = LeftUpdateSelecterState();
		SelecterState rightNextSelecterState = RightUpdateSelecterState();
		LeftMouseUpdate(leftNextSelecterState);
		RightMouseUpdate(rightNextSelecterState);
	}

	private void LeftMouseUpdate(SelecterState nextSelecterState)
	{
		if (nextSelecterState != leftSelecterState)
		{
			leftSelecterState = nextSelecterState;
			Selecter_Released();
			Selecter_Start();
		}
		Selecter_Update();

		void Selecter_Start()
		{
			leftCurrentSelecter?.Dispose();
			leftCurrentSelecter = CreateSelecter(leftSelecterState);
			leftCurrentSelecter?.Start();
		}
		void Selecter_Update()
		{
			if (leftCurrentSelecter == null) return;

			if (leftCurrentSelecter.Valid())
			{
				leftCurrentSelecter.Pressed();
			}
			else
			{
				leftSelecterState = SelecterState.None;
				leftCurrentSelecter.Dispose();
				leftCurrentSelecter = null;
			}
		}
		void Selecter_Released()
		{
			if (leftSelecterState == SelecterState.Released)
			{
				leftCurrentSelecter?.Released();
				leftSelecterState = SelecterState.None;
			}
		}
	}
	private void RightMouseUpdate(SelecterState nextSelecterState)
	{
		if (nextSelecterState != rightSelecterState)
		{
			rightSelecterState = nextSelecterState;
			Selecter_Released();
			Selecter_Start();
		}
		Selecter_Update();

		void Selecter_Start()
		{
			rightCurrentSelecter?.Dispose();
			rightCurrentSelecter = CreateSelecter(rightSelecterState);
			rightCurrentSelecter?.Start();
		}
		void Selecter_Update()
		{
			if (rightCurrentSelecter == null) return;

			if (rightCurrentSelecter.Valid())
			{
				rightCurrentSelecter.Pressed();
			}
			else
			{
				rightSelecterState = SelecterState.None;
				rightCurrentSelecter.Dispose();
				rightCurrentSelecter = null;
			}
		}
		void Selecter_Released()
		{
			if (rightSelecterState == SelecterState.Released)
			{
				rightCurrentSelecter?.Released();
				rightSelecterState = SelecterState.None;
			}
		}
	}
	void CommonInputUpdate()
	{
		inputData.isPointerOver = eventSystem.IsPointerOverGameObject();
		inputData.shift = keyboard.shiftKey.isPressed;
		inputData.alt = keyboard.altKey.isPressed;
		inputData.mouseCurrPosition = mouse.position.ReadValue();
		inputData.mouseCurrDelta = mouse.delta.ReadValue();
	}
	SelecterState LeftUpdateSelecterState()
	{
		if (inputData.isPointerOver)
		{
			return leftSelecterState;
		}

		inputData.leftPressedThisFrame = mouse.leftButton.wasPressedThisFrame;
		inputData.leftIsPressed = mouse.leftButton.isPressed;
		inputData.leftReleasedThisFrame = mouse.leftButton.wasReleasedThisFrame;

		if (inputData.leftReleasedThisFrame && inputData.leftIsDown)
		{
			inputData.leftIsDown = false;
			inputData.leftReleasedTime = Time.unscaledTime;
			return SelecterState.Released;
		}
		else if (inputData.leftIsReleased)
		{
			inputData.leftIsDown = false;
			return SelecterState.None;
		}
		if (inputData.leftPressedThisFrame)
		{
			inputData.leftPressedTime = Time.unscaledTime;
			inputData.leftMouseDownPosition = inputData.mouseCurrPosition;
			inputData.leftIsDown = true;
			inputData.leftIsDrag = false;
			return SelecterState.Click;
		}
		if (inputData.leftIsPressed && !inputData.leftIsDrag && inputData.leftIsDown)
		{
			float distance = Vector2.Distance(inputData.leftMouseDownPosition, inputData.mouseCurrPosition);
			float dragThreshold = eventSystem.pixelDragThreshold;

			inputData.leftIsDrag = distance > dragThreshold;

			return inputData.leftIsDrag ? SelecterState.Drag : SelecterState.Click;
		}
		return leftSelecterState;
	}
	SelecterState RightUpdateSelecterState()
	{
		if (inputData.isPointerOver)
		{
			return rightSelecterState;
		}

		inputData.rightPressedThisFrame = mouse.rightButton.wasPressedThisFrame;
		inputData.rightIsPressed = mouse.rightButton.isPressed;
		inputData.rightReleasedThisFrame = mouse.rightButton.wasReleasedThisFrame;

		if (inputData.rightReleasedThisFrame && inputData.rightIsDown)
		{
			inputData.rightIsDown = false;
			inputData.rightReleasedTime = Time.unscaledTime;
			return SelecterState.Released;
		}
		else if (inputData.rightIsReleased)
		{
			inputData.rightIsDown = false;
			return SelecterState.None;
		}
		if (inputData.rightPressedThisFrame)
		{
			inputData.rightPressedTime = Time.unscaledTime;
			inputData.rightMouseDownPosition = inputData.mouseCurrPosition;
			inputData.rightIsDown = true;
			inputData.rightIsDrag = false;
			return SelecterState.Click;
		}
		if (inputData.rightIsPressed && !inputData.rightIsDrag && inputData.rightIsDown)
		{
			float distance = Vector2.Distance(inputData.rightMouseDownPosition, inputData.mouseCurrPosition);
			float dragThreshold = eventSystem.pixelDragThreshold;

			inputData.rightIsDrag = distance > dragThreshold;

			return inputData.rightIsDrag ? SelecterState.None : SelecterState.Click;
		}
		return rightSelecterState;
	}
	public void OnSystemSelectObject(ISelectable target, bool beforeClearList = true)
	{
		if (target == null) return;
		if (beforeClearList) ClearInSelectItemList();
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

	private BaseSelecter CreateSelecter(SelecterState state) => state switch
	{
		SelecterState.Click => new ClickSelecter(this),
		SelecterState.Drag => new DragSelecter(this),
		_ => null
	};
	public void AddListener_OnSelectedAndDeselected(Action<ISelectable> onSelected, Action<ISelectable> onDeselected)
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
	public void RemoveListener_OnSelectedAndDeselected(Action<ISelectable> onSelected, Action<ISelectable> onDeselected)
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
	public void AddListener_OnSingleAndDeselected(Action<ISelectable> onSingleSelected, Action<ISelectable> onSingleDeselected)
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
	public void AddListener_OnSingleSelectedAndDeselected(Action<ISelectable> onSelected, Action<ISelectable> onSingleDeselected)
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
	public void AddListener_OnFirstAndLast(Action<ISelectable> onFirstSelected, Action<ISelectable> onLastDeselected)
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
	public void RemoveListener_OnFirsAndLast(Action<ISelectable> onFirstSelected, Action<ISelectable> onLastDeselected)
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
	private bool AddInSelectItemList(ISelectable target)
	{
		if (!(target != null && selectItemList.Add(target))) return false;

		if (target is ISelectableByMouse targetByMouse)
		{
			targetByMouse.IsSelectMouse = true;
		}

		HashSet<ISelectable> passingList = new (){ target };
		while (target.HasPass(out var pass))
		{
			if (pass == null || !passingList.Add(pass)) break;
			target = pass;
		}

		if (!target.CanSelect())
		{
			selectItemList.Add(target);
			return false;
		}

		target.OnSelect();
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
	private bool RemoveInSelectItemList(ISelectable target)
	{
		if (target != null && selectItemList.Remove(target))
		{
			if (target is ISelectableByMouse targetByMouse)
			{
				targetByMouse.IsSelectMouse = false;
			}

			target.OnDeselect();
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
		return false;
	}
	private bool ContainsInSelectItemList(ISelectable target)
	{
		return target != null && selectItemList.Contains(target);
	}
	private void ClearInSelectItemList()
	{
		ISelectable last = null;
		foreach (ISelectable target in selectItemList)
		{
			if (target == null) continue;
			target.OnDeselect();
			last = target;
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

	public void AddListener_OnPointingTarget(Action<ISelectable> onPointingTarget)
	{
		if (onPointingTarget == null) return;
		this.onPointingTarget -= onPointingTarget;
		this.onPointingTarget += onPointingTarget;
	}
	public void RemoveListener_OnPointingTarget(Action<ISelectable> onPointingTarget)
	{
		if (onPointingTarget == null) return;
		this.onPointingTarget -= onPointingTarget;
	}
	private void OnPointingTarget(ISelectable target)
	{
		if (target == null || onPointingTarget == null) return;
		onPointingTarget.Invoke(target);
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
		protected ISelectableByMouse GetTargetUnderMouse(in Vector2 mousePosition)
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
				var target = hit.collider.GetComponentInParent<ISelectableByMouse>();
				if (target != null) return target;
			}
			return null;
		}
		protected virtual void OnSelect(ISelectableByMouse target)
		{
			Selecter.AddInSelectItemList(target);
		}
		protected virtual void Deselect(ISelectableByMouse target)
		{
			Selecter.RemoveInSelectItemList(target);
		}
		protected virtual void ClearSelect()
		{
			Selecter.ClearInSelectItemList();
		}
		protected virtual void ToggleSelect(ISelectableByMouse target)
		{
			if (target == null) return;
			if (Selecter.ContainsInSelectItemList(target)) Deselect(target);
			else OnSelect(target);
		}
		public void Dispose()
		{
			OnDeinit();
		}
	}

	[Serializable]
	public class ClickSelecter : BaseSelecter
	{
		protected ISelectableByMouse mouseDownTarget;
		public ClickSelecter(StrategyMouseSelecter selecter) : base(selecter)
		{
			mouseDownTarget = null;
		}
		public override void Start()
		{
			mouseDownTarget = GetTargetUnderMouse(InputData.leftMouseDownPosition);
		}
		public override bool Valid()
		{
			return !InputData.leftIsDrag;
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
				OnSelect(mouseDownTarget);
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
			return InputData.leftIsDrag;
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

			Vector2 start = InputData.leftMouseDownPosition;
			Vector2 end = InputData.mouseCurrPosition;

			float xMin = Mathf.Min(start.x, end.x);
			float xMax = Mathf.Max(start.x, end.x);
			float yMin = Mathf.Min(start.y, end.y);
			float yMax = Mathf.Max(start.y, end.y);

			Rect rect = Rect.MinMaxRect(xMin, yMin, xMax, yMax);
			foreach (IList list in StrategyManager.Collector.GetAllEnumerable())
			{
				foreach (IStrategyElement item in list)
				{
					if (item is not ISelectableByMouse target) continue;

					Vector2 screenPos = StrategyManager.MainCamera.WorldToScreenPoint(target.ClickCenter);
					if (rect.Contains(screenPos))
					{
						if (InputData.alt) Deselect(target);
						else OnSelect(target);
					}
				}
			}
		}
	}


	public class RIghtPointer : BaseSelecter
	{
		protected ISelectableByMouse mouseDownTarget;
		public RIghtPointer(StrategyMouseSelecter selecter) : base(selecter)
		{
			mouseDownTarget = null;
		}
		public override void Start()
		{
			mouseDownTarget = GetTargetUnderMouse(InputData.rightMouseDownPosition);
		}
		public override bool Valid()
		{
			return !InputData.leftIsDrag;
		}
		public override void Pressed()
		{

		}
		public override void Released()
		{
			if (mouseDownTarget != GetTargetUnderMouse(InputData.mouseCurrPosition)) return;

			OnSelect(mouseDownTarget);
		}
		protected override void OnSelect(ISelectableByMouse target)
		{
			Selecter.OnPointingTarget(target);
		}
	}
}
