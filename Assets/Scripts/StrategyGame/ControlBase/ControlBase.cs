using UnityEngine;

public partial class ControlBase // ControlBaseData
{
	public void Init(StrategyStartSetterData.ControlBaseData data)
	{
		maxManpower = data.maxManpower;

		maxSuppliePoint = data.maxSuppliePoint;
		maxElectricPoint = data.maxElectricPoint;

		defenseAddition = data.defenseAddition;
		defenseMultiplication = data.defenseMultiplication;
		attackAddition = data.attackAddition;
		attackMultiplication = data.attackMultiplication;
		hpRecoveryAddition = data.hpRecoveryAddition;
		hpRecoveryMultiplication = data.hpRecoveryMultiplication;
		moraleRecoveryAddition = data.moraleRecoveryAddition;
		moraleRecoveryMultiplication = data.moraleRecoveryMultiplication;
	}
	public string ControlBaseName
	{
		get => string.IsNullOrWhiteSpace(controlBaseName) ? gameObject.name : controlBaseName;
		set => controlBaseName = value;
	}
	public int MaxManpower { get => maxManpower; set => maxManpower = value; }
	public int MaxSuppliePoint { get => maxSuppliePoint; set => maxSuppliePoint = value; }
	public float MaxElectricPoint { get => maxElectricPoint; set => maxElectricPoint = value; }
	public float OccupationTime { get => occupationSpeed; set => occupationSpeed = value; }
	public float DefenseAddition { get => defenseAddition; set => defenseAddition = value; }
	public float DefenseMultiplication { get => defenseMultiplication; set => defenseMultiplication = value; }
	public float AttackAddition { get => attackAddition; set => attackAddition = value; }
	public float AttackMultiplication { get => attackMultiplication; set => attackMultiplication = value; }
	public float HpRecoveryAddition { get => hpRecoveryAddition; set => hpRecoveryAddition = value; }
	public float HpRecoveryMultiplication { get => hpRecoveryMultiplication; set => hpRecoveryMultiplication = value; }
	public float MoraleRecoveryAddition { get => moraleRecoveryAddition; set => moraleRecoveryAddition = value; }
	public float MoraleRecoveryMultiplication { get => moraleRecoveryMultiplication; set => moraleRecoveryMultiplication = value; }

	private string controlBaseName;

	private int maxManpower;
	private int maxSuppliePoint;
	private float maxElectricPoint;

	private float occupationSpeed;

	private float defenseAddition;
	private float defenseMultiplication;

	private float attackAddition;
	private float attackMultiplication;

	private float hpRecoveryAddition;
	private float hpRecoveryMultiplication;

	private float moraleRecoveryAddition;
	private float moraleRecoveryMultiplication;
}
public partial class ControlBase // OccupationData
{
	public void Init(StrategyStartSetterData.OccupationData data)
	{
		currentFactionID = StrategyGameManager.Collector.FactionNameToID(data.occupyingFaction);
		occupationProgress = data.occupationProgress;
		suppliesQuantity = data.suppliesQuantity;

		if (controlBaseOccupation == null) controlBaseOccupation = GetComponent<ControlBaseOccupation>();
		if (controlBaseOccupation != null) controlBaseOccupation.SetOccupation(currentFactionID);
	}
	public Faction OccupyingFaction
	{
		get => StrategyGameManager.Collector.FindFaction(currentFactionID);
		set => currentFactionID = value == null ? -1 : value.FactionID;
	}
	public float OccupationProgress { get => occupationProgress; set => occupationProgress = Mathf.Clamp01(value); }
	public int SuppliesQuantity { get => suppliesQuantity; set => suppliesQuantity = value; }

	private int currentFactionID = -1;
	private float occupationProgress;
	private int suppliesQuantity;
}
public partial class ControlBase : MonoBehaviour
{
	public void Init()
	{
		ControlBaseName = gameObject.name;
		currentFactionID = -1;
		OccupationProgress = 1;

		maxManpower = 0;
		maxSuppliePoint = 0;
		maxElectricPoint = 0;
		defenseAddition = 0;
		defenseMultiplication = 1;
		attackAddition = 0;
		attackMultiplication = 1;
		hpRecoveryAddition = 0;
		hpRecoveryMultiplication = 1;
		moraleRecoveryAddition = 0;
		moraleRecoveryMultiplication = 1;

		controlBaseOccupation = GetComponentInChildren<ControlBaseOccupation>();
		controlBaseColor = GetComponentInChildren<ControlBaseColor>();
	}

	private ControlBaseOccupation controlBaseOccupation;
	private ControlBaseColor controlBaseColor;

	// Update is called once per frame
	public void UpdateControlBase()
	{
		UpdateOccupation();
		UpdateColor();

		void UpdateOccupation()
		{
			if (controlBaseOccupation == null) return;
			controlBaseOccupation.UpdateOccupation(this);
		}
		void UpdateColor()
		{
			if (controlBaseColor == null) return;
			controlBaseColor.UpdateColor(OccupyingFaction, OccupationProgress);
		}
	}
}


public partial class ControlBase : IStrategyElement
{
	public bool IsInCollector { get; set; }

	public void InStrategyCollector()
	{
	}

	public void OutStrategyCollector()
	{
	}
}