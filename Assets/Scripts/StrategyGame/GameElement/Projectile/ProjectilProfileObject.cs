using Sirenix.OdinInspector;

using UnityEditor;

using UnityEngine;

using static StrategyGamePlayData;

using Object = UnityEngine.Object;

[CreateAssetMenu(fileName = "ProjectileProfileObject", menuName = "Scriptable Objects/StrategyGame/ProjectileProfileObject")]
public class ProjectileProfileObject : ScriptableObject
{
	[InlineButton("CreatePrefab","New",ShowIf = "@prefab == null")]
	public GameObject prefab;
	public string displayName;

	[InlineButton("PushData"), InlineButton("PullData")]
	public ProjectileKey projectileKey;

#if UNITY_EDITOR
	private void CreatePrefab()
	{
		string basePath = "Assets/Resources/Prefabs/ProjectileObject/_ProjectileObject.prefab";
		string newPrefabPath = $"Assets/Resources/Prefabs/ProjectileObject/{projectileKey}.prefab";

		GameObject basePrefab = prefab != null
			? PrefabUtility.GetCorrespondingObjectFromOriginalSource(prefab)
			: AssetDatabase.LoadAssetAtPath<GameObject>(basePath);

		if (basePrefab == null)
		{
			Debug.LogError($"Base prefab not found at {basePath}");
			return;
		}

		if (prefab != null && prefab.name == projectileKey.ToString())
		{
			Debug.Log($"Prefab '{projectileKey}' already exists. Creation skipped.");
			return;
		}

		GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
		instance.name = projectileKey.ToString();

		GameObject variant = PrefabUtility.SaveAsPrefabAsset(instance, newPrefabPath);
		if (variant != null)
		{
			prefab = variant;
			EditorUtility.SetDirty(this);
			Debug.Log($"Created prefab variant: {newPrefabPath}");
		}
		else
		{
			Debug.LogError($"Failed to create prefab at {newPrefabPath}");
		}

		Object.DestroyImmediate(instance);
	}
	private void PullData()
	{
		if (prefab == null)
		{
			Debug.LogWarning("Prefab is null. Cannot pull data.");
			return;
		}

		if (!prefab.TryGetComponent<ProjectileObject>(out var obj))
		{
			Debug.LogWarning("Prefab does not contain ProjectileObject component.");
			return;
		}

		if (obj.StatsData == null)
		{
			Debug.LogWarning("ProjectileObject.statsData is null. Cannot pull data.");
			return;
		}

		var st = obj.StatsData;

		weaponType = st.WeaponType;
		moveStartSpeed = st.MoveStartSpeed;
		isShiftSpeed = st.IsShiftSpeed;
		moveMaxSpeed = st.MoveMaxSpeed;
		moveSpeedCurve = st.MoveSpeedCurve;
		timeFromStartToMaxSpeed = st.TimeFromStartToMaxSpeed;

		homingEnabled = st.HomingEnabled;
		homingTurnSpeed = st.HomingTurnSpeed;
		homingActivationDelay = st.HomingActivationDelay;

		lifeTime = st.LifeTime;
		destroyDelayAfterHit = st.DestroyDelayAfterHit;

		collisionRadius = st.CollisionRadius;

		hitDamageMultiplier = st.HitDamageMultiplier;
		hitEffectsFlag = st.HitEffectsFlag;
		hitEffectsTimeMultiplier = st.HitEffectsTimeMultiplier;

		piercingEnable = st.PiercingEnable;
		piercingMinMaxCount = st.PiercingMinMaxPoint;
		piercingFalloffCurve = st.PiercingFalloffCurve;

		explosionEnabled = st.ExplosionEnabled;
		explosionMinMaxRadius = st.ExplosionMinMaxRadius;
		explosionFalloffCurve = st.ExplosionFalloffCurve;

		EditorUtility.SetDirty(this);
		Debug.Log($"Pulled statsData from prefab '{prefab.name}'.");
	}
	private void PushData()
	{
		if (prefab == null)
		{
			Debug.LogWarning("Prefab is null. Cannot push data.");
			return;
		}

		if (!prefab.TryGetComponent<ProjectileObject>(out var obj))
		{
			Debug.LogWarning("Prefab does not contain ProjectileObject component.");
			return;
		}

		obj.Init(this);
		EditorUtility.SetDirty(prefab);
	}
#endif

	[Title("StatsData")]
	[LabelText("공격 속성"), SerializeField]
	private WeaponType weaponType;

	[BoxGroup("Movement")]
	[LabelText("이동 시작 속도"), SerializeField]
	private float moveStartSpeed;
	[ToggleGroup("isShiftSpeed", GroupID = "Movement/T", ToggleGroupTitle = "가속 여부"), SerializeField]
	private bool isShiftSpeed;
	[ToggleGroup("isShiftSpeed",GroupID = "Movement/T"), LabelText("최대 속도"), SerializeField]
	private float moveMaxSpeed;
	[ToggleGroup("isShiftSpeed",GroupID = "Movement/T"), LabelText("속도 커브"), SerializeField]
	private AnimationCurve moveSpeedCurve = AnimationCurve.Linear(0,0,1,1);
	[ToggleGroup("isShiftSpeed",GroupID = "Movement/T"), LabelText("최대 속도 도달 시간"), SerializeField]
	private float timeFromStartToMaxSpeed;

	[ToggleGroup("homingEnabled", GroupID = "Movement/H", ToggleGroupTitle ="유도 여부"), SerializeField]
	private bool homingEnabled;
	[ToggleGroup("homingEnabled", GroupID = "Movement/H"), LabelText("유도 활성 지연"), SerializeField]
	private float homingActivationDelay;
	[ToggleGroup("homingEnabled", GroupID = "Movement/H"), LabelText("유도 회전 속도"), SerializeField]
	private float homingTurnSpeed;
	[ToggleGroup("homingEnabled", GroupID = "Movement/H"), LabelText("MaxSpeed 일때 회전 속도"), ShowIf("isShiftSpeed"), SerializeField]
	private float homingTurnSpeedWhenMaxSpeed;
	[ToggleGroup("homingEnabled", GroupID = "Movement/H"), LabelText("유도 한계 각도"), SerializeField]
	[Range(0f,180f)]
	private float homingLimitAngle;
	[ToggleGroup("homingEnabled", GroupID = "Movement/H"), LabelText("유도 한계 거리"), SerializeField]
	private float homingLimitDistance;

	[BoxGroup("LifeCycle"), LabelText("생존 시간"), SerializeField]
	private float lifeTime;
	[BoxGroup("LifeCycle"), LabelText("명중 후 삭제 지연"), SerializeField]
	private float destroyDelayAfterHit;

	[BoxGroup("Collision"), LabelText("충돌 반경"), SerializeField]
	private float collisionRadius;

	[BoxGroup("Hit"), LabelText("명중시 피해 배율"), SerializeField]
	private float hitDamageMultiplier = 1f;
	[BoxGroup("Hit"), LabelText("명중시 상태이상 플래그"), SerializeField]
	private StatusEffectsFlag hitEffectsFlag = StatusEffectsFlag.None;
#if UNITY_EDITOR
	private bool _isHitStatusEffects => hitEffectsFlag != StatusEffectsFlag.None;
	[EnableIf("_isHitStatusEffects")]
#endif
	[BoxGroup("Hit"), LabelText("명중시 상태이상 시간 배율"), SerializeField]
	private float hitEffectsTimeMultiplier = 1;

	[ToggleGroup("piercingEnable", GroupID = "Hit/P", ToggleGroupTitle ="관통 사용 여부"), SerializeField]
	private bool piercingEnable = false;
	[ToggleGroup("piercingEnable", GroupID = "Hit/P"), LabelText("관통 최소/최대 회수"), SerializeField]
	private Vector2Int piercingMinMaxCount;
	[ToggleGroup("piercingEnable", GroupID = "Hit/P"), LabelText("관통 효과 감쇠 커브"), SerializeField]
	private AnimationCurve piercingFalloffCurve;
#if UNITY_EDITOR
	[HorizontalGroup("Hit/P/Test"), ShowInInspector, LabelText("Test 관통 횟수"), OnValueChanged("TestPiercingFalloffMultiplier")]
	[PropertyRange("testMinTestpiercingCount","testMaxTestpiercingCount")]
	private int testpiercingPoint;
	private float testMinTestpiercingCount => 0;
	private float testMaxTestpiercingCount => Mathf.Max(piercingMinMaxCount.x, piercingMinMaxCount.y);
	[HorizontalGroup("Hit/P/Test"), ShowInInspector, LabelText("Result"),ReadOnly]
	private float testPiercingFalloffMultiplier;
	private void TestPiercingFalloffMultiplier()
	{
		testPiercingFalloffMultiplier = PiercingFalloffMultiplier(testpiercingPoint);
	}
#endif


	[ToggleGroup("explosionEnabled", GroupID = "Hit/E", ToggleGroupTitle ="폭발 사용 여부"), SerializeField]
	private bool explosionEnabled = false;
	[ToggleGroup("explosionEnabled", GroupID = "Hit/E"), LabelText("폭발 최소/최대 반경"), SerializeField]
	private Vector2 explosionMinMaxRadius;
	[ToggleGroup("explosionEnabled", GroupID = "Hit/E"),  LabelText("폭발 효과 감쇠 커브"), SerializeField]
	private AnimationCurve explosionFalloffCurve;
#if UNITY_EDITOR
	[HorizontalGroup("Hit/E/Test"), ShowInInspector, LabelText("Test 폭발 거리"), OnValueChanged("TestExplosionFalloffMultiplier")]
	[PropertyRange("testMintExplosionDistance","testMaxtExplosionDistance")]
	private float testExplosionDistance;
	private float testMintExplosionDistance => 0;
	private float testMaxtExplosionDistance => Mathf.Max(explosionMinMaxRadius.x, explosionMinMaxRadius.y);
	[HorizontalGroup("Hit/E/Test"), ShowInInspector, LabelText("Result"),ReadOnly]
	private float testExplosionFalloffMultiplier;
	private void TestExplosionFalloffMultiplier()
	{
		testExplosionFalloffMultiplier = ExplosionFalloffMultiplier(testExplosionDistance);
	}
#endif

	public WeaponType WeaponType => weaponType;

	public float MoveStartSpeed => moveStartSpeed;
	public bool IsShiftSpeed => isShiftSpeed;
	public float MoveMaxSpeed => moveMaxSpeed;
	public AnimationCurve MoveSpeedCurve => moveSpeedCurve;
	public float TimeFromStartToMaxSpeed => timeFromStartToMaxSpeed;

	public bool HomingEnabled => homingEnabled;
	public float HomingTurnSpeed => homingTurnSpeed;
	public float HomingActivationDelay => homingActivationDelay;

	public float LifeTime => lifeTime;
	public float DestroyDelayAfterHit => destroyDelayAfterHit;

	public float CollisionRadius => collisionRadius;

	public float HitDamageMultiplier => hitDamageMultiplier;
	public StatusEffectsFlag HitEffectsFlag => hitEffectsFlag;
	public float HitEffectsTimeMultiplier => hitEffectsTimeMultiplier;

	public bool PiercingEnable => piercingEnable;
	public Vector2Int PiercingMinMaxPoint => piercingMinMaxCount;
	public AnimationCurve PiercingFalloffCurve => piercingFalloffCurve;
	public float PiercingFalloffMultiplier(int currentCount)
	{
		if (!PiercingEnable) return 1f;
		Vector2Int minMax = PiercingMinMaxPoint;
		float min = Mathf.Min(minMax.x, minMax.y);
		float max = Mathf.Max(minMax.x, minMax.y);
		float point = (float)currentCount;
		if (Mathf.Approximately(min, max))
		{
			return 1f;
		}
		float rate = (point - min) / (max - min);
		return PiercingFalloffCurve.Evaluate(rate);
	}

	public bool ExplosionEnabled => explosionEnabled;
	public Vector2 ExplosionMinMaxRadius => explosionMinMaxRadius;
	public AnimationCurve ExplosionFalloffCurve => explosionFalloffCurve;
	public float ExplosionFalloffMultiplier(float currentDistance)
	{
		if (!ExplosionEnabled) return 1f;
		Vector2 minMax = ExplosionMinMaxRadius;
		float min = Mathf.Min(minMax.x, minMax.y);
		float max = Mathf.Max(minMax.x, minMax.y);
		float point = currentDistance;
		if (Mathf.Approximately(min, max))
		{
			return 1f;
		}
		float rate = (point - min) / (max - min);
		return ExplosionFalloffCurve.Evaluate(rate);
	}
#if UNITY_EDITOR
	private void Reset()
	{
		SetTestStatsValue();
	}
	[Button]
	private void SetTestStatsValue()
	{
		weaponType = WeaponType.일반;

		moveStartSpeed = 10f;
		isShiftSpeed = false;
		moveMaxSpeed = 20f;
		moveSpeedCurve = AnimationCurve.Linear(0, 0, 1, 1);
		timeFromStartToMaxSpeed = 2f;

		homingEnabled = false;
		homingActivationDelay = 0.0f;
		homingTurnSpeed = 180f;
		homingTurnSpeedWhenMaxSpeed = 180f;
		homingLimitAngle = 180f;
		homingLimitDistance = float.PositiveInfinity;

		lifeTime = 10f;
		destroyDelayAfterHit = 0.1f;

		collisionRadius = 0.1f;

		hitDamageMultiplier = 1f;
		hitEffectsFlag = StatusEffectsFlag.None;
		hitEffectsTimeMultiplier = 1f;

		piercingEnable = false;
		piercingMinMaxCount = new Vector2Int(1, 1);
		piercingFalloffCurve = AnimationCurve.Linear(0, 1, 1, 0);

		explosionEnabled = false;
		explosionMinMaxRadius = new Vector2(1f, 5f);
		explosionFalloffCurve = AnimationCurve.Linear(0, 1, 1, 0);
	}

#endif
}
