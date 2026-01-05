using System;
using System.Collections.Generic;
using System.Linq;

using GameUI;

using UnityEngine;

using static StrategyGamePlayData;
using static StrategyManagerModule.StrategyUpdate;
public partial class OperationObject : MonoBehaviour  // Main
{
	[SerializeField]
	private int operationID;
	[SerializeField]
	private string teamName;
	[SerializeField]
	private int factionID;
	private float operationRadius = 5;
	public OperationObject This => this;
	public int OperationID => operationID;
	public string TeamName => teamName;
	public int FactionID => factionID;
	public Faction Faction => FactionAPI.ID2Faction(FactionID);
	public float OperationRadius => operationRadius;
	internal void Awake()
	{
		this.operationID = -1;
		this.factionID = -1;
		this.teamName = "";
		operationRadius = 5;
	}
	internal void Init(int factionID, string teamName)
	{
		this.factionID = factionID;
		this.teamName = teamName;
	}
	public void Init(in List<int> unitList)
	{
		InitOrganization(in unitList);
	}
	public void InitOther()
	{
		InitMovement();
		InitNearby();
		InitFSM();
	}
	partial void InitOrganization(in List<int> unitList);
	partial void InitMovement();
	partial void InitFSM();
	partial void InitNearby();
	public void DeInit()
	{
		var faction = FactionAPI.ID2Faction(FactionID);
		faction.RemoveOperation(this);

		DeInitOrganization();
		DeselectSelf();
		DeinitFSM();
		DeInitNearby();
	}
	partial void DeInitOrganization();
	partial void DeselectSelf();
	partial void DeinitFSM();
	partial void DeInitNearby();
}
public partial class OperationObject // StatsData
{
	int computeFrame = -1;
	public void ComputeOperationValue()
	{
		int thisFrame = Time.frameCount;
		if (computeFrame == thisFrame) return;
		computeFrame = thisFrame;
		moveSpeed = ComputeMoveSpeed();
		searchVisionRange = ComputeVisionRange();
		searchActionRange = ComputeActionRange();
		(searchGroupCenter, searchGroupRadius) = ComputeCenter();
	}
	private float ComputeMoveSpeed()
	{
		return UnitOrganizationList.Count == 0 ? 0 : UnitOrganizationList.Select(i => i.StatsData.MovementSpeed).Average();
	}
	private float ComputeVisionRange()
	{
		return UnitOrganizationList.Count == 0 ? 0 : UnitOrganizationList.Select(i => i.StatsData.VisionRange).Max();
	}
	private float ComputeActionRange()
	{
		return UnitOrganizationList.Count == 0 ? 0 : UnitOrganizationList.Select(i => i.StatsData.ActionRange).Max();
	}
	private (Vector3, float) ComputeCenter()
	{
		int length = UnitOrganizationList.Count;
		if (length == 0) return (transform.position, 0f);
		Vector3 sumPosition = Vector3.zero; int validCount = 0;
		foreach (var unit in UnitOrganizationList)
		{
			if (unit == null) continue;
			var unitMove = unit.ThisMovement;
			if (unitMove.IsNullRef()) continue;
			sumPosition += unitMove.CurrentPosition; validCount++;
		}
		if (validCount == 0) return (transform.position, 0f);
		Vector3 center = sumPosition / validCount; float maxDistance = 0f;
		foreach (var unit in UnitOrganizationList)
		{
			if (unit == null) continue;
			var unitMove = unit.ThisMovement;
			if (unitMove.IsNullRef()) continue;
			float distWithRadius = Vector3.Distance(center, unitMove.CurrentPosition) + unitMove.CurrentRadius;
			if (distWithRadius > maxDistance) maxDistance = distWithRadius;
		}
		return (center, maxDistance);
	}
}

public partial class OperationObject : ISupplyStats
{
	public IStatsValueControl ThisStatsValue => this;
	bool ISupplyStats.IsEnableResourcesSupply { get; set; }
	public event Action<ISupplyStats> OnSupplyChange;

	int IStatsValueControl.GetStatsValue(StatsType type)
	{
		int baseValue = type switch
		{
			StatsType.자원_인력_최대 => 0,
			StatsType.자원_재료_최대 => 0,
			StatsType.자원_전력_최대 => 0,

			StatsType.자원_인력_회복 => 0,
			StatsType.자원_재료_회복 => 0,
			StatsType.자원_전력_회복 => 0,

			StatsType.자원_인력_현재 => 0,
			StatsType.자원_재료_현재 => 0,
			StatsType.자원_전력_현재 => 0,
			_ => 0,
		};
		return baseValue;
	}

	float IStatsValueControl.GetStatsValuePrecent(StatsType type)
	{
		int baseValue = type switch
		{
			StatsType.자원_인력_최대 => 0,
			StatsType.자원_재료_최대 => 0,
			StatsType.자원_전력_최대 => 0,

			StatsType.자원_인력_회복 => 0,
			StatsType.자원_재료_회복 => 0,
			StatsType.자원_전력_회복 => 0,

			StatsType.자원_인력_현재 => 0,
			StatsType.자원_재료_현재 => 0,
			StatsType.자원_전력_현재 => 0,
			_ => 0,
		};
		return baseValue;
	}

	void IStatsValueControl.SetStatsValue(StatsType type, int value)
	{
	}

	void IStatsValueControl.SetStatsValuePrecent(StatsType type, float valuePercent)
	{
	}

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
}
public partial class OperationObject : IStrategyMonoElement, IStrategyElementDestroyer
{
	public IStrategyElement ThisElement => this;
	int IStrategyElement.ID { get => operationID; set => operationID = value; }
	void IStrategyElement.InStrategyCollector()
	{
		if (FactionID >= 0)
		{
			Faction.AddOperation(this);
		}
	}
	void IStrategyElement.OutStrategyCollector()
	{
		if (FactionID >= 0)
		{
			Faction.RemoveOperation(this);
		}
	}
	void IStrategyStartGame.OnStartGame()
	{
		if (FactionID >= 0)
		{
			Faction.AddOperation(this);
		}
	}
	void IStrategyStartGame.OnStopGame()
	{
		if (FactionID >= 0)
		{
			Faction.RemoveOperation(this);
		}
	}

	public IStrategyElementDestroyer ThisDestroyer => this;
	bool IStrategyElementDestroyer.IsDestroy { get; set; }

	public void InitLife()
	{
		ThisDestroyer.IsDestroy = false;
	}

	private void OnDestroy()
	{
		if (!ThisDestroyer.IsDestroy)
		{
			ThisDestroyer.OnDestroy();
		}
	}

	void IStrategyElementDestroyer.OnDestroy()
	{
		ThisDestroyer.IsDestroy = true;
		StrategyElementFactory.Destroy(this);
	}
	private void EmptyUnitDestory()
	{
		ThisDestroyer.OnReservationDestroy();
	}

	private void ControllerDestory()
	{
		ThisDestroyer.OnReservationDestroy();
	}
}
public partial class OperationObject : ISelectable
{
	partial void DeselectSelf()
	{
		(this as ISelectable).SelfDeselect();
	}
	void ISelectable.OnSelect()
	{

	}
	void ISelectable.OnDeselect()
	{

	}
	void ISelectable.OnFirstSelect()
	{
	}
	void ISelectable.OnLastDeselect()
	{
	}
	void ISelectable.OnSingleSelect()
	{
		if (StrategyManager.ViewAndControl.CurrentMode == ViewAndControlModeType.OperationsMode)
		{
			StrategyManager.GameUI.ControlPanelUI.OpenUI();
			var setTarget = StrategyManager.GameUI.ControlPanelUI.ShowOperationPlannerPanel();
			if (setTarget == null) return;
			setTarget.AddTarget(this);
		}
	}
	void ISelectable.OnSingleDeselect()
	{
		if (StrategyManager.ViewAndControl.CurrentMode == ViewAndControlModeType.OperationsMode)
		{
			StrategyManager.GameUI.ControlPanelUI.HideOperationPlannerPanel();
		}
	}
}
public partial class OperationObject : IOperationToPanelAPI
{
	#region ITargetToLabelAPI
	string ITargetToLabelAPI.GetLabelName()
	{
		return TeamName;
	}

	Sprite ITargetToLabelAPI.GetLabelIcon()
	{
		return null;
	}
	Color ITargetToLabelAPI.GetLabelAccentColor()
	{
		return Faction.FactionColor;
	}
	Color ITargetToLabelAPI.GetLabelTextColor()
	{
		return Color.black;
	}
	Vector3 ITargetToLabelAPI.LabelWorldPosition()
	{
		return ThisMovement.CurrentPosition;
	}
	#endregion

	#region ITargetForCardAPI
	Sprite ITargetToCardAPI.GetCardImage()
	{
		return null;
	}
	string ITargetToCardAPI.GetCardName()
	{
		return TeamName;
	}
	#endregion

	#region IOperationForPanel
	string IOperationToPanelAPI.GetFactionName()
	{
		var faction = FactionAPI.ID2Faction(FactionID);
		if (faction == null) return "중립";
		return faction.FactionName;
	}
	public (float[] values, float total, float max) GetShieldDetailValue()
	{
		return (Array.Empty<float>(), 0, 0);
	}
	public (float[] values, float total, float max) GetPersonnelDetailValue()
	{
		return (Array.Empty<float>(), 0, 0);
	}
	public (float[] values, float total, float max) GetMaterialDetailValue()
	{
		return (Array.Empty<float>(), 0, 0);
	}
	public (float[] values, float total, float max) GetElectricDetailValue()
	{
		return (Array.Empty<float>(), 0, 0);
	}
	public (float total, float max) GetShieldSimpleValue()
	{
		return (0, 0);
	}
	public (float total, float max) GetPersonnelSimpleValue()
	{
		return (0, 0);
	}
	public (float total, float max) GetMaterialSimpleValue()
	{
		return (0, 0);
	}
	public (float total, float max) GetElectricSimpleValue()
	{
		return (0, 0);
	}
	public string GetShieldValueText(bool simpleText = false)
	{
		if (simpleText)
		{
			(float total, float max) = GetShieldSimpleValue();
			string text = $"보호막: {total}/{max}";
			return text;
		}
		else
		{
			(float[] values, float total, float max) = GetShieldDetailValue();
			string text = $"보호막: {total}/{max}";
			int length = values.Length;
			for (int i = 0 ; i < length ; i++)
			{
				float value = values[i];
				text += $"\t{value:+#;-#;0}";
			}

			return text;
		}
	}
	public string GetPersonnelValueText(bool simpleText = false)
	{
		if (simpleText)
		{
			(float total, float max) = GetPersonnelSimpleValue();
			string text = $"인력: {total}/{max}";
			return text;
		}
		else
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
	}
	public string GetMaterialValueText(bool simpleText = false)
	{
		if (simpleText)
		{
			(float total, float max) = GetMaterialSimpleValue();
			string text = $"재료: {total}/{max}";
			return text;
		}
		else
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
	}
	public string GetElectricValueText(bool simpleText = false)
	{
		if (simpleText)
		{
			(float total, float max) = GetElectricSimpleValue();
			string text = $"전력: {total}/{max}";
			return text;
		}
		else
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
	}
	#endregion
}