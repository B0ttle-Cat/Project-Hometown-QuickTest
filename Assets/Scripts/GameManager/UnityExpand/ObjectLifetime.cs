using System;

using UnityEngine;

[DefaultExecutionOrder(99)]
public class ObjectLifetime : MonoBehaviour
{
        [SerializeField]
	private float lifeTime;
    private Action deathAction;

	protected virtual void OnEnable()
    {
        ResetTime(lifeTime, deathAction ?? TimeoutDestroy);
	}
	protected virtual void Update()
    {
		float deltaTIme = Time.deltaTime;
        UpdateTime(in deltaTIme);
	}

    public virtual void ResetTime(float lifeTime, Action deathAction)
    {
        this.lifeTime = lifeTime; 
        this. deathAction = deathAction;
    }
    protected virtual void UpdateTime(in float deltaTIme)
    {
        if (lifeTime <= 0f) return;

		lifeTime -= deltaTIme;
		
        if (lifeTime < 0f)
		{
            deathAction?.Invoke();
		}
	}
    protected virtual void TimeoutDestroy()
    {
		Destroy(gameObject);
	}
}
