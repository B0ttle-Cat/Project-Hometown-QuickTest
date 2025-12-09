using Sirenix.OdinInspector;

using StrategyManagerModule;

using UnityEngine;

public partial class ProjectileObject : MonoBehaviour
{
	[SerializeField, InlineProperty, HideLabel]
	private ProjectileRuntimeData runtimeData;
	[SerializeField, InlineProperty, HideLabel]
    private ProjectileStatsData statsData;
	public ProjectileRuntimeData RuntimeData { get => runtimeData; private set => runtimeData = value; }
	public ProjectileStatsData StatsData { get => statsData; private set => statsData = value; }

	public void Init()
	{
		runtimeData = null;
		statsData = null;
	}
	public void Init(StrategyStartSetterData.ProjectileData.Info setterData)
    {
		RuntimeData = new ProjectileRuntimeData(setterData);
	}
    public void Init(ProjectileProfileObject profile)
	{
		StatsData = new ProjectileStatsData(profile);
		InitOther();
	}
	
	private void InitOther()
	{
		InitMovement();
	}
	partial void InitMovement();

	private void DeInit()
	{
		DeinitMovment();
	}
	partial void DeinitMovment();
}
public partial class ProjectileObject : IStrategyPoolingElement
{
	IStrategyElement IStrategyElement.ThisElement => this;
    int IStrategyElement.ID { get; set; }
	GameObject IStrategyPoolingElement.PrefabReference { get ; set ; }
    void IStrategyElement.InStrategyCollector()
    {
    }
    void IStrategyElement.OutStrategyCollector()
    {
    }
	void IStrategyStartGame.OnStartGame()
	{
	}
	void IStrategyStartGame.OnStopGame()
	{
	}
}

[RequireComponent(typeof(ProjectileMovement))]
public partial class ProjectileObject : IProjectileMovement
{
	private ProjectileMovement movement;
	public IProjectileMovement ThisMovement => movement;
	public int OrderElementID => ThisMovement.OrderElementID;
    public int TargetElementID => ThisMovement.TargetElementID;
    public Vector3 StartPosition => ThisMovement.StartPosition;
    public Vector3 TargetPosition => ThisMovement.TargetPosition;
    public Vector3 CurrentPosition => ThisMovement.CurrentPosition;
    public float MoveSpeed => ThisMovement.MoveSpeed;
    public Vector3 MoveDiraction => ThisMovement.MoveDiraction;

	partial void InitMovement()
	{
		movement = GetComponent<ProjectileMovement>();
		movement.Init(StatsData);
	}

	partial void DeinitMovment()
	{
		movement.Deinit();
		movement = null;
	}
	void IProjectileMovement.MovmentUpdate(in float deltaTime)=>ThisMovement.MovmentUpdate(deltaTime);
    void IProjectileMovement.SetTarget(IUnitCombatController order, ITargetableCombatant target)=> ThisMovement.SetTarget(order, target);
}