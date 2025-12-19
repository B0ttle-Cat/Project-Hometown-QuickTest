using System;
using System.Collections;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using StrategyManagerModule;

using UnityEngine;

using static StrategyGamePlayData;
using static StrategyManagerModule.StrategyUpdate;

[Serializable]
public partial class Faction : IEquatable<Faction>, IDisposable
{
	public Faction(in StrategyStartSetterData.FactionData data)
	{
		factionName = data.factionName;
		factionColor = data.factionColor;
		factionIcon = data.factionIcon;
		defaultUnitPrefab = data.defaultUnitPrefab;

		enableResourcesSupply = data.enableResourcesSupply;

		factionStats = new StatsList(new StatsValue(StatsType.세력_점령속도비율_c, data.captureSpeed),
			new StatsValue(StatsType.세력_인력_최대, data.maxOperationPoint),
			new StatsValue(StatsType.세력_인력_현재, data.currentOperationPoint),
			new StatsValue(StatsType.세력_물자_최대, data.maxMaterialPoint),
			new StatsValue(StatsType.세력_물자_현재, data.currentMaterialPoint),
			new StatsValue(StatsType.세력_전력_최대, data.maxElectricPoint),
			new StatsValue(StatsType.세력_전력_현재, data.currentElectricPoint)
		);
		availableUnitKeyList = data.AvailableUnitKeyList();

		detectedList = new ();
	}
	public void Dispose()
	{
		factionIcon = null;
		defaultUnitPrefab = null;
		availableUnitKeyList = null;
		detectedList = null;
		OnSupplyChange = null;
	}

	private string factionName;
	private int factionID;

	private Color factionColor;
	private Sprite factionIcon;
	private GameObject defaultUnitPrefab;
	private bool enableResourcesSupply;

	private StatsList factionStats;
	private List<UnitKey> availableUnitKeyList;

	private ElementSet detectedList;

	[ShowInInspector]
	public string FactionName => factionName;
	[ShowInInspector]
	public int FactionID => factionID;
	[ShowInInspector]
	public Color FactionColor => factionColor;
	public Sprite FactionIcon => factionIcon;
	public GameObject DefaultUnitPrefab => defaultUnitPrefab;
	public bool IsEnableResourcesSupply {get => enableResourcesSupply; set => enableResourcesSupply = value; }
	[ShowInInspector]
	public StatsList FactionStats => factionStats;
	public List<UnitKey> AvailableUnitKeyList => availableUnitKeyList;
	public ElementSet DetectedList => detectedList;

	public static bool TryFindFaction(string factionName, out Faction find)
	{
		return StrategyManager.Collector.TryFind<Faction>(f => f.factionName == factionName, out find);
	}
	public static bool FindFaction(int factionID, out Faction find)
	{
		return StrategyManager.Collector.TryFind<Faction>(f => f.factionID == factionID, out find);
	}
	public override bool Equals(object obj)
	{
		return Equals(obj as Faction);
	}
	public bool Equals(Faction other)
	{
		return other is not null &&
			   factionName == other.factionName &&
			   factionID == other.factionID;
	}
	public override int GetHashCode()
	{
		return HashCode.Combine(factionName, factionID);
	}
    public static bool operator ==(Faction left, Faction right)
	{
		return EqualityComparer<Faction>.Default.Equals(left, right);
	}
	public static bool operator !=(Faction left, Faction right)
	{
		return !(left == right);
	}
}
public partial class Faction : IStrategyElement
{
	public IStrategyElement ThisElement => this;
	int IStrategyElement.ID { get => factionID; set => factionID = value; }

    public void InStrategyCollector()
	{
	}
	public void OutStrategyCollector()
	{
	}

    void IStrategyStartGame.OnStartGame()
	{
	}
	void IStrategyStartGame.OnStopGame()
	{
	}

}
public partial class Faction // ElementSet
{
	public class ElementSet : ISet<IStrategyElement>, IDisposable
	{
		private readonly HashSet<IStrategyElement> elementList = new HashSet<IStrategyElement>();
		private readonly HashSet<ITargetableCombatant> targetableList = new HashSet<ITargetableCombatant>();
		private readonly HashSet<INearbyElement> nearbyList = new HashSet<INearbyElement>();
		public IEnumerable<ITargetableCombatant> TargetableType => targetableList;
		public IEnumerable<INearbyElement> NearbyType => nearbyList;
		public int Count => elementList.Count;
		public ElementSet() { } 
		public ElementSet(IEnumerable<IStrategyElement> detectedList) 
		{
			foreach (var detected in detectedList)
			{
				Add(detected);
			}
		}
		public void Dispose()
		{
			Clear();
		}
		
		#region 자주사용하는 함수
		public bool Add(IStrategyElement item)
		{
			if (elementList.Add(item))
			{
				if (item is INearbyElement nearby) nearbyList.Add(nearby);
				if (item is ITargetableCombatant target) targetableList.Add(target);
				return true;
			}
			return false;
		}
		public bool Remove(IStrategyElement item)
		{
			if (elementList.Remove(item))
			{
				if (item is INearbyElement nearby) nearbyList.Remove(nearby);
				if (item is ITargetableCombatant target) targetableList.Remove(target);
				return true;
			}
			return false;
		}
		public void Clear()
		{
			elementList.Clear();
			nearbyList.Clear();
			targetableList.Clear();
		}
		public bool Contains(IStrategyElement item) => elementList.Contains(item);
		#endregion
	
		#region ISet<T>
		bool ICollection<IStrategyElement>.IsReadOnly => false;
		public void CopyTo(IStrategyElement[] array, int arrayIndex) => elementList.CopyTo(array, arrayIndex);
		public void ExceptWith(IEnumerable<IStrategyElement> other) => elementList.ExceptWith(other);
		public void IntersectWith(IEnumerable<IStrategyElement> other) => elementList.IntersectWith(other);
		public bool IsProperSubsetOf(IEnumerable<IStrategyElement> other) => elementList.IsProperSubsetOf(other);
		public bool IsProperSupersetOf(IEnumerable<IStrategyElement> other) => elementList.IsProperSupersetOf(other);
		public bool IsSubsetOf(IEnumerable<IStrategyElement> other) => elementList.IsSubsetOf(other);
		public bool IsSupersetOf(IEnumerable<IStrategyElement> other) => elementList.IsSupersetOf(other);
		public bool Overlaps(IEnumerable<IStrategyElement> other) => elementList.Overlaps(other);
		public bool SetEquals(IEnumerable<IStrategyElement> other) => elementList.SetEquals(other);
		public void SymmetricExceptWith(IEnumerable<IStrategyElement> other) => elementList.SymmetricExceptWith(other);
		public void UnionWith(IEnumerable<IStrategyElement> other) => elementList.UnionWith(other);
		void ICollection<IStrategyElement>.Add(IStrategyElement item) => Add(item);
        IEnumerator<IStrategyElement> IEnumerable<IStrategyElement>.GetEnumerator()	=> elementList.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => elementList.GetEnumerator();
		#endregion
    }
}

public partial class Faction : IStatsValueControl
{
	public Action<Faction> OnSupplyChange;

	public IStatsValueControl StatsValue => this;

    int IStatsValueControl.GetStatsValue(StatsType type)
    {
		int baseValue = type switch
        {
            StatsType.세력_인력_최대 => 1,
            StatsType.세력_인력_현재 => 1,
            StatsType.세력_물자_최대 => 1,
            StatsType.세력_물자_현재 => 1,
            StatsType.세력_전력_최대 => 1,
            StatsType.세력_전력_현재 => 1,
            StatsType.세력_인력_회복률_c => 1,
            StatsType.세력_물자_회복률_c => 1,
            StatsType.세력_전력_회복률_c => 1,
            StatsType.세력_점령속도비율_c => 1,
            _ => 0,
        } + factionStats.GetValue(type);
		return baseValue;
	}

    float IStatsValueControl.GetStatsValuePrecent(StatsType type)
	{
		int baseValue = type switch
		{
			StatsType.세력_인력_최대 => 1,
			StatsType.세력_인력_현재 => 1,
			StatsType.세력_물자_최대 => 1,
			StatsType.세력_물자_현재 => 1,
			StatsType.세력_전력_최대 => 1,
			StatsType.세력_전력_현재 => 1,
			StatsType.세력_인력_회복률_c => 1,
			StatsType.세력_물자_회복률_c => 1,
			StatsType.세력_전력_회복률_c => 1,
			StatsType.세력_점령속도비율_c => 1,
			_ => 0,
		} + factionStats.GetValue(type);
		return baseValue * 0.01f;
	}

    void IStatsValueControl.SetStatsValue(StatsType type, int value)
	{
		switch(type)
		{
			case StatsType.세력_인력_현재: factionStats.SetValue(type, value); break;
			case StatsType.세력_물자_현재: factionStats.SetValue(type, value); break;
			case StatsType.세력_전력_현재: factionStats.SetValue(type, value); break;
			default: factionStats.SetValue(type, value); break;
		}
	}

    void IStatsValueControl.SetStatsValuePrecent(StatsType type, float valuePercent)
	{
		int value = Mathf.FloorToInt(valuePercent * 100);
		switch (type)
		{
			case StatsType.세력_인력_현재: factionStats.SetValue(type, value); break;
			case StatsType.세력_물자_현재: factionStats.SetValue(type, value); break;
			case StatsType.세력_전력_현재: factionStats.SetValue(type, value); break;
			default: factionStats.SetValue(type, value); break;
		}
	}


	public void OnSupplyUpdate(SupplyRequest supplyRequest)
	{
		if (!supplyRequest.IsUpdateFlag()) return;

		supplyRequest.ResetAndLeaveDecimal(
			out int integerPersonnel,
			out int integerMaterial,
			out int integerElectric);

		int maxPersonnel = StatsValue.GetStatsValue(StatsType.거점_인력_최대);
		int maxMaterial = StatsValue.GetStatsValue(StatsType.거점_재료_최대);
		int maxElectric = StatsValue.GetStatsValue(StatsType.거점_전력_최대);

		integerPersonnel += StatsValue.GetStatsValue(StatsType.거점_인력_현재);
		integerMaterial += StatsValue.GetStatsValue(StatsType.거점_재료_현재);
		integerElectric += StatsValue.GetStatsValue(StatsType.거점_전력_현재);

		if (maxPersonnel < integerPersonnel) integerPersonnel = maxPersonnel;
		if (maxMaterial < integerMaterial) integerMaterial = maxMaterial;
		if (maxElectric < integerElectric) integerElectric = maxElectric;

		StatsValue.SetStatsValue(StatsType.거점_인력_현재, integerPersonnel);
		StatsValue.SetStatsValue(StatsType.거점_재료_현재, integerMaterial);
		StatsValue.SetStatsValue(StatsType.거점_전력_현재, integerElectric);

		OnSupplyChange.Invoke(this);
	}
}