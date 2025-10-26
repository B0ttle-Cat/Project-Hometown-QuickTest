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
	private List<IStrategyElement> enterOtherList;

	public HashSet<Collider> ColliderList => colliderList;
	public List<SectorObject> EnterSectorList => enterSectorList;
	public List<UnitObject> ClosedUnitList => closedUnitList;
	public List<SkillObject> EnterSkillList => enterSkillList;
	public  List<IStrategyElement> EnterOtherList => enterOtherList;
	private void Awake()
    {
		thisUnit = GetComponentInParent<UnitObject>();
        
		colliderList = new HashSet<Collider>();
		enterSectorList = new List<SectorObject>();
		closedUnitList = new List<UnitObject>();
		enterSkillList = new List<SkillObject>();
		enterOtherList = new List<IStrategyElement>();
	}
    private void OnDestroy()
    {
		thisUnit = null;
		
		ClearList(colliderList);
		ClearList(enterSectorList);
		ClearList(closedUnitList);
		ClearList(enterSkillList);
		ClearList(enterOtherList);

		colliderList = null;
		enterSectorList = null;
		closedUnitList = null;
		enterSkillList = null;
		enterOtherList = null;

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

    public bool OnEnter(SectorObject item)
	{
		enterSectorList.Add(item);
		return true;
	}
	public bool OnEnter(UnitObject item)
	{
		closedUnitList.Add(item);
		return true;
	}
	public bool OnEnter(SkillObject item)
	{
		enterSkillList.Add(item);
		return true;
	}
	public bool OnEnter(IStrategyElement item)
	{
		enterOtherList.Add(item);
		return true;
	}

	public bool OnExit(SectorObject item)
	{
		return enterSectorList.Remove(item);
	}
	public bool OnExit(UnitObject item)
	{
		return closedUnitList.Remove(item);
	}
	public bool OnExit(SkillObject item)
	{
		return enterSkillList.Remove(item);
	}
	public bool OnExit(IStrategyElement item)
	{
		return enterOtherList.Remove(item);
	}
}
