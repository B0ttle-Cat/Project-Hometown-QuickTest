using System;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;

using static StrategyGamePlayData;

public partial class TroopsObject : MonoBehaviour
{
	[SerializeField]
	private int troopsID;
	[SerializeField]
	private int factionID;
	[ShowInInspector]
	private  Dictionary<UnitKey, OrganizationCounter> organizationPlan;

	[Serializable]
	public struct OrganizationCounter
	{
		public readonly UnitKey UnitKey;   // 편제된 유닛 타입
		public int planCount;              // 편제된 계획
		public int currCount;              // 편제된 현제 수
		public readonly int PlanCount => planCount;
		public readonly int CurrCount => currCount;
		public OrganizationCounter(UnitKey unitKey, int planCount, int currCount)
		{
			this.UnitKey = unitKey;
			this.planCount = planCount;
			this.currCount = currCount;
		}
		public void SetCurrCount(int count)
		{
			this.currCount = count;
		}
		public void SetPlanCount(int count)
		{
			planCount = count;
		}
		public void SetCount(int planCount, int currCount)
		{
			this.planCount = planCount;
			this.currCount = currCount;
		}
		public static OrganizationCounter operator +(OrganizationCounter a, OrganizationCounter b)
		{
			return new OrganizationCounter(a.UnitKey,
				a.PlanCount + b.PlanCount,
				a.CurrCount + b.CurrCount);
		}
		public static OrganizationCounter operator -(OrganizationCounter a, OrganizationCounter b)
		{
			return new OrganizationCounter(a.UnitKey,
				a.PlanCount - b.PlanCount,
				a.CurrCount - b.CurrCount);
		}
	}
	public void Init(in ISectorController.SpawnTroopsInfo troopsInfo)
	{
		factionID = troopsInfo.factionID;
		organizationPlan = new Dictionary<UnitKey, OrganizationCounter>();
		var organizations  = troopsInfo.organizations;
		int length = organizations.Length;
		for (int i = 0 ; i < length ; i++)
		{
			(UnitKey key, int plan, int count) = organizations[i];
			if (key == UnitKey.None || plan <= 0) continue;

			UpdateOrganizationPlan(in key, plan, count, false);
		}
	}
	private void UpdateOrganizationPlan(in UnitKey key, int plan, int count, bool cleanPlan = true)
	{
		OrganizationCounter organizationCounter = new OrganizationCounter(key,plan,count);
		if (!cleanPlan && organizationPlan.TryGetValue(key, out var counter))
		{
			organizationCounter += counter;
		}
		organizationPlan[key] = organizationCounter;
	}
	public bool HasPlan(in UnitKey unitKey)
	{
		return organizationPlan.ContainsKey(unitKey);
	}
}

public partial class TroopsObject : IStrategyElement
{
	public IStrategyElement ThisElement => this;
	bool IStrategyElement.IsInCollector { get; set; }
	public int ID { get => troopsID; set => troopsID = value; }
	void IStrategyElement.InStrategyCollector()
	{
	}
	void IStrategyElement.OutStrategyCollector()
	{
	}
	void IStrategyStartGame.OnStartGame()
	{
	}
	void IStrategyStartGame.OnStopGame()
	{
	}
}