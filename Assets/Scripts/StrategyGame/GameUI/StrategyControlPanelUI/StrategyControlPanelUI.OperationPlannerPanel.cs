using System;

using Sirenix.OdinInspector;

using UnityEngine;
using UnityEngine.InputSystem;
public partial class StrategyControlPanelUI
{
	[SerializeField, FoldoutGroup("OperationPlanner")]
	private Transform operationPlannerToot;
	[SerializeField, FoldoutGroup("OperationPlanner")]
	private MovementPathRenderer pathRenderPrefab;
	[SerializeField, FoldoutGroup("OperationPlanner"), InlineProperty, HideLabel]
	private OperationPlannerPanel operationPlannerPanel;

	public IPanelTarget ShowOperationPlannerPanel()
	{
		if (ViewStack.Peek(out var peek) && peek is OperationPlannerPanel)
		{
			return operationPlannerPanel;
		}
		else if (ViewStack.TryGetType<OperationPlannerPanel>(out var tryGet))
		{
			operationPlannerPanel = tryGet;
		}
		else
		{
			operationPlannerPanel = new OperationPlannerPanel(operationPlannerToot, pathRenderPrefab, this, HideOperationPlannerPanel);
		}
		ViewStack.Push(operationPlannerPanel);
		return operationPlannerPanel;
	}
	public void HideOperationPlannerPanel()
	{
		if (operationPlannerPanel == null) return;
		ViewStack.Pop(operationPlannerPanel);
		operationPlannerPanel = null;
	}

	[Serializable]
	public class OperationPlannerPanel : ControlPanelItem, IPanelTarget
	{
		private MovementPathRenderer pathRenderPrefab;
		private OperationObject selectOperation;
		private const string infoMessage = @"표시된 거점을 우클릭하여 이동 목적지를 설정 할 수 있습니다.
shift를 누르고 선택하면 경로를 지정할 수 있습니다.";

		private MovePath movementPlan;
		public OperationPlannerPanel(Transform root, MovementPathRenderer pathRenderPrefab, StrategyControlPanelUI panelUI, Action onClose) : base(null, root, panelUI, onClose)
		{
			selectOperation = null;
			this.pathRenderPrefab = pathRenderPrefab;
			movementPlan = null;
		}

		protected override void OnDispose()
		{
			selectOperation = null;
			pathRenderPrefab = null;
			if (movementPlan != null)
			{
				movementPlan.Dispose();
			}
		}
		protected override void OnHide()
		{
			StrategyManager.PopupUI.HideTopMessage(this);
			StrategyManager.Selecter.RemoveListener_OnPointingTarget(OnPointing);
			RemoveTarget(selectOperation);
		}
		protected override void OnShow()
		{
			StrategyManager.PopupUI.ShowTopMessage(this, infoMessage);
			StrategyManager.Selecter.AddListener_OnPointingTarget(OnPointing);
		}
		private void OnPointing(ISelectable selectable)
		{
			if (selectable == null || selectable is not SectorObject sector) return;

			bool clearPath = !Keyboard.current.shiftKey.isPressed;
			selectOperation.ThisMovement.SetMovePath(clearPath, sector);
		}
		void IPanelTarget.AddTarget(IStrategyElement element)
		{
			if (element is not OperationObject operation) return;
			AddTarget(operation);
		}
		void IPanelTarget.RemoveTarget(IStrategyElement element)
		{
			if (element is not OperationObject operation) return;
			RemoveTarget(operation);
		}
		void IPanelTarget.ClearTarget()
		{
			selectOperation = null;
			if (movementPlan != null)
				movementPlan.Dispose();
			movementPlan = null;
		}
		void AddTarget(OperationObject operation)
		{
			if (operation == null) return;
			if (selectOperation != operation)
			{
				if (selectOperation != null)
				{
					RemoveTarget(selectOperation);
				}

				selectOperation = operation;
				selectOperation.ThisMovement.OnChangeMovePath += OnChangeMovePath;
				selectOperation.ThisMovement.OnChangeMoveProgress += OnChangeMoveProgress;
				OnChangeMovePath();
				float progress = 1f - selectOperation.ThisMovement.TotalLength / selectOperation.ThisMovement.InitLength;
				OnChangeMoveProgress(progress);
			}
		}
		void RemoveTarget(OperationObject operation)
		{
			if (operation == null) return;
			if (selectOperation == operation)
			{
				selectOperation.ThisMovement.OnChangeMovePath -= OnChangeMovePath;
				selectOperation.ThisMovement.OnChangeMoveProgress -= OnChangeMoveProgress;
				selectOperation = null;
				OnChangeMovePath();
			}
		}
		private void OnChangeMovePath()
		{
			if (selectOperation == null)
			{
				if (movementPlan != null)
				{
					movementPlan.Dispose();
					movementPlan = null;
				}
				return;
			}

			Vector3[] planList = selectOperation.ThisMovement.InitPath;
			if (planList.Length == 0)
			{
				if (movementPlan != null)
				{
					movementPlan.Dispose();
					movementPlan = null;
				}
				return;
			}

			if (movementPlan == null)
			{
				movementPlan = new MovePath(planList, pathRenderPrefab, this);
			}
			else
			{
				movementPlan.ChangeValue(planList);
			}
		}
		private void OnChangeMoveProgress(float progress)
		{
			if (movementPlan == null) return;
			movementPlan.OnChangeProgress(progress);
		}
		protected class MovePath : ViewItem<Vector3[]>
		{
			private MovementPathRenderer pathRenderer;
			public MovePath(Vector3[] item, MovementPathRenderer pathRenderPrefab, ControlPanelItem panel) : base(item, panel)
			{
				if (pathRenderPrefab != null)
				{
					pathRenderer = GameObject.Instantiate(pathRenderPrefab);
				}
				ChangeValue(Value);
			}
			protected override void OnDispose()
			{
				if (pathRenderer != null)
				{
					GameObject.Destroy(pathRenderer.gameObject);
					pathRenderer = null;
				}
			}
			protected override void OnBeforeChangeValue()
			{
				if (pathRenderer != null)
					pathRenderer.ClearMovementPlan();
			}
			protected override void OnAfterChangeValue()
			{
				if (pathRenderer != null)
					pathRenderer.SetMovementPlan(Value);
			}
			protected override void OnVisible()
			{
				if (pathRenderer != null)
					pathRenderer.gameObject.SetActive(true);
			}
			protected override void OnInvisible()
			{
				if (pathRenderer != null)
					pathRenderer.gameObject.SetActive(false);
			}
			public void OnChangeProgress(float progress)
			{
				if (pathRenderer != null)
					pathRenderer.SetProgress(progress);

			}
		}
	}
}