using Sirenix.OdinInspector;

using UnityEngine;

public partial class UnitObject : IOperationBelonger
{
	[HideInEditorMode, FoldoutGroup("Operation", VisibleIf = "HasOperation"), InlineProperty, HideLabel]
	public OperationObject operationObject;
	public int OperationID => operationObject == null ? -1 : operationObject.OperationID;
	public bool HasOperation => OperationID >= 0;

	public Vector3 operationOffset;

	partial void InitOperationObject()
	{
		operationObject = null;
	}
	void IOperationBelonger.SetOperationBelong(OperationObject operationObject)
	{
		if (operationObject == null) return;
		this.operationObject = operationObject;

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
		operationOffset = ThisMovement.CurrentPosition - operationObject.ThisMovement.CurrentPosition;
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
		operationOffset = Vector3.zero;
	}
}
