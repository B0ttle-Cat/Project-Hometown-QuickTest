using UnityEngine;

public interface IStrategyPoolingElement
{
	GameObject PrefabReference { get; set; }
	public IStrategyPoolingElement ThisElement { get; }
	void InStrategyCollector();
	void OutStrategyCollector();
}
