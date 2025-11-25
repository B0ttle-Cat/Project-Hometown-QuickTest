using Sirenix.OdinInspector;

using UnityEngine;

public abstract class DataGetterSetter<T> : ScriptableObject
{
	[Space, SerializeField, InlineProperty, HideLabel]
	protected T data;
	public T GetData() => data;
	public ref T RefData() => ref data;
	public ref readonly T ReadonlyData() => ref data;
	public virtual void SetData(T data) { this.data = data; }
}
