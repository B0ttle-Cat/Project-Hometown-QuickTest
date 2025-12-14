using UnityEngine;

/// <summary>
/// StrategyElementCollector에 수집될 다음을 포함해야 한다: IStrategyElement
/// </summary>
public interface IStrategyElement : IStrategyStartGame 
{
	public IStrategyElement ThisElement { get; }
	public int ID { get; set; }
	void InStrategyCollector();
	void OutStrategyCollector();
}
public interface IStrategyMonoElement : IStrategyElement
{
	GameObject gameObject { get; }
}
public interface IStrategyPoolingElement : IStrategyMonoElement
{
	GameObject PrefabReference { get; set; }
}