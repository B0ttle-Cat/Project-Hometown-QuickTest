using System;
using System.Collections;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using StrategyManagerModule;

using UnityEngine;

using static StrategyGamePlayData;

[Serializable]
public partial class Faction : IDisposable
{
	[SerializeField, BoxGroup("Faction Data")]
	private readonly FactionStatsData statsData;
	[SerializeField, BoxGroup("Faction Data")]
	private readonly FactionRuntimeData runtimeData;
	public FactionStatsData StatsData => statsData;
	public FactionRuntimeData RuntimeData => runtimeData;

	public Faction(in StrategyStartSetterData.FactionData data)
	{
		statsData = new FactionStatsData();
		runtimeData = new FactionRuntimeData();

		statsData.FactionName = data.FactionName;
		statsData.FactionColor = data.FactionColor;
		statsData.FactionIcon = data.FactionIcon;
		statsData.EnableResourcesSupply = data.EnableResourcesSupply;
		statsData.AvailableUnitKeyList = data.AvailableUnitKeyList();

		statsData.CapacityPersonnel = data.CapacityPersonnel;
		statsData.CapacityMaterial = data.CapacityMaterial;
		statsData.CapacityElectric = data.CapacityElectric;
		statsData.RecoveryPersonnel = data.RecoveryPersonnel;
		statsData.RecoveryMaterial = data.RecoveryMaterial;
		statsData.RecoveryElectric = data.RecoveryElectric;

		runtimeData.CurrentPersonnel = data.CurrentPersonnel;
		runtimeData.CurrentMaterial = data.CurrentMaterial;
		runtimeData.CurrentElectric = data.CurrentElectric;
		runtimeData.AssignedMilitaryPersonnel = 0;
		runtimeData.AssignedFacilitiesPersonnel = 0;
		runtimeData.MaintenanceCostFacilitiesMaterial = 0;
		runtimeData.MaintenanceCostFacilitiesElectric = 0;
		runtimeData.DynamicAvailableUnitKeyList = new List<UnitKey>();
		runtimeData.DynamicKeyStatsList = new StatsList();
		detectedList = new();

		OnSupplyChange = null;
	}
	public void Dispose()
	{
		detectedList?.Dispose();
		detectedList = null;
		OnSupplyChange = null;
	}
	public void ComputeAllRuntimeData()
	{
		ComputeAssignedMilitaryPersonnel();
		ComputeFacilitiesRuntimeData();
		ComputeDynamicKeyStatsList();
	}
	public void ComputeFacilitiesRuntimeData()
	{
		ComputeAssignedFacilitiesPersonnel();
		ComputeMaintenanceCostFacilitiesMaterial();
		ComputeMaintenanceCostFacilitiesElectric();
	}

	public void ComputeAssignedMilitaryPersonnel()
	{

	}
	public void ComputeAssignedFacilitiesPersonnel()
	{

	}
	public void ComputeMaintenanceCostFacilitiesMaterial()
	{

	}
	public void ComputeMaintenanceCostFacilitiesElectric()
	{

	}
	public void ComputeDynamicAvailableUnitKeyList()
	{

	}
	public void ComputeDynamicKeyStatsList()
	{

	}


	private ElementSet detectedList;

	[ShowInInspector]
	public string FactionName => StatsData.FactionName;
	[ShowInInspector]
	public int FactionID => StatsData.FactionID;
	[ShowInInspector]
	public Color FactionColor => StatsData.FactionColor;
	public Sprite FactionIcon => StatsData.FactionIcon;
	public bool IsEnableResourcesSupply { get => StatsData.EnableResourcesSupply; set => StatsData.EnableResourcesSupply = value; }
	[ShowInInspector]
	public StatsList DynamicKeyStatsList => RuntimeData.DynamicKeyStatsList;
	public ElementSet DetectedList => detectedList;
}
public partial class Faction : IStrategyElement
{
	public IStrategyElement ThisElement => this;
	int IStrategyElement.ID { get => StatsData.FactionID; set => StatsData.FactionID = value; }

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
		IEnumerator<IStrategyElement> IEnumerable<IStrategyElement>.GetEnumerator() => elementList.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => elementList.GetEnumerator();
		#endregion
	}
}

public partial class Faction : IStatsValueControl, ISupplyStats
{
	public IStatsValueControl StatsValue => this;
	public Action<ISupplyStats> OnSupplyChange { get; set; }

	int IStatsValueControl.GetStatsValue(StatsType type)
	{
		int baseValue = type switch
		{
			StatsType.자원_인력_최대 => StatsData.CapacityPersonnel,
			StatsType.자원_재료_최대 => StatsData.CapacityMaterial,
			StatsType.자원_전력_최대 => StatsData.CapacityElectric,

			StatsType.자원_인력_현재 => StatsData.RecoveryPersonnel,
			StatsType.자원_재료_현재 => StatsData.RecoveryMaterial,
			StatsType.자원_전력_현재 => StatsData.RecoveryElectric,

			StatsType.자원_인력_회복 => RuntimeData.CurrentPersonnel,
			StatsType.자원_재료_회복 => RuntimeData.CurrentMaterial,
			StatsType.자원_전력_회복 => RuntimeData.CurrentElectric,
			_ => 0,
		} + DynamicKeyStatsList.GetValue(type);
		return baseValue;
	}

	float IStatsValueControl.GetStatsValuePrecent(StatsType type)
	{
		int baseValue = type switch
		{
			StatsType.자원_인력_최대 => StatsData.CapacityPersonnel,
			StatsType.자원_재료_최대 => StatsData.CapacityMaterial,
			StatsType.자원_전력_최대 => StatsData.CapacityElectric,

			StatsType.자원_인력_현재 => StatsData.RecoveryPersonnel,
			StatsType.자원_재료_현재 => StatsData.RecoveryMaterial,
			StatsType.자원_전력_현재 => StatsData.RecoveryElectric,

			StatsType.자원_인력_회복 => RuntimeData.CurrentPersonnel,
			StatsType.자원_재료_회복 => RuntimeData.CurrentMaterial,
			StatsType.자원_전력_회복 => RuntimeData.CurrentElectric,
			_ => 0,
		} + DynamicKeyStatsList.GetValue(type);
		return baseValue * 0.01f;
	}

	void IStatsValueControl.SetStatsValue(StatsType type, int value)
	{
		switch (type)
		{
			case StatsType.자원_인력_현재: RuntimeData.CurrentPersonnel = value; break;
			case StatsType.자원_재료_현재: RuntimeData.CurrentMaterial = value; break;
			case StatsType.자원_전력_현재: RuntimeData.CurrentElectric = value; break;
			default: DynamicKeyStatsList.SetValue(type, value); break;
		}
	}

	void IStatsValueControl.SetStatsValuePrecent(StatsType type, float valuePercent)
	{
		int value = Mathf.FloorToInt(valuePercent * 100);
		switch (type)
		{
			case StatsType.자원_인력_현재: RuntimeData.CurrentPersonnel = value; break;
			case StatsType.자원_재료_현재: RuntimeData.CurrentMaterial = value; break;
			case StatsType.자원_전력_현재: RuntimeData.CurrentElectric = value; break;
			default: DynamicKeyStatsList.SetValue(type, value); break;
		}
	}
}