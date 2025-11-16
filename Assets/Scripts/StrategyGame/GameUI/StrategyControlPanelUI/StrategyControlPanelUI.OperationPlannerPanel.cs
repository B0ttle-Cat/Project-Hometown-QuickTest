using System;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;
using UnityEngine.InputSystem;
public partial class StrategyControlPanelUI
{
	[SerializeField, FoldoutGroup("OperationPlanner")]
	private GameObject operationPlannerPrefab;
	[SerializeField, FoldoutGroup("OperationPlanner")]
	private Transform operationPlannerToot;
	[SerializeField, FoldoutGroup("OperationPlanner")]
	private MovementPathRenderer pathRenderPrefab;
	[SerializeField, FoldoutGroup("OperationPlanner"), InlineProperty, HideLabel]
	private OperationPlannerPanel operationPlannerPanel;

	public IPanelTarget ShowOperationPlannerPanel()
	{
		operationPlannerPanel = new OperationPlannerPanel(operationPlannerPrefab, operationPlannerToot, pathRenderPrefab, this);
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
		private MovementPathRenderer pathRenderObject;
		private OperationObject selectOperation;
		private const string infoMessage = @"표시된 거점을 우클릭하여 이동 목적지를 설정 할 수 있습니다.
shift를 누르고 선택하면 경로를 지정할 수 있습니다.";

		private MovePath movementPlan;

		public OperationPlannerPanel(GameObject prefab, Transform root, MovementPathRenderer pathRenderPrefab, StrategyControlPanelUI panelUI) : base(prefab, root, panelUI)
		{
			selectOperation = null;
			this.pathRenderPrefab = pathRenderPrefab;
			movementPlan = null;
			StrategyManager.PopupUI.ShowTopMessage(this, infoMessage);
		}

		protected override void OnDispose()
		{
			selectOperation = null;
			pathRenderPrefab = null;
			if (movementPlan != null)
			{
				movementPlan.Dispose();
			}
			StrategyManager.PopupUI.HideTopMessage(this);
		}

		protected override void OnHide()
		{
			StrategyManager.Selecter.RemoveListener_OnPointingTarget(OnPointing);
		}


		protected override void OnShow()
		{
			StrategyManager.Selecter.AddListener_OnPointingTarget(OnPointing);
		}
		private void OnPointing(ISelectable selectable)
		{
			if (selectable == null || selectable is not SectorObject sector) return;

			bool clearPath = !Keyboard.current.shiftKey.isPressed;
			selectOperation.ThisMovement.SetMovePath(clearPath, sector);
			List<Vector3> planList = selectOperation.ThisMovement.MovePath;
			if (movementPlan == null)
			{
				movementPlan = new MovePath(planList, pathRenderPrefab, this);
			}
			else
			{
				movementPlan.ChangeValue(planList);
			}
		}
		private void OnLastDeselect(ISelectable selectable)
		{

		}


		void IPanelTarget.AddTarget(IStrategyElement element)
		{
			if (element is not OperationObject operation) return;
			if (selectOperation != operation)
			{
				selectOperation = operation;
			}
		}
		void IPanelTarget.RemoveTarget(IStrategyElement element)
		{
			if (element is not OperationObject operation) return;
			if (selectOperation == operation)
			{
				selectOperation = null;
			}
		}
		void IPanelTarget.ClearTarget()
		{
			selectOperation = null;
		}


		protected class MovePath : ViewItem<List<Vector3>>
		{
			private MovementPathRenderer pathRenderer;
			public MovePath(List<Vector3> item, MovementPathRenderer pathRenderPrefab, ControlPanelItem panel) : base(item, panel)
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
					GameObject.Destroy(pathRenderer);
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
					pathRenderer.SetMovementPlan(Value.ToArray());
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
		}
	}
}