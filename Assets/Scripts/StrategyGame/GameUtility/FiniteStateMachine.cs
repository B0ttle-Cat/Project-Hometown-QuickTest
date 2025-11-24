using System;
using System.Collections.Generic;

using UnityEngine;

public interface IFSMController<T> where T : Enum
{
	GameObject gameObject { get; }
	IFSMController<T> FSMController { get; }
	IFSMInterface<T> FSMInterface { get; set; }
	IFSMInterface<T> GetFSM()
	{
		return FSMInterface ??= gameObject.GetComponent<FiniteStateMachine<T>>();
	}

	public T CurrentStateType => GetFSM().CurrentStateType;
	void InitState(Action<T> OnStateEnterCallback, Action<T> OnStateExitCallback, T initState, params IState<T>[] state)
		=> GetFSM().InitState(OnStateEnterCallback, OnStateExitCallback, initState, state);
	IState<T>[] GetStateList() => GetFSM().GetStateList();
	void DeinitState() => GetFSM().DeinitState();
	void ForceChangeImmediate(T nextState) => GetFSM().ForceChangeImmediate(nextState);
}
public interface IFSMInterface<T> where T : Enum
{
	public T CurrentStateType { get; }
	void InitState(Action<T> onStateEnterCallback, Action<T> onStateExitCallback, T initState, params IState<T>[] state);
	IState<T>[] GetStateList();
	void DeinitState();
	void ForceChangeImmediate(T nextState);
}
public interface IFSMUpdater
{
	void StateUpdate(in float deltaTime);
}
public interface IState<T> where T : Enum
{
	T ThisType { get; }
	public void StateEnter();
	public void StateExit();
	public T StateUpdate(in float deltaTime);
}

public abstract class FiniteStateMachine<T> : MonoBehaviour, IFSMInterface<T>, IFSMUpdater where T : Enum
{
	private IState<T> currentState;
	public T CurrentStateType => currentState.ThisType;

	public Dictionary<T, IState<T>> stateList;
	private Action<T> onStateEnterCallback;
	private Action<T> onStateExitCallback;
	protected virtual async void Awake()
	{
		while (StrategyManager.IsNotReadyManager)
		{
			await Awaitable.NextFrameAsync();
			if (destroyCancellationToken.IsCancellationRequested) return;
		}
		StrategyManager.Collector.AddOther<IFSMUpdater>(this);
	}
	protected virtual void OnDestroy()
	{
		StrategyManager.Collector.RemoveOther<IFSMUpdater>(this);
	}
	public void InitState(Action<T> onStateEnterCallback, Action<T> onStateExitCallback, T initState, params IState<T>[] state)
	{
		this.onStateEnterCallback += onStateEnterCallback;
		this.onStateExitCallback += onStateExitCallback;
		stateList = new Dictionary<T, IState<T>>();

		int length = state == null ? 0 : state.Length;
		for (int i = 0 ; i < length ; i++)
		{
			if (stateList.ContainsKey(state[i].ThisType)) continue;
			stateList.Add(state[i].ThisType, state[i]);
		}
		currentState = null;
		ForceChangeImmediate(initState);
	}
	public abstract IState<T>[] GetStateList();
	public void DeinitState()
	{
		onStateEnterCallback = null;
		onStateExitCallback = null;

		if (stateList != null)
		{
			foreach (var item in stateList)
			{
				if (item.Value is IDisposable disposable) disposable.Dispose();
			}
		}
		currentState = null;
	}
	public void ForceChangeImmediate(T nextState)
	{
		if (stateList == null) return;

		if (currentState != null)
		{
			onStateExitCallback?.Invoke(currentState.ThisType);
			currentState.StateExit();
		}
		currentState = stateList.TryGetValue(nextState, out var iState) ? iState : null;
		if (currentState != null)
		{
			currentState?.StateEnter();
			onStateEnterCallback?.Invoke(nextState);
		}
	}
	public void StateUpdate(in float deltaTime)
	{
		if (currentState == null || stateList == null) return;

		T prevState = currentState.ThisType;
		T nextState = currentState.StateUpdate(in deltaTime);
		if (!prevState.Equals(nextState))
		{
			ForceChangeImmediate(nextState);
		}
	}
	public abstract class BaseState : IState<T>, IDisposable
	{
		private readonly FiniteStateMachine<T> fsm;
		private readonly T type;
		private bool isAwake;
		private bool isStart;
		public FiniteStateMachine<T> Fsm => fsm;
		public T ThisType => type;

		public BaseState(FiniteStateMachine<T> fsm, T type)
		{
			this.fsm = fsm;
			this.type = type;
			isAwake = false;
			isStart = false;
		}
		public void Dispose()
		{
			OnDispose();
		}
		public void StateEnter()
		{
			StateAwake();
			OnStateEnter();
		}
		public void StateExit()
		{
			OnStateExit();
		}
		public T StateUpdate(in float deltaTime)
		{
			StateStart();
			return OnStateUpdate(in deltaTime);
		}
		private void StateAwake()
		{
			if (isAwake) return;
			isAwake = true;
			OnStateAwake();
		}
		private void StateStart()
		{
			if (isStart) return;
			isStart = true;
			OnStateStart();
		}
		protected abstract void OnDispose();
		protected abstract void OnStateAwake();
		protected abstract void OnStateEnter();
		protected abstract void OnStateExit();

		protected abstract void OnStateStart();
		protected abstract T OnStateUpdate(in float deltaTime);
	}
	public class EmptyState : BaseState
	{
		public EmptyState(FiniteStateMachine<T> fsm, T type) : base(fsm, type)
		{
		}

		protected override void OnDispose()
		{
		}
		protected override void OnStateAwake()
		{
		}

		protected override void OnStateEnter()
		{
		}

		protected override void OnStateExit()
		{
		}

		protected override void OnStateStart()
		{
		}

		protected override T OnStateUpdate(in float deltaTime)
		{
			return ThisType;
		}
	}
}
