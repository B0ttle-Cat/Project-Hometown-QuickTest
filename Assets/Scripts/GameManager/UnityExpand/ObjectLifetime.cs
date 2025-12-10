using UnityEngine;

[DefaultExecutionOrder(99)]
public class ObjectLifetime : MonoBehaviour
{
    public float lifeTime;
    protected virtual void OnEnable()
    {
        ResetTime(lifeTime);
	}
	protected virtual void Update()
    {
		float deltaTIme = Time.deltaTime;
        UpdateTime(in deltaTIme);
	}

    public virtual void ResetTime(float lifeTime)
    {
        this.lifeTime = lifeTime; 
    }
    protected virtual void UpdateTime(in float deltaTIme)
    {
        if (lifeTime <= 0f) return;

		lifeTime -= deltaTIme;
		
        if (lifeTime < 0f)
		{
            TimeoutDestroy();
		}
	}
    protected virtual void TimeoutDestroy()
    {
		Destroy(gameObject);
	}
}
