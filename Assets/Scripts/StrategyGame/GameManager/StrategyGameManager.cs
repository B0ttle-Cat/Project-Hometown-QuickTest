using UnityEngine;

public class StrategyGameManager : MonoBehaviour
{
	public static StrategyGameManager Manager;
	public static StrategyElementCollector Collector => Manager == null ? null : Manager.collector;

	public bool IsGameSceneReady { get; private set ; }

	private StrategyElementCollector collector;
	private void Awake()
	{
		IsGameSceneReady = false;
		Manager = this;
		collector = GetComponentInChildren<StrategyElementCollector>();
	}
	private void OnDestroy()
	{
		Manager = null;
	}

	void Start()
	{
		GameStart();
	}
	public async void GameStart()
	{
		if(IsGameSceneReady)
		{
			Debug.LogWarning("GameStart: Game Scene is already ready.");
			return;
		}

		Debug.Log("GameStart: Start");
		IsGameSceneReady = true;

		if(TryGetComponent<StrategyGameUpdate>(out var _update))
		{
			_update.enabled = false;
			await Awaitable.NextFrameAsync();
		}

		// 초기화에 필요한 컴퍼넌트 확보
		StrategyStartSetter setter = GetComponent<StrategyStartSetter>();


		// StrategyStartSetter 컴퍼넌트가 있으면 초기화
		if (setter == null)
		{
			Debug.LogError("GameStart: No StrategyStartSetter component found on GameManager.");
			return;
		}
		if (!setter.StartSetterIsValid())
		{
			Debug.LogError("GameStart: StrategyStartSetter is not valid.");
			return;
		}
		if(Collector == null)
		{
			Debug.LogError("GameStart: No StrategyElementCollector component found in children of GameManager.");
			return;
		}
		Collector.InitList();

		// 시작 세력 세팅
		setter.OnStartSetter_Faction();

		// CB 세팅
		setter.OnStartSetter_ControlBase();

		// Unit 세팅
		setter.OnStartSetter_Unit();

		// 점령 지역 세팅
		setter.OnStartSetter_Occupation();

		// 시작 전 대기 프레임
		await Awaitable.NextFrameAsync();

		Destroy(setter);
		Collector.ForeachAll(element =>
		{
			element.OnStartGame();
		});

		if (_update == null) gameObject.AddComponent<StrategyGameUpdate>();
		else _update.enabled = true;
	}
}
