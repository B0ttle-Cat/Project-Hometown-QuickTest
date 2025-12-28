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
	public void ComputeDynamicAvailableUnitKeyList()
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
	private DetectedSet detectedList;
	private CaptureSectorSet capturedList;
	private OperationSet operationList;
	private UnitSet unitList;
	private Action<IStrategyElement, bool> onChangeDetected;
	private Action<IStrategyElement, bool> onChangeCaptured;
	private Action<IStrategyElement, bool> onChangeOperation;
	private Action<IStrategyElement, bool> onChangeUnit;
	private void AddChangeEvent(ref Action<IStrategyElement, bool> action, ElementSet elementsSet,  Action<IStrategyElement, bool> onChange, bool invokeForExisting)
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

	public DetectedSet DetectedList => detectedList;
	public CaptureSectorSet CapturedList => capturedList;
	public OperationSet OperationList => operationList;
	public UnitSet UnitList => unitList;


	partial void InitElementSet()
	{
		detectedList = new DetectedSet();
		capturedList = new CaptureSectorSet();
		operationList = new OperationSet();
		unitList = new UnitSet();

		onChangeDetected = null;
		onChangeCaptured = null;
		onChangeOperation = null;
		onChangeUnit = null;
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

		onChangeDetected = null;
		onChangeCaptured = null;
		onChangeOperation = null;
		onChangeUnit = null;
	}

	public void OnChangeElementSetEvent()
	{
		ClearHasChange(DetectedList);
		ClearHasChange(CapturedList);
		ClearHasChange(OperationList);
		ClearHasChange(UnitList);

		void ClearHasChange(ElementSet elementset)
		{
			if (elementset.HasChange)
			{
				foreach (var item in elementset.ChangeRemvoe)
					onChangeCaptured?.Invoke(item, false);
				foreach (var item in elementset.ChangeAdd)
					onChangeCaptured?.Invoke(item, true);

				elementset.ClearHasChange();
			}
		}
	}
	public class ElementSet : ISet<IStrategyElement>, IDisposable
	{
		public bool HasChange { get; private set; }
		protected readonly HashSet<IStrategyElement> elementList = new HashSet<IStrategyElement>();
		public readonly HashSet<IStrategyElement> ChangeAdd = new HashSet<IStrategyElement>();
		public readonly HashSet<IStrategyElement> ChangeRemvoe = new HashSet<IStrategyElement>();
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
			HasChange = false;
			Clear();
		}

		#region 자주사용하는 함수
		public virtual bool Add(IStrategyElement item)
		{
			if (elementList.Add(item))
			{
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
		}
		public bool Contains(IStrategyElement item) => elementList.Contains(item);
		public void ClearHasChange()
		{
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
		private readonly HashSet<ITargetableCombatant> targetableList = new HashSet<ITargetableCombatant>();
		private readonly HashSet<INearbyElement> nearbyList = new HashSet<INearbyElement>();
		public IEnumerable<ITargetableCombatant> TargetableType => targetableList;
		public IEnumerable<INearbyElement> NearbyType => nearbyList;

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
		private readonly HashSet<ISectorController> controllerList = new HashSet<ISectorController>();
		private readonly HashSet<ISupplyStats> supplyList = new HashSet<ISupplyStats>();
		private readonly HashSet<ISectorCardUIObject> cardUIList = new HashSet<ISectorCardUIObject>();
		public IEnumerable<ISectorController> ControllerType => controllerList;
		public IEnumerable<ISupplyStats> SupplyType => supplyList;
		public IEnumerable<ISectorCardUIObject> CardUIType => cardUIList;
		public override bool Add(IStrategyElement item)
		{
			if (base.Add(item))
			{
				if (item is ISectorController control) controllerList.Add(control);
				if (item is ISupplyStats supply) supplyList.Add(supply);
				if (item is ISectorCardUIObject card) cardUIList.Add(card);
				return true;
			}
			return false;
		}
		public override bool Remove(IStrategyElement item)
		{
			if (base.Remove(item))
			{
				if (item is ISectorController control) controllerList.Remove(control);
				if (item is ISectorCardUIObject card) cardUIList.Remove(card);
				if (item is ISupplyStats supply) supplyList.Remove(supply);
				return true;
			}
			return false;
		}
		public override void Clear()
		{
			base.Clear();
			controllerList.Clear();
			supplyList.Clear();
			cardUIList.Clear();
		}
	}
	public class OperationSet : ElementSet
	{
		private readonly HashSet<IOperationController> controllerList = new HashSet<IOperationController>();
		private readonly HashSet<IUnitOrganization> organizationList = new HashSet<IUnitOrganization>();
		private readonly HashSet<IOperationCardUIObject> cardUIList = new HashSet<IOperationCardUIObject>();
		public override bool Add(IStrategyElement item)
		{
			if (base.Add(item))
			{
				if (item is IOperationController control) controllerList.Add(control);
				if (item is IUnitOrganization organization) organizationList.Add(organization);
				if (item is IOperationCardUIObject card) cardUIList.Add(card);
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
				if (item is IOperationCardUIObject card) cardUIList.Remove(card);
				return true;
			}
			return false;
		}
		public override void Clear()
		{
			base.Clear();
			controllerList.Clear();
			organizationList.Clear();
			cardUIList.Clear();
		}
	}

	public class UnitSet : ElementSet
	{
		private readonly HashSet<ITargetableCombatant> targetableList = new HashSet<ITargetableCombatant>();
		private readonly HashSet<INearbyElement> nearbyList = new HashSet<INearbyElement>();
		public IEnumerable<ITargetableCombatant> TargetableType => targetableList;
		public IEnumerable<INearbyElement> NearbyType => nearbyList;

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
}
public partial class Faction : IStatsValueControl, ISupplyStats
{
	public IStatsValueControl StatsValue => this;
	public Action<ISupplyStats> OnSupplyChange { get; set; }

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


	#region ISupplyStats
	public (float[] values, float total, float max) GetPersonnelDetailValue()
	{
		float max = StatsValue.GetStatsValue(StatsType.자원_인력_최대);
		float total = StatsValue.GetStatsValue(StatsType.자원_인력_현재);
		float[] values = new float[1];
		values[0] = StatsValue.GetStatsValue(StatsType.자원_인력_현재);
		return (values, total, max);
	}
	public (float[] values, float total, float max) GetMaterialDetailValue()
	{
		float max = StatsValue.GetStatsValue(StatsType.자원_재료_최대);
		float total = StatsValue.GetStatsValue(StatsType.자원_재료_현재);
		float[] values = new float[1];
		values[0] = StatsValue.GetStatsValue(StatsType.자원_재료_현재);
		return (values, total, max);
	}
	public (float[] values, float total, float max) GetElectricDetailValue()
	{
		float max = StatsValue.GetStatsValue(StatsType.자원_전력_최대);
		float total = StatsValue.GetStatsValue(StatsType.자원_전력_현재);

		float[] values = new float[1];
		values[0] = StatsValue.GetStatsValue(StatsType.자원_전력_현재);
		return (values, total, max);
	}
	public (float total, float max) GetPersonnelSimpleValue()
	{
		float max = StatsValue.GetStatsValue(StatsType.자원_인력_최대);
		float total = StatsValue.GetStatsValue(StatsType.자원_인력_현재);
		return (total, max);
	}
	public (float total, float max) GetMaterialSimpleValue()
	{
		float max = StatsValue.GetStatsValue(StatsType.자원_재료_최대);
		float total = StatsValue.GetStatsValue(StatsType.자원_재료_현재);
		return (total, max);
	}
	public (float total, float max) GetElectricSimpleValue()
	{
		float max = StatsValue.GetStatsValue(StatsType.자원_전력_최대);
		float total = StatsValue.GetStatsValue(StatsType.자원_전력_현재);
		return (total, max);
	}
	public string GetPersonnelDetailText()
	{
		(float[] values, float total, float max) = GetPersonnelDetailValue();
		string text = $"인력: {total}/{max}";
		int length = values.Length;
		for (int i = 0 ; i < length ; i++)
		{
			float value = values[i];
			text += $"\t{value:+#;-#;0}";
		}

		return text;
	}
	public string GetMaterialDetailText()
	{
		(float[] values, float total, float max) = GetMaterialDetailValue();
		string text = $"재료: {total}/{max}";
		int length = values.Length;
		for (int i = 0 ; i < length ; i++)
		{
			float value = values[i];
			text += $"\t{value:+#;-#;0}";
		}

		return text;
	}
	public string GetElectricDetailText()
	{
		(float[] values, float total, float max) = GetElectricDetailValue();
		string text = $"전력: {total}/{max}";
		int length = values.Length;
		for (int i = 0 ; i < length ; i++)
		{
			float value = values[i];
			text += $"\t{value:+#;-#;0}";
		}

		return text;
	}
	public string GetPersonnelSimpleText()
	{
		(float total, float max) = GetPersonnelSimpleValue();
		string text = $"인력: {total}/{max}";
		return text;
	}
	public string GetMaterialSimpleText()
	{
		(float total, float max) = GetMaterialSimpleValue();
		string text = $"재료: {total}/{max}";
		return text;
	}
	public string GetElectricSimpleText()
	{
		(float total, float max) = GetElectricSimpleValue();
		string text = $"전력: {total}/{max}";
		return text;
	}
	#endregion
}