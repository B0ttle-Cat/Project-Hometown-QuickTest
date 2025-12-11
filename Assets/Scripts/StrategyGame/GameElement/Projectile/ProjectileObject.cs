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
	
	public void Init(StrategyStartSetterData.ProjectileData.Info setterData)
	{
		RuntimeData = new ProjectileRuntimeData(setterData);
	}
	public void Init(ProjectileProfileObject profile)
	{
		if (StatsData == null)
		{
			StatsData = new ProjectileStatsData(profile);
		}
		if(RuntimeData == null)
		{
			RuntimeData = new ProjectileRuntimeData(profile);
		}
	}


	public void InitOther()
	{
		InitScale();
		InitLifetime();
		InitMovement();
	}
	partial void InitLifetime();
	partial void InitMovement();
	partial void InitHitReporting();
	public void DeInit()
	{
		DeinitMovment();
		runtimeData = null;
	}
	partial void DeinitMovment();
	partial void DeinitHitReporting();


	private void InitScale()
	{
		transform.localScale = Vector3.one * statsData.CollisionRadius;
	}
}
public partial class ProjectileObject // Init Lifetime
{
	ProjectileLifetime objectLifetime;
	partial void InitLifetime()
	{
		if(objectLifetime == null || !TryGetComponent<ProjectileLifetime>(out objectLifetime))
		{
			objectLifetime = gameObject.AddComponent<ProjectileLifetime>();
		}
		objectLifetime.ResetTime(RuntimeData.LifeTime);
	}

}
public partial class ProjectileObject : IStrategyPoolingElement
{
	IStrategyElement IStrategyElement.ThisElement => this;
	int IStrategyElement.ID { get; set; }
	GameObject IStrategyPoolingElement.PrefabReference { get; set; }
	void IStrategyElement.InStrategyCollector()
	{
		runtimeData = null;
		statsData = null;
	}
	void IStrategyElement.OutStrategyCollector()
	{
		runtimeData = null;
		statsData = null;
	}
	void IStrategyStartGame.OnStartGame()
	{
	}
	void IStrategyStartGame.OnStopGame()
	{
	}
}
public partial class ProjectileObject : IProjectileHitReporting
{
	partial void InitHitReporting()
	{

	}
	partial void DeinitHitReporting()
	{

	}


	void IProjectileHitReporting.HitOtherObject(GameObject gameObject)
    {
    }

    void IProjectileHitReporting.HitTargetable(ITargetableCombatant targetable)
    {

	}
	void IProjectileHitReporting.HitOtherElement(IStrategyElement hit)
	{

	}

}