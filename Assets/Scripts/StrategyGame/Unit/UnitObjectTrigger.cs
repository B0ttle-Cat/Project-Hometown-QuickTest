using System.Collections.Generic;

using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class UnitObjectTrigger : MonoBehaviour
{
	private UnitObject thisUnit;
	private HashSet<Collider> colliderList;

	private List<ControlBase> enterControlBaseList;
	private List<UnitObject> closedUnitList;
	private List<SkillObject> enterSkillList;

	public HashSet<Collider> ColliderList => colliderList;
	public List<ControlBase> EnterControlBaseList => enterControlBaseList;
	public List<UnitObject> ClosedUnitList => closedUnitList;
	public List<SkillObject> EnterSkillList => enterSkillList;


	private void Awake()
    {
		thisUnit = GetComponentInParent<UnitObject>();
        
		colliderList = new HashSet<Collider>();
		enterControlBaseList = new List<ControlBase>();
		closedUnitList = new List<UnitObject>();
		enterSkillList = new List<SkillObject>();
	}
    private void OnDestroy()
    {
		thisUnit = null;
		
		ClearList(colliderList);
		ClearList(enterControlBaseList);
		ClearList(closedUnitList);
		ClearList(enterSkillList);

		colliderList = null;
		enterControlBaseList = null;
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
				ControlBase item => OnEnter(item),
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
				ControlBase item => OnExit(item),
				UnitObject item => OnExit(item),
				SkillObject item => OnExit(item),
				_ => OnExit(element)
			};
		}
    }

    public bool OnEnter(ControlBase cb)
	{
		enterControlBaseList.Add(cb);
		return true;
	}
	public bool OnEnter(UnitObject cb)
	{

		return true;
	}
	public bool OnEnter(SkillObject cb)
	{
		return true;
	}
	public bool OnEnter(IStrategyElement cb)
	{
		return true;
	}

	public bool OnExit(ControlBase cb)
	{
		return enterControlBaseList.Remove(cb);
	}
	public bool OnExit(UnitObject cb)
	{
		return true;
	}
	public bool OnExit(SkillObject cb)
	{
		return true;
	}
	public bool OnExit(IStrategyElement cb)
	{
		return true;
	}
}
