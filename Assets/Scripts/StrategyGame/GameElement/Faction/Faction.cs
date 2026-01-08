using System;
using System.Collections;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using StrategyManagerModule;

using UnityEngine;

using static StrategyGamePlayData;
using static StrategyManagerModule.StrategyUpdate;

[Serializable]
public partial class Faction : IDisposable
{
	private readonly FactionStatsData statsData;
	private readonly FactionRuntimeData runtimeData;
	[ShowInInspector, BoxGroup("Faction Data")]
	public FactionStatsData StatsData => statsData;
	[ShowInInspector, BoxGroup("Faction Data")]
	public FactionRuntimeData RuntimeData => runtimeData;

	public Faction(in StrategyStartSetterData.FactionData data)
	{
		statsData = new FactionStatsData();
		runtimeData = new FactionRuntimeData();

		statsData.FactionName = data.FactionName;
		Color factionColor = data.FactionColor;
		factionColor.a = 1f;
		statsData.FactionColor = factionColor;
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

		InitElementSet();
		InitSupply();
	}
	public void Dispose()
	{
		DeinitElementSet();
		DeinitSupply();
	}
	partial void InitElementSet();
	partial void DeinitElementSet();

	partial void InitSupply();
	partial void DeinitSupply();

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

	public void ComputeDynamicKeyStatsList()
	{

	}

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
	[ShowInInspector,ReadOnly]
	private DetectedSet detectedList;
	[ShowInInspector,ReadOnly]
	private CaptureSectorSet capturedList;
	[ShowInInspector,ReadOnly]
	private OperationSet operationList;
	[ShowInInspector,ReadOnly]
	private UnitSet unitList;
	[ShowInInspector, ReadOnly]
	private FacilitySet facilityList;

	private Action<IStrategyElement, bool> onChangeDetected;
	private Action<IStrategyElement, bool> onChangeCaptured;
	private Action<IStrategyElement, bool> onChangeOperation;
	private Action<IStrategyElement, bool> onChangeUnit;
	private Action<IStrategyElement, bool> onChangeFacility;
	private void AddChangeEvent(ref Action<IStrategyElement, bool> action, ElementSet elementsSet, Action<IStrategyElement, bool> onChange, bool invokeForExisting)
	{
		action -= onChange;
		action += onChange;
		if (invokeForExisting)
		{
			foreach (var item in elementsSet)
			{
				onChange?.Invoke(item, true);
			}
		}
	}
	public void AddChangeDetected(Action<IStrategyElement, bool> onChange, bool invokeForExisting = true)
	{
		AddChangeEvent(ref onChangeDetected, detectedList, onChange, invokeForExisting);
	}
	public void RemoveChangeDetected(Action<IStrategyElement, bool> onChange)
	{
		onChangeDetected -= onChange;
	}
	public void AddChangeCaptured(Action<IStrategyElement, bool> onChange, bool invokeForExisting = true)
	{
		AddChangeEvent(ref onChangeCaptured, capturedList, onChange, invokeForExisting);
	}
	public void RemoveChangeCaptured(Action<IStrategyElement, bool> onChange)
	{
		onChangeCaptured -= onChange;
	}
	public void AddChangeOperation(Action<IStrategyElement, bool> onChange, bool invokeForExisting = true)
	{
		AddChangeEvent(ref onChangeOperation, operationList, onChange, invokeForExisting);
	}
	public void RemoveChangeOperation(Action<IStrategyElement, bool> onChange)
	{
		onChangeOperation -= onChange;
	}
	public void AddChangeUnit(Action<IStrategyElement, bool> onChange, bool invokeForExisting = true)
	{
		AddChangeEvent(ref onChangeUnit, unitList, onChange, invokeForExisting);
	}
	public void RemoveChangeUnit(Action<IStrategyElement, bool> onChange)
	{
		onChangeUnit -= onChange;
	}
	public void AddChangeFacility(Action<IStrategyElement, bool> onChange, bool invokeForExisting = true)
	{
		AddChangeEvent(ref onChangeFacility, facilityList, onChange, invokeForExisting);
	}
	public void RemoveChangeFacility(Action<IStrategyElement, bool> onChange)
	{
		onChangeFacility -= onChange;
	}

	public DetectedSet DetectedList => detectedList;
	public CaptureSectorSet CapturedList => capturedList;
	public OperationSet OperationList => operationList;
	public UnitSet UnitList => unitList;
	public FacilitySet FacilityList => facilityList;

	partial void InitElementSet()
	{
		detectedList = new DetectedSet();
		capturedList = new CaptureSectorSet();
		operationList = new OperationSet();
		unitList = new UnitSet();
		facilityList = new FacilitySet();

		onChangeDetected = null;
		onChangeCaptured = null;
		onChangeOperation = null;
		onChangeUnit = null;
		onChangeFacility = null;
	}
	partial void DeinitElementSet()
	{
		detectedList?.Dispose();
		detectedList = null;

		capturedList?.Dispose();
		capturedList = null;

		operationList?.Dispose();
		operationList = null;

		unitList?.Dispose();
		unitList = null;

		facilityList?.Dispose();
		facilityList = null;

		onChangeDetected = null;
		onChangeCaptured = null;
		onChangeOperation = null;
		onChangeUnit = null;
		onChangeFacility = null;
	}

	public void OnChangeElementSetEvent()
	{
		DetectedList.ClearAndChangeInvoke(onChangeDetected);
		CapturedList.ClearAndChangeInvoke(onChangeCaptured);
		OperationList.ClearAndChangeInvoke(onChangeOperation);
		UnitList.ClearAndChangeInvoke(onChangeUnit);
		FacilityList.ClearAndChangeInvoke(onChangeFacility);
	}
	public class ElementSet : ISet<IStrategyElement>, IDisposable
	{
		public bool HasChange { get; private set; }
		protected readonly HashSet<IStrategyElement> elementList = new HashSet<IStrategyElement>();
		protected readonly HashSet<IStrategyElement> ChangeAdd = new HashSet<IStrategyElement>();
		protected readonly HashSet<IStrategyElement> ChangeRemvoe = new HashSet<IStrategyElement>();
		public int Count => elementList.Count;
		public readonly List<IStrategyElement> ElementList = new List<IStrategyElement>();
		public IStrategyElement this[int index] => ElementList[index];
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
			HasChange = false;
			Clear();
		}

		#region 자주사용하는 함수
		public virtual bool Add(IStrategyElement item)
		{
			if (elementList.Add(item))
			{
				ElementList.Add(item);
				ChangeAdd.Add(item);
				ChangeRemvoe.Remove(item);

				HasChange = true;
				return true;
			}
			return false;
		}
		public virtual bool Remove(IStrategyElement item)
		{
			if (elementList.Remove(item))
			{
				ElementList.Remove(item);
				ChangeAdd.Remove(item);
				ChangeRemvoe.Add(item);

				HasChange = true;
				return true;
			}
			return false;
		}
		public virtual void Clear()
		{
			HasChange = true;
			elementList.Clear();
			ElementList.Clear();
		}
		public bool Contains(IStrategyElement item) => elementList.Contains(item);
		public void ClearAndChangeInvoke(Action<IStrategyElement, bool> onChangeAction)
		{
			if (onChangeAction != null)
			{
				foreach (var item in ChangeRemvoe)
					onChangeAction.Invoke(item, false);
				foreach (var item in ChangeAdd)
					onChangeAction.Invoke(item, true);
			}

			ChangeAdd.Clear();
			ChangeRemvoe.Clear();
		}
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
	public class DetectedSet : ElementSet
	{
		protected readonly List<ITargetableCombatant> targetableList = new List<ITargetableCombatant>();
		protected readonly List<INearbyElement> nearbyList = new List<INearbyElement>();
		public List<ITargetableCombatant> TargetableList => targetableList;
		public List<INearbyElement> NearbyList => nearbyList;

		public override bool Add(IStrategyElement item)
		{
			if (base.Add(item))
			{
				if (item is INearbyElement nearby) nearbyList.Add(nearby);
				if (item is ITargetableCombatant target) targetableList.Add(target);
				return true;
			}
			return false;
		}
		public override bool Remove(IStrategyElement item)
		{
			if (base.Remove(item))
			{
				if (item is INearbyElement nearby) nearbyList.Remove(nearby);
				if (item is ITargetableCombatant target) targetableList.Remove(target);
				return true;
			}
			return false;
		}
		public override void Clear()
		{
			base.Clear();
			nearbyList.Clear();
			targetableList.Clear();
		}
	}
	public class CaptureSectorSet : ElementSet
	{
		protected readonly List<ISectorController> controllerList = new List<ISectorController>();
		protected readonly List<ISupplyStateForSector> supplyList = new List<ISupplyStateForSector>();
		protected readonly List<ISectorToPanelAPI> forPanel = new List<ISectorToPanelAPI>();

		public List<ISectorController> ControllerList => controllerList;
		public List<ISupplyStateForSector> SupplyList => supplyList;
		public List<ISectorToPanelAPI> ForPanel => forPanel;
		public override bool Add(IStrategyElement item)
		{
			if (base.Add(item))
			{
				if (item is ISectorController control) controllerList.Add(control);
				if (item is ISupplyStateForSector supply) supplyList.Add(supply);
				if (item is ISectorToPanelAPI panel) forPanel.Add(panel);
				return true;
			}
			return false;
		}
		public override bool Remove(IStrategyElement item)
		{
			if (base.Remove(item))
			{
				if (item is ISectorController control) controllerList.Remove(control);
				if (item is ISupplyStateForSector supply) supplyList.Remove(supply);
				if (item is ISectorToPanelAPI panel) forPanel.Remove(panel);
				return true;
			}
			return false;
		}
		public override void Clear()
		{
			base.Clear();
			controllerList.Clear();
			supplyList.Clear();
			forPanel.Clear();
		}
	}
	public class OperationSet : ElementSet
	{
		protected readonly List<IOperationController> controllerList = new List<IOperationController>();
		protected readonly List<IUnitOrganization> organizationList = new List<IUnitOrganization>();
		protected readonly List<IOperationToPanelAPI> forPanel = new List<IOperationToPanelAPI>();

		public List<IOperationController> ControllerList => controllerList;
		public List<IUnitOrganization> OrganizationList => organizationList;
		public List<IOperationToPanelAPI> ForPanel => forPanel;
		public override bool Add(IStrategyElement item)
		{
			if (base.Add(item))
			{
				if (item is IOperationController control) controllerList.Add(control);
				if (item is IUnitOrganization organization) organizationList.Add(organization);
				if (item is IOperationToPanelAPI panel) forPanel.Add(panel);
				return true;
			}
			return false;
		}
		public override bool Remove(IStrategyElement item)
		{
			if (base.Remove(item))
			{
				if (item is IOperationController control) controllerList.Remove(control);
				if (item is IUnitOrganization organization) organizationList.Remove(organization);
				if (item is IOperationToPanelAPI panel) forPanel.Remove(panel);
				return true;
			}
			return false;
		}
		public override void Clear()
		{
			base.Clear();
			controllerList.Clear();
			organizationList.Clear();
			forPanel.Clear();
		}
	}
	public class UnitSet : ElementSet
	{
		protected readonly List<ITargetableCombatant> targetableList = new List<ITargetableCombatant>();
		protected readonly List<INearbyElement> nearbyList = new List<INearbyElement>();
		protected readonly List<IUnitToPanelAPI> forPanel = new List<IUnitToPanelAPI>();
		public List<ITargetableCombatant> TargetableType => targetableList;
		public List<INearbyElement> NearbyType => nearbyList;
		public List<IUnitToPanelAPI> ForPanel => forPanel;

		public override bool Add(IStrategyElement item)
		{
			if (base.Add(item))
			{
				if (item is INearbyElement nearby) nearbyList.Add(nearby);
				if (item is ITargetableCombatant target) targetableList.Add(target);
				if (item is IUnitToPanelAPI panel) forPanel.Add(panel);
				return true;
			}
			return false;
		}
		public override bool Remove(IStrategyElement item)
		{
			if (base.Remove(item))
			{
				if (item is INearbyElement nearby) nearbyList.Remove(nearby);
				if (item is ITargetableCombatant target) targetableList.Remove(target);
				if (item is IUnitToPanelAPI panel) forPanel.Remove(panel);
				return true;
			}
			return false;
		}
		public override void Clear()
		{
			base.Clear();
			nearbyList.Clear();
			targetableList.Clear();
			forPanel.Clear();
		}
	}
	public class FacilitySet : ElementSet
	{
		protected readonly List<ITargetableCombatant> targetableList = new List<ITargetableCombatant>();

		public override bool Add(IStrategyElement item)
		{
			if (base.Add(item))
			{
				if (item is ITargetableCombatant target) targetableList.Add(target);
				return true;
			}
			return false;
		}
		public override bool Remove(IStrategyElement item)
		{
			if (base.Remove(item))
			{
				if (item is ITargetableCombatant target) targetableList.Remove(target);
				return true;
			}
			return false;
		}
		public override void Clear()
		{
			base.Clear();
			targetableList.Clear();
		}
	}
}

public partial class Faction : IStatsValueControl, ISupplyStats
{
	public IStatsValueControl ThisStatsValue => this;
	public event Action<ISupplyStats> OnSupplyChange;

	partial void InitSupply()
	{
		OnSupplyChange = null;
	}
	partial void DeinitSupply()
	{
		OnSupplyChange = null;
	}
	int IStatsValueControl.GetStatsValue(StatsType type)
	{
		int baseValue = type switch
		{
			StatsType.자원_인력_최대 => StatsData.CapacityPersonnel + SumPersonnel(),
			StatsType.자원_재료_최대 => StatsData.CapacityMaterial + SumMaterial(),
			StatsType.자원_전력_최대 => StatsData.CapacityElectric + SumElectric(),

			StatsType.자원_인력_회복 => StatsData.RecoveryPersonnel,
			StatsType.자원_재료_회복 => StatsData.RecoveryMaterial,
			StatsType.자원_전력_회복 => StatsData.RecoveryElectric,

			StatsType.자원_인력_현재 => RuntimeData.CurrentPersonnel,
			StatsType.자원_재료_현재 => RuntimeData.CurrentMaterial,
			StatsType.자원_전력_현재 => RuntimeData.CurrentElectric,

			StatsType.사용중_인력_병력 => RuntimeData.AssignedMilitaryPersonnel,
			StatsType.사용중_인력_시설 => RuntimeData.AssignedFacilitiesPersonnel,
			StatsType.유지비_재료_시설 => RuntimeData.MaintenanceCostFacilitiesMaterial,
			StatsType.유지비_전력_시설=> RuntimeData.MaintenanceCostFacilitiesElectric,

			_ => 0,
		} + DynamicKeyStatsList.GetValue(type);
		return baseValue;

		int SumPersonnel()
		{
			var list = CapturedList.SupplyList;
			int length = list.Count;
			int sum = 0;
			for (int i = 0 ; i < length ; i++)
			{
				sum += list[i].MaxPersonnelCapacityBonus;
			}
			return sum;
		}
		int SumMaterial()
		{
			var list = CapturedList.SupplyList;
			int length = list.Count;
			int sum = 0;
			for (int i = 0 ; i < length ; i++)
			{
				sum += list[i].MaxMaterialCapacityBonus;
			}
			return sum;
		}
		int SumElectric()
		{
			var list = CapturedList.SupplyList;
			int length = list.Count;
			int sum = 0;
			for (int i = 0 ; i < length ; i++)
			{
				sum += list[i].MaxElectricCapacityBonus;
			}
			return sum;
		}
	}
	float IStatsValueControl.GetStatsValuePrecent(StatsType type)
	{
		return 0;
	}
	void IStatsValueControl.SetStatsValue(StatsType type, int value)
	{
		switch (type)
		{
			case StatsType.자원_인력_현재: RuntimeData.CurrentPersonnel = value; break;
			case StatsType.자원_재료_현재: RuntimeData.CurrentMaterial = value; break;
			case StatsType.자원_전력_현재: RuntimeData.CurrentElectric = value; break;

			case StatsType.사용중_인력_병력: RuntimeData.AssignedMilitaryPersonnel = value; break;
			case StatsType.사용중_인력_시설: RuntimeData.AssignedFacilitiesPersonnel = value; break;
			case StatsType.유지비_재료_시설: RuntimeData.MaintenanceCostFacilitiesMaterial = value; break;
			case StatsType.유지비_전력_시설: RuntimeData.MaintenanceCostFacilitiesElectric = value; break;
			default: DynamicKeyStatsList.SetValue(type, value); break;
		}
	}
	void IStatsValueControl.SetStatsValuePrecent(StatsType type, float valuePercent)
	{
		int value = Mathf.FloorToInt(valuePercent * 100);
		//switch (type)
		//{
		//	case StatsType.자원_인력_현재: RuntimeData.CurrentPersonnel = value; break;
		//	case StatsType.자원_재료_현재: RuntimeData.CurrentMaterial = value; break;
		//	case StatsType.자원_전력_현재: RuntimeData.CurrentElectric = value; break;
		//	default: DynamicKeyStatsList.SetValue(type, value); break;
		//}
	}
	#region ISupplyStats
	void ISupplyStats.OnSupplyUpdate(SupplyRequest supplyRequest)
	{
		if (!supplyRequest.IsUpdateFlag()) return;

		supplyRequest.ResetAndLeaveDecimal(
			out int integerPersonnel,
			out int integerMaterial,
			out int integerElectric);

		int maxPersonnel = ThisStatsValue.GetStatsValue(StatsType.자원_인력_최대);
		int maxMaterial = ThisStatsValue.GetStatsValue(StatsType.자원_재료_최대);
		int maxElectric = ThisStatsValue.GetStatsValue(StatsType.자원_전력_최대);

		integerPersonnel += ThisStatsValue.GetStatsValue(StatsType.자원_인력_현재);
		integerMaterial += ThisStatsValue.GetStatsValue(StatsType.자원_재료_현재);
		integerElectric += ThisStatsValue.GetStatsValue(StatsType.자원_전력_현재);

		if (maxPersonnel < integerPersonnel) integerPersonnel = maxPersonnel;
		if (maxMaterial < integerMaterial) integerMaterial = maxMaterial;
		if (maxElectric < integerElectric) integerElectric = maxElectric;

		ThisStatsValue.SetStatsValue(StatsType.자원_인력_현재, integerPersonnel);
		ThisStatsValue.SetStatsValue(StatsType.자원_재료_현재, integerMaterial);
		ThisStatsValue.SetStatsValue(StatsType.자원_전력_현재, integerElectric);

		OnSupplyChange?.Invoke(this);
	}
	public (int[] values, int total, int max) GetPersonnelDetailValue()
	{
		int max = ThisStatsValue.GetStatsValue(StatsType.자원_인력_최대);
		int[] values = new int[3];
		values[0] = ThisStatsValue.GetStatsValue(StatsType.자원_인력_현재);
		values[1] = ThisStatsValue.GetStatsValue(StatsType.사용중_인력_병력);
		values[2] = ThisStatsValue.GetStatsValue(StatsType.사용중_인력_시설);
		int total =  values[0] + values[1] + values[2];
		return (values, total, max);
	}
	public (int[] values, int total, int max) GetMaterialDetailValue()
	{
		int max = ThisStatsValue.GetStatsValue(StatsType.자원_재료_최대);
		int total = ThisStatsValue.GetStatsValue(StatsType.자원_재료_현재);
		int[] values = new int[1];
		values[0] = ThisStatsValue.GetStatsValue(StatsType.자원_재료_현재);
		return (values, total, max);
	}
	public (int[] values, int total, int max) GetElectricDetailValue()
	{
		int max = ThisStatsValue.GetStatsValue(StatsType.자원_전력_최대);
		int total = ThisStatsValue.GetStatsValue(StatsType.자원_전력_현재);

		int[] values = new int[1];
		values[0] = ThisStatsValue.GetStatsValue(StatsType.자원_전력_현재);
		return (values, total, max);
	}
	public (int total, int max) GetPersonnelSimpleValue()
	{
		int max = ThisStatsValue.GetStatsValue(StatsType.자원_인력_최대);
		int total = ThisStatsValue.GetStatsValue(StatsType.자원_인력_현재);
		return (total, max);
	}
	public (int total, int max) GetMaterialSimpleValue()
	{
		int max = ThisStatsValue.GetStatsValue(StatsType.자원_재료_최대);
		int total = ThisStatsValue.GetStatsValue(StatsType.자원_재료_현재);
		return (total, max);
	}
	public (int total, int max) GetElectricSimpleValue()
	{
		int max = ThisStatsValue.GetStatsValue(StatsType.자원_전력_최대);
		int total = ThisStatsValue.GetStatsValue(StatsType.자원_전력_현재);
		return (total, max);
	}
	#endregion
}