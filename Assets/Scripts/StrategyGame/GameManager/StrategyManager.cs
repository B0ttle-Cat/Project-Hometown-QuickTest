using UnityEngine;

using static StrategyGamePlayData;

public class StrategyManager : MonoBehaviour
{
	public static StrategyManager Manager;

	public static CommonGamePlayData GamePlayData => Manager == null ? null : Manager.gamePlayData;
	public static StrategyElementCollector Collector => Manager == null ? null : Manager.collector;
	public static StrategyMissionTree Mission => Manager == null ? null : Manager.mission;
	public static StrategyStatistics Statistics => Manager == null ? null : Manager.statistics;
	public static StrategyUpdate Updater => Manager == null ? null : Manager.updater;
	public static StrategyMouseSelecter Selecter => Manager == null ? null : Manager.selecter;
	public static KeyPairDisplayName Key2Name => Manager == null ? null : Manager.key2Name;
	public static KeyPairSprite Key2Sprite => Manager == null ? null : Manager.key2Sprite;
	//public static 
	public bool IsGameSceneReady { get; private set; }

	CommonGamePlayData gamePlayData;
	private StrategyElementCollector collector;
	private StrategyMissionTree mission;
	private StrategyStatistics statistics;
	private StrategyUpdate updater;
	private StrategyMouseSelecter selecter;
	private KeyPairDisplayName key2Name;
	private KeyPairSprite key2Sprite;
	private void Awake()
	{
		IsGameSceneReady = false;
		Manager = this;
		gamePlayData = new CommonGamePlayData();
		collector = GetComponentInChildren<StrategyElementCollector>();
		mission = GetComponentInChildren<StrategyMissionTree>();
		statistics = GetComponentInChildren<StrategyStatistics>();
		updater = GetComponentInChildren<StrategyUpdate>();
		selecter = GetComponentInChildren<StrategyMouseSelecter>();
	}
	private void OnDestroy()
	{
		Manager = null;

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

		if ((updater = updater != null ? updater : GetComponentInChildren<StrategyUpdate>()) != null)
		{
			updater.enabled = false;
		}
		if ((selecter = selecter != null ? selecter : GetComponentInChildren<StrategyMouseSelecter>()) != null)
		{
			selecter.enabled = false;
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
		key2Name = KeyPairDisplayName.Load(StrategyGamePlayData.PreparedData.GetData().LanguageType, "Strategy");
		key2Sprite = KeyPairSprite.Load(StrategyGamePlayData.PreparedData.GetData().LanguageType, "Strategy");
		
		// 시작 세력 세팅
		setter.OnStartSetter_Faction();

		// CB 세팅
		setter.OnStartSetter_Sector();

		// Unit 세팅
		setter.OnStartSetter_Unit();

		// 점령 지역 세팅
		setter.OnStartSetter_Capture();

		// 메인 미션 세팅
		Mission.InitMainMission();

		// 서브 미션 세팅
		Mission.InitSubMission();

		// 시작 전 대기 프레임
		await Awaitable.NextFrameAsync();

		Destroy(setter);
		setter = null;

		if (updater == null) gameObject.AddComponent<StrategyUpdate>();
		else updater.enabled = true;

		if (selecter == null) gameObject.AddComponent<StrategyMouseSelecter>();
		else selecter.enabled = true;

		OnStartGame();
	}
	private void OnStopGame()
	{
		var allComponent = GameObject.FindObjectsByType<Component>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
		int length = allComponent.Length;
		for (int i = 0 ; i < allComponent.Length ; i++)
		{
			if (allComponent[i] is IStartGame startGame)
			{
				startGame.OnStopGame();
			}
		}
	}
	private void OnStartGame()
	{
		var allComponent = GameObject.FindObjectsByType<Component>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
		int length = allComponent.Length;
		for (int i = 0 ; i < allComponent.Length ; i++)
		{
			if (allComponent[i] is IStartGame startGame)
			{
				startGame.OnStartGame();
			}
		}
	}
}
