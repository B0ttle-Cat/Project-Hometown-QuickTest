using UnityEngine;

[RequireComponent(typeof(UnitMovement))]
public partial class UnitObject // Movement
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
		if (unitMovement != null) unitMovement.Deinit();
		OnChangeCurrentCombatTarget -= OnNewCombatMovementPath;
	}
	private void OnNewCombatMovementPath(ITargetableCombatant target)
	{
		unitMovement.OnNewCombatMovementPath(target);
	}
}
public partial class UnitObject
{
	public IMovement ThisMovement
	{
		get
		{
			if (unitMovement == null)
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
			if (unitMovement == null)
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

}