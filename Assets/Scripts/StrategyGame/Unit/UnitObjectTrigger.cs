using System.Collections.Generic;

using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class UnitObjectTrigger : MonoBehaviour
{
	private UnitObject thisUnit;
	private HashSet<Collider> colliderList;

	private List<SectorObject> enterSectorList;
	private List<UnitObject> closedUnitList;
	private List<SkillObject> enterSkillList;

	public HashSet<Collider> ColliderList => colliderList;
	public List<SectorObject> EnterSectorList => enterSectorList;
	public List<UnitObject> ClosedUnitList => closedUnitList;
	public List<SkillObject> EnterSkillList => enterSkillList;


	private void Awake()
    {
		thisUnit = GetComponentInParent<UnitObject>();
        
		colliderList = new HashSet<Collider>();
		enterSectorList = new List<SectorObject>();
		closedUnitList = new List<UnitObject>();
		enterSkillList = new List<SkillObject>();
	}
    private void OnDestroy()
    {
		thisUnit = null;
		
		ClearList(colliderList);
		ClearList(enterSectorList);
		ClearList(closedUnitList);
		ClearList(enterSkillList);

		colliderList = null;
		enterSectorList = null;
		closedUnitList = null;
		enterSkillList = null;

		void ClearList<T>(ICollection<T> list)
		{
			if (list == null) return;
			list.Clear();
		}
	}

    public void OnTriggerEnter(Collider other)
	{
		if (colliderList.Add(other))
		{
			IStrategyElement element = other.gameObject.GetComponentInParent<IStrategyElement>();
			_ = element switch
			{
				SectorObject item => OnEnter(item),
				UnitObject item => OnEnter(item),
				SkillObject item => OnEnter(item),
				_ => OnEnter(element)
			};
		}
	}
    public void OnTriggerExit(Collider other)
	{
		if (colliderList.Remove(other))
		{
			IStrategyElement element = other.gameObject.GetComponentInParent<IStrategyElement>();
			_ = element switch
			{
				SectorObject item => OnExit(item),
				UnitObject item => OnExit(item),
				SkillObject item => OnExit(item),
				_ => OnExit(element)
			};
		}
    }

    public bool OnEnter(SectorObject sector)
	{
		enterSectorList.Add(sector);
		return true;
	}
	public bool OnEnter(UnitObject unit)
	{

		return true;
	}
	public bool OnEnter(SkillObject skill)
	{
		return true;
	}
	public bool OnEnter(IStrategyElement other)
	{
		return true;
	}

	public bool OnExit(SectorObject sector)
	{
		return enterSectorList.Remove(sector);
	}
	public bool OnExit(UnitObject unit)
	{
		return true;
	}
	public bool OnExit(SkillObject skill)
	{
		return true;
	}
	public bool OnExit(IStrategyElement other)
	{
		return true;
	}
}
