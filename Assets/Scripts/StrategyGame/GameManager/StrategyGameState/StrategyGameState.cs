using System;
using System.Collections.Generic;
using System.Linq;

using Sirenix.OdinInspector;

using UnityEngine;

public class StrategyGameState : MonoBehaviour, IStrategyStartGame
{
	public enum StrategyStateFlag : uint
	{
		None = 0,
		ViewAndControlModeType_OperationsMode,
		ViewAndControlModeType_TacticsMode,
	}
	private HashSet<StrategyStateFlag> CurrentStateFlag;
	public event Action OnFlagChanged;
#if UNITY_EDITOR
	[InlineButton("TestUnset")]
	[InlineButton("TestSet")]
	[ShowInInspector]
	private StrategyStateFlag testFlag { get; set; }
	[ShowInInspector, HideInEditorMode]
	private StrategyStateFlag[] ShowFlag => CurrentStateFlag == null ? null : CurrentStateFlag.ToArray();
	private void TestSet()
	{
		SetFlag(testFlag);
	}
	private void TestUnset()
	{
		UnsetFlag(testFlag);
	}
#endif

	public void Init()
	{
		CurrentStateFlag ??= new HashSet<StrategyStateFlag>();
		CurrentStateFlag.Clear();
		OnFlagChanged = null;
	}
	public void Deinit()
	{
		CurrentStateFlag?.Clear();
		CurrentStateFlag = null;
		OnFlagChanged = null;
	}


	public void SetFlag(params StrategyStateFlag[] flags)
	{
		bool change = false;
		int length = flags == null ? 0 : flags.Length;
		for (int i = 0 ; i < length ; i++)
		{
			if (CurrentStateFlag.Add(flags[i]))
			{
				change = true;
			}
		}

		if (change) OnFlagChanged?.Invoke();
	}
	public void UnsetFlag(params StrategyStateFlag[] flags)
	{
		bool change = false;
		int length = flags == null ? 0 : flags.Length;
		for (int i = 0 ; i < length ; i++)
		{
			if (CurrentStateFlag.Remove(flags[i]))
			{
				change = true;
			}
		}

		if (change) OnFlagChanged?.Invoke();
	}
	public bool HasAnyFlag(params StrategyStateFlag[] flags)
	{
		int length = flags == null ? 0 : flags.Length;
		for (int i = 0 ; i < length ; i++)
		{
			if (CurrentStateFlag.Contains(flags[i]))
			{
				return true;
			}
		}
		return false;
	}
	public bool HasAllFlag(params StrategyStateFlag[] flags)
	{
		int length = flags == null ? 0 : flags.Length;
		for (int i = 0 ; i < length ; i++)
		{
			if (!CurrentStateFlag.Contains(flags[i]))
			{
				return false;
			}
		}
		return true;
	}
	public void ClearFlag()
	{
		if (CurrentStateFlag.Count >= 1)
		{
			CurrentStateFlag.Clear();
			OnFlagChanged?.Invoke();
		}
	}
	void IStrategyStartGame.OnStartGame()
	{
	}
	void IStrategyStartGame.OnStopGame()
	{
		Deinit();
	}
}
