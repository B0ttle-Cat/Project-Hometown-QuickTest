using UnityEngine;

public partial class ProjectileObject : MonoBehaviour
{

}
public partial class ProjectileObject : IStrategyPoolingElement
{
    public IStrategyPoolingElement ThisElement => this;
	public GameObject PrefabReference { get ; set ; }
    void IStrategyPoolingElement.InStrategyCollector()
    {
    }
    void IStrategyPoolingElement.OutStrategyCollector()
    {
    }
}
