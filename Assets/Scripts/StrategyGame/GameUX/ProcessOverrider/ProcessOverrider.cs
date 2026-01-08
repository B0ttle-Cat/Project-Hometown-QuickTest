using System;
using System.Collections.Generic;

using UnityEngine;

namespace StrategyManagerModule
{
	public abstract record ProcessOverrider : IDisposable
	{
		public bool isDisposed;
		readonly object Equality;
		public abstract IStrategyProcess OriginalProcess { get; }
		public ProcessOverrider(object equality)
		{
			Equality = equality;
			isDisposed = false;

			var originalProcess = OriginalProcess;
			if (originalProcess.IsNullRef())
			{
				throw new ArgumentNullException();
			}
			OriginalProcess.OnAddProcessOverride(this);
			OnOverride();
		}
		public void Dispose()
		{
			if (isDisposed) return;
			isDisposed = true;
			OriginalProcess.OnRemoveProcessOverride(this);
			OnDispose();
		}
		public void ReProcess()
		{
			isDisposed = false;
			OriginalProcess.OnAddProcessOverride(this);
			OnOverride();
		}
		protected virtual void OnOverride() { }
		protected abstract void OnDispose();
		public virtual bool Equals(ProcessOverrider overrider)
		{
			return overrider is not null &&
				   EqualityComparer<object>.Default.Equals(Equality, overrider.Equality);
		}
		public override int GetHashCode()
		{
			return System.HashCode.Combine(Equality);
		}
	}

	public abstract record ProcessOverriderAction : ProcessOverrider
	{
		private Action action;
		public ProcessOverriderAction(Action action) : base(action) { this.action = action; }
		protected override void OnDispose() => action = null;
		public void InvokeOverrider() { try { action?.Invoke(); } catch (Exception ex) { Debug.LogException(ex); Dispose(); } }
	}

	public abstract record ProcessOverriderAction<T> : ProcessOverrider
	{
		private Action<T> action;
		public ProcessOverriderAction(Action<T> action) : base(action) { this.action = action; }
		protected override void OnDispose() => action = null;
		public void InvokeOverrider(T value) { try { action?.Invoke(value); } catch (Exception ex) { Debug.LogException(ex); Dispose(); } }
	}

	public abstract record ProcessOverriderAction<T1, T2> : ProcessOverrider
	{
		private Action<T1, T2> action;
		public ProcessOverriderAction(Action<T1, T2> action) : base(action) { this.action = action; }
		protected override void OnDispose() => action = null;
		public void InvokeOverrider(T1 t1, T2 t2) { try { action?.Invoke(t1, t2); } catch (Exception ex) { Debug.LogException(ex); Dispose(); } }
	}

	public abstract record ProcessOverriderAction<T1, T2, T3> : ProcessOverrider
	{
		private Action<T1, T2, T3> action;
		public ProcessOverriderAction(Action<T1, T2, T3> action) : base(action) { this.action = action; }
		protected override void OnDispose() => action = null;
		public void InvokeOverrider(T1 t1, T2 t2, T3 t3) { try { action?.Invoke(t1, t2, t3); } catch (Exception ex) { Debug.LogException(ex); Dispose(); } }
	}

	public abstract record ProcessOverriderAction<T1, T2, T3, T4> : ProcessOverrider
	{
		private Action<T1, T2, T3, T4> action;
		public ProcessOverriderAction(Action<T1, T2, T3, T4> action) : base(action) { this.action = action; }
		protected override void OnDispose() => action = null;
		public void InvokeOverrider(T1 t1, T2 t2, T3 t3, T4 t4) { try { action?.Invoke(t1, t2, t3, t4); } catch (Exception ex) { Debug.LogException(ex); Dispose(); } }
	}

	public abstract record ProcessOverriderAction<T1, T2, T3, T4, T5> : ProcessOverrider
	{
		private Action<T1, T2, T3, T4, T5> action;
		public ProcessOverriderAction(Action<T1, T2, T3, T4, T5> action) : base(action) { this.action = action; }
		protected override void OnDispose() => action = null;
		public void InvokeOverrider(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5) { try { action?.Invoke(t1, t2, t3, t4, t5); } catch (Exception ex) { Debug.LogException(ex); Dispose(); } }
	}

	public abstract record ProcessOverriderAction<T1, T2, T3, T4, T5, T6> : ProcessOverrider
	{
		private Action<T1, T2, T3, T4, T5, T6> action;
		public ProcessOverriderAction(Action<T1, T2, T3, T4, T5, T6> action) : base(action) { this.action = action; }
		protected override void OnDispose() => action = null;
		public void InvokeOverrider(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6) { try { action?.Invoke(t1, t2, t3, t4, t5, t6); } catch (Exception ex) { Debug.LogException(ex); Dispose(); } }
	}

	public abstract record ProcessOverriderAction<T1, T2, T3, T4, T5, T6, T7> : ProcessOverrider
	{
		private Action<T1, T2, T3, T4, T5, T6, T7> action;
		public ProcessOverriderAction(Action<T1, T2, T3, T4, T5, T6, T7> action) : base(action) { this.action = action; }
		protected override void OnDispose() => action = null;
		public void InvokeOverrider(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7) { try { action?.Invoke(t1, t2, t3, t4, t5, t6, t7); } catch (Exception ex) { Debug.LogException(ex); Dispose(); } }
	}

	public abstract record ProcessOverriderAction<T1, T2, T3, T4, T5, T6, T7, T8> : ProcessOverrider
	{
		private Action<T1, T2, T3, T4, T5, T6, T7, T8> action;
		public ProcessOverriderAction(Action<T1, T2, T3, T4, T5, T6, T7, T8> action) : base(action) { this.action = action; }
		protected override void OnDispose() => action = null;
		public void InvokeOverrider(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8) { try { action?.Invoke(t1, t2, t3, t4, t5, t6, t7, t8); } catch (Exception ex) { Debug.LogException(ex); Dispose(); } }
	}

	public abstract record ProcessOverriderAction<T1, T2, T3, T4, T5, T6, T7, T8, T9> : ProcessOverrider
	{
		private Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action;
		public ProcessOverriderAction(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action) : base(action) { this.action = action; }
		protected override void OnDispose() => action = null;
		public void InvokeOverrider(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9) { try { action?.Invoke(t1, t2, t3, t4, t5, t6, t7, t8, t9); } catch (Exception ex) { Debug.LogException(ex); Dispose(); } }
	}

	public abstract record ProcessOverriderAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : ProcessOverrider
	{
		private Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action;
		public ProcessOverriderAction(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action) : base(action) { this.action = action; }
		protected override void OnDispose() => action = null;
		public void InvokeOverrider(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10) { try { action?.Invoke(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10); } catch (Exception ex) { Debug.LogException(ex); Dispose(); } }
	}

	public abstract record ProcessOverriderAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : ProcessOverrider
	{
		private Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> action;
		public ProcessOverriderAction(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> action) : base(action) { this.action = action; }
		protected override void OnDispose() => action = null;
		public void InvokeOverrider(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11) { try { action?.Invoke(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11); } catch (Exception ex) { Debug.LogException(ex); Dispose(); } }
	}

	public abstract record ProcessOverriderAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : ProcessOverrider
	{
		private Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> action;
		public ProcessOverriderAction(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> action) : base(action) { this.action = action; }
		protected override void OnDispose() => action = null;
		public void InvokeOverrider(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12) { try { action?.Invoke(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12); } catch (Exception ex) { Debug.LogException(ex); Dispose(); } }
	}

	public abstract record ProcessOverriderAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> : ProcessOverrider
	{
		private Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> action;
		public ProcessOverriderAction(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> action) : base(action) { this.action = action; }
		protected override void OnDispose() => action = null;
		public void InvokeOverrider(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, T13 t13) { try { action?.Invoke(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13); } catch (Exception ex) { Debug.LogException(ex); Dispose(); } }
	}

	public abstract record ProcessOverriderAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> : ProcessOverrider
	{
		private Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> action;
		public ProcessOverriderAction(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> action) : base(action) { this.action = action; }
		protected override void OnDispose() => action = null;
		public void InvokeOverrider(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, T13 t13, T14 t14) { try { action?.Invoke(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14); } catch (Exception ex) { Debug.LogException(ex); Dispose(); } }
	}

	public abstract record ProcessOverriderAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> : ProcessOverrider
	{
		private Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> action;
		public ProcessOverriderAction(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> action) : base(action) { this.action = action; }
		protected override void OnDispose() => action = null;
		public void InvokeOverrider(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, T13 t13, T14 t14, T15 t15) { try { action?.Invoke(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14, t15); } catch (Exception ex) { Debug.LogException(ex); Dispose(); } }
	}

	public abstract record ProcessOverriderAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> : ProcessOverrider
	{
		private Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> action;
		public ProcessOverriderAction(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> action) : base(action) { this.action = action; }
		protected override void OnDispose() => action = null;
		public void InvokeOverrider(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, T13 t13, T14 t14, T15 t15, T16 t16) { try { action?.Invoke(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14, t15, t16); } catch (Exception ex) { Debug.LogException(ex); Dispose(); } }
	}
	public abstract record ProcessOverriderFunc<TResult> : ProcessOverrider
	{
		private Func<TResult> action;
		private Action<TResult> result;
		public ProcessOverriderFunc(Func<TResult> action, Action<TResult> result) : base(action) { this.action = action; this.result = result; }
		protected override void OnDispose() { action = null; result = null; }
		public void InvokeOverrider() { try { if (action != null) result?.Invoke(action.Invoke()); } catch (Exception ex) { Debug.LogException(ex); Dispose(); } }
	}

	public abstract record ProcessOverriderFunc<T1, TResult> : ProcessOverrider
	{
		private Func<T1, TResult> action;
		private Action<TResult> result;
		public ProcessOverriderFunc(Func<T1, TResult> action, Action<TResult> result) : base(action) { this.action = action; this.result = result; }
		protected override void OnDispose() { action = null; result = null; }
		public void InvokeOverrider(T1 t1) { try { if (action != null) result?.Invoke(action.Invoke(t1)); } catch (Exception ex) { Debug.LogException(ex); Dispose(); } }
	}

	public abstract record ProcessOverriderFunc<T1, T2, TResult> : ProcessOverrider
	{
		private Func<T1, T2, TResult> action;
		private Action<TResult> result;
		public ProcessOverriderFunc(Func<T1, T2, TResult> action, Action<TResult> result) : base(action) { this.action = action; this.result = result; }
		protected override void OnDispose() { action = null; result = null; }
		public void InvokeOverrider(T1 t1, T2 t2) { try { if (action != null) result?.Invoke(action.Invoke(t1, t2)); } catch (Exception ex) { Debug.LogException(ex); Dispose(); } }
	}

	public abstract record ProcessOverriderFunc<T1, T2, T3, TResult> : ProcessOverrider
	{
		private Func<T1, T2, T3, TResult> action;
		private Action<TResult> result;
		public ProcessOverriderFunc(Func<T1, T2, T3, TResult> action, Action<TResult> result) : base(action) { this.action = action; this.result = result; }
		protected override void OnDispose() { action = null; result = null; }
		public void InvokeOverrider(T1 t1, T2 t2, T3 t3) { try { if (action != null) result?.Invoke(action.Invoke(t1, t2, t3)); } catch (Exception ex) { Debug.LogException(ex); Dispose(); } }
	}

	public abstract record ProcessOverriderFunc<T1, T2, T3, T4, TResult> : ProcessOverrider
	{
		private Func<T1, T2, T3, T4, TResult> action;
		private Action<TResult> result;
		public ProcessOverriderFunc(Func<T1, T2, T3, T4, TResult> action, Action<TResult> result) : base(action) { this.action = action; this.result = result; }
		protected override void OnDispose() { action = null; result = null; }
		public void InvokeOverrider(T1 t1, T2 t2, T3 t3, T4 t4) { try { if (action != null) result?.Invoke(action.Invoke(t1, t2, t3, t4)); } catch (Exception ex) { Debug.LogException(ex); Dispose(); } }
	}

	public abstract record ProcessOverriderFunc<T1, T2, T3, T4, T5, TResult> : ProcessOverrider
	{
		private Func<T1, T2, T3, T4, T5, TResult> action;
		private Action<TResult> result;
		public ProcessOverriderFunc(Func<T1, T2, T3, T4, T5, TResult> action, Action<TResult> result) : base(action) { this.action = action; this.result = result; }
		protected override void OnDispose() { action = null; result = null; }
		public void InvokeOverrider(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5) { try { if (action != null) result?.Invoke(action.Invoke(t1, t2, t3, t4, t5)); } catch (Exception ex) { Debug.LogException(ex); Dispose(); } }
	}

	public abstract record ProcessOverriderFunc<T1, T2, T3, T4, T5, T6, TResult> : ProcessOverrider
	{
		private Func<T1, T2, T3, T4, T5, T6, TResult> action;
		private Action<TResult> result;
		public ProcessOverriderFunc(Func<T1, T2, T3, T4, T5, T6, TResult> action, Action<TResult> result) : base(action) { this.action = action; this.result = result; }
		protected override void OnDispose() { action = null; result = null; }
		public void InvokeOverrider(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6) { try { if (action != null) result?.Invoke(action.Invoke(t1, t2, t3, t4, t5, t6)); } catch (Exception ex) { Debug.LogException(ex); Dispose(); } }
	}

	public abstract record ProcessOverriderFunc<T1, T2, T3, T4, T5, T6, T7, TResult> : ProcessOverrider
	{
		private Func<T1, T2, T3, T4, T5, T6, T7, TResult> action;
		private Action<TResult> result;
		public ProcessOverriderFunc(Func<T1, T2, T3, T4, T5, T6, T7, TResult> action, Action<TResult> result) : base(action) { this.action = action; this.result = result; }
		protected override void OnDispose() { action = null; result = null; }
		public void InvokeOverrider(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7) { try { if (action != null) result?.Invoke(action.Invoke(t1, t2, t3, t4, t5, t6, t7)); } catch (Exception ex) { Debug.LogException(ex); Dispose(); } }
	}

	public abstract record ProcessOverriderFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult> : ProcessOverrider
	{
		private Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> action;
		private Action<TResult> result;
		public ProcessOverriderFunc(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> action, Action<TResult> result) : base(action) { this.action = action; this.result = result; }
		protected override void OnDispose() { action = null; result = null; }
		public void InvokeOverrider(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8) { try { if (action != null) result?.Invoke(action.Invoke(t1, t2, t3, t4, t5, t6, t7, t8)); } catch (Exception ex) { Debug.LogException(ex); Dispose(); } }
	}

	public abstract record ProcessOverriderFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> : ProcessOverrider
	{
		private Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> action;
		private Action<TResult> result;
		public ProcessOverriderFunc(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> action, Action<TResult> result) : base(action) { this.action = action; this.result = result; }
		protected override void OnDispose() { action = null; result = null; }
		public void InvokeOverrider(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9) { try { if (action != null) result?.Invoke(action.Invoke(t1, t2, t3, t4, t5, t6, t7, t8, t9)); } catch (Exception ex) { Debug.LogException(ex); Dispose(); } }
	}

	public abstract record ProcessOverriderFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> : ProcessOverrider
	{
		private Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> action;
		private Action<TResult> result;
		public ProcessOverriderFunc(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> action, Action<TResult> result) : base(action) { this.action = action; this.result = result; }
		protected override void OnDispose() { action = null; result = null; }
		public void InvokeOverrider(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10) { try { if (action != null) result?.Invoke(action.Invoke(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10)); } catch (Exception ex) { Debug.LogException(ex); Dispose(); } }
	}

	public abstract record ProcessOverriderFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> : ProcessOverrider
	{
		private Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> action;
		private Action<TResult> result;
		public ProcessOverriderFunc(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> action, Action<TResult> result) : base(action) { this.action = action; this.result = result; }
		protected override void OnDispose() { action = null; result = null; }
		public void InvokeOverrider(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11) { try { if (action != null) result?.Invoke(action.Invoke(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11)); } catch (Exception ex) { Debug.LogException(ex); Dispose(); } }
	}

	public abstract record ProcessOverriderFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> : ProcessOverrider
	{
		private Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> action;
		private Action<TResult> result;
		public ProcessOverriderFunc(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> action, Action<TResult> result) : base(action) { this.action = action; this.result = result; }
		protected override void OnDispose() { action = null; result = null; }
		public void InvokeOverrider(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12) { try { if (action != null) result?.Invoke(action.Invoke(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12)); } catch (Exception ex) { Debug.LogException(ex); Dispose(); } }
	}

	public abstract record ProcessOverriderFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> : ProcessOverrider
	{
		private Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> action;
		private Action<TResult> result;
		public ProcessOverriderFunc(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> action, Action<TResult> result) : base(action) { this.action = action; this.result = result; }
		protected override void OnDispose() { action = null; result = null; }
		public void InvokeOverrider(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, T13 t13) { try { if (action != null) result?.Invoke(action.Invoke(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13)); } catch (Exception ex) { Debug.LogException(ex); Dispose(); } }
	}

	public abstract record ProcessOverriderFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> : ProcessOverrider
	{
		private Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> action;
		private Action<TResult> result;
		public ProcessOverriderFunc(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> action, Action<TResult> result) : base(action) { this.action = action; this.result = result; }
		protected override void OnDispose() { action = null; result = null; }
		public void InvokeOverrider(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, T13 t13, T14 t14) { try { if (action != null) result?.Invoke(action.Invoke(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14)); } catch (Exception ex) { Debug.LogException(ex); Dispose(); } }
	}

	public abstract record ProcessOverriderFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> : ProcessOverrider
	{
		private Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> action;
		private Action<TResult> result;
		public ProcessOverriderFunc(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> action, Action<TResult> result) : base(action) { this.action = action; this.result = result; }
		protected override void OnDispose() { action = null; result = null; }
		public void InvokeOverrider(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, T13 t13, T14 t14, T15 t15) { try { if (action != null) result?.Invoke(action.Invoke(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14, t15)); } catch (Exception ex) { Debug.LogException(ex); Dispose(); } }
	}

	public abstract record ProcessOverriderFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> : ProcessOverrider
	{
		private Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> action;
		private Action<TResult> result;
		public ProcessOverriderFunc(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> action, Action<TResult> result) : base(action) { this.action = action; this.result = result; }
		protected override void OnDispose() { action = null; result = null; }
		public void InvokeOverrider(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, T13 t13, T14 t14, T15 t15, T16 t16) { try { if (action != null) result?.Invoke(action.Invoke(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14, t15, t16)); } catch (Exception ex) { Debug.LogException(ex); Dispose(); } }
	}
}