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
	GameObject IStrategyPoolingElement.PrefabReference { get; set; }
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

public partial class ProjectileObject // Projectile Hit Listener
{
	public void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.TryGetComponent(out ITargetableCombatant target))
		{
			// Handle hit logic
		}
	}
}