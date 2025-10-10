using UnityEngine;

public abstract class DataGetterSetter<T> : ScriptableObject
{
	protected abstract T _data { get; set; }
	public virtual T GetData() => _data;
	public virtual void SetData(T data) { _data = data; }
}
