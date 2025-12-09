using Sirenix.OdinInspector;

using StrategyManagerModule;

using UnityEngine;

[RequireComponent(typeof(ProjectileMovement))]
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
		ThisMovement = GetComponent<ProjectileMovement>();
	}
	public void Init(StrategyStartSetterData.ProjectileData.Info setterData)
    {
		RuntimeData = new ProjectileRuntimeData(setterData);
	}
    public void Init(ProjectileProfileObject profile)
	{
		StatsData = new ProjectileStatsData(profile);
	}
}
public partial class ProjectileObject : IStrategyPoolingElement
{
    public IStrategyPoolingElement ThisElement => this;
	public GameObject PrefabReference { get ; set ; }
    IStrategyElement IStrategyElement.ThisElement { get; }
    int IStrategyElement.ID { get; set; }

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
public partial class ProjectileObject : IProjectileMovement
{
	public IProjectileMovement ThisMovement { get; private set; }

    public int OrderElementID => ThisMovement.OrderElementID;

    public int TargetElementID => ThisMovement.TargetElementID;

    public Vector3 StartPosition => ThisMovement.StartPosition;

    public Vector3 TargetPosition => ThisMovement.TargetPosition;

    public void SetTarget(int orderID, int targetID)=> ThisMovement.SetTarget(orderID, targetID);
}