using UnityEngine;

[RequireComponent(typeof(ProjectileObject))]
public class ProjectileLifetime : ObjectLifetime
{
    private ProjectileObject thisObject;

	private void Awake()
    {
		thisObject = GetComponent<ProjectileObject>();

	}
    protected override void TimeoutDestroy()
    {
        StrategyElementFactory.Destroy(thisObject);
    }
}
