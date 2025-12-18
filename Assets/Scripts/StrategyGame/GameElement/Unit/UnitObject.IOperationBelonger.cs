using Sirenix.OdinInspector;

using UnityEngine;

public partial class UnitObject : IOperationBelonger
{
	[HideInEditorMode, FoldoutGroup("Operation", VisibleIf = "HasOperation"), InlineProperty, HideLabel]
	[SerializeField]
	private OperationObject operationObject;
	public OperationObject Operation => operationObject;
	public int OperationID => operationObject == null ? -1 : operationObject.OperationID;
	public bool HasOperation => OperationID >= 0;
	public Vector3 OperationOffset { get; set; }
	partial void InitOperationObject()
	{
		operationObject = null;
	}
	void IOperationBelonger.SetOperationBelong(OperationObject operationObject)
	{
		if (operationObject == null) return;
		this.operationObject = operationObject;
		SetupOperationBelong();
	}
	private void SetupOperationBelong()
	{
		if (operationObject == null) return;

		ThisVisibility.OnChangeVisible -= operationObject.ChangeVisibleUnit;
		ThisVisibility.OnChangeVisible += operationObject.ChangeVisibleUnit;

		ThisVisibility.OnChangeInvisible -= operationObject.ChangeInvisibleUnit;
		ThisVisibility.OnChangeInvisible += operationObject.ChangeInvisibleUnit;
		if (ThisVisibility.IsVisible)
		{
			operationObject.ChangeVisibleUnit(this);
		}
		else
		{
			operationObject.ChangeInvisibleUnit(this);
		}

		OperationOffset = ThisMovement.CurrentPosition - operationObject.ThisMovement.CurrentPosition;
	}
    OperationObject IOperationBelonger.GetBelongedOperation()
	{
		return operationObject;
	}
	void IOperationBelonger.RelaseOperationBelong()
	{
		if (operationObject != null)
		{
			ThisVisibility.OnChangeVisible -= operationObject.ChangeVisibleUnit;
			ThisVisibility.OnChangeInvisible -= operationObject.ChangeInvisibleUnit;

			operationObject = null;
		}
		OperationOffset = Vector3.zero;
	}
}
