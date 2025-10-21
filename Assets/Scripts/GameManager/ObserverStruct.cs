using System;
[Serializable]
public abstract class ObserverValue : IDisposable
{
	public abstract void Invoke();
	public abstract void AddListener_ToString(Action<string> listener);
	public abstract void RemoveListener_ToString(Action<string> listener);
	public abstract void RemoveAllListener();
	public abstract void Dispose();
}

[Serializable]
public class ObserverValue<T> : ObserverValue
{
	protected T value;

	public ObserverValue(T value)
	{
		this.value = value;
	}

	public virtual T Value
	{
		get => value; set
		{
			if (IsEquals(value)) return;
			this.value = value;
			Invoke();
		}
	}
	private event Action<T> onValueChanged;
	private event Action<T> onLateValueChanged;
	private event Action<string> toStringChanged;
	public override void Invoke()
	{
		if (onValueChanged != null) onValueChanged.Invoke(value);
		if(onLateValueChanged != null) onLateValueChanged.Invoke(value);
		if (toStringChanged != null) toStringChanged.Invoke(ToString());
	}
	public virtual bool IsEquals(T value)
	{
		return this.value.Equals(value);
	}

	public virtual void AddListener(Action<T> listener)
	{
		if (listener == null) return;
		onValueChanged -= listener;
		onValueChanged += listener;
	}
	public virtual void AddLateListener(Action<T> listener)
	{
		if (listener == null) return;
		onLateValueChanged -= listener;
		onLateValueChanged += listener;
	}
	public virtual void RemoveListener(Action<T> listener)
	{
		if (listener == null) return;
		onValueChanged -= listener;
		onLateValueChanged -= listener;
	}
	public override void AddListener_ToString(Action<string> listener)
	{
		if (listener == null) return;
		toStringChanged -= listener;
		toStringChanged += listener;
	}
	public override void RemoveListener_ToString(Action<string> listener)
	{
		if (listener == null) return;
		toStringChanged -= listener;
	}

	public override void RemoveAllListener()
	{
		onValueChanged = null;
		onLateValueChanged = null;
		toStringChanged = null;
	}
	public override void Dispose()
	{
		onValueChanged = null;
		toStringChanged = null;
		try
		{
			((IDisposable)value).Dispose();
		}
		catch { }
	}
	public override string ToString()
	{
		return value.ToString();
	}
}
[Serializable]
public class ObserverStruct<T> : ObserverValue<T> where T : struct
{
    public ObserverStruct(T value) : base(value){}
}
public class ObserverString : ObserverValue<string>
{
    public ObserverString(string value) : base(value)
    {
    }
}
public class ObserverInt : ObserverStruct<int>
{
	public ObserverInt(int value) : base(value)
	{
	}
}
public class ObserverFloat : ObserverStruct<float>
{
	public ObserverFloat(float value) : base(value)
	{
	}
}   