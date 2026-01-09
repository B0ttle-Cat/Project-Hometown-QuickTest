using System;
using System.Collections;

using Sirenix.OdinInspector;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace StrategyManagerModule
{
	public partial class StrategyMouseSelectComputer : SelectComputer
	{
		public enum ClickSelectMode
		{
			NoSelect,
			SaveSelect,
			TempSelect,
		}
		public enum MouseState
		{
			Released,
			Click,          // 일반적인 마우스 클릭
			Drag,           // 마우스 드래그로 범위 선택
		}
		public StrategySelecter Selecter { get; private set; }

		[SerializeField, ReadOnly] private Mouse mouse;
		[SerializeField, ReadOnly] private Keyboard keyboard;
		[SerializeField, ReadOnly] private EventSystem eventSystem;
		[SerializeField] private LayerMask layerMask;

		[SerializeField, ReadOnly] private InputData inputData;

		[SerializeField, ReadOnly] private MouseState leftSelecterState;
		[SerializeField, ReadOnly] private BaseSelecter leftCurrentSelecter;

		[SerializeField, ReadOnly] private MouseState rightSelecterState;
		[SerializeField, ReadOnly] private BaseSelecter rightCurrentSelecter;
		public InputData GetInputData => inputData;

		public MouseState LeftSelecterState => leftSelecterState;
		public MouseState RightSelecterState => rightSelecterState;

		public event Action<MouseState, InputData> OnChangeRightMouseState;
		public event Action<MouseState, InputData> OnChangeLeftMouseState;

		[Serializable]
		public record InputData
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

		public override void Init(StrategySelecter selecter)
		{
			Selecter = selecter;

			mouse = Mouse.current;
			keyboard = Keyboard.current;
			eventSystem = EventSystem.current;
			leftSelecterState = MouseState.Released;
			leftCurrentSelecter = null;
		}
		public override void Deinit()
		{
			Selecter = null;
			mouse = null;
			keyboard = null;
			eventSystem = null;
			if (leftCurrentSelecter != null)
			{
				leftCurrentSelecter.Dispose();
				leftCurrentSelecter = null;
			}
			if (rightCurrentSelecter != null)
			{
				rightCurrentSelecter.Dispose();
				rightCurrentSelecter = null;
			}
		}
		public override bool IsVaild()
		{
			if (Selecter.IsNullRef()) return false;
			if (eventSystem == null) eventSystem = EventSystem.current;
			if (mouse == null) mouse = Mouse.current;
			if (keyboard == null) keyboard = Keyboard.current;
			if (eventSystem == null || mouse == null || keyboard == null) return false;

			Camera mainCamera = StrategyManager.MainCamera;
			if (mainCamera == null || !mainCamera.isActiveAndEnabled) return false;

			return true;
		}
		public override void InputUpdate()
		{
			CommonInputUpdate();
		}
		public override void Compute()
		{
			MouseState leftNextSelecterState = LeftUpdateSelecterState();
			MouseState rightNextSelecterState = RightUpdateSelecterState();
			LeftMouseUpdate(leftNextSelecterState);
			RightMouseUpdate(rightNextSelecterState);
		}
		private void LeftMouseUpdate(MouseState nextSelecterState)
		{
			if (nextSelecterState != leftSelecterState)
			{
				leftSelecterState = nextSelecterState;
				Selecter_Released();
				Selecter_Start();
				OnChangeLeftMouseState?.Invoke(leftSelecterState, inputData);
			}
			Selecter_Update();

			void Selecter_Start()
			{
				leftCurrentSelecter?.Dispose();
				leftCurrentSelecter = CreateSelecter(leftSelecterState, true);
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
					leftSelecterState = MouseState.Released;
					leftCurrentSelecter.Dispose();
					leftCurrentSelecter = null;
				}
			}
			void Selecter_Released()
			{
				if (leftSelecterState == MouseState.Released)
				{
					leftCurrentSelecter?.Released();
				}
			}
		}
		private void RightMouseUpdate(MouseState nextSelecterState)
		{
			if (nextSelecterState != rightSelecterState)
			{
				rightSelecterState = nextSelecterState;
				Selecter_Released();
				Selecter_Start();
				OnChangeRightMouseState?.Invoke(rightSelecterState, inputData);
			}
			Selecter_Update();

			void Selecter_Start()
			{
				rightCurrentSelecter?.Dispose();
				rightCurrentSelecter = CreateSelecter(rightSelecterState, false);
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
					rightSelecterState = MouseState.Released;
					rightCurrentSelecter.Dispose();
					rightCurrentSelecter = null;
				}
			}
			void Selecter_Released()
			{
				if (rightSelecterState == MouseState.Released)
				{
					rightCurrentSelecter?.Released();
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
		MouseState LeftUpdateSelecterState()
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
				return MouseState.Released;
			}
			else if (inputData.leftIsReleased)
			{
				inputData.leftIsDown = false;
				return MouseState.Released;
			}
			if (inputData.leftPressedThisFrame)
			{
				inputData.leftPressedTime = Time.unscaledTime;
				inputData.leftMouseDownPosition = inputData.mouseCurrPosition;
				inputData.leftIsDown = true;
				inputData.leftIsDrag = false;
				return MouseState.Click;
			}
			if (inputData.leftIsPressed && !inputData.leftIsDrag && inputData.leftIsDown)
			{
				float distance = Vector2.Distance(inputData.leftMouseDownPosition, inputData.mouseCurrPosition);
				float dragThreshold = eventSystem.pixelDragThreshold;

				inputData.leftIsDrag = distance > dragThreshold;

				return inputData.leftIsDrag ? MouseState.Drag : MouseState.Click;
			}
			return leftSelecterState;
		}
		MouseState RightUpdateSelecterState()
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
				return MouseState.Released;
			}
			else if (inputData.rightIsReleased)
			{
				inputData.rightIsDown = false;
				return MouseState.Released;
			}
			if (inputData.rightPressedThisFrame)
			{
				inputData.rightPressedTime = Time.unscaledTime;
				inputData.rightMouseDownPosition = inputData.mouseCurrPosition;
				inputData.rightIsDown = true;
				inputData.rightIsDrag = false;
				return MouseState.Click;
			}
			if (inputData.rightIsPressed && !inputData.rightIsDrag && inputData.rightIsDown)
			{
				float distance = Vector2.Distance(inputData.rightMouseDownPosition, inputData.mouseCurrPosition);
				float dragThreshold = eventSystem.pixelDragThreshold;

				inputData.rightIsDrag = distance > dragThreshold;

				return inputData.rightIsDrag ? MouseState.Released : MouseState.Click;
			}
			return rightSelecterState;
		}
		private BaseSelecter CreateSelecter(MouseState state, bool isLeft) => (state, isLeft) switch
		{
			(MouseState.Click, true) => new ClickSelecter(this),
			(MouseState.Click, false) => new RightPointer(this),
			(MouseState.Drag, true) => new DragSelecter(this),
			_ => null
		};
		private void AddInSelectItemList(ISelectable target) => Selecter.AddInSelectItemList(target);
		private void RemoveInSelectItemList(ISelectable target) => Selecter.RemoveInSelectItemList(target);
		private void ClearInSelectItemList() => Selecter.ClearInSelectItemList();
		private void OnPointingTarget(ISelectable target) => Selecter.OnPointingTarget(target);
	}
	public partial class StrategyMouseSelectComputer
	{
		[Serializable]
		public abstract class BaseSelecter : IDisposable
		{
			protected BaseSelecter(StrategyMouseSelectComputer selecter)
			{
				Selecter = selecter;
			}

			protected StrategyMouseSelectComputer Selecter { get; private set; }
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
			protected IMouseSelectable GetTargetUnderMouse(in Vector2 mousePosition)
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
					var target = hit.collider.GetComponentInParent<IMouseSelectable>();
					if (target != null) return target;
				}
				return null;
			}
			protected virtual void OnSelect(IMouseSelectable target)
			{
				Selecter.AddInSelectItemList(target);
			}
			protected virtual void Deselect(IMouseSelectable target)
			{
				Selecter.RemoveInSelectItemList(target);
			}
			protected virtual void ClearSelect()
			{
				Selecter.ClearInSelectItemList();
			}

			public void Dispose()
			{
				OnDeinit();
			}
		}

		[Serializable]
		public class ClickSelecter : BaseSelecter
		{
			protected IMouseSelectable mouseDownTarget;
			public ClickSelecter(StrategyMouseSelectComputer selecter) : base(selecter)
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
				if (mouseDownTarget.IsNullRef()) return;

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
			private Vector3 startWorldPos;
			private Vector3 endWorldPos;

			public DragSelecter(StrategyMouseSelectComputer selecter) : base(selecter)
			{
			}

			public override void Start()
			{
				// 시작 시점의 월드 좌표 저장 (레이캐스트 등을 통해 바닥 지점을 가져오는 것이 정확하지만, 
				// 요구사항에 따라 ScreenToWorldPoint를 사용하되 Plane 인터섹션을 권장함)
				startWorldPos = GetMouseWorldPosition(InputData.leftMouseDownPosition);
			}

			public override bool Valid()
			{
				return InputData.leftIsDrag;
			}

			public override void Pressed()
			{
				endWorldPos = GetMouseWorldPosition(InputData.mouseCurrPosition);
			}

			public override void Released()
			{
				if (!InputData.shift)
				{
					ClearSelect();
				}
				ComputeEnterRect();
			}

			private Vector3 GetMouseWorldPosition(Vector3 screenPos)
			{
				// XZ 평면(Y=0)과의 교점을 계산하여 정확한 월드 좌표 추출
				Ray ray = StrategyManager.MainCamera.ScreenPointToRay(screenPos);
				Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
				if (groundPlane.Raycast(ray, out float distance))
				{
					return ray.GetPoint(distance);
				}
				return StrategyManager.MainCamera.ScreenToWorldPoint(screenPos);
			}

			private void ComputeEnterRect()
			{
				if (StrategyManager.MainCamera == null) return;

				// 선택 영역의 중심과 크기 계산
				Vector3 center = (startWorldPos + endWorldPos) * 0.5f;

				// 카메라의 Y축 회전만 반영한 쿼터니언 생성
				Vector3 forward = Vector3.Cross(StrategyManager.MainCamera.transform.right, Vector3.up).normalized;
				Quaternion rotation = Quaternion.LookRotation(forward, Vector3.up);

				// 월드 좌표 공간의 차이를 로컬 공간으로 변환하여 크기 측정
				Matrix4x4 m = Matrix4x4.TRS(center, rotation, Vector3.one).inverse;
				Vector3 localStart = m.MultiplyPoint3x4(startWorldPos);
				Vector3 localEnd = m.MultiplyPoint3x4(endWorldPos);

				Vector3 size = new Vector3(Mathf.Abs(localStart.x - localEnd.x), 2000f, Mathf.Abs(localStart.z - localEnd.z));

				foreach (IList list in StrategyManager.Collector.GetAllElementLists())
				{
					foreach (var item in list)
					{
						if (item is not IMouseSelectable target) continue;

						// 대상의 위치를 선택 영역 로컬 공간으로 변환
						Vector3 targetPos = target.SelectCenter;
						targetPos.y = 0; // Y좌표 무시
						Vector3 localTargetPos = m.MultiplyPoint3x4(targetPos);

						// 로컬 바운드 체크 (Y값은 충분히 큰 범위로 설정)
						if (Mathf.Abs(localTargetPos.x) <= size.x * 0.5f &&
							Mathf.Abs(localTargetPos.z) <= size.z * 0.5f)
						{
							if (InputData.alt) Deselect(target);
							else OnSelect(target);
						}
					}
				}
			}

			// 디버깅용 기즈모 그리기
			public void OnDrawGizmos()
			{
				if (!InputData.leftIsDrag) return;

				Vector3 center = (startWorldPos + endWorldPos) * 0.5f;
				Vector3 forward = Vector3.Cross(StrategyManager.MainCamera.transform.right, Vector3.up).normalized;
				Quaternion rotation = Quaternion.LookRotation(forward, Vector3.up);

				Matrix4x4 m = Matrix4x4.TRS(center, rotation, Vector3.one).inverse;
				Vector3 localStart = m.MultiplyPoint3x4(startWorldPos);
				Vector3 localEnd = m.MultiplyPoint3x4(endWorldPos);
				Vector3 size = new Vector3(Mathf.Abs(localStart.x - localEnd.x), 0.1f, Mathf.Abs(localStart.z - localEnd.z));

				Gizmos.color = Color.green;
				Gizmos.matrix = Matrix4x4.TRS(center, rotation, Vector3.one);
				Gizmos.DrawWireCube(Vector3.zero, size);

				Gizmos.color = new Color(0, 1, 0, 0.2f);
				Gizmos.DrawCube(Vector3.zero, size);
				Gizmos.matrix = Matrix4x4.identity;
			}
		}
		[Serializable]
		public class RightPointer : BaseSelecter
		{
			protected IMouseSelectable mouseDownTarget;
			public RightPointer(StrategyMouseSelectComputer selecter) : base(selecter)
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
				if (mouseDownTarget.IsNullRef()) return;

				if (mouseDownTarget != GetTargetUnderMouse(InputData.mouseCurrPosition)) return;

				OnSelect(mouseDownTarget);
			}
			protected override void OnSelect(IMouseSelectable target)
			{
				Selecter.OnPointingTarget(target);
			}
		}
	}
	public partial class StrategySelecter
	{
		private StrategyMouseSelectComputer mouse;
		public StrategyMouseSelectComputer Mouse
		{
			get
			{
				if (mouse == null)
				{
					mouse = GetComponent<StrategyMouseSelectComputer>();
				}
				return mouse;
			}
		}

	}
}