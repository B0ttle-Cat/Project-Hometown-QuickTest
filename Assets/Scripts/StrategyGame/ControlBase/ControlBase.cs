using UnityEngine;

public partial class ControlBase // ControlBaseData
{
	public void Init(StrategyStartSetterData.ControlBaseData data)
	{
		maxManpower = data.maxManpower;

		maxSupplyPoint = data.maxSupplyPoint;
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
	public int MaxSupplyPoint { get => maxSupplyPoint; set => maxSupplyPoint = value; }
	public float MaxElectricPoint { get => maxElectricPoint; set => maxElectricPoint = value; }
	public float CaptureTime { get => captureSpeed; set => captureSpeed = value; }
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
	private int maxSupplyPoint;
	private float maxElectricPoint;

	private float captureSpeed;

	private float defenseAddition;
	private float defenseMultiplication;

	private float attackAddition;
	private float attackMultiplication;

	private float hpRecoveryAddition;
	private float hpRecoveryMultiplication;

	private float moraleRecoveryAddition;
	private float moraleRecoveryMultiplication;
}
public partial class ControlBase // CaptureData
{
	public void Init(StrategyStartSetterData.CaptureData data)
	{
		currentFactionID = StrategyManager.Collector.FactionNameToID(data.captureFaction);
		captureProgress = data.captureProgress;
		currentSupplyPoint = data.supplysQuantity;

		if (controlBaseCapture == null) controlBaseCapture = GetComponent<ControlBaseCapture>();
		if (controlBaseCapture != null) controlBaseCapture.SetCapture(currentFactionID);
	}
	public Faction CaptureFaction
	{
		get => StrategyManager.Collector.FindFaction(currentFactionID);
		set => currentFactionID = value == null ? -1 : value.FactionID;
	}
	public int CaptureFactionID
	{
		get => currentFactionID;
		set => currentFactionID = value;
	}
	public float CaptureProgress { get => captureProgress; set => captureProgress = Mathf.Clamp01(value); }
	public int CurrentSupplyPoint { get => currentSupplyPoint; set => currentSupplyPoint = value; }

	private int currentFactionID = -1;
	private float captureProgress;
	private int currentSupplyPoint;
}
public partial class ControlBase : MonoBehaviour
{
	public void Init()
	{
		ControlBaseName = gameObject.name;
		currentFactionID = -1;
		CaptureProgress = 1;

		maxManpower = 0;
		maxSupplyPoint = 0;
		maxElectricPoint = 0;
		defenseAddition = 0;
		defenseMultiplication = 1;
		attackAddition = 0;
		attackMultiplication = 1;
		hpRecoveryAddition = 0;
		hpRecoveryMultiplication = 1;
		moraleRecoveryAddition = 0;
		moraleRecoveryMultiplication = 1;

		controlBaseCapture = GetComponentInChildren<ControlBaseCapture>();
		controlBaseColor = GetComponentInChildren<ControlBaseColor>();
	}

	private ControlBaseCapture controlBaseCapture;
	private ControlBaseColor controlBaseColor;

	// Update is called once per frame
	public void UpdateControlBase()
	{
		UpdateCapture();
		UpdateColor();

		void UpdateCapture()
		{
			if (controlBaseCapture == null) return;
			controlBaseCapture.UpdateCapture(this);
		}
		void UpdateColor()
		{
			if (controlBaseColor == null) return;
			controlBaseColor.UpdateColor(CaptureFaction, CaptureProgress);
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