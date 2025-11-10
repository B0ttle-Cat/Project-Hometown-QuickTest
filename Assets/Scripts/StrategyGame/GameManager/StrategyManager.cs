using System.Linq;

using UnityEngine;

using static StrategyGamePlayData;

public class StrategyManager : MonoBehaviour
{
	public static StrategyManager Manager;
	
	public static bool IsReady => Manager != null && Manager.didAwake;
	public static bool IsNotReady => !IsReady;
	public static Camera MainCamera => Manager == null ? null : Manager.mainCamera;
	public static StrategyGameUI GameUI => Manager == null ? null : Manager.gameUI;
	public static KeyValueData GamePlayTempData => Manager == null ? null : Manager.gamePlayTempData;
	public static StrategyElementCollector Collector => Manager == null ? null : Manager.collector;
	public static StrategyMissionTree Mission => Manager == null ? null : Manager.mission;
	public static StrategyStatistics Statistics => Manager == null ? null : Manager.statistics;
	public static StrategyTime Time => Manager == null ? null : Manager.time;
	public static StrategyUpdate Updater => Manager == null ? null : Manager.updater;
	public static StrategyMouseSelecter Selecter => Manager == null ? null : Manager.selecter;
	public static StrategyNodeNetwork SectorNetwork => Manager == null ? null : Manager.sectorNetwork;
	public static StrategyViewAndControlModeChanger ViewAndControl => Manager == null ? null : Manager.viewAndControl;
	public static KeyPairDisplayName Key2Name => Manager == null ? null : Manager.key2Name;
	public static KeyPairSprite Key2Sprite => Manager == null ? null : Manager.key2Sprite;
	public static KeyPairUnitInfo Key2UnitInfo => Manager == null ? null : Manager.key2UnitInfo;

	public static int PlayerFactionID;
	public static GameStartingData PreparedData;
	//public static 
	public bool IsGameSceneReady { get; private set; }

	[SerializeField]
	private Camera mainCamera;
	private StrategyGameUI gameUI;
	private KeyValueData gamePlayTempData;
	private StrategyElementCollector collector;
	private StrategyMissionTree mission;
	private StrategyStatistics statistics;
	private StrategyTime time;
	private StrategyUpdate updater;
	private StrategyMouseSelecter selecter;
	private StrategyNodeNetwork sectorNetwork;
	private StrategyViewAndControlModeChanger viewAndControl;

	private KeyPairDisplayName key2Name;
	private KeyPairSprite key2Sprite;
	private KeyPairUnitInfo key2UnitInfo;
	private void Awake()
	{
		IsGameSceneReady = false;
		Manager = this;
		mainCamera = mainCamera == null ? Camera.main : mainCamera;
		gameUI = FindAnyObjectByType<StrategyGameUI>();
		gamePlayTempData = KeyValueData.Empty;
		collector = GetComponentInChildren<StrategyElementCollector>();
		mission = GetComponentInChildren<StrategyMissionTree>();
		statistics = GetComponentInChildren<StrategyStatistics>();
		updater = GetComponentInChildren<StrategyUpdate>();
		selecter = GetComponentInChildren<StrategyMouseSelecter>();
		sectorNetwork = GetComponentInChildren<StrategyNodeNetwork>();
		viewAndControl = GetComponentInChildren<StrategyViewAndControlModeChanger>();

	}
	private void OnDestroy()
	{
		Manager = null;

		if(gameUI != null)
		{
			gameUI.enabled = false;
			gameUI = null;
		}
		if (collector != null)
		{
			collector.Dispose();
			collector = null;
		}
		if (mission != null)
		{
			mission.Dispose();
			mission = null;
		}
		if (statistics != null)
		{
			statistics.Dispose();
			statistics = null;
		}
		if(updater != null)
		{
			updater.enabled = false;
			updater = null;
		}
		if(selecter != null)
		{
			selecter.enabled = false;
			selecter = null;
		}

		key2Name = null;
		key2Sprite = null;
	}

	void Start()
	{
		GameStart();
	}
	public async void GameStart()
	{
		if (IsGameSceneReady)
		{
			Debug.LogWarning("GameStart: Game Scene is already ready.");
			return;
		}

		Debug.Log("GameStart: Start");
		IsGameSceneReady = true;

		OnStopGame();

		if ((gameUI = gameUI != null ? gameUI : FindAnyObjectByType<StrategyGameUI>()) != null)
		{
			gameUI.DeInit();
			gameUI.enabled = false;
		}
		if ((time = time != null ? time : GetComponentInChildren<StrategyTime>()) != null)
		{
			time.enabled = false;
		}
		if ((updater = updater != null ? updater : GetComponentInChildren<StrategyUpdate>()) != null)
		{
			updater.enabled = false;
		}
		if ((selecter = selecter != null ? selecter : GetComponentInChildren<StrategyMouseSelecter>()) != null)
		{
			selecter.enabled = false;
		}
		if (gameUI == null)
		{
			Debug.LogError("GameStart: No StrategyGameUI found in any GameObject.");
			return;
		}
		await Awaitable.NextFrameAsync();

		// 초기화 전용 컴퍼넌트 확보
		StrategyStartSetter setter = GetComponent<StrategyStartSetter>();

		if (setter == null)
		{
			Debug.LogError("GameStart: No StrategyStartSetter ThisComponent found on GameManager.");
			return;
		}
		if (!setter.StartSetterIsValid())
		{
			Debug.LogError("GameStart: StrategyStartSetter is not valid.");
			return;
		}
		setter.OnSetPreparedData();
		if (Collector == null)
		{
			Debug.LogError("GameStart: No StrategyElementCollector ThisComponent found in children of GameManager.");
			return;
		}
		if (Mission == null)
		{
			Debug.LogError("GameStart: No StrategyMissionTree ThisComponent found in children of GameManager.");
			return;
		}
		if (Statistics == null)
		{
			Debug.LogError("GameStart: No StrategyStatistics ThisComponent found in children of GameManager.");
			return;
		}

		Collector.Init();
		Mission.Init();
		Statistics.Init();
		Time.Init();
		ViewAndControl.Init();
		var preparedData = StrategyManager.PreparedData.GetData();
		key2Name = KeyPairDisplayName.Load(preparedData.LanguageType, "_Default");
		key2Sprite = KeyPairSprite.Load(preparedData.LanguageType, "_Default");
		key2UnitInfo = KeyPairUnitInfo.Load(preparedData.LanguageType, "_Default");
		// 시작 세력 세팅
		setter.OnStartSetter_Faction();

		// Sector 세팅
		await setter.OnStartSetter_Sector();

		// Sector Network 초기화
		await setter.OnStartSetter_SectorNetwork(sectorNetwork);

		// Unit 세팅
		await setter.OnStartSetter_Unit();

		// 점령 지역 세팅
		setter.OnStartSetter_Capture();

		setter.OnStartSetter_Mission(mission);

		// 시작 전 대기 프레임
		await Awaitable.NextFrameAsync();

		Destroy(setter);
		setter = null;

		if (gameUI != null)
		{
			gameUI.enabled = true;
			gameUI.Init();
		}

		if (updater == null) updater = gameObject.AddComponent<StrategyUpdate>();
		else updater.enabled = true;

		if (selecter == null) selecter = gameObject.AddComponent<StrategyMouseSelecter>();
		else selecter.enabled = true;

		if(time != null)
		{
			updater.SetTime(time);
			time.enabled = true;
		}

		OnStartGame();
	}
	private void OnStopGame()
	{
		var allComponent = GameObject.FindObjectsByType<Component>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
		var allList = allComponent.Where(c => c is IStrategyStartGame).Select(c => c as IStrategyStartGame).OrderBy(i => i.StopEventOrder());
		foreach (var item in allList)
		{
			item.OnStopGame();
		}
	}
	private void OnStartGame()
	{
		var allComponent = GameObject.FindObjectsByType<Component>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
		var allList = allComponent.Where(c => c is IStrategyStartGame).Select(c => c as IStrategyStartGame).OrderBy(i => i.StartEventOrder());
		foreach (var item in allList)
		{
			item.OnStartGame();
		}
	}
}
