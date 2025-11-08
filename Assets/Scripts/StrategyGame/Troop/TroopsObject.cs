using System.Collections.Generic;

using UnityEngine;

using static StrategyGamePlayData;

public partial class TroopsObject : MonoBehaviour
{
	[SerializeField]
	private int troopsID;
	[SerializeField]
	private int factionID;
	[SerializeField]
	private  Dictionary<UnitKey, Organization> unitList;

    internal Transform influencer;
    internal Transform followers;

	private struct Organization
	{
		private readonly UnitKey unitKey;
		private readonly int unitID;
		private int count;

		public Organization(UnitKey unitKey, int unitID, int count)
		{
			this.unitKey = unitKey;
			this.unitID = unitID;
			this.count = count;
		}
		public void AddCount(int change)
		{
			count += change;
			if (count < 0) count = 0;
		}
		public void RemoveCount(int change)
		{
			count -= change;
			if (count < 0) count = 0;
		}
		public void SetCount(int count)
		{
			if (count < 0) count = 0;
			this.count = count;
		}
		public readonly UnitKey UnitKey => unitKey;
		public readonly int UnitID => unitID;
		public readonly int Count => count;
		public readonly UnitObject UnitObject => StrategyManager.Collector.FindUnit(unitID);
	}

	public void Init(in ISectorController.SpawnTroopsInfo troopsInfo)
	{
		factionID = troopsInfo.factionID;
		unitList = new Dictionary<UnitKey, Organization>();
		(StrategyGamePlayData.UnitKey key, int count)[] organizations  = troopsInfo.organizations;
		int length = organizations.Length;
		Transform parent = influencer;
		for (int i = 0 ; i < length ; i++)
		{
			(UnitKey key, int count) = organizations[i];
			if (key == UnitKey.None || count <= 0) continue;

			if(CreateChildUnitObject(in factionID, in key, in count, parent))
			{
				parent = followers;
			}
		}
	}

	public bool CreateChildUnitObject(in int factionID, in UnitKey key, in int count, in Transform parent)
	{
		if (unitList.TryGetValue(key, out var unit))
		{
			unit.AddCount(count);
		}
		else
		{
			var unitObject = StrategyElementUtility.Instantiate(key, factionID, parent);
			if (unitObject == null) return false;

			unit = new Organization(key, unitObject.UnitID, count);
		}
		unitList[key] = unit;
		return true;
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
	void IStartGame.OnStartGame()
	{
	}
	void IStartGame.OnStopGame()
	{
	}

}
