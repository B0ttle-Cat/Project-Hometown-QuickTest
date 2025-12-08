using Sirenix.OdinInspector;

using UnityEditor;

using UnityEngine;

using static StrategyGamePlayData;

using Object = UnityEngine.Object;

[CreateAssetMenu(fileName = "ProjectilProfileObject", menuName = "Scriptable Objects/StrategyGame/ProjectilProfileObject")]
public class ProjectilProfileObject : ScriptableObject
{
	[InlineButton("CreatePrefab","New",ShowIf = "@prefab == null")]
	public GameObject prefab;

	[InlineButton("PushData"), InlineButton("PullData")]
	public ProjectileKey projectilKey;

#if UNITY_EDITOR
	private void CreatePrefab()
	{
		string basePath = "Assets/Resources/Prefabs/ProjectilObject/_ProjectilObject.prefab";
		string newPrefabPath = $"Assets/Resources/Prefabs/ProjectilObject/{projectilKey}.prefab";

		GameObject basePrefab = prefab != null
			? PrefabUtility.GetCorrespondingObjectFromOriginalSource(prefab)
			: AssetDatabase.LoadAssetAtPath<GameObject>(basePath);

		if (basePrefab == null)
		{
			Debug.LogError($"Base prefab not found at {basePath}");
			return;
		}

		if (prefab != null && prefab.name == projectilKey.ToString())
		{
			Debug.Log($"Prefab '{projectilKey}' already exists. Creation skipped.");
			return;
		}

		GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
		instance.name = projectilKey.ToString();

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
		piercingMinPoint = st.PiercingMinPoint;
		piercingMaxPoint = st.PiercingMaxPoint;
		piercingFalloffCurve = st.PiercingFalloffCurve;

		explosionEnabled = st.ExplosionEnabled;
		explosionMinRadius = st.ExplosionMinRadius;
		explosionMaxRadius = st.ExplosionMaxRadius;
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
	private float moveStartSpeed = 10f;
	[ToggleGroup("isShiftSpeed", GroupID = "Movement/T", ToggleGroupTitle = "가속 여부"), SerializeField]
	private bool isShiftSpeed = false;
	[ToggleGroup("isShiftSpeed",GroupID = "Movement/T"), LabelText("최대 속도"), SerializeField]
	private float moveMaxSpeed = 20f;
	[ToggleGroup("isShiftSpeed",GroupID = "Movement/T"), LabelText("속도 커브"), SerializeField]
	private AnimationCurve moveSpeedCurve = AnimationCurve.Linear(0,0,1,1);
	[ToggleGroup("isShiftSpeed",GroupID = "Movement/T"), LabelText("최대 속도 도달 시간"), SerializeField]
	private float timeFromStartToMaxSpeed = 1f;

	[ToggleGroup("homingEnabled", GroupID = "Movement/H", ToggleGroupTitle ="유도 여부"), SerializeField]
	private bool homingEnabled = false;
	[ToggleGroup("homingEnabled", GroupID = "Movement/H"), LabelText("유도 활성 지연"), SerializeField]
	private float homingActivationDelay = 0f;
	[ToggleGroup("homingEnabled", GroupID = "Movement/H"), LabelText("유도 회전 속도"), SerializeField]
	private float homingTurnSpeed = 180f;
	[ToggleGroup("homingEnabled", GroupID = "Movement/H"), LabelText("MaxSpeed 일때 회전 속도"), ShowIf("isShiftSpeed"), SerializeField]
	private float homingTurnSpeedWhenMaxSpeed = 180f;

	[ToggleGroup("homingEnabled", GroupID = "Movement/H"), LabelText("유도 한계 각도"), SerializeField]
	[Range(0f,180f)]
	private float homingLimitAngle = 180;
	[ToggleGroup("homingEnabled", GroupID = "Movement/H"), LabelText("유도 한계 거리"), SerializeField]
	private float homingLimitDistance = float.PositiveInfinity;

	[BoxGroup("LifeCycle"), LabelText("생존 시간"), SerializeField]
	private float lifeTime = 5f;
	[BoxGroup("LifeCycle"), LabelText("명중 후 삭제 지연"), SerializeField]
	private float destroyDelayAfterHit = 0.1f;

	[BoxGroup("Collision"), LabelText("충돌 반경"), SerializeField]
	private float collisionRadius = 0.1f;

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
	[ToggleGroup("piercingEnable", GroupID = "Hit/P"), LabelText("관통 최소 점수"), SerializeField]
	private int piercingMinPoint = 1;
	[ToggleGroup("piercingEnable", GroupID = "Hit/P"), LabelText("관통 최대 점수"), SerializeField]
	private int piercingMaxPoint = 1;
	[ToggleGroup("piercingEnable", GroupID = "Hit/P"), LabelText("관통 효과 감쇠 커브"), SerializeField]
	private AnimationCurve piercingFalloffCurve;

	[ToggleGroup("explosionEnabled", GroupID = "Hit/E", ToggleGroupTitle ="폭발 사용 여부"), SerializeField]
	private bool explosionEnabled = false;
	[ToggleGroup("explosionEnabled", GroupID = "Hit/E"), LabelText("폭발 최소 반경"), SerializeField]
	private float explosionMinRadius = 0f;
	[ToggleGroup("explosionEnabled", GroupID = "Hit/E"), LabelText("폭발 최대 반경"), SerializeField]
	private float explosionMaxRadius = 0f;
	[ToggleGroup("explosionEnabled", GroupID = "Hit/E"),  LabelText("폭발 효과 감쇠 커브"), SerializeField]
	private AnimationCurve explosionFalloffCurve;


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
	public int PiercingMinPoint => piercingMinPoint;
	public int PiercingMaxPoint => piercingMaxPoint;
	public AnimationCurve PiercingFalloffCurve => piercingFalloffCurve;

	public bool ExplosionEnabled => explosionEnabled;
	public float ExplosionMinRadius => explosionMinRadius;
	public float ExplosionMaxRadius => explosionMaxRadius;
	public AnimationCurve ExplosionFalloffCurve => explosionFalloffCurve;
}
