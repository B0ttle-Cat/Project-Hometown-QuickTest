public abstract class Command 
{
	public Squad order;

	public abstract void Cancel(params object[] args);
    public abstract void Execute(params object[] args);
}
