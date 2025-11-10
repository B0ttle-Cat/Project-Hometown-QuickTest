using Sirenix.OdinInspector;

using UnityEngine;

public partial class StrategyMapPanelUI // OperationLabelGroup
{
	[SerializeField, FoldoutGroup("OperationLabel")]
	private GameObject operationLabelPreafab;
	[SerializeField, FoldoutGroup("OperationLabel")]
	private Transform operationLabelRoot;
	[SerializeField, FoldoutGroup("OperationLabel"), InlineProperty, HideLabel]
	private OperationLabelGroup operationLabelGroup;

	private void ShowOperationLabelGroup()
	{
		operationLabelGroup = new OperationLabelGroup(operationLabelPreafab, operationLabelRoot, this);
		operationLabelGroup.Show();
	}
	private void HideOperationLabelGroup()
	{
		if (operationLabelGroup == null) return;
		operationLabelGroup.Hide();
		operationLabelGroup.Dispose();
		operationLabelGroup = null;
	}
	private void OperationLabelGroupUpdate()
	{
		if (operationLabelGroup == null) return;
		operationLabelGroup.Update();
	}

	public class OperationLabelGroup : MapPanelUI
	{
		public OperationLabelGroup(GameObject preafab, Transform root,StrategyMapPanelUI panel) : base(preafab, root, panel)
		{
		}

		protected override void OnDispose()
		{
		}

		protected override void OnHide()
		{
		}

		protected override void OnShow()
		{
		}
	}
}