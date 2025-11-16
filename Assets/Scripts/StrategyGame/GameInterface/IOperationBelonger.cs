public interface IOperationBelonger
{
	void SetOperationBelong(OperationObject operationObject);
	OperationObject GetBelongedOperation();
	void RelaseOperationBelong();
}
