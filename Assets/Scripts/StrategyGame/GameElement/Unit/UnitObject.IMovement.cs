using System;
using System.Collections.Generic;

using Pathfinding;

using UnityEngine;

[RequireComponent(typeof(UnitMovement))]
public partial class UnitObject: IMovement, INodeMovement, INavMovement
{
	UnitMovement unitMovement;
	partial void InitMovement()
	{
		if (unitMovement == null)
		{
			unitMovement = GetComponent<UnitMovement>();
			unitMovement.Init(this);
		}

		OnChangeCurrentCombatTarget -= OnNewCombatMovementPath;
		OnChangeCurrentCombatTarget += OnNewCombatMovementPath;
	}
	partial void DeinitMovement()
	{
		if (unitMovement != null)
		{
			unitMovement.Deinit();
			unitMovement = null;
		}
		OnChangeCurrentCombatTarget -= OnNewCombatMovementPath;
	}
	private void OnNewCombatMovementPath(ITargetableCombatant target)
	{
		unitMovement.OnNewCombatMovementPath(target);
	}

	public IMovement ThisMovement
	{
		get
		{
			if (unitMovement.IsNullRef())
			{
				unitMovement = GetComponent<UnitMovement>();
				unitMovement.Init(this);
			}
			return unitMovement.ThisMovement;
		}
	}
	public INodeMovement ThisNodeMovement
	{
		get
		{
			if (unitMovement.IsNullRef())
			{
				unitMovement = GetComponent<UnitMovement>();
				unitMovement.Init(this);
			}
			return unitMovement.ThisNodeMovement;
		}
	}
	public INodeMovement ParentMovement => operationObject;
	public INavMovement ThisNavMovement
	{
		get
		{
			if (unitMovement == null)
			{
				unitMovement = GetComponent<UnitMovement>();
				unitMovement.Init(this);
			}
			return unitMovement.ThisNavMovement;
		}
	}

	#region IMovement
	public Seeker ThisSeeker => ThisMovement.ThisSeeker;

	public Vector3 CurrentPosition => ThisMovement.CurrentPosition;

	public Vector3 CurrentVelocity => ThisMovement.CurrentVelocity;

	public float CurrentRadius => ThisMovement.CurrentRadius;

	public Vector3 NextMovePosition { get => ThisMovement.NextMovePosition; set => ThisMovement.NextMovePosition = value; }

	public float SmoothTime => ThisMovement.SmoothTime;

	public float MaxSpeed => ThisMovement.MaxSpeed;

	public int MovementIndex { get => ThisMovement.MovementIndex; set => ThisMovement.MovementIndex = value; }
	public Vector3[] InitPath { get => ThisMovement.InitPath; set => ThisMovement.InitPath = value; }
	public List<Vector3> MovePath { get => ThisMovement.MovePath; set => ThisMovement.MovePath = value; }
	public List<Vector3> TempMovePath { get => ThisMovement.TempMovePath; set => ThisMovement.TempMovePath = value; }
	public Queue<Vector3> FindingPoints { get => ThisMovement.FindingPoints; set => ThisMovement.FindingPoints = value; }
	public float InitLength { get => ThisMovement.InitLength; set => ThisMovement.InitLength = value; }
	public float TotalLength { get => ThisMovement.TotalLength; set => ThisMovement.TotalLength = value; }
	public float TempLength { get => ThisMovement.TempLength; set => ThisMovement.TempLength = value; }
	public Action OnStartMove { get => ThisMovement.OnStartMove; set => ThisMovement.OnStartMove = value; }
	public Action OnEndedMove { get => ThisMovement.OnEndedMove; set => ThisMovement.OnEndedMove = value; }
	public Action OnChangeMovePath { get => ThisMovement.OnChangeMovePath; set => ThisMovement.OnChangeMovePath = value; }
	public Action<float> OnChangeMoveProgress { get => ThisMovement.OnChangeMoveProgress; set => ThisMovement.OnChangeMoveProgress = value; }
	public void OnMoveStart()
	{
		ThisMovement.OnMoveStart();
	}
	public void OnMoveStop()
	{
		ThisMovement.OnMoveStop();
	}
	public bool IsMovableState()
	{
		return ThisNodeMovement.IsMovableState();
	}
	public void SetPositionAndVelocity(in Vector3 position, in Vector3 delteMove, in Vector3 velocity, in float deltaTime)
	{
		ThisNodeMovement.SetPositionAndVelocity(position, delteMove, velocity, deltaTime);
	}
	public void OnStayUpdate(in float deltaTime)
	{
		ThisNodeMovement.OnStayUpdate(deltaTime);
	}
	public bool IsChangeTargetPositionCheck()
	{
		return ThisNavMovement.IsChangeTargetPositionCheck();
	}
	#endregion
}