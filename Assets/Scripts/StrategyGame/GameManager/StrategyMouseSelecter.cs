using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

	[SerializeField, ReadOnly] private Camera mainCamera;
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
	private Action onFirstSelected;
	private Action onLastReleased;
	[Serializable]
	public struct InputData
	{
		public Vector2 mouseDownPosition;
		public Vector2 mouseCurrPosition;

		public float leftPressedTime;
		public float leftReleasedTime;

		public bool leftPressedThisFrame;
		public bool leftIsPressed;
		public bool leftIsReleased => !leftIsPressed;
		public bool leftReleasedThisFrame;
		public bool shift;
		public bool alt;
		public bool isDrag;
	}
	private void Awake()
	{
		onFirstSelected = null;
		onLastReleased = null;
	}
	private void OnEnable()
	{
		mainCamera = Camera.main;
		mouse = Mouse.current;
		keyboard = Keyboard.current;
		eventSystem = EventSystem.current;
		selecterState = SelecterState.None;
		currentSelecter = null;
		selectItemList = new HashSet<ISelectMouse>();
	}
	private void OnDisable()
	{
		mainCamera = null;
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
		if (mainCamera == null) mainCamera = Camera.main;
		if (mouse == null) mouse = Mouse.current;
		if (keyboard == null) keyboard = Keyboard.current;
		if (eventSystem == null || mainCamera == null || !mainCamera.isActiveAndEnabled || mouse == null || keyboard == null) return;

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
				int beforeCount = selectItemList.Count;
				currentSelecter?.Released();
				selecterState = SelecterState.None;
				int afterCount = selectItemList.Count;

				if (beforeCount == 0 && afterCount > 0) Update_OnFirstSelected();
				if (beforeCount > 0 && afterCount == 0) Update_OnLastReleased();
				Update_SingleSelectItem(afterCount);
			}
		}
		void Update_SingleSelectItem(int afterCount)
		{
			if (afterCount == 1)
			{
				var single = selectItemList.First();
				if (single != singleSelectItem)
				{
					singleSelectItem?.OnSingleDeselect();
					singleSelectItem = single;
					singleSelectItem?.OnSingleSelect();
				}
			}
			else
			{
				singleSelectItem?.OnSingleDeselect();
				singleSelectItem = null;
			}
		}
		void Update_OnFirstSelected()
		{
			onFirstSelected?.Invoke();
		}
		void Update_OnLastReleased()
		{
			onLastReleased?.Invoke();
		}
	}
	private SelecterState UpdateSelecterState()
	{
		inputData.leftPressedThisFrame = mouse.leftButton.wasPressedThisFrame;
		inputData.leftIsPressed = mouse.leftButton.isPressed;
		inputData.leftReleasedThisFrame = mouse.leftButton.wasReleasedThisFrame;
		inputData.shift = keyboard.shiftKey.isPressed;
		inputData.alt = keyboard.altKey.isPressed;
		inputData.mouseCurrPosition = mouse.position.ReadValue();

		if (inputData.leftReleasedThisFrame)
		{
			inputData.leftReleasedTime = Time.unscaledTime;
			return SelecterState.Released;
		}
		else if (inputData.leftIsReleased)
		{
			return SelecterState.None;
		}

		if (inputData.leftPressedThisFrame)
		{
			inputData.leftPressedTime = Time.unscaledTime;
			inputData.mouseDownPosition = inputData.mouseCurrPosition;
			inputData.isDrag = false;
			return SelecterState.Click;
		}
		if (inputData.leftIsPressed && !inputData.isDrag)
		{
			float distance = Vector2.Distance(inputData.mouseDownPosition, inputData.mouseCurrPosition);
			float dragThreshold = eventSystem.pixelDragThreshold;

			inputData.isDrag = distance > dragThreshold;

			return inputData.isDrag ? SelecterState.Drag : SelecterState.Click;
		}
		return selecterState;
	}
	private BaseSelecter CreateSelecter(SelecterState state) => state switch
	{
		SelecterState.Click => new ClickSelecter(this),
		SelecterState.Drag => new DragSelecter(this),
		_ => null
	};

	public HashSet<ISelectMouse> GetCurrentSelectList => selectItemList;
	public void AddListener_OnFirstAndLast(Action onFirstSelect, Action onLastReleased)
	{
		if (onFirstSelected != null)
		{
			this.onFirstSelected -= onFirstSelected;
			this.onFirstSelected += onFirstSelected;
		}
		if (onLastReleased != null)
		{
			this.onLastReleased -= onLastReleased;
			this.onLastReleased += onLastReleased;
		}
	}
	public void RemoveListener_OnFirsAndLast(Action onFirstSelect, Action onLastReleased)
	{
		if (onFirstSelected != null)
		{
			this.onFirstSelected -= onFirstSelected;
		}
		if (onLastReleased != null)
		{
			this.onLastReleased -= onLastReleased;
		}
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
		protected Camera MainCamera => Selecter.mainCamera;
		protected Mouse MainMouse => Selecter.mouse;
		protected EventSystem MainEventSystem => Selecter.eventSystem;
		protected LayerMask LayerMask => Selecter.layerMask;
		protected HashSet<ISelectMouse> SelectItemList => Selecter.selectItemList;
		protected HashSet<ISelectMouse> EnterItemList => Selecter.enterItemList;
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
			if (MainEventSystem.IsPointerOverGameObject()) return null;

			Ray ray = MainCamera.ScreenPointToRay(mousePosition);
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
			if (target == null) return;
			if (!SelectItemList.Contains(target))
			{
				SelectItemList.Add(target);
				target.IsSelectMouse = true;
				target.OnSelect();
			}
		}
		protected void Deselect(ISelectMouse target)
		{
			if (target == null) return;
			if (SelectItemList.Contains(target))
			{
				SelectItemList.Remove(target);
				target.IsSelectMouse = false;
				target.OnDeselect();
			}
		}
		protected void ClearSelect()
		{
			SelectItemList.Clear();
		}
		protected void ToggleSelect(ISelectMouse target)
		{
			if (target == null) return;
			if (SelectItemList.Contains(target)) Deselect(target);
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

					Vector2 screenPos = MainCamera.WorldToScreenPoint(target.ClickCenter);
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
