using UnityEngine;

public interface IOperationBelonger
{
	bool HasOperation { get; }
	Vector3 OperationOffset { get; }
	void SetOperationBelong(OperationObject operationObject);
	OperationObject GetBelongedOperation();
	void RelaseOperationBelong();
}
